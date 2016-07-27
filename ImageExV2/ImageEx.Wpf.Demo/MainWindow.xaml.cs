using Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageEx.Wpf.Demo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //var bitmap = await DefaultImageLoader.Instance.GetBytesAsync(@"C:\Users\h8225\Desktop\failed_image.png");
            //var b = new BitmapImage();
            //b.BeginInit();
            //b.StreamSource = new MemoryStream(bitmap);
            //b.EndInit();
            //Image.Source = b;
        }
    }
}