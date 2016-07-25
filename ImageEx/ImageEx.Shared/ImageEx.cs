using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Controls.Extensions;
using Weakly;

#if WINDOWS_UWP

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

#else

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

#endif

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

        private const string CacheFolderName = "ImageExCache";

        private const string ImageTemplateName = "PART_Image";

        private const string PlaceholderContentControlTemplateName = "PART_PlaceholderContentControl";

        private static readonly WeakValueDictionary<string, BitmapImage> CacheBitmapImages = new WeakValueDictionary<string, BitmapImage>();

        private Image _image;

        private ContentControl _placeholderContentControl;

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

        public static long CalculateCacheSize()
        {
            return (from cacheFileName in Directory.EnumerateFiles(CacheFolderPath)
                    select new FileInfo(cacheFileName).Length).Sum();
        }

        public static bool ContainsCache(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var uri = GetUriSource(source);
            var cacheFileName = GetCacheFileName(uri);
            return File.Exists(cacheFileName);
        }

        public static void DeleteAllCache()
        {
            Directory.Delete(CacheFolderPath, true);
        }

        public static bool DeleteCache(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var uri = GetUriSource(source);
            var cacheFileName = GetCacheFileName(uri);
            if (File.Exists(cacheFileName))
            {
                File.Delete(cacheFileName);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool IsHttpUri(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var scheme = uri.Scheme;
            return uri.IsAbsoluteUri && (string.Equals(scheme, "http", StringComparison.OrdinalIgnoreCase) || string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase));
        }

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (ImageEx)d;
            var value = (string)e.NewValue;

            obj.SetSource(value);
        }

        public static async Task<byte[]> GetBytesAsync(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (IsHttpUri(uri))
            {
                var cacheFileName = GetCacheFileName(uri);
                if (File.Exists(cacheFileName))
                {
                    return await FileExtensions.ReadAllBytesAsync(cacheFileName);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}