using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

/*
           Lemon App 通用主题模板 V2 Run on .Net Core 3.1
           2020.1.19
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
namespace LemonApp.Theme.Dtpp
{
    /// <summary>
    /// Drawe.xaml 的交互逻辑
    /// </summary>
    public partial class Drawer : ThemeBase
    {
        public Drawer()
        {
            ThemeName = "泡泡";
            ThemeColor = Color.FromArgb(255, 32, 143, 255);
            FontColor = "White";
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
            Draw();
        }
        public static new string NameSpace = "LemonApp.Theme.Dtpp.Drawer";
        public override ThemeBase GetPage()
        {
            return new Drawer();
        }

        public async void Draw()
        {
            await Task.Delay(1000);
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(i * 100);
                Ellipse b = new Ellipse();
                Random rd = new Random();
                int width = rd.Next(25, 150);
                b.Width = width;
                b.Height = width;
                int color = rd.Next(51, 127);
                b.Fill = new SolidColorBrush(Color.FromArgb((byte)color, 255, 255, 255));
                int left = rd.Next(0, (int)canvas.ActualWidth);
                Canvas.SetLeft(b, left);
                int speed = rd.Next(8, 15);
                DoubleAnimation d = new DoubleAnimation() {
                    Duration = TimeSpan.FromSeconds(speed),
                    From = canvas.ActualHeight, To = 0-width,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                Timeline.SetDesiredFrameRate(d, 35);
                canvas.Children.Add(b);
                b.BeginAnimation(Canvas.TopProperty, d);
            }
        }
    }
}
