using LemonLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace bin
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();
        private int RadomPsw;
        private void LoginBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (id.Text != "" && yzm.Text != "")
                if (yzm.Text == RadomPsw.ToString())
                    (Resources["Login"] as Storyboard).Begin();
                else tb.Text = "验证码错误(oﾟvﾟ)ノ";
        }

        private async void yzm_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (id.Text != "")
            {
                RadomPsw = new Random().Next(10000, 99999);
                MailMessage m = new MailMessage();
                m.From = new MailAddress("lemon.app@qq.com", "小萌登录");
                m.To.Add(new MailAddress(id.Text + "@qq.com"));
                m.Subject = "小萌:您的登录验证码";
                m.SubjectEncoding = Encoding.UTF8;
                m.Body = RadomPsw.ToString();
                m.BodyEncoding = Encoding.UTF8;
                m.IsBodyHtml = true;
                SmtpClient s = new SmtpClient();
                s.Host = "smtp.qq.com";
                s.Port = 587;
                s.EnableSsl = true;
                s.Credentials = new NetworkCredential("lemon.app@qq.com", "qtmiqibczofmddbi");
                await s.SendMailAsync(m);
                tb.Text = "发送成功=￣ω￣=";
            }else tb.Text = "先输入你的企鹅号(*/ω＼*)";
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        private async void Storyboard_Completed(object sender, EventArgs e)
        {
            string qq = id.Text;
            if (File.Exists(InfoHelper.GetPath() + qq + ".st"))
                Settings.LoadUSettings(Encoding.Default.GetString(Convert.FromBase64String(TextHelper.TextDecrypt(File.ReadAllText(InfoHelper.GetPath() + qq + ".st"), TextHelper.MD5.EncryptToMD5string(qq + ".st")))));
            else Settings.SaveSettings(qq);
            var sl = await HttpHelper.GetWebAsync("https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={qq}&reqfrom=1&reqtype=0", Encoding.UTF8);
            await HttpHelper.HttpDownloadFileAsync($"http://q2.qlogo.cn/headimg_dl?bs=qq&dst_uin={qq}&spec=100", InfoHelper.GetPath() + qq + ".jpg");
            var image = new System.Drawing.Bitmap(InfoHelper.GetPath() + qq + ".jpg");
            TX.Background = new ImageBrush(image.ToImageSource());
            Settings.USettings.UserName = TextHelper.XtoYGetTo(sl, "\"nick\":\"", "\", \"", 0);
            Settings.USettings.UserImage = InfoHelper.GetPath() + qq + ".jpg";
            Settings.USettings.LemonAreeunIts = qq;
            Settings.SaveSettings();
            Settings.LSettings.NAME = qq;
            Settings.LSettings.RNBM = (Boolean)RM.IsChecked;
            Settings.LSettings.TX = InfoHelper.GetPath() + qq + ".jpg";
            Settings.SaveLoadSettings();
            Process.Start(AppDomain.CurrentDomain.BaseDirectory + "Lemon App.exe", id.Text);
            Environment.Exit(0);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Storyboard_Completed_1(object sender, EventArgs e)
        {
            var c = new Task(new Action(delegate
            {
                if (File.Exists(InfoHelper.GetPath() + "Settings.st"))
                    Settings.LoadLSettings(Encoding.Default.GetString(Convert.FromBase64String(TextHelper.TextDecrypt(File.ReadAllText(InfoHelper.GetPath() + "Settings.st"), LemonLibrary.TextHelper.MD5.EncryptToMD5string("Settings.st")))));
                else Settings.SaveLoadSettings();
            }));
            c.Start();
            c.Wait();
            if (Settings.LSettings.RNBM)
                (Resources["Login"] as Storyboard).Begin();
            id.Text = Settings.LSettings.NAME;
            if (File.Exists(Settings.LSettings.TX))
            {
                var image = new System.Drawing.Bitmap(Settings.LSettings.TX);
                TX.Background = new ImageBrush(image.ToImageSource());
            }
            RM.IsChecked = Settings.LSettings.RNBM;
        }
    }
}
