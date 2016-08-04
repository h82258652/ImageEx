using System.Threading.Tasks;

namespace Controls.Uwp
{
    public interface IImageLoader
    {
        long CalculateCacheSize();

        bool ContainsCache(string source);

        void DeleteAllCache();

        bool DeleteCache(string source);

        Task<BitmapResult> GetBitmapAsync(string source);

        Task<byte[]> GetBytesAsync(string source);
    }
}