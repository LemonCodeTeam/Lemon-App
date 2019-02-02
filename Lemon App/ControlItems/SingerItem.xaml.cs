using LemonLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace Lemon_App
{
    /// <summary>
    /// SingerItem.xaml 的交互逻辑
    /// </summary>
    public partial class SingerItem : UserControl
    {
        public string img { get; set; }
        public string singer { get; set; }
        public SingerItem(string ig, string sing)
        {
            InitializeComponent();
            img = ig;
            singer = sing;
            name.Text = singer;
            Loaded += async delegate {
                try
                {
                    im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(img));
                }
                catch
                {
                    im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl("https://y.gtimg.cn/mediastyle/global/img/singer_300.png?max_age=31536000"));
                }
            };
        }
    }
}
