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
        public int imgurl;
        public Color theme;
        public string txtColor;
        public SkinControl(int imgurl,string name,Color themecolor)
        {
            InitializeComponent();
            this.imgurl = imgurl;
            theme = themecolor;
            if(imgurl!=-1)
               img.Background = new ImageBrush(new BitmapImage(new Uri(InfoHelper.GetPath() + "Skin\\" + + imgurl + ".jpg",UriKind.Absolute)));
            this.name.Text = name;
        }
    }
}
