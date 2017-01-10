using System.Threading.Tasks;

namespace Controls
{
    public interface IImageLoader
    {
        long CalculateCacheSize();

        bool ContainsCache(string source);

        Task DeleteAllCacheAsync();

        Task<bool> DeleteCacheAsync(string source);

        Task<BitmapResult> GetBitmapAsync(string source);

        Task<byte[]> GetBytesAsync(string source);
    }
}