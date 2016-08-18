using Windows.UI.Xaml.Media.Imaging;

namespace Controls
{
    public class BitmapResult
    {
        internal BitmapResult(BitmapImage bitmap)
        {
            Value = bitmap;
            Status = BitmapStatus.Opened;
        }

        internal BitmapResult(string errorMessage)
        {
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

        public static implicit operator BitmapImage(BitmapResult result)
        {
            return result.Value;
        }
    }
}