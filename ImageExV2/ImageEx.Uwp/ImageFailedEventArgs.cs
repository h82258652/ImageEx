using System;

namespace Controls
{
    public class ImageFailedEventArgs : ExceptionEventArgs
    {
        internal ImageFailedEventArgs(string url, string errorMessage) : base(errorMessage)
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