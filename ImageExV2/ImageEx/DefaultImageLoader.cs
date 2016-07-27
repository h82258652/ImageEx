using Controls.Extensions;
using Controls.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Weakly;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace Controls
{
    public class DefaultImageLoader : IImageLoader
    {
        private const string CacheFolderName = "ImageExCache";

        private static readonly WeakValueDictionary<string, BitmapImage> CacheBitmapImages = new WeakValueDictionary<string, BitmapImage>();

        private static readonly string CacheFolderPath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, CacheFolderName);

        private static readonly Dictionary<string, Task<byte[]>> ImageDownloadTasks = new Dictionary<string, Task<byte[]>>();

        private DefaultImageLoader()
        {
        }

        public event EventHandler<HttpDownloadProgressEventArgs> DownloadProgressChanged;

        public event EventHandler<ImageFailedEventArgs> ImageFailed;

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
                throw new ArgumentNullException(nameof(source));
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
                throw new ArgumentNullException(nameof(source));
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

        public async Task<BitmapImage> GetBitmapAsync(string source)
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
                return bitmap;
            }
            else
            {
                var uriSource = ToUriSource(source);
                if (IsHttpUri(uriSource))
                {
                    var cacheFilePath = GetCacheFilePath(uriSource);
                    if (File.Exists(cacheFilePath))
                    {
                        bitmap = new BitmapImage();
                        bitmap.ImageOpened += (sender, e) =>
                        {
                            // 放入内存缓存。
                            CacheBitmapImages[source] = bitmap;
                        };
                        bitmap.ImageFailed += (sender, e) =>
                        {
                            ImageFailed?.Invoke(this, new ImageFailedEventArgs(source, e.ErrorMessage));
                        };
                        bitmap.UriSource = new Uri(cacheFilePath);
                    }
                    else
                    {
                        Task<byte[]> task;
                        if (ImageDownloadTasks.TryGetValue(source, out task) == false)
                        {
                            task = DownloadImageAsync(source, uriSource);
                            ImageDownloadTasks[source] = task;
                        }

                        try
                        {
                            var bytes = await task;

                            bitmap = new BitmapImage();
                            bitmap.ImageOpened += (sender, e) =>
                            {
                                // 放入内存缓存。
                                CacheBitmapImages[source] = bitmap;

                                SaveImageDataToCacheFolderAsync(cacheFilePath, bytes);
                            };
                            bitmap.ImageFailed += (sender, e) =>
                            {
                                ImageFailed?.Invoke(this, new ImageFailedEventArgs(source, e.ErrorMessage));
                            };
                            await bitmap.SetSourceAsync(new MemoryStream(bytes).AsRandomAccessStream());
                        }
                        finally
                        {
                            ImageDownloadTasks.Remove(source);
                        }
                    }
                }
                else
                {
                    bitmap = new BitmapImage();
                    bitmap.ImageOpened += (sender, e) =>
                    {
                        // 放入内存缓存。
                        CacheBitmapImages[source] = bitmap;
                    };
                    bitmap.ImageFailed += (sender, e) =>
                    {
                        ImageFailed?.Invoke(this, new ImageFailedEventArgs(source, e.ErrorMessage));
                    };
                    bitmap.UriSource = uriSource;
                }

                return bitmap;
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
                    Task<byte[]> task;
                    if (ImageDownloadTasks.TryGetValue(source, out task) == false)
                    {
                        task = DownloadImageAsync(source, uriSource);
                        ImageDownloadTasks[source] = task;
                    }

                    try
                    {
                        var bytes = await task;

                        var bitmap = new BitmapImage();
                        bitmap.ImageOpened += (sender, e) =>
                        {
                            SaveImageDataToCacheFolderAsync(cacheFilePath, bytes);
                        };
                        await bitmap.SetSourceAsync(new MemoryStream(bytes).AsRandomAccessStream());

                        return bytes;
                    }
                    finally
                    {
                        ImageDownloadTasks.Remove(source);
                    }
                }
            }
            else
            {
                return await FileExtensions.ReadAllBytesAsync(source);
            }
        }

        private static string GetCacheFilePath(Uri uriSource)
        {
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

        private static async void SaveImageDataToCacheFolderAsync(string cacheFilePath, byte[] bytes)
        {
            Directory.CreateDirectory(CacheFolderPath);
            try
            {
                await FileExtensions.WriteAllBytesAsync(cacheFilePath, bytes);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private static Uri ToUriSource(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Uri uri;
            if (Uri.TryCreate(source, UriKind.RelativeOrAbsolute, out uri))
            {
                if (uri.IsAbsoluteUri == false)
                {
                    Uri.TryCreate("ms-appx:///" + (source.StartsWith("/") ? source.Substring(1) : source), UriKind.Absolute, out uri);
                }
            }

            if (uri == null)
            {
                throw new NotSupportedException();
            }

            return uri;
        }

        private async Task<byte[]> DownloadImageAsync(string source, Uri uriSource)
        {
            using (var client = new HttpClient())
            {
                var task = client.GetBufferAsync(uriSource);
                task.Progress = (asyncInfo, progressInfo) =>
                {
                    DownloadProgressChanged?.Invoke(this, new HttpDownloadProgressEventArgs(source, progressInfo));
                };
                var buffer = await task;
                var bytes = buffer.ToArray();
                return bytes;
            }
        }
    }
}