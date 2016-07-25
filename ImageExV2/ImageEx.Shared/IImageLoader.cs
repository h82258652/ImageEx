using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Controls
{
    public interface IImageLoader
    {
        long CalculateCacheSize();

        bool ContainsCache(Uri source);

        void DeleteAllCache();

        bool DeleteCache(Uri source);

        Task<BitmapImage> GetBitmapAsync(Uri source);

        Task<byte[]> GetBytesAsync(Uri source);
    }
}