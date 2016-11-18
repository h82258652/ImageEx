using BingoWallpaper.Models;
using BingoWallpaper.Services;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> _collection;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            _collection = new List<string>()
            {
                "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20161107162444.jpg",
                "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20161107135054.jpg",
                "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20161103180635.jpg",
                "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20161027175413.jpg",
                "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923145229.jpg",
                "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923160309.jpg",
                "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923161102.jpg",
                "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923153921.jpg",
                "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923180626.jpg",
                "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923165057.jpg",
                "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923153336.jpg",
                "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923144406.jpg",
            };
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var leanCloudWallpaperService = new LeanCloudWallpaperService();
            var result = await leanCloudWallpaperService.GetWallpapersAsync(2016, 10, "zh-CN");
            var size = new WallpaperSize(1920, 1080);
            foreach (var wallpaper in result)
            {
                _collection.Add(leanCloudWallpaperService.GetUrl(wallpaper.Image, size));
            }
            ListView.ItemsSource = _collection;
        }
    }
}