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
using System.Threading;

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
        }
        private void NaAsync(object sender, WebBrowserNavigatedEventArgs e)
        {
            var wb = sender as System.Windows.Forms.WebBrowser;
            op.IsOpen = false;
            if (wb.DocumentTitle == "我的QQ中心")
                LoginAsync(wb.Document.Cookie).Start();
            else if (wb.DocumentTitle != "我的QQ中心" || !wb.DocumentText.Contains("安全验证"))
                rk.Text = "登录失败,请检查账号和密码.";
            else if (wb.DocumentText.Contains("安全验证"))
            {
                op.IsOpen = true;
                rk.Text = "请输入验证码";
                wfh.Child = wb;
            }
        }

        public async Task LoginAsync(string Cookie)
        {
            var qq = LemonLibrary.TextHelper.XtoYGetTo(Cookie, "uin=o", ";", 0);
            if (File.Exists(InfoHelper.GetPath() + qq + ".st"))
                Settings.LoadUSettings(Encoding.Default.GetString(Convert.FromBase64String(LemonLibrary.TextHelper.TextDecrypt(File.ReadAllText(InfoHelper.GetPath() + qq + ".st"), LemonLibrary.TextHelper.MD5.EncryptToMD5string(qq + ".st")))));
            else LemonLibrary.Settings.SaveSettings(qq);
            var sl = TextHelper.XtoYGetTo(await HttpHelper.GetWebAsync("http://r.pengyou.com/fcg-bin/cgi_get_portrait.fcg?uins=" + qq, Encoding.Default), "portraitCallBack(", ")", 0);
            JObject o = JObject.Parse(sl);
            await HttpHelper.HttpDownloadFileAsync($"http://q2.qlogo.cn/headimg_dl?bs=qq&dst_uin={qq}&spec=100", InfoHelper.GetPath() + qq + ".jpg");
            var image = new System.Drawing.Bitmap(InfoHelper.GetPath() + qq + ".jpg");
            this.Dispatcher.Invoke(new Action(() => { TX.Background = new ImageBrush(image.ToImageSource()); }));
            Settings.USettings.UserName = o[qq][6].ToString();
            Settings.USettings.UserImage = InfoHelper.GetPath() + qq + ".jpg";
            Settings.USettings.LemonAreeunIts = qq;
            Settings.SaveSettings();
            Settings.LSettings.NAME = qq;
            Settings.LSettings.RNBM = (Boolean)RM.IsChecked;
            Settings.LSettings.TX = InfoHelper.GetPath() + qq + ".jpg";
            Settings.SaveLoadSettings();
            Dispatcher.Invoke(new Action(() => { (Resources["OnLoaded1"] as Storyboard).Begin(); }));
            tr.Start();
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
            {
                if (Email.Text != string.Empty || PSW.Password != string.Empty)
                {
                    var wb = new System.Windows.Forms.WebBrowser();
                    wb.ScriptErrorsSuppressed = true;
                    wb.Navigate("http://ui.ptlogin2.qq.com/cgi-bin/login?appid=1006102&s_url=http://id.qq.com/index.html&hide_close_icon=1");
                    rk.Text = "登录中...";
                    wb.Navigated += CAsync;
                    wfh.Child = wb;
                }
            }
        }
        public async void CAsync(object sender,WebBrowserNavigatedEventArgs e) {
            await Task.Delay(2000);
            var wb = sender as System.Windows.Forms.WebBrowser;
            var doc = wb.Document;
            doc.GetElementById("switcher_plogin").InvokeMember("click");
            doc.GetElementById("u").InnerText = Email.Text;
            doc.GetElementById("p").InnerText = PSW.Password;
            doc.GetElementById("login_button").InvokeMember("click");
            wb.Navigated += NaAsync;
            wb.Navigated -= CAsync;
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
                if (System.IO.File.Exists(InfoHelper.GetPath() + Email.Text + ".jpg"))
                {
                    var image = new System.Drawing.Bitmap(InfoHelper.GetPath() + Email.Text + ".jpg");
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
        private void qrcode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var wb = new System.Windows.Forms.WebBrowser();
            wb.ScriptErrorsSuppressed = true;
            wb.Navigate("http://ui.ptlogin2.qq.com/cgi-bin/login?appid=1006102&s_url=http://id.qq.com/index.html&hide_close_icon=1");
            wb.Navigated += XAsync;
            wfh.Child = wb;
        }
        public async void XAsync(object sender, WebBrowserNavigatedEventArgs e) {
            await Task.Delay(2000);
            var wb = sender as System.Windows.Forms.WebBrowser;
            string str = wb.Document.Body.OuterHtml;
            Task task = new Task(() => { T(str); });
            task.Start();
            wb.Navigated += NaAsync;
            wb.Navigated -= XAsync;
        }
        public void T(string str)
        {
            MatchCollection matches;
            matches = Regex.Matches(str, @"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>", RegexOptions.IgnoreCase);
            var t = matches[1].Value.ToString();
            Regex reg = new Regex(@"<img.*?src=""(?<src>[^""]*)""[^>]*>", RegexOptions.IgnoreCase);
            MatchCollection mc = reg.Matches(t);
            var content = mc[0].Groups["src"].Value;
            t = TextHelper.XtoYGetTo(content + "\"", "t=", "\"", 0);
            this.Dispatcher.Invoke(() => { qrcode.Background = new ImageBrush(new BitmapImage(new Uri(content))); });
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
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
                        if (File.Exists(InfoHelper.GetPath() + Settings.LSettings.NAME + ".st"))
                            Settings.LoadUSettings(Encoding.Default.GetString(Convert.FromBase64String(LemonLibrary.TextHelper.TextDecrypt(File.ReadAllText(InfoHelper.GetPath() + Settings.LSettings.NAME + ".st"), LemonLibrary.TextHelper.MD5.EncryptToMD5string(Settings.LSettings.NAME + ".st")))));
                        else LemonLibrary.Settings.SaveSettings(Settings.LSettings.NAME + ".st");
                        Dispatcher.Invoke(new Action(() =>
                        {
                            (Resources["OnLoaded1"] as Storyboard).Begin();
                            tr.Start();
                        }));
                    }
                    else txb.Text = "识别失败";
                }
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

        private void TX_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                op.IsOpen = !op.IsOpen;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var d = (Resources["l"] as Storyboard);
            d.Completed += delegate {
                RM.IsChecked = LemonLibrary.Settings.LSettings.RNBM;
                tr.Interval = 3000;
                tr.Tick += T;
                trs.Interval = 1000;
                trs.Tick += Trs;
                if (Console.CapsLock)
                {
                    oldtext = rk.Text;
                    rk.Text = "已开启大写锁定";
                }
                else { if (oldtext != "已开启大写锁定") rk.Text = oldtext; else { rk.Text = ""; oldtext = ""; } }
                var c = new Task(new Action(delegate
                 {
                     if (File.Exists(InfoHelper.GetPath() + "Settings.st"))
                         Settings.LoadLSettings(Encoding.Default.GetString(Convert.FromBase64String(LemonLibrary.TextHelper.TextDecrypt(File.ReadAllText(InfoHelper.GetPath() + "Settings.st"), LemonLibrary.TextHelper.MD5.EncryptToMD5string("Settings.st")))));
                     else LemonLibrary.Settings.SaveLoadSettings();
                 }));
                c.Start();
                c.Wait();
                if (Settings.LSettings.RNBM)
                {
                    new Task(new Action(delegate
                    {
                        if (File.Exists(InfoHelper.GetPath() + Settings.LSettings.NAME + ".st"))
                            Settings.LoadUSettings(Encoding.Default.GetString(Convert.FromBase64String(LemonLibrary.TextHelper.TextDecrypt(File.ReadAllText(InfoHelper.GetPath() + Settings.LSettings.NAME + ".st"), LemonLibrary.TextHelper.MD5.EncryptToMD5string(Settings.LSettings.NAME + ".st")))));
                        else LemonLibrary.Settings.SaveSettings(Settings.LSettings.NAME);
                    })).Start();
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
                (Resources["START"] as Storyboard).Completed += delegate { qrcode_MouseDown(null, null); };
            };
            d.Begin();
        }
    }
}
