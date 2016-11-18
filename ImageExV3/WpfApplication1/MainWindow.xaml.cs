using Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ListView.ItemsSource = new string[]
            {
                "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20161107162444.jpg",
     "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20161107135054.jpg",
     "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20161103180635.jpg",
   "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20161027175413.jpg",
     "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923145229.jpg",
     "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923160309.jpg",
     "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923161102.jpg",
     "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923153921.jpg",
    "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923180626.jpg",
    "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923165057.jpg",
     "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923153336.jpg",
     "http://gamergift.oss-cn-hangzhou.aliyuncs.com/gameinfo/20160923144406.jpg",
            };
        }
    }
}