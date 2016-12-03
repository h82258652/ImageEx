using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    public class Window1ViewModel : ViewModelBase
    {
        public string Url
        {
            get
            {
                return "https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/bd_logo1_31bdc765.png";
            }
        }

        public Window1ViewModel()
        {
            _images = new ObservableCollection<string>();
            _images.Add("https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/bd_logo1_31bdc765.png");
            _images.Add("https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/bd_logo1_31bdc765.png");
            _images.Add("https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/bd_logo1_31bdc765.png");
            _images.Add("https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/bd_logo1_31bdc765.png");
            _images.Add("https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superman/img/logo/bd_logo1_31bdc765.png");
        }

        private ObservableCollection<string> _images;

        public ObservableCollection<string> Images
        {
            get
            {
                return _images;
            }
        }
    }
}