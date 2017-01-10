using Controls.Extensions;
using Controls.Utils;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Weakly;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Controls
{
    public sealed class DefaultImageLoader : IImageLoader
    {
        private const string CacheFolderName = "ImageExCache";

        private const string FileScheme = "file";

        private const string HttpScheme = "http";

        private const string HttpsScheme = "https";

        private static readonly WeakValueDictionary<string, BitmapImage> CacheBitmapImages = new WeakValueDictionary<string, BitmapImage>();

        private static readonly string CacheFolderPath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, CacheFolderName);

        private static readonly ConcurrentDictionary<string, Task<byte[]>> ImageDownloadTasks = new ConcurrentDictionary<string, Task<byte[]>>();

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

        public async Task DeleteAllCacheAsync()
        {
            await Task.Run(() =>
            {
                Directory.Delete(CacheFolderPath, true);
            });
        }

        public async Task<bool> DeleteCacheAsync(string source)
        {
            if (source == null)
            {
                return false;
            }

            var uriSource = ToUriSource(source);
            var cacheFilePath = GetCacheFilePath(uriSource);
            if (File.Exists(cacheFilePath))
            {
                await Task.Run(() =>
                {
                    File.Delete(cacheFilePath);
                });
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
                        var bytes = await FileExtensions.ReadAllBytesAsync(cacheFilePath);
                        bitmap = new BitmapImage();
                        try
                        {
                            await bitmap.SetSourceAsync(new MemoryStream(bytes).AsRandomAccessStream());
                            // 放入内存缓存。
                            CacheBitmapImages[source] = bitmap;

                            return new BitmapResult(bitmap);
                        }
                        catch (Exception ex)
                        {
                            return new BitmapResult(ex);
                        }
                    }
                    else
                    {
                        Task<byte[]> imageDownloadTask;
                        if (ImageDownloadTasks.TryGetValue(source, out imageDownloadTask) == false)
                        {
                            imageDownloadTask = DownloadImageAsync(uriSource);
                            ImageDownloadTasks[source] = imageDownloadTask;
                        }

                        byte[] bytes;
                        try
                        {
                            bytes = await imageDownloadTask;
                        }
                        catch (HttpRequestException ex)
                        {
                            ImageDownloadTasks.TryRemove(source, out imageDownloadTask);
                            return new BitmapResult(ex);
                        }

                        bitmap = new BitmapImage();
                        try
                        {
                            await bitmap.SetSourceAsync(new MemoryStream(bytes).AsRandomAccessStream());
                            // 放入内存缓存。
                            CacheBitmapImages[source] = bitmap;

                            Action asyncAction = async () =>
                            {
                                try
                                {
                                    Directory.CreateDirectory(CacheFolderPath);
                                    await FileExtensions.WriteAllBytesAsync(cacheFilePath, bytes);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                                finally
                                {
                                    ImageDownloadTasks.TryRemove(source, out imageDownloadTask);
                                }
                            };
                            asyncAction.Invoke();

                            return new BitmapResult(bitmap);
                        }
                        catch (Exception ex)
                        {
                            ImageDownloadTasks.TryRemove(source, out imageDownloadTask);
                            return new BitmapResult(ex);
                        }
                    }
                }
                else
                {
                    byte[] bytes;
                    try
                    {
                        if (string.Equals(uriSource.Scheme, FileScheme, StringComparison.OrdinalIgnoreCase))
                        {
                            // 绝对路径。
                            bytes = await FileExtensions.ReadAllBytesAsync(source);
                        }
                        else
                        {
                            // ms-appx 或 ms-appdata，其它路径不支持。
                            var file = await StorageFile.GetFileFromApplicationUriAsync(uriSource);
                            var buffer = await FileIO.ReadBufferAsync(file);
                            bytes = buffer.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        return new BitmapResult(ex);
                    }

                    bitmap = new BitmapImage();
                    try
                    {
                        await bitmap.SetSourceAsync(new MemoryStream(bytes).AsRandomAccessStream());
                        // 放入内存缓存。
                        CacheBitmapImages[source] = bitmap;

                        return new BitmapResult(bitmap);
                    }
                    catch (Exception ex)
                    {
                        return new BitmapResult(ex);
                    }
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
                    using (var httpClient = new HttpClient())
                    {
                        bytes = await httpClient.GetByteArrayAsync(uriSource);
                    }

                    Action asyncAction = async () =>
                    {
                        var bitmap = new BitmapImage();
                        RoutedEventHandler imageOpenedHandler = null;
                        imageOpenedHandler = async (sender, e) =>
                        {
                            bitmap.ImageOpened -= imageOpenedHandler;
                            // 放入内存缓存。
                            CacheBitmapImages[source] = bitmap;

                            Directory.CreateDirectory(CacheFolderPath);
                            await FileExtensions.WriteAllBytesAsync(cacheFilePath, bytes);
                        };
                        bitmap.ImageOpened += imageOpenedHandler;
                        await bitmap.SetSourceAsync(new MemoryStream(bytes).AsRandomAccessStream());
                    };
                    asyncAction.Invoke();

                    return bytes;
                }
            }
            else
            {
                if (string.Equals(uriSource.Scheme, FileScheme, StringComparison.OrdinalIgnoreCase))
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
            var cacheFileName = HashHelper.GenerateMD5Hash(originalString) + extension;
            return Path.Combine(CacheFolderPath, cacheFileName);
        }

        private static bool IsHttpUri(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var scheme = uri.Scheme;
            return uri.IsAbsoluteUri
                && (string.Equals(scheme, HttpScheme, StringComparison.OrdinalIgnoreCase)
                || string.Equals(scheme, HttpsScheme, StringComparison.OrdinalIgnoreCase));
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

        private async Task<byte[]> DownloadImageAsync(Uri uriSource)
        {
            using (var httpClient = new HttpClient())
            {
                return await httpClient.GetByteArrayAsync(uriSource);
            }
        }
    }
}