using System;
using System.Windows.Media.Imaging;

namespace Controls
{
    public sealed class BitmapResult
    {
        internal BitmapResult(BitmapImage bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            Value = bitmap;
            Status = BitmapStatus.Opened;
        }

        internal BitmapResult(Exception failedException)
        {
            if (failedException == null)
            {
                throw new ArgumentNullException(nameof(failedException));
            }

            FailedException = failedException;
            Status = BitmapStatus.Failed;
        }

        public Exception FailedException
        {
            get;
        }

        public BitmapStatus Status
        {
            get;
        }

        public BitmapImage Value
        {
            get;
        }
    }
}