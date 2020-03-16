using LemonLib;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// PlControl.xaml 的交互逻辑
    /// </summary>
    public partial class PlControl : UserControl
    {
        public MusicPL data;
        public bool couldpraise = true;
        public PlControl(MusicPL dt)
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
            data = dt;
            Image.Background = new ImageBrush(new BitmapImage(new Uri(dt.img)));
            name.Text = dt.name;
            Te.Text = dt.text;
            time.Text = dt.time;
            ztb.Text = dt.like;
            if (dt.ispraise) zp.SetResourceReference(Shape.FillProperty, "ThemeColor");
            Loaded += delegate { Height = a.ActualHeight + 35; };
        }

        private async void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (couldpraise)
                if (data.ispraise)
                {
                    zp.SetResourceReference(Shape.FillProperty, "PlayDLPage_Font_Low");
                    ztb.Text = (int.Parse(data.like) - 1).ToString();
                    data.like = ztb.Text;
                    Console.WriteLine(await MusicLib.PraiseMusicPLAsync(Settings.USettings.Playing.MusicID, data));
                }
                else
                {
                    zp.SetResourceReference(Shape.FillProperty, "ThemeColor");
                    ztb.Text = (int.Parse(data.like) + 1).ToString();
                    data.like = ztb.Text;
                    Console.WriteLine(await MusicLib.PraiseMusicPLAsync(Settings.USettings.Playing.MusicID, data));
                }
        }

        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            Height = cons.ActualHeight + 72;
        }
    }
}
