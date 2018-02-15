using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPFMediaKit.DirectShow.Controls;

namespace Lemon_App
{
    /// <summary>
    /// FC.xaml 的交互逻辑
    /// </summary>
    public partial class FC : Window
    {
        public FC()
        {
            InitializeComponent();
            (Resources["START"] as Storyboard).Begin();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var c = (Resources["CLOSE"] as Storyboard);
            c.Completed += delegate { Close(); };
            c.Begin();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (MultimediaUtil.VideoInputNames.Length > 0)
                vce.VideoCaptureSource = MultimediaUtil.VideoInputNames[0];
            else
                MessageBox.Show("没有检测到任何摄像头");
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var c = (Resources["STAR"] as Storyboard);
            c.Completed += delegate {
                var b = (Resources["CLOSE"] as Storyboard);
                b.Completed += delegate { Close(); };
                b.Begin();
            };
            c.Begin();
            vce.Play();
            RenderTargetBitmap bmp = new RenderTargetBitmap(
                (int)vce.ActualWidth,
                (int)vce.ActualHeight,
                96, 96, PixelFormats.Default);
            bmp.Render(vce);
            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                byte[] captureData = ms.ToArray();
                var data = LemonLibrary.TextHelper.TextEncrypt(Convert.ToBase64String(captureData), LemonLibrary.TextHelper.MD5.EncryptToMD5string(LemonLibrary.Settings.LSettings.NAME + ".FaceData"));
                File.WriteAllText(LemonLibrary.Settings.LSettings.NAME + ".FaceData", data);
            }
        }
    }
}
