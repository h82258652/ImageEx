using System;
using Windows.UI.Xaml.Media;

namespace Controls
{
    public sealed class BitmapResult
    {
        public BitmapResult(ImageSource bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            Value = bitmap;
            Status = BitmapStatus.Opened;
        }

        public BitmapResult(Exception failedException)
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

        public ImageSource Value
        {
            get;
        }
    }
}