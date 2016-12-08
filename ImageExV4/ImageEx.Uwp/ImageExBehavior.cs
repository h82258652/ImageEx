using Microsoft.Xaml.Interactivity;
using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Controls
{
    public class ImageExBehavior : Behavior<ImageBrush>
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(string), typeof(ImageExBehavior), new PropertyMetadata(default(string), SourceChanged));

        private IImageLoader _loader;

        public event ImageFailedEventHandler ImageFailed;

        public event EventHandler ImageOpened;

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

        protected virtual IImageLoader Loader
        {
            get
            {
                _loader = _loader ?? DefaultImageLoader.Instance;
                return _loader;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            SetSource(Source);
        }

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (ImageExBehavior)d;
            var value = (string)e.NewValue;

            obj.SetSource(value);
        }

        private async void SetSource(string source)
        {
            var imageBrush = AssociatedObject;
            if (imageBrush != null)
            {
                // 设计模式下直接显示。
                if (DesignMode.DesignModeEnabled)
                {
                    imageBrush.ImageSource = source == null ? null : new BitmapImage(new Uri(source, UriKind.RelativeOrAbsolute));
                }
                else
                {
                    if (source == null)
                    {
                        imageBrush.ImageSource = null;
                    }
                    else
                    {
                        var result = await Loader.GetBitmapAsync(source);
                        if (source == Source)// 确保在执行异步操作过程中，Source 没有变动。
                        {
                            switch (result.Status)
                            {
                                case BitmapStatus.Opened:
                                    imageBrush.ImageSource = result.Value;
                                    ImageOpened?.Invoke(this, EventArgs.Empty);
                                    break;

                                case BitmapStatus.Failed:
                                    imageBrush.ImageSource = null;
                                    ImageFailed?.Invoke(this, new ImageFailedEventArgs(source, result.FailedException));
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