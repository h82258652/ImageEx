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

        public BitmapResult(string failedMessage)
        {
            if (failedMessage == null)
            {
                throw new ArgumentNullException(nameof(failedMessage));
            }

            FailedMessage = failedMessage;
            Status = BitmapStatus.Failed;
        }

        public string FailedMessage
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