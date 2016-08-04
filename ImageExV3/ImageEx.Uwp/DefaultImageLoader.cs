using Controls.Uwp.Extensions;
using Controls.Uwp.Utils;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Weakly;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace Controls.Uwp
{
    public sealed class DefaultImageLoader : IImageLoader
    {
        private const string CacheFolderName = "ImageEx";

        private static readonly WeakValueDictionary<string, BitmapImage> CacheBitmapImages = new WeakValueDictionary<string, BitmapImage>();

        private static readonly string CacheFolderPath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path,
            CacheFolderName);

        private DefaultImageLoader()
        {
        }

        public static IImageLoader Instance
        {
            get;
        } = new DefaultImageLoader();

        public long CalculateCacheSize()
        {
            return (from cacheFilePath in Directory.EnumerateFiles(CacheFolderPath)
                    select new FileInfo(cacheFilePath).Length).Sum();
        }

        public bool ContainsCache(string source)
        {
            if (source == null)
            {
                return false;
            }

            var uriSource = ToUriSource(source);
            var cacheFilePath = GetCacheFilePath(uriSource);
            return File.Exists(cacheFilePath);
        }

        public void DeleteAllCache()
        {
            Directory.Delete(CacheFolderPath, true);
        }

        public bool DeleteCache(string source)
        {
            if (source == null)
            {
                return false;
            }

            var uriSource = ToUriSource(source);
            var cacheFilePath = GetCacheFilePath(uriSource);
            if (File.Exists(cacheFilePath))
            {
                File.Delete(cacheFilePath);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<BitmapResult> GetBitmapAsync(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // 检查内存缓存。
            BitmapImage bitmap;
            if (CacheBitmapImages.TryGetValue(source, out bitmap))
            {
                // 内存缓存存在，直接使用内存缓存。
                return new BitmapResult(bitmap);
            }
            else
            {
                var uriSource = ToUriSource(source);
                if (IsHttpUri(uriSource))
                {
                    var cacheFilePath = GetCacheFilePath(uriSource);
                    if (File.Exists(cacheFilePath))
                    {
                        TaskCompletionSource<BitmapResult> tcs = new TaskCompletionSource<BitmapResult>();
                        bitmap = new BitmapImage();
                        RoutedEventHandler imageOpenedHandler = null;
                        imageOpenedHandler = (sender, e) =>
                        {
                            bitmap.ImageOpened -= imageOpenedHandler;
                            // 放入内存缓存。
                            CacheBitmapImages[source] = bitmap;
                            tcs.SetResult(new BitmapResult(bitmap));
                        };
                        bitmap.ImageOpened += imageOpenedHandler;
                        ExceptionRoutedEventHandler imageFailedHandler = null;
                        imageFailedHandler = (sender, e) =>
                        {
                            bitmap.ImageFailed -= imageFailedHandler;
                            tcs.SetResult(new BitmapResult(e.ErrorMessage));
                        };
                        bitmap.ImageFailed += imageFailedHandler;
                        bitmap.UriSource = new Uri(cacheFilePath, UriKind.Absolute);
                        return await tcs.Task;
                    }
                    else
                    {
                        // TODO
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    TaskCompletionSource<BitmapResult> tcs = new TaskCompletionSource<BitmapResult>();
                    bitmap = new BitmapImage();
                    RoutedEventHandler imageOpenedHandler = null;
                    imageOpenedHandler = (sender, e) =>
                    {
                        bitmap.ImageOpened -= imageOpenedHandler;
                        // 放入内存缓存。
                        CacheBitmapImages[source] = bitmap;
                        tcs.SetResult(new BitmapResult(bitmap));
                    };
                    bitmap.ImageOpened += imageOpenedHandler;
                    ExceptionRoutedEventHandler imageFailedHandler = null;
                    imageFailedHandler = (sender, e) =>
                    {
                        bitmap.ImageFailed -= imageFailedHandler;
                        tcs.SetResult(new BitmapResult(e.ErrorMessage));
                    };
                    bitmap.ImageFailed += imageFailedHandler;
                    bitmap.UriSource = uriSource;
                    return await tcs.Task;
                }
            }
        }

        public async Task<byte[]> GetBytesAsync(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var uriSource = ToUriSource(source);
            if (IsHttpUri(uriSource))
            {
                var cacheFilePath = GetCacheFilePath(uriSource);
                if (File.Exists(cacheFilePath))
                {
                    return await FileExtensions.ReadAllBytesAsync(cacheFilePath);
                }
                else
                {
                    byte[] bytes;
                    using (var client = new HttpClient())
                    {
                        var buffer = await client.GetBufferAsync(uriSource);
                        bytes = buffer.ToArray();
                    }

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.ImageOpened += (sender, e) =>
                    {
                        //TODO 放入内存缓存。

                        //TODO 保存文件。
                    };
                    bitmap.SetSourceAsync(new MemoryStream(bytes).AsRandomAccessStream());

                    return bytes;
                }
            }
            else
            {
                if (string.Equals(uriSource.Scheme, "file", StringComparison.OrdinalIgnoreCase))
                {
                    // 绝对路径。
                    return await FileExtensions.ReadAllBytesAsync(source);
                }
                else
                {
                    // ms-appx 或 ms-appdata，其它路径不支持。
                    var file = await StorageFile.GetFileFromApplicationUriAsync(uriSource);
                    var buffer = await FileIO.ReadBufferAsync(file);
                    return buffer.ToArray();
                }
            }
        }

        private static string GetCacheFilePath(Uri uriSource)
        {
            if (uriSource == null)
            {
                throw new ArgumentNullException(nameof(uriSource));
            }

            var originalString = uriSource.OriginalString;
            var extension = Path.GetExtension(originalString);
            var cacheFileName = HashHelper.GenerateMd5Hash(originalString) + extension;
            return Path.Combine(CacheFolderPath, cacheFileName);
        }

        private static bool IsHttpUri(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var scheme = uri.Scheme;
            return uri.IsAbsoluteUri && (string.Equals(scheme, "http", StringComparison.OrdinalIgnoreCase) || string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase));
        }

        private static Uri ToUriSource(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Uri uriSource;
            if (Uri.TryCreate(source, UriKind.RelativeOrAbsolute, out uriSource))
            {
                if (uriSource.IsAbsoluteUri == false)
                {
                    Uri.TryCreate("ms-appx:///" + (source.StartsWith("/") ? source.Substring(1) : source), UriKind.Absolute, out uriSource);
                }
            }

            if (uriSource == null)
            {
                throw new NotSupportedException();
            }

            return uriSource;
        }

        private async Task<byte[]> DownloadImageAsync(string source, Uri uriSource)
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] bytes;
                try
                {
                    var task = client.GetBufferAsync(uriSource);
                    task.Progress = (asyncInfo, progressInfo) =>
                    {
                        // TODO
                    };
                    var buffer = await task;
                    bytes = buffer.ToArray();
                }
                catch (Exception)
                {
                    bytes = null;
                    // TODO image failed
                }
                return bytes;
            }
        }
    }
}