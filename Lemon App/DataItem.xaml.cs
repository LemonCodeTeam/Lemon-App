using LemonLibrary;
using System;
using System.IO;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            try
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
                    v.DownloadFileCompleted += delegate
                    {
                        im.Background = new ImageBrush(new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "Cache/Data" + id + ".jpg", UriKind.Relative)));
                        var dt = new System.Drawing.Bitmap(AppDomain.CurrentDomain.BaseDirectory + "Cache/Data" + id + ".jpg").GetMajorColor();
                        var color = Color.FromArgb(dt.A, dt.R, dt.G, dt.B);
                        back.Background = new SolidColorBrush(color);
                    };
                }
                else
                {
                    im.Background = new ImageBrush(new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "Cache/Data" + id + ".jpg", UriKind.Relative)));
                    var dt = new System.Drawing.Bitmap(AppDomain.CurrentDomain.BaseDirectory + "Cache/Data" + id + ".jpg").GetMajorColor();
                    var color = Color.FromArgb(dt.A, dt.R, dt.G, dt.B);
                    back.Background = new SolidColorBrush(color);
                }
                name.Text = SongName;
                ser.Text = Singer;
            }
            catch { }
        }
    }
}
