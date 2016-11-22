using BingoWallpaper.Models;
using BingoWallpaper.Services;
using Controls;
using System.Collections.ObjectModel;
using System.Linq;
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
            var size = new WallpaperSize(1920, 1080);
            foreach (var wallpaper in result)
            {
                collection.Add(leanCloudWallpaperService.GetUrl(wallpaper.Image, size));
                collection.Add(leanCloudWallpaperService.GetUrl(wallpaper.Image, size));
                collection.Add(leanCloudWallpaperService.GetUrl(wallpaper.Image, size));
                collection.Add(leanCloudWallpaperService.GetUrl(wallpaper.Image, size));
                collection.Add(leanCloudWallpaperService.GetUrl(wallpaper.Image, size));
                //Download(leanCloudWallpaperService.GetUrl(wallpaper.Image, size));
                //Download(leanCloudWallpaperService.GetUrl(wallpaper.Image, size));
                //Download(leanCloudWallpaperService.GetUrl(wallpaper.Image, size));
                //Download(leanCloudWallpaperService.GetUrl(wallpaper.Image, size));
                //Download(leanCloudWallpaperService.GetUrl(wallpaper.Image, size));
            }
            collection = new ObservableCollection<string>(collection.OrderBy(temp => temp.GetHashCode()));
            ListView.ItemsSource = collection;
        }

        private async void Download(string url)
        {
            await DefaultImageLoader.Instance.GetBytesAsync(url);
        }
    }
}