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
            Loaded += async delegate {
                Height = ActualWidth + 40;
                im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(img));
            };
        }
        public RadioItem(string ID) {
            id = ID;
        }

        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            Height = Height = ActualWidth + 40;
        }
    }
}
