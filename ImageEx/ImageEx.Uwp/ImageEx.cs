using System;
using Windows.ApplicationModel;
using Windows.Media.Casting;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Controls
{
    public partial class ImageEx : Control
    {
        public static readonly DependencyProperty NineGridProperty = DependencyProperty.Register(nameof(NineGrid), typeof(Thickness), typeof(ImageEx), new PropertyMetadata(default(Thickness)));

        public ImageEx()
        {
            DefaultStyleKey = typeof(ImageEx);
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

        private void SetHttpSource(Uri uri)
        {
            throw new NotImplementedException();
        }

        private void SetLocalSource(Uri uri)
        {
            throw new NotImplementedException();
        }

        private void SetSource(string source)
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
                                SetHttpSource(uri);
                            }
                            else
                            {
                                if (uri.IsAbsoluteUri == false)
                                {
                                    Uri.TryCreate("ms-appx:///" + (source.StartsWith("/") ? source.Substring(1) : source), UriKind.Absolute, out uri);
                                }
                                SetLocalSource(uri);
                            }
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
            }
        }
    }
}