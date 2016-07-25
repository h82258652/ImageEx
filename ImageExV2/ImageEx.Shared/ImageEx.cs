using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Controls
{
    [TemplatePart(Name = ImageTemplateName, Type = typeof(Image))]
    [TemplatePart(Name = PlaceholderContentControlTemplateName, Type = typeof(ContentControl))]
    public partial class ImageEx
    {
        public static readonly DependencyProperty PlaceholderTemplateProperty = DependencyProperty.Register(nameof(PlaceholderTemplate), typeof(DataTemplate), typeof(ImageEx), new PropertyMetadata(null));

        public static readonly DependencyProperty PlaceholderTemplateSelectorProperty = DependencyProperty.Register(nameof(PlaceholderTemplateSelector), typeof(DataTemplateSelector), typeof(ImageEx), new PropertyMetadata(null));

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(string), typeof(ImageEx), new PropertyMetadata(null, SourceChanged));

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(ImageEx), new PropertyMetadata(Stretch.Uniform));

        private const string ImageTemplateName = "PART_Image";

        private const string PlaceholderContentControlTemplateName = "PART_PlaceholderContentControl";

        private IImageLoader _loader;

        public IImageLoader Loader
        {
            get
            {
                if (_loader == null)
                {
                    _loader = DefaultImageLoader.GetInstance();
                }
                return _loader;
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

        public void SetLoader(Func<IImageLoader> factory)
        {
            _loader = factory?.Invoke();
        }

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (ImageEx)d;
            var value = (string)e.NewValue;

            obj.SetSource(value);
        }

        private void SetSource(string source)
        {
            throw new NotImplementedException();
        }
    }
}