using System;
using System.Windows.Media.Imaging;

namespace Controls
{
    public class BitmapResult
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

        internal BitmapResult(Exception errorException)
        {
            if (errorException == null)
            {
                throw new ArgumentNullException(nameof(errorException));
            }

            ErrorException = errorException;
            Status = BitmapStatus.Failed;
        }

        public Exception ErrorException
        {
            get;
        }

        public BitmapImage Value
        {
            get;
        }

        public BitmapStatus Status
        {
            get;
        }
    }
}