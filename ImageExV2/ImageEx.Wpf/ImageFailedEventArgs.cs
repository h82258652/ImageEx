using System;

namespace Controls
{
    public class ImageFailedEventArgs : ExceptionEventArgs
    {
        internal ImageFailedEventArgs(string url, Exception errorException) : base(errorException)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            Url = url;
        }

        public string Url
        {
            get;
        }
    }
}