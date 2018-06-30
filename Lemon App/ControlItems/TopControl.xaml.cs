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
    /// TopControl.xaml 的交互逻辑
    /// </summary>
    public partial class TopControl : UserControl
    {
        public string topID { set; get; }
        public string pic { set; get; }
        public string name { set; get; }
        public TopControl(string id, string img, string n)
        {
            InitializeComponent();
            if (!File.Exists(InfoHelper.GetPath() + "Cache/Top" + id + ".jpg"))
            {
                WebClient v = new WebClient();
                v.DownloadFileAsync(new Uri(img), InfoHelper.GetPath() + "Cache/Top" + id + ".jpg");
                v.DownloadFileCompleted += delegate { v.Dispose(); im.Background = new ImageBrush(new BitmapImage(new Uri(InfoHelper.GetPath() + "Cache/Top" + id + ".jpg", UriKind.Relative))); };
            }else im.Background = new ImageBrush(new BitmapImage(new Uri(InfoHelper.GetPath() + "Cache/Top" + id + ".jpg", UriKind.Relative)));
            topID = id;
            pic = img;
            name = n;
        }
    }
}