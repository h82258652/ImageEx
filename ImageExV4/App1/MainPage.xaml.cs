using BingoWallpaper.Models;
using BingoWallpaper.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace App1
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //var bitmap = new WriteableBitmap(1, 1);
            //using (var httpClient = new HttpClient())
            //{
            //    var bytes = await httpClient.GetByteArrayAsync("https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/bd_logo1_31bdc765.png");
            //    BitmapImage bitmap2 = new BitmapImage();
            //    bytes = new byte[2] { 2, 5 };
            //    await bitmap2.SetSourceAsync(new MemoryStream(bytes).AsRandomAccessStream());
            //    await bitmap.SetSourceAsync(new MemoryStream(bytes).AsRandomAccessStream());
            //    Image.Source = bitmap;
            //}

            var collection = new ObservableCollection<string>();
            var leanCloudWallpaperService = new LeanCloudWallpaperService();
            var result = await leanCloudWallpaperService.GetWallpapersAsync(2016, 10, "zh-CN");
            var size = new WallpaperSize(1920, 1080);
            foreach (var wallpaper in result)
            {
                var url = leanCloudWallpaperService.GetUrl(wallpaper.Image, size);
                collection.Add(url);
            }
            GridView.ItemsSource = collection;
        }
    }
}