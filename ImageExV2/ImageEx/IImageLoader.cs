using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Controls
{
    public interface IImageLoader
    {
        event EventHandler<HttpDownloadProgressEventArgs> DownloadProgressChanged;

        event EventHandler<ImageFailedEventArgs> ImageFailed;

        long CalculateCacheSize();

        bool ContainsCache(string source);

        void DeleteAllCache();

        bool DeleteCache(string source);

        Task<BitmapImage> GetBitmapAsync(string source);

        Task<byte[]> GetBytesAsync(string source);
    }
}