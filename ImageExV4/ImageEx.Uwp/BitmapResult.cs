using System;
using Windows.UI.Xaml.Media.Imaging;

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

        internal BitmapResult(string failedMessage)
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

        public BitmapImage Value
        {
            get;
        }
    }
}