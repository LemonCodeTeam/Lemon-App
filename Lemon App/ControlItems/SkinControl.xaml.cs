using LemonLibrary;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lemon_App
{
    /// <summary>
    /// SkinControl.xaml 的交互逻辑
    /// </summary>
    public partial class SkinControl : UserControl
    {
        public string imgurl;
        public Color theme;
        public string txtColor;
        public SkinControl(string imgurl,string name,Color themecolor)
        {
            InitializeComponent();
            this.imgurl = imgurl;
            theme = themecolor;
            if (imgurl == "-2")
                img.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D2D30"));
            else if (imgurl == "-3")
                img.Background = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
            else if (imgurl != "-1")
                img.Background = new ImageBrush(new BitmapImage(new Uri(Settings.USettings.CachePath + "Skin\\" + imgurl + ".jpg", UriKind.Absolute)));
            this.name.Text = name;
        }
        public SkinControl(string name,Brush image,Color themecolor) {
            InitializeComponent();
            this.name.Text = name;
            img.Background = image;
            theme = themecolor;
        }
    }
}
