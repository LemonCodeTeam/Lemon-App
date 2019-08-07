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
using static LemonLibrary.InfoHelper;

namespace Lemon_App
{
    /// <summary>
    /// SingerItem.xaml 的交互逻辑
    /// </summary>
    public partial class SingerItem : UserControl
    {
        public MusicSinger data { get; set; }
        public SingerItem(MusicSinger dt)
        {
            InitializeComponent();
            data = dt;
            name.Text = dt.Name;
            Loaded += async delegate
            {
                try
                {
                    im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.Photo));
                }
                catch
                {
                    im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl("https://y.gtimg.cn/mediastyle/global/img/singer_300.png?max_age=31536000"));
                }
            };
        }
        public MusicSinger mData { get => data; set {
                data = value;
                new Action(async ()=> {
                    try{
                        im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(data.Photo));
                    }
                    catch{
                        im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl("https://y.gtimg.cn/mediastyle/global/img/singer_300.png?max_age=31536000"));
                    }
                })();
            } }
        public SingerItem()
        {
            InitializeComponent();
        }
    }
}
