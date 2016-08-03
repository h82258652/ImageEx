using System;
using Windows.UI.Xaml.Media.Imaging;

namespace Controls.Uwp
{
    public class BitmapResult
    {
        public BitmapStatus Status
        {
            get;
        }

        public Exception FailedException
        {
            get;
        }

        public BitmapImage Result
        {
            get;
        }
    }
}