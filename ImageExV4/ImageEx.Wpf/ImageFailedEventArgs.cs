using System;

namespace Controls
{
    public sealed class ImageFailedEventArgs : ExceptionEventArgs
    {
        internal ImageFailedEventArgs(string source, Exception failedException) : base(failedException)
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