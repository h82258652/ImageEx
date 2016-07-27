using Controls.Extensions;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Controls.Wpf.Demo
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var bytes = await ImageEx.GetBytesAsync(
                 new Uri("https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/bd_logo1_31bdc765.png"));
            await FileExtensions.WriteAllBytesAsync(@"C:\Temp\ddd.jpg", bytes);
            return;

            if (Image.Source == "https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/bd_logo1_31bdc765.png")
            {
                Image.Source = null;
            }
            else
            {
                Image.Source =
                    "https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/bd_logo1_31bdc765.png";
            }
        }

        private void Image_OnImageOpened(object sender, EventArgs e)
        {
            //var r = sender as ImageEx;
            //RenderTargetBitmap bitmap = new RenderTargetBitmap(540, 258, 96, 96, PixelFormats.Default);
            //bitmap.Render(r);
            //RR.Source = bitmap;
        }

        private void BitmapSource_OnDownloadCompleted(object sender, EventArgs e)
        {
            var r = Tf;
            var b = sender as BitmapImage;
            RenderTargetBitmap bitmap = new RenderTargetBitmap(b.PixelWidth, b.PixelHeight, 96, 96, PixelFormats.Default);
            bitmap.Render(r);
            RR.Source = bitmap;
        }
    }
}