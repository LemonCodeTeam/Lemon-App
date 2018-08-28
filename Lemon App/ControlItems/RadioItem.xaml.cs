using LemonLibrary;
using System;
using System.IO;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lemon_App
{
    /// <summary>
    /// RadioItem.xaml 的交互逻辑
    /// </summary>
    public partial class RadioItem : UserControl
    {
        public string id { get; set; }
        public string Nam { get; set; }
        public string img { get; set; }
        public RadioItem(string ID, string name, string pic)
        {
            InitializeComponent();
            id = ID;
            Nam = name;
            img = pic;
            this.name.Text = Nam;
            if (!File.Exists(InfoHelper.GetPath() + "Cache/Radio" + id + ".jpg"))
            {
                WebClient v = new WebClient();
                v.DownloadFileAsync(new Uri(img), InfoHelper.GetPath() + "Cache/Radio" + id + ".jpg");
                v.DownloadFileCompleted += delegate
                {
                    v.Dispose();
                    var image = new System.Drawing.Bitmap(InfoHelper.GetPath() + "Cache/Radio" + id + ".jpg");
                    im.Background = new ImageBrush(image.ToImageSource());
                };
            }
            else {
                var image = new System.Drawing.Bitmap(InfoHelper.GetPath() + "Cache/Radio" + id + ".jpg");
                im.Background = new ImageBrush(image.ToImageSource());
            }
        }
        public RadioItem(string ID) {
            id = ID;
        }
    }
}
