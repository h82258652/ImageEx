using Controls.Extensions;
using Controls.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Media.Casting;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace Controls
{
    public partial class ImageEx : Control
    {
        public static readonly DependencyProperty NineGridProperty = DependencyProperty.Register(nameof(NineGrid), typeof(Thickness), typeof(ImageEx), new PropertyMetadata(default(Thickness)));

        private static readonly string CacheFolderPath = Path.Combine(ApplicationData.Current.LocalCacheFolder.Path, CacheFolderName);

        private static readonly Dictionary<Uri, Task<byte[]>> ImageDownloadTasks = new Dictionary<Uri, Task<byte[]>>();

        public ImageEx()
        {
            DefaultStyleKey = typeof(ImageEx);
        }

        public event EventHandler<HttpDownloadProgressEventArgs> DownloadProgressChanged;

        public event EventHandler<ExceptionEventArgs> ImageFailed;

        public event EventHandler ImageOpened;

        public Thickness NineGrid
        {
            get
            {
                return (Thickness)GetValue(NineGridProperty);
            }
            set
            {
                SetValue(NineGridProperty, value);
            }
        }

        public CastingSource GetAsCastingSource()
        {
            return _image.GetAsCastingSource();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _image = (Image)GetTemplateChild(ImageTemplateName);
            _placeholderContentControl = (ContentControl)GetTemplateChild(PlaceholderContentControlTemplateName);
            SetSource(Source);
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

            var bitmap = new BitmapImage();
            bitmap.ImageFailed += (sender, e) =>
            {
                ImageFailed?.Invoke(this, new ExceptionEventArgs(e.ErrorMessage));
            };
            bitmap.ImageOpened += (sender, e) =>
            {
                SaveHttpSourceToCacheFolderAsync(cacheFileName, bytes);
            };
            await bitmap.SetSourceAsync(new MemoryStream(bytes).AsRandomAccessStream());

            return bitmap;
        }

        private async Task<byte[]> DownloadImageAsync(Uri uri)
        {
            using (var client = new HttpClient())
            {
                byte[] bytes;
                try
                {
                    var task = client.GetBufferAsync(uri);
                    task.Progress = (info, progressInfo) =>
                    {
                        DownloadProgressChanged?.Invoke(this, new HttpDownloadProgressEventArgs(progressInfo));
                    };
                    var buffer = await task;
                    bytes = buffer.ToArray();
                }
                catch (Exception ex)
                {
                    bytes = null;
                    ImageFailed?.Invoke(this, new ExceptionEventArgs(ex.Message));
                }
                return bytes;
            }
        }

        private string GetCacheFileName(Uri uri)
        {
            var originalString = uri.OriginalString;
            var extension = Path.GetExtension(originalString);
            var cacheFileName = HashHelper.GenerateMd5Hash(originalString) + extension;
            return Path.Combine(CacheFolderPath, cacheFileName);
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
            var bitmap = new BitmapImage();
            bitmap.ImageFailed += (sender, e) =>
            {
                ImageFailed?.Invoke(this, new ExceptionEventArgs(e.ErrorMessage));
            };
            bitmap.UriSource = uri;
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
                if (DesignMode.DesignModeEnabled)
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
                            }
                            else
                            {
                                if (uri.IsAbsoluteUri == false)
                                {
                                    Uri.TryCreate("ms-appx:///" + (source.StartsWith("/") ? source.Substring(1) : source), UriKind.Absolute, out uri);
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