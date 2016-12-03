using Controls;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            this.DataContext = new Window1ViewModel();
        }

        private async void ImageEx_OnImageFailed(object sender, ImageFailedEventArgs e)
        {
            var window1ViewModel = this.DataContext as Window1ViewModel;
            await Task.Delay(100);
            window1ViewModel.RaisePropertyChanged("");
        }
    }
}