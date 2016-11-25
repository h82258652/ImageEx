using BingoWallpaper.Models;
using BingoWallpaper.Services;
using Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var collection = new ObservableCollection<string>();
            var leanCloudWallpaperService = new LeanCloudWallpaperService();
            var result = await leanCloudWallpaperService.GetWallpapersAsync(2016, 10, "zh-CN");
            var size = new WallpaperSize(800, 480);
            foreach (var wallpaper in result)
            {
                var url = leanCloudWallpaperService.GetUrl(wallpaper.Image, size);
                GetBitmap(url);
            }
        }

        private async void GetBitmap(string source)
        {
            await DefaultImageLoader.Instance.GetBitmapAsync(source);
        }
    }
}