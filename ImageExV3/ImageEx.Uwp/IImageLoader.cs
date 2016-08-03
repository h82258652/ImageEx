using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Controls.Uwp
{
    public interface IImageLoader
    {
        long CalculateCacheSize();

        bool ContainsCache(string source);

        void DeleteAllCache();

        bool DeleteCache(string source);

        Task<BitmapImage> GetBitmapAsync(string source);

        Task<byte[]> GetBytesAsync(string source);
    }
}