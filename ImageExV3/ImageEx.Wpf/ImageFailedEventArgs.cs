using System;

namespace Controls
{
    public class ImageFailedEventArgs : ExceptionEventArgs
    {
        internal ImageFailedEventArgs(string source, Exception errorException) : base(errorException)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Source = source;
        }

        public string Source
        {
            get;
        }
    }
}