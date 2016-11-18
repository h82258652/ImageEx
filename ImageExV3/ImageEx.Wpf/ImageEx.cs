using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Controls
{
    [TemplatePart(Name = ImageTemplateName, Type = typeof(Image))]
    [TemplatePart(Name = FailedContentControlTemplateName, Type = typeof(ContentControl))]
    [TemplatePart(Name = LoadingContentControlTemplateName, Type = typeof(ContentControl))]
    [TemplateVisualState(GroupName = "ImageStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "ImageStates", Name = "Opened")]
    [TemplateVisualState(GroupName = "ImageStates", Name = "Failed")]
    [TemplateVisualState(GroupName = "ImageStates", Name = "Loading")]
    public class ImageEx : Control
    {
        public static readonly DependencyProperty FailedTemplateProperty = DependencyProperty.Register(nameof(FailedTemplate), typeof(DataTemplate), typeof(ImageEx), new PropertyMetadata(default(DataTemplate)));
        public static readonly DependencyProperty FailedTemplateSelectorProperty = DependencyProperty.Register(nameof(FailedTemplateSelector), typeof(DataTemplateSelector), typeof(ImageEx), new PropertyMetadata(default(DataTemplateSelector)));
        public static readonly DependencyProperty LoadingTemplateProperty = DependencyProperty.Register(nameof(LoadingTemplate), typeof(DataTemplate), typeof(ImageEx), new PropertyMetadata(default(DataTemplate)));
        public static readonly DependencyProperty LoadingTemplateSelectorProperty = DependencyProperty.Register(nameof(LoadingTemplateSelector), typeof(DataTemplateSelector), typeof(ImageEx), new PropertyMetadata(default(DataTemplateSelector)));
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(string), typeof(ImageEx), new PropertyMetadata(default(string), SourceChanged));

        public static readonly DependencyProperty StretchDirectionProperty = DependencyProperty.Register(nameof(StretchDirection), typeof(StretchDirection), typeof(ImageEx), new PropertyMetadata(StretchDirection.Both));

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(ImageEx), new PropertyMetadata(Stretch.Uniform));

        private const string FailedContentControlTemplateName = "PART_FailedContentControl";

        private const string ImageTemplateName = "PART_Image";

        private const string LoadingContentControlTemplateName = "PART_LoadingContentControl";

        private Image _image;

        private IImageLoader _loader;

        static ImageEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageEx), new FrameworkPropertyMetadata(typeof(ImageEx)));
        }

        public event EventHandler<ImageFailedEventArgs> ImageFailed;

        public event EventHandler ImageOpened;

        public DataTemplate FailedTemplate
        {
            get
            {
                return (DataTemplate)GetValue(FailedTemplateProperty);
            }
            set
            {
                SetValue(FailedTemplateProperty, value);
            }
        }

        public DataTemplateSelector FailedTemplateSelector
        {
            get
            {
                return (DataTemplateSelector)GetValue(FailedTemplateSelectorProperty);
            }
            set
            {
                SetValue(FailedTemplateSelectorProperty, value);
            }
        }

        public virtual IImageLoader Loader
        {
            get
            {
                _loader = _loader ?? DefaultImageLoader.Instance;
                return _loader;
            }
        }

        public DataTemplate LoadingTemplate
        {
            get
            {
                return (DataTemplate)GetValue(LoadingTemplateProperty);
            }
            set
            {
                SetValue(LoadingTemplateProperty, value);
            }
        }

        public DataTemplateSelector LoadingTemplateSelector
        {
            get
            {
                return (DataTemplateSelector)GetValue(LoadingTemplateSelectorProperty);
            }
            set
            {
                SetValue(LoadingTemplateSelectorProperty, value);
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
            if (_image != null)
            {
                // 设计模式下直接显示。
                if ((bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)
                {
                    _image.Source = source == null ? null : new BitmapImage(new Uri(source, UriKind.RelativeOrAbsolute));
                }
                else
                {
                    if (source == null)
                    {
                        _image.Source = null;
                        VisualStateManager.GoToState(this, "Normal", true);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, "Loading", true);
                        var result = await Loader.GetBitmapAsync(source);
                        if (source == Source)// 确保在执行异步操作过程中，Source 没有变动。
                        {
                            switch (result.Status)
                            {
                                case BitmapStatus.Opened:
                                    _image.Source = result.Value;
                                    VisualStateManager.GoToState(this, "Opened", true);
                                    ImageOpened?.Invoke(this, EventArgs.Empty);
                                    break;

                                case BitmapStatus.Failed:
                                    _image.Source = result.Value;
                                    VisualStateManager.GoToState(this, "Failed", true);
                                    ImageFailed?.Invoke(this, new ImageFailedEventArgs(source, result.ErrorException));
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException(nameof(result.Status));
                            }
                        }
                    }
                }
            }
        }
    }
}