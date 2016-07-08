using Controls.Extensions;
using Controls.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Controls
{
    public partial class ImageEx : Control
    {
        public static readonly DependencyProperty StretchDirectionProperty = DependencyProperty.Register(nameof(StretchDirection), typeof(StretchDirection), typeof(ImageEx), new PropertyMetadata(StretchDirection.Both));

        private static readonly Dictionary<string, Task<byte[]>> ImageDownloadTasks = new Dictionary<string, Task<byte[]>>();

        static ImageEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageEx), new FrameworkPropertyMetadata(typeof(ImageEx)));
        }

        public event EventHandler ImageFailed;

        public StretchDirection StretchDirection
        {
            get
            {
                return (StretchDirection)GetValue(StretchDirectionProperty);
            }
            set
            {
                SetValue(StretchDirectionProperty, value);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _image = (Image)GetTemplateChild(ImageTemplateName);
            _placeholderContentControl = (ContentControl)GetTemplateChild(PlaceholderContentControlTemplateName);
            SetSource(Source);
        }

        private static string GetCacheFileName(Uri uri)
        {
            var originalString = uri.OriginalString;
            var extension = Path.GetExtension(originalString);
            var cacheFileName = HashHelper.GenerateMd5Hash(originalString) + extension;
            // TODO
            return Path.Combine(Path.Combine(Path.GetTempPath(), "ImageExCache"), cacheFileName);
        }

        private async Task<BitmapImage> DownloadHttpSourceAsync(Uri uri, string cacheFileName)
        {
            Task<byte[]> task;
            if (ImageDownloadTasks.TryGetValue(cacheFileName, out task) == false)
            {
                task = DownloadImageAsync(uri);
                ImageDownloadTasks[cacheFileName] = task;
            }
            var bytes = await task;
            ImageDownloadTasks.Remove(cacheFileName);

            BitmapImage bitmap;
            try
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(bytes);
                bitmap.EndInit();
            }
            catch (NotSupportedException ex)
            {
                // TODO
                bitmap = null;
            }

            SaveHttpSourceToDiskCacheAsync(cacheFileName, bytes);

            return bitmap;
        }

        private async Task<byte[]> DownloadImageAsync(Uri uri)
        {
            byte[] bytes = null;
            using (var client = new HttpClient())
            {
                try
                {
                    // TODO
                    bytes = await client.GetByteArrayAsync(uri, new Progress<HttpProgress>(progress =>
                    {
                        // TODO
                    }));
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                    //TODO Failed
                }
            }
            return bytes;
        }

        private async Task<BitmapImage> GetHttpSourceAsync(Uri uri)
        {
            var cacheFileName = GetCacheFileName(uri);
            if (File.Exists(cacheFileName))
            {
                return GetLocalSource(new Uri(cacheFileName));
            }
            else
            {
                return await DownloadHttpSourceAsync(uri, cacheFileName);
            }
        }

        private BitmapImage GetLocalSource(Uri uri)
        {
            BitmapImage bitmap = null;
            try
            {
                bitmap = new BitmapImage();
                bitmap.DecodeFailed += (sender, e) =>
                {
                    //TODO Failed
                    ImageFailed?.Invoke(this, null);
                };
                bitmap.BeginInit();
                bitmap.UriSource = uri;
                bitmap.EndInit();
            }
            catch (NotSupportedException ex)
            {
                //TODO Failed
            }
            return bitmap;
        }

        private async void SaveHttpSourceToDiskCacheAsync(string cacheFileName, byte[] bytes)
        {
            await FileExtensions.WriteAllBytesAsync(cacheFileName, bytes);
        }

        private async void SetSource(string source)
        {
            if (_image != null && _placeholderContentControl != null)
            {
                // 设计模式下直接显示。
                if ((bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)
                {
                    try
                    {
                        _image.Source = new BitmapImage(new Uri(Source));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    return;
                }

                if (source == null)
                {
                    _image.Source = null;
                }
                else
                {
                    // 检查缓存。
                    BitmapImage bitmap;
                    if (CacheBitmapImages.TryGetValue(source, out bitmap))
                    {
                        // 缓存存在，直接使用缓存。
                        _image.Source = bitmap;
                    }
                    else
                    {
                        Uri uri;
                        if (Uri.TryCreate(source, UriKind.RelativeOrAbsolute, out uri))
                        {
                            if (IsHttpUri(uri))
                            {
                                bitmap = await GetHttpSourceAsync(uri);
                                if (bitmap != null)
                                {
                                    CacheBitmapImages[source] = bitmap;
                                }
                            }
                            else
                            {
                                if (uri.IsAbsoluteUri == false)
                                {
                                    Uri.TryCreate("pack://application:,,,/" + (source.StartsWith("/") ? source.Substring(1) : source), UriKind.Absolute, out uri);
                                }
                                bitmap = GetLocalSource(uri);
                                if (bitmap != null)
                                {
                                    CacheBitmapImages[source] = bitmap;
                                }
                            }
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }

                    if (source == Source)
                    {
                        if (bitmap != null)
                        {
                            // Opened.
                        }

                        _image.Source = bitmap;
                    }

                    // TODO
                }
            }
        }
    }
}