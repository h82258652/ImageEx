using Windows.Media.Casting;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Controls
{
    [TemplatePart(Name = ImageTemplateName, Type = typeof(Image))]
    public class ImageEx : Control
    {
        public static readonly DependencyProperty FailedTemplateProperty = DependencyProperty.Register(nameof(FailedTemplate), typeof(DataTemplate), typeof(ImageEx), new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyProperty FailedTemplateSelectorProperty = DependencyProperty.Register(nameof(FailedTemplateSelector), typeof(DataTemplateSelector), typeof(ImageEx), new PropertyMetadata(default(DataTemplateSelector)));

        public static readonly DependencyProperty LoadingTemplateProperty = DependencyProperty.Register(nameof(LoadingTemplate), typeof(DataTemplate), typeof(ImageEx), new PropertyMetadata(default(DataTemplate)));

        public static readonly DependencyProperty LoadingTemplateSelectorProperty = DependencyProperty.Register(nameof(LoadingTemplateSelector), typeof(DataTemplateSelector), typeof(ImageEx), new PropertyMetadata(default(DataTemplateSelector)));

        public static readonly DependencyProperty NineGridProperty = DependencyProperty.Register(nameof(NineGrid), typeof(Thickness), typeof(ImageEx), new PropertyMetadata(default(Thickness)));

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(string), typeof(ImageEx), new PropertyMetadata(default(string), SourceChanged));

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(ImageEx), new PropertyMetadata(Stretch.Uniform));

        private const string FailedContentControlTemplateName = "PART_FailedContentControl";

        private const string ImageTemplateName = "PART_Image";

        private const string LoadingContentControlTemplateName = "PART_LoadingContentControl";

        private Image _image;

        public ImageEx()
        {
            DefaultStyleKey = typeof(ImageEx);
        }

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
        }

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}