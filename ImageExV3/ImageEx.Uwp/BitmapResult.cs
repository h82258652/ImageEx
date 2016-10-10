using System;
using Windows.UI.Xaml.Media.Imaging;

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

        internal BitmapResult(string errorMessage)
        {
            if (errorMessage == null)
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            ErrorMessage = errorMessage;
            Status = BitmapStatus.Failed;
        }

        public string ErrorMessage
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