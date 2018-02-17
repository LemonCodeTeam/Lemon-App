using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LemonLibrary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Text.RegularExpressions;
using WPFMediaKit.DirectShow.Controls;

namespace Lemon_App
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        System.Windows.Forms.Timer tr = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer trs = new System.Windows.Forms.Timer();
        public LoginWindow()
        {
            InitializeComponent();
            wb.Navigated += NaAsync;
            RM.IsChecked = LemonLibrary.Settings.LSettings.RNBM;
            tr.Interval = 5000;
            tr.Tick += T;
            trs.Interval = 1000;
            trs.Tick += Trs;
            if (Console.CapsLock)
            {
                oldtext = rk.Text;
                rk.Text = "已开启大写锁定";
            }
            else { if (oldtext != "已开启大写锁定") rk.Text = oldtext; else { rk.Text = ""; oldtext = ""; } }
            (Resources["l"] as Storyboard).Begin();
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Settings.st"))
                Settings.LoadLSettings(Encoding.Default.GetString(Convert.FromBase64String(LemonLibrary.TextHelper.TextDecrypt(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "Settings.st"), LemonLibrary.TextHelper.MD5.EncryptToMD5string("Settings.st")))));
            else LemonLibrary.Settings.SaveLoadSettings();
        }
        int index = 0;
        private async void NaAsync(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (index != 0)
            {
                if (wb.DocumentTitle == "我的QQ中心")
                {
                    op.IsOpen = false;
                    var qq = LemonLibrary.TextHelper.XtoYGetTo(wb.Document.Cookie, "uin=o", ";", 0);
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + qq + ".st"))
                        Settings.LoadUSettings(Encoding.Default.GetString(Convert.FromBase64String(LemonLibrary.TextHelper.TextDecrypt(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + qq + ".st"), LemonLibrary.TextHelper.MD5.EncryptToMD5string(qq + ".st")))));
                    else LemonLibrary.Settings.SaveSettings(qq);
                    var sl = LemonLibrary.TextHelper.XtoYGetTo(await LemonLibrary.HttpHelper.GetWebAsync("http://r.pengyou.com/fcg-bin/cgi_get_portrait.fcg?uins=" + qq, Encoding.Default), "portraitCallBack(", ")", 0);
                    JObject o = JObject.Parse(sl);
                    try
                    {
                        await LemonLibrary.HttpHelper.HttpDownloadFileAsync($"http://q2.qlogo.cn/headimg_dl?bs=qq&dst_uin={qq}&spec=100", AppDomain.CurrentDomain.BaseDirectory + qq + ".jpg");
                        var image = new System.Drawing.Bitmap(AppDomain.CurrentDomain.BaseDirectory + qq + ".jpg");
                        TX.Background = new ImageBrush(image.ToImageSource());
                    }
                    catch { }
                    Settings.USettings.UserName = o[qq][6].ToString();
                    Settings.USettings.UserImage = AppDomain.CurrentDomain.BaseDirectory + qq + ".jpg";
                    Settings.USettings.LemonAreeunIts = qq;
                    Settings.SaveSettings();
                    Settings.LSettings.NAME = qq;
                    Settings.LSettings.RNBM = (Boolean)RM.IsChecked;
                    Settings.LSettings.TX = AppDomain.CurrentDomain.BaseDirectory + qq + ".jpg";
                    Settings.SaveLoadSettings();
                    (Resources["OnLoaded1"] as Storyboard).Begin();
                    tr.Start();
                }
                else if (wb.DocumentText.Contains("安全验证"))
                {
                    op.IsOpen = true;
                    rk.Text = "请输入验证码";
                }
                else { rk.Text = "登录失败,请检查账号和密码."; op.IsOpen = false; }
            }
            else { index++; }
        }

        private void Trs(object sender, EventArgs e)
        {
            OS.Visibility = Visibility.Visible;
            RM.Visibility = Visibility.Visible;
            rk.Text = "验证码错误";
            trs.Stop();
        }

        private void T(object sender, EventArgs e)
        {
            new MainWindow().Show();
            this.Close();
            tr.Stop();
            wb.Dispose();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.LSettings.RNBM)
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + Settings.LSettings.NAME + ".st"))
                    Settings.LoadUSettings(Encoding.Default.GetString(Convert.FromBase64String(LemonLibrary.TextHelper.TextDecrypt(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + Settings.LSettings.NAME + ".st"), LemonLibrary.TextHelper.MD5.EncryptToMD5string(Settings.LSettings.NAME + ".st")))));
                else LemonLibrary.Settings.SaveSettings(Settings.LSettings.NAME);
                (Resources["OnLoaded1"] as Storyboard).Begin();
                tr.Start();
            }
            Email.Text = Settings.LSettings.NAME;
            if (System.IO.File.Exists(Settings.LSettings.TX))
            {
                var image = new System.Drawing.Bitmap(Settings.LSettings.TX);
                TX.Background = new ImageBrush(image.ToImageSource());
            }
            RM.IsChecked = Settings.LSettings.RNBM;
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var c = (Resources["c"] as Storyboard);
            c.Completed += delegate { Process.GetCurrentProcess().Kill(); };
            c.Begin();
        }
        private void Email_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Border_MouseDown_1(null, null);
        }
        private bool IsValidEmail(string strIn)
        {
            return Regex.IsMatch(strIn, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)" + @"|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }
        //   System.Windows.Forms.WebBrowser wb = new System.Windows.Forms.WebBrowser();
        private async void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (Email.Text != string.Empty || PSW.Password != string.Empty)
            {
                wb.Navigate("http://ui.ptlogin2.qq.com/cgi-bin/login?appid=1006102&s_url=http://id.qq.com/index.html&hide_close_icon=1");
                rk.Text = "登录中...";
                await Task.Delay(2000);
                System.Windows.Forms.HtmlDocument doc = wb.Document;
                doc.GetElementById("switcher_plogin").InvokeMember("click");
                await Task.Delay(200);
                doc.GetElementById("u").InnerText = Email.Text;
                await Task.Delay(200);
                doc.GetElementById("p").InnerText = PSW.Password;
                await Task.Delay(200);
                doc.GetElementById("login_button").InvokeMember("click");
                await Task.Delay(1000);
                if (wb.DocumentTitle != "我的QQ中心" || !wb.DocumentText.Contains("安全验证"))
                    rk.Text = "登录失败,请检查账号和密码.";
                else if (wb.DocumentText.Contains("安全验证"))
                {
                    op.IsOpen = true;
                    rk.Text = "请输入验证码";
                }
            }
        }

        private async void Grid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (wb.DocumentTitle == "我的QQ中心")
                {
                    op.IsOpen = false;
                    var qq = LemonLibrary.TextHelper.XtoYGetTo(wb.Document.Cookie, "uin=o", ";", 0);
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + qq + ".st"))
                        Settings.LoadUSettings(Encoding.Default.GetString(Convert.FromBase64String(LemonLibrary.TextHelper.TextDecrypt(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + qq + ".st"), LemonLibrary.TextHelper.MD5.EncryptToMD5string(qq + ".st")))));
                    else LemonLibrary.Settings.SaveSettings(qq);
                    var sl = LemonLibrary.TextHelper.XtoYGetTo(await LemonLibrary.HttpHelper.GetWebAsync("http://r.pengyou.com/fcg-bin/cgi_get_portrait.fcg?uins=" + qq, Encoding.Default), "portraitCallBack(", ")", 0);
                    JObject o = JObject.Parse(sl);
                    try
                    {
                        await LemonLibrary.HttpHelper.HttpDownloadFileAsync($"http://q2.qlogo.cn/headimg_dl?bs=qq&dst_uin={qq}&spec=100", AppDomain.CurrentDomain.BaseDirectory + qq + ".jpg");
                        var image = new System.Drawing.Bitmap(AppDomain.CurrentDomain.BaseDirectory + qq + ".jpg");
                        TX.Background = new ImageBrush(image.ToImageSource());
                    }
                    catch { }
                    Settings.USettings.UserName = o[qq][6].ToString();
                    Settings.USettings.UserImage = AppDomain.CurrentDomain.BaseDirectory + qq + ".jpg";
                    Settings.USettings.LemonAreeunIts = qq ;
                    Settings.SaveSettings();
                    Settings.LSettings.NAME = qq;
                    Settings.LSettings.RNBM = (Boolean)RM.IsChecked;
                    Settings.LSettings.TX = AppDomain.CurrentDomain.BaseDirectory + qq + ".jpg";
                    Settings.SaveLoadSettings();
                    (Resources["OnLoaded1"] as Storyboard).Begin();
                    tr.Start();
                }
                else if (wb.DocumentText.Contains("安全验证"))
                {
                    op.IsOpen = true;
                    rk.Text = "请输入验证码";
                }
                else { rk.Text = "登录失败,请检查账号和密码."; op.IsOpen = false; }

            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 3)
            {
                op.IsOpen = !op.IsOpen;
                wb.Navigate("http://ui.ptlogin2.qq.com/cgi-bin/login?appid=1006102&s_url=http://id.qq.com/index.html&hide_close_icon=1");
            }
        }
        string oldtext = "";
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Console.CapsLock)
            {
                oldtext = rk.Text;
                rk.Text = "已开启大写锁定";
            }
            else { if (oldtext != "已开启大写锁定") rk.Text = oldtext; else { rk.Text = ""; oldtext = ""; } }
        }

        private void Email_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Email.Text.Count() >= 5)
            {
                if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + Email.Text + ".jpg"))
                {
                    var image = new System.Drawing.Bitmap(AppDomain.CurrentDomain.BaseDirectory + Email.Text + ".jpg");
                    TX.Background = new ImageBrush(image.ToImageSource());
                    TX.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2)));
                }
                else { TX.Background = new ImageBrush(new BitmapImage(new Uri("http://q2.qlogo.cn/headimg_dl?bs=qq&dst_uin={qq}&spec=100"))); }
            }
        }

        private void border_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            tr.Stop();
            (Resources["OnLoaded1"] as Storyboard).Stop();
            (Resources["FXC"] as Storyboard).Begin();
        }
        private async void qrcode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            wb.Navigate("http://ui.ptlogin2.qq.com/cgi-bin/login?appid=1006102&s_url=http://id.qq.com/index.html&hide_close_icon=1");
            await Task.Delay(1000);
            string str = wb.Document.Body.OuterHtml;
            MatchCollection matches;
            matches = Regex.Matches(str, @"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);
            var t = matches[1].Value.ToString();
            Regex reg = new Regex(@"<img.*?src=""(?<src>[^""]*)""[^>]*>", RegexOptions.IgnoreCase);
            MatchCollection mc = reg.Matches(t);
            var content = mc[0].Groups["src"].Value;
            t = TextHelper.XtoYGetTo(content + "\"", "t=", "\"", 0);
            qrcode.Background = new ImageBrush(new BitmapImage(new Uri(content)));
            //       op.IsOpen = true;
            index = 0;
        }

        private async void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            wb.Navigate("http://ui.ptlogin2.qq.com/cgi-bin/login?appid=1006102&s_url=http://id.qq.com/index.html&hide_close_icon=1");
            await Task.Delay(5000);
            string str = wb.Document.Body.OuterHtml;
            MatchCollection matches;
            matches = Regex.Matches(str, @"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);
            var t = matches[1].Value.ToString();
            Regex reg = new Regex(@"<img.*?src=""(?<src>[^""]*)""[^>]*>", RegexOptions.IgnoreCase);
            MatchCollection mc = reg.Matches(t);
            var content = mc[0].Groups["src"].Value;
            qrcode.Background = new ImageBrush(new BitmapImage(new Uri(content)));
            index = 0;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
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
                    byte[] a = ms.ToArray();
                    byte[] b = Convert.FromBase64String(TextHelper.TextDecrypt(File.ReadAllText(Settings.LSettings.NAME + ".FaceData"), TextHelper.MD5.EncryptToMD5string(Settings.LSettings.NAME + ".FaceData")));
                    vce.Stop();
                    var client = new Baidu.Aip.Face.Face("75bl82qIt9Rtly6Na6wqYUmm", "pMO9ZSQSsZFNvMMnXy5L3GaQbpWG6Fyw");
                    var images = new byte[][] { a, b };
                    var result = double.Parse(client.FaceMatch(images).First.First.Last.Last.First.ToString());
                    if (result >= 90)
                    {
                        if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + Settings.LSettings.NAME + ".st"))
                            Settings.LoadUSettings(Encoding.Default.GetString(Convert.FromBase64String(LemonLibrary.TextHelper.TextDecrypt(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + Settings.LSettings.NAME + ".st"), LemonLibrary.TextHelper.MD5.EncryptToMD5string(Settings.LSettings.NAME + ".st")))));
                        else LemonLibrary.Settings.SaveSettings(Settings.LSettings.NAME+".st");
                        (Resources["OnLoaded1"] as Storyboard).Begin();
                        tr.Start();
                    }
                    else txb.Text = "识别失败";
                }
            }
            catch(Exception ex) { throw ex; }
        }

        private void face_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Email.Text != "QQ账号" && File.Exists(Email.Text + ".FaceData"))
            {
                vce.VideoCaptureSource = MultimediaUtil.VideoInputNames[0];
                (Resources["FACESTAR"] as Storyboard).Begin();
            }
            else { rk.Text = "没有录入面部数据"; }
        }
    }
}
