using System;
using Windows.ApplicationModel;
using Windows.Media.Casting;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Controls
{
    [TemplatePart(Name = ImageTemplateName, Type = typeof(Image))]
    [TemplatePart(Name = PlaceholderContentControlTemplateName, Type = typeof(ContentControl))]
    public class ImageEx : Control
    {
        public static readonly DependencyProperty NineGridProperty = DependencyProperty.Register(nameof(NineGrid), typeof(Thickness), typeof(ImageEx), new PropertyMetadata(default(Thickness)));

        public static readonly DependencyProperty PlaceholderTemplateProperty = DependencyProperty.Register(nameof(PlaceholderTemplate), typeof(DataTemplate), typeof(ImageEx), new PropertyMetadata(null));

        public static readonly DependencyProperty PlaceholderTemplateSelectorProperty = DependencyProperty.Register(nameof(PlaceholderTemplateSelector), typeof(DataTemplateSelector), typeof(ImageEx), new PropertyMetadata(null));

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(string), typeof(ImageEx), new PropertyMetadata(null, SourceChanged));

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(ImageEx), new PropertyMetadata(Stretch.Uniform));

        private const string ImageTemplateName = "PART_Image";

        private const string PlaceholderContentControlTemplateName = "PART_PlaceholderContentControl";

        private Image _image;

        private IImageLoader _loader;

        private ContentControl _placeholderContentControl;

        public ImageEx()
        {
            DefaultStyleKey = typeof(ImageEx);
        }

        public event EventHandler<HttpDownloadProgressEventArgs> DownloadProgressChanged;

        public event EventHandler<ImageFailedEventArgs> ImageFailed;

        public virtual IImageLoader Loader
        {
            get
            {
                if (_loader == null)
                {
                    _loader = DefaultImageLoader.Instance;
                    _loader.DownloadProgressChanged += async (sender, e) =>
                    {
                        if (DownloadProgressChanged != null)
                        {
                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                if (e.Url == Source)
                                {
                                    DownloadProgressChanged?.Invoke(this, e);
                                }
                            });
                        }
                    };
                    _loader.ImageFailed += (sender, e) =>
                    {
                        if (e.Url == Source)
                        {
                            ImageFailed?.Invoke(this, e);
                        }
                    };
                }
                return _loader;
            }
        }

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

        public DataTemplate PlaceholderTemplate
        {
            get
            {
                return (DataTemplate)GetValue(PlaceholderTemplateProperty);
            }
            set
            {
                SetValue(PlaceholderTemplateProperty, value);
            }
        }

        public DataTemplateSelector PlaceholderTemplateSelector
        {
            get
            {
                return (DataTemplateSelector)GetValue(PlaceholderTemplateSelectorProperty);
            }
            set
            {
                SetValue(PlaceholderTemplateSelectorProperty, value);
            }
        }

        public string Source
        {
            get
            {
                return (string)GetValue(SourceProperty);
            }
            set
            {
                SetValue(SourceProperty, value);
            }
        }

        public Stretch Stretch
        {
            get
            {
                return (Stretch)GetValue(StretchProperty);
            }
            set
            {
                SetValue(StretchProperty, value);
            }
        }

        public CastingSource GetAsCastingSource()
        {
            ApplyTemplate();
            return _image?.GetAsCastingSource();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _image = (Image)GetTemplateChild(ImageTemplateName);
            _placeholderContentControl = (ContentControl)GetTemplateChild(PlaceholderContentControlTemplateName);
            SetSource(Source);
        }

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (ImageEx)d;
            var value = (string)e.NewValue;

            obj.SetSource(value);
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
                        _image.Source = new BitmapImage(new Uri(source));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                else
                {
                    if (source == null)
                    {
                        _image.Source = null;
                    }
                    else
                    {
                        _image.Visibility = Visibility.Collapsed;
                        _placeholderContentControl.Visibility = Visibility.Visible;
                        var bitmap = await Loader.GetBitmapAsync(source);
                        if (source == Source)// 确保在执行异步操作过程中，Source 没有变动。
                        {
                            _image.Visibility = Visibility.Visible;
                            _placeholderContentControl.Visibility = Visibility.Collapsed;
                            _image.Source = bitmap;
                        }
                    }
                }
            }
        }
    }
}