using System;
using Windows.Web.Http;

namespace Controls
{
    public class HttpDownloadProgressEventArgs : EventArgs
    {
        public HttpDownloadProgressEventArgs(string url, HttpProgress progress)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            Url = url;
            Progress = progress;
        }

        public HttpProgress Progress
        {
            get;
        }

        public string Url
        {
            get;
        }
    }
}