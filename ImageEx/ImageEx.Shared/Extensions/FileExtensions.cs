using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Controls.Extensions
{
    public static class FileExtensions
    {
        public static async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            using (var fs = File.OpenWrite(path))
            {
                await fs.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
            }
        }

        public static Task WriteAllBytesAsync(string path, byte[] bytes)
        {
            return WriteAllBytesAsync(path, bytes, CancellationToken.None);
        }
    }
}