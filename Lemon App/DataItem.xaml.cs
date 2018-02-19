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
    /// DataItem.xaml 的交互逻辑
    /// </summary>
    public partial class DataItem : UserControl
    {
        public string ID { set; get; }
        public string SongName { set; get; }
        public string Singer { set; get; }
        public string Image { set; get; }
        public DataItem(string id, string songname, string singer, string img)
        {
            InitializeComponent();
            ID = id;
            SongName = songname;
            Singer = singer;
            Image = img;
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Cache/Data" + id + ".jpg"))
            {
                WebClient v = new WebClient();
                v.DownloadFileAsync(new Uri(img), AppDomain.CurrentDomain.BaseDirectory + "Cache/Data" + id + ".jpg");
                v.DownloadFileCompleted += delegate { im.Background = new ImageBrush(new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "Cache/Data" + id + ".jpg", UriKind.Relative))); };
            }
            else im.Background = new ImageBrush(new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "Cache/Data" + id + ".jpg", UriKind.Relative)));
            name.Text = SongName;
            ser.Text = Singer;
        }
    }
}
