﻿using BingoWallpaper.Models;
using BingoWallpaper.Services;
using Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;

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
        }

        private async void GetBitmap(string source)
        {
            await DefaultImageLoader.Instance.GetBitmapAsync(source);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var uiElement = sender as DependencyObject;
            while (true)
            {
                if (uiElement == null)
                {
                    break;
                }
                if (uiElement is ImageEx)
                {
                    break;
                }
                uiElement = VisualTreeHelper.GetParent(uiElement);
            }
            ImageEx image = uiElement as ImageEx;
            if (image != null)
            {
                image.Source =
                    "https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/bd_logo1_31bdc765.png";
            }
            else
            {
                MessageBox.Show("null");
            }
        }

        private void ImageExBehavior_OnImageFailed(object sender, ImageFailedEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
        }
    }
}