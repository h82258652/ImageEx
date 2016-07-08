using Controls.Core.Extensions;
using Controls.Extensions;
using Controls.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private static readonly string CacheFolderPath = Path.Combine(Path.GetTempPath(), CacheFolderName);

        private static readonly Dictionary<Uri, Task<byte[]>> ImageDownloadTasks = new Dictionary<Uri, Task<byte[]>>();

        static ImageEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageEx), new FrameworkPropertyMetadata(typeof(ImageEx)));
        }

        public event EventHandler<HttpDownloadProgressEventArgs> DownloadProgressChanged;

        public event EventHandler<ExceptionEventArgs> ImageFailed;

        public event EventHandler ImageOpened;

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
            return Path.Combine(CacheFolderPath, cacheFileName);
        }

        private async Task<BitmapImage> DownloadHttpSourceAsync(Uri uri, string cacheFileName)
        {
            _image.Visibility = Visibility.Collapsed;
            _placeholderContentControl.Visibility = Visibility.Visible;

            Task<byte[]> task;
            if (ImageDownloadTasks.TryGetValue(uri, out task) == false)
            {
                task = DownloadImageAsync(uri);
                ImageDownloadTasks[uri] = task;
            }
            var bytes = await task;
            ImageDownloadTasks.Remove(uri);

            if (bytes == null)
            {
                return null;
            }

            BitmapImage bitmap;
            try
            {
                bitmap = new BitmapImage();
                bitmap.DecodeFailed += (sender, e) =>
                {
                    ImageFailed?.Invoke(this, new ExceptionEventArgs(e.ErrorException));
                };
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(bytes);
                bitmap.EndInit();
            }
            catch (NotSupportedException ex)
            {
                bitmap = null;
                ImageFailed?.Invoke(this, new ExceptionEventArgs(ex));
            }

            if (bitmap != null)
            {
                SaveHttpSourceToCacheFolderAsync(cacheFileName, bytes);
            }

            return bitmap;
        }

        private async Task<byte[]> DownloadImageAsync(Uri uri)
        {
            using (var client = new HttpClient())
            {
                byte[] bytes;
                try
                {
                    bytes = await client.GetByteArrayAsync(uri, new Progress<HttpProgress>(progress =>
                    {
                        DownloadProgressChanged?.Invoke(this, new HttpDownloadProgressEventArgs(progress));
                    }));
                }
                catch (HttpRequestException ex)
                {
                    bytes = null;
                    ImageFailed?.Invoke(this, new ExceptionEventArgs(ex));
                }
                return bytes;
            }
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
            BitmapImage bitmap;
            try
            {
                bitmap = new BitmapImage();
                bitmap.DecodeFailed += (sender, e) =>
                {
                    ImageFailed?.Invoke(this, new ExceptionEventArgs(e.ErrorException));
                };
                bitmap.BeginInit();
                bitmap.UriSource = uri;
                bitmap.EndInit();
            }
            catch (FileNotFoundException ex)
            {
                bitmap = null;
                ImageFailed?.Invoke(this, new ExceptionEventArgs(ex));
            }
            catch (NotSupportedException ex)
            {
                bitmap = null;
                ImageFailed?.Invoke(this, new ExceptionEventArgs(ex));
            }
            return bitmap;
        }

        private async void SaveHttpSourceToCacheFolderAsync(string cacheFileName, byte[] bytes)
        {
            Directory.CreateDirectory(CacheFolderPath);
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
                        _image.Source = new BitmapImage(new Uri(source));
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
                            }
                            else
                            {
                                if (uri.IsAbsoluteUri == false)
                                {
                                    Uri.TryCreate("pack://application:,,,/" + (source.StartsWith("/") ? source.Substring(1) : source), UriKind.Absolute, out uri);
                                }
                                bitmap = GetLocalSource(uri);
                            }
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }

                        if (bitmap != null)
                        {
                            // 成功加载图片，放入缓存。
                            CacheBitmapImages[source] = bitmap;
                        }
                    }

                    if (source == Source)
                    {
                        if (bitmap != null)
                        {
                            ImageOpened?.Invoke(this, EventArgs.Empty);
                        }

                        _image.Visibility = Visibility.Visible;
                        _placeholderContentControl.Visibility = Visibility.Collapsed;
                        _image.Source = bitmap;
                    }
                }
            }
        }
    }
}