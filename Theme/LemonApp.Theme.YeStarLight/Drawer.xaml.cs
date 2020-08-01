using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

/*
           Lemon App 通用主题模板 
     Twilight./Lemon QQ:2728578956

   需要用到的几个部分:
       1.Background:在此Page上自由绘制   或 
          在 GetBrush方法内以Brush类型返回
       2.ThemeColor 主题颜色
       3.Font 字体颜色 White or Black
   可以随意更改的部分:
       1.命名空间 namespace
       2.作者,版权,程序集信息
    不可更改的:
       1.此类名
       2.基本方法 GetPage GetThemeColor GetFont
     */
namespace LemonApp.Theme.YeStarLight
{
    /// <summary>
    /// Drawe.xaml 的交互逻辑
    /// </summary>
    public partial class Drawer : ThemeBase
    {
        public Drawer(bool needDraw=true)
        {
            ThemeName = "昨日星空";
            ThemeColor = Color.FromArgb(255,111,152,254);
            FontColor = "White";
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
            cat.Source = Properties.Resources.cat.ToBitmapImage();
            moon.Source = Properties.Resources.moon.ToBitmapImage();
            page.Background = new ImageBrush(Properties.Resources.staticImage.ToBitmapImage());
            (Resources["Cat"] as Storyboard).Begin();
            if (needDraw)Draw();
        }
        public override ThemeBase GetPage()
        {
            return new Drawer();
        }
        public static new string NameSpace = "LemonApp.Theme.YeStarLight.Drawer";
        public override async void Draw()
        {
            canvas.Children.Clear();
            await Task.Delay(1000);
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    //Add Stars;
                    await Task.Delay(i * 1000);
                    Random rd = new Random();
                    int size = rd.Next(30, 50);
                    star s = new star();
                    s.Width = size;
                    s.Height = size;
                    Canvas.SetLeft(s, rd.Next(200, (int)canvas.ActualWidth - 200));
                    Canvas.SetTop(s, rd.Next(20, (int)canvas.ActualHeight - 150));
                    var sb = Resources["StarLight"] as Storyboard;
                    var ani = sb.Children[0] as DoubleAnimationUsingKeyFrames;
                    Storyboard.SetTarget(ani, s);
                    sb.Begin();
                    //Timeline.SetDesiredFrameRate(d, 35);
                    canvas.Children.Add(s);

                    //Add rainstar
                    rainstar rs = new rainstar();
                    Canvas.SetLeft(rs, rd.Next(200, (int)canvas.ActualWidth - 200));
                    Canvas.SetTop(rs, rd.Next(20, (int)canvas.ActualHeight - 150));
                    var sbb = Resources["rainstar"] as Storyboard;

                    var anii = sbb.Children[0] as DoubleAnimationUsingKeyFrames;
                    Storyboard.SetTarget(anii, rs);

                    //Left
                    var anii1 = sbb.Children[1] as DoubleAnimationUsingKeyFrames;
                    anii1.KeyFrames[0].Value = Canvas.GetLeft(rs);
                    anii1.KeyFrames[1].Value = Canvas.GetLeft(rs) - 200;
                    Storyboard.SetTarget(anii1, rs);

                    //Top
                    var anii2 = sbb.Children[2] as DoubleAnimationUsingKeyFrames;
                    anii2.KeyFrames[0].Value = Canvas.GetTop(rs);
                    anii2.KeyFrames[1].Value = Canvas.GetTop(rs) + 200;
                    Storyboard.SetTarget(anii2, rs);
                    sbb.Begin();

                    canvas.Children.Add(rs);
                }
                catch { }
            }
        }
    }

    public static class h
    {
        public static BitmapImage ToBitmapImage(this byte[] array, int[] DecodePixel = null)
        {
            using (var ms = new MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                if (DecodePixel != null)
                {
                    image.DecodePixelHeight = DecodePixel[0];
                    image.DecodePixelWidth = DecodePixel[1];
                }
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

    }
}
