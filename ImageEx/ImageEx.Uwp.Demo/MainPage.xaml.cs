using Windows.UI.Xaml;

namespace Controls.Uwp.Demo
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (Image.Source == "/Assets/Images/test.jpg")
            {
                Image.Source = null;
            }
            else
            {
                Image.Source = "/Assets/Images/test.jpg";
            }
        }
    }
}