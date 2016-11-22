using Controls.Extensions;
using Controls.Utils;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Weakly;

namespace Controls
{
    public sealed class DefaultImageLoader : IImageLoader
    {
        private const string CacheFolderName = "ImageExCache";

        private static readonly WeakValueDictionary<string, BitmapImage> CacheBitmapImages = new WeakValueDictionary<string, BitmapImage>();

        private static readonly string CacheFolderPath = Path.Combine(Path.GetTempPath(), CacheFolderName);

        private static readonly ConcurrentDictionary<string, Task<byte[]>> ImageDownloadTasks = new ConcurrentDictionary<string, Task<byte[]>>();

        private static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1);

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
                        await SemaphoreSlim.WaitAsync();
                        try
                        {
                            bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(cacheFilePath);
                            bitmap.EndInit();
                            // 放入内存缓存。
                            CacheBitmapImages[source] = bitmap;

                            return new BitmapResult(bitmap);
                        }
                        catch (NotSupportedException ex)
                        {
                            return new BitmapResult(ex);
                        }
                        finally
                        {
                            await Task.Delay(1);
                            SemaphoreSlim.Release();
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

                        await SemaphoreSlim.WaitAsync();
                        try
                        {
                            bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = new MemoryStream(bytes);
                            bitmap.EndInit();
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
                        catch (NotSupportedException ex)
                        {
                            ImageDownloadTasks.TryRemove(source, out imageDownloadTask);
                            return new BitmapResult(ex);
                        }
                        finally
                        {
                            await Task.Delay(1);
                            SemaphoreSlim.Release();
                        }
                    }
                }
                else
                {
                    await SemaphoreSlim.WaitAsync();
                    try
                    {
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = uriSource;
                        bitmap.EndInit();
                        // 放入内存缓存。
                        CacheBitmapImages[source] = bitmap;

                        return new BitmapResult(bitmap);
                    }
                    catch (FileNotFoundException ex)
                    {
                        return new BitmapResult(ex);
                    }
                    catch (NotSupportedException ex)
                    {
                        return new BitmapResult(ex);
                    }
                    finally
                    {
                        await Task.Delay(1);// 防止同时加载大量图片时，UI 线程卡死。
                        SemaphoreSlim.Release();
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
                        await SemaphoreSlim.WaitAsync();
                        try
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = new MemoryStream(bytes);
                            bitmap.EndInit();
                            // 放入内存缓存。
                            CacheBitmapImages[source] = bitmap;

                            Directory.CreateDirectory(CacheFolderPath);
                            await FileExtensions.WriteAllBytesAsync(cacheFilePath, bytes);
                        }
                        catch (NotSupportedException)
                        {
                        }
                        finally
                        {
                            await Task.Delay(1);// 防止同时加载大量图片时，UI 线程卡死。
                            SemaphoreSlim.Release();
                        }
                    };
                    asyncAction.Invoke();

                    return bytes;
                }
            }
            else
            {
                if (string.Equals(uriSource.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
                {
                    // 绝对路径。
                    return await FileExtensions.ReadAllBytesAsync(source);
                }
                else
                {
                    // pack://application:,,,/，其它路径不支持。
                    var streamResourceInfo = System.Windows.Application.GetResourceStream(uriSource);
                    if (streamResourceInfo != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await streamResourceInfo.Stream.CopyToAsync(memoryStream);
                            return memoryStream.ToArray();
                        }
                    }
                    else
                    {
                        throw new FileNotFoundException();
                    }
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
                && (string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
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
                    Uri.TryCreate("pack://application:,,,/" + (source.StartsWith("/") ? source.Substring(1) : source), UriKind.Absolute, out uriSource);
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