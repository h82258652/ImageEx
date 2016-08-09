using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Controls.Extensions
{
    public static class FileExtensions
    {
        public static Task<byte[]> ReadAllBytesAsync(string path)
        {
            return ReadAllBytesAsync(path, CancellationToken.None);
        }

        public static async Task<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            using (var fs = File.OpenRead(path))
            {
                var buffer = new byte[fs.Length];
                await fs.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                return buffer;
            }
        }

        public static Task WriteAllBytesAsync(string path, byte[] bytes)
        {
            return WriteAllBytesAsync(path, bytes, CancellationToken.None);
        }

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
    }
}