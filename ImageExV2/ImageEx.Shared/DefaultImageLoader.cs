using System;
using System.Windows.Media.Imaging;

namespace Controls
{
    public class DefaultImageLoader : IImageLoader
    {
        private static readonly IImageLoader Instance = new DefaultImageLoader();

        private DefaultImageLoader()
        {
        }

        public static IImageLoader GetInstance()
        {
            return Instance;
        }

        public long CalculateCacheSize()
        {
            throw new NotImplementedException();
        }

        public bool ContainsCache(Uri source)
        {
            throw new NotImplementedException();
        }

        public void DeleteAllCache()
        {
            throw new NotImplementedException();
        }

        public bool DeleteCache(Uri source)
        {
            throw new NotImplementedException();
        }

        public BitmapImage GetBitmapAsync(Uri source)
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytesAsync(Uri source)
        {
            throw new NotImplementedException();
        }
    }
}