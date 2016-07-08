using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Controls.Extensions
{
    public static class HttpClientExtensions
    {
        public static Task<byte[]> GetByteArrayAsync(this HttpClient client, Uri requestUri, IProgress<HttpProgress> progress)
        {
            return GetByteArrayAsync(client, requestUri, CancellationToken.None, progress);
        }

        public static async Task<byte[]> GetByteArrayAsync(this HttpClient client, Uri requestUri, CancellationToken cancellationToken, IProgress<HttpProgress> progress)
        {
            var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var responseContent = response.Content;
            var length = responseContent.Headers.ContentLength;
            var totalBytesToReceive = (int?)length;
            progress?.Report(new HttpProgress(0, totalBytesToReceive));
            var stream = await responseContent.ReadAsStreamAsync();
            var bytes = new List<byte>();
            while (true)
            {
                const int bufferLength = 1024;
                var buffer = new byte[bufferLength];
                var size = await stream.ReadAsync(buffer, 0, bufferLength, cancellationToken);
                if (size <= 0)
                {
                    break;
                }
                bytes.AddRange(buffer.Take(size));
                progress?.Report(new HttpProgress(bytes.Count, totalBytesToReceive));
            }
            return bytes.ToArray();
        }

        public static Task<byte[]> GetByteArrayAsync(this HttpClient client, string requestUri, CancellationToken cancellationToken, IProgress<HttpProgress> progress)
        {
            return GetByteArrayAsync(client, CreateUri(requestUri), cancellationToken, progress);
        }

        public static Task<byte[]> GetByteArrayAsync(this HttpClient client, string requestUri, IProgress<HttpProgress> progress)
        {
            return GetByteArrayAsync(client, requestUri, CancellationToken.None, progress);
        }

        private static Uri CreateUri(string uri)
        {
            return string.IsNullOrEmpty(uri) ? null : new Uri(uri, UriKind.RelativeOrAbsolute);
        }
    }
}