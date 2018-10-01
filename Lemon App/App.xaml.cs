using LemonLibrary;
using Lierda.WPFHelper;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Lemon_App
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static App BaseApp = null;
        public static string EM = "1028";
        public void SetColor(string id,Color c)
        {
            var color = new SolidColorBrush() { Color = c };
            Resources[id] = color;
        }
        public bool skin = false;
        public void Skin()
        {
            skin = true;
            SetColor("ThemeColor", (Color)ColorConverter.ConvertFromString("#FF46FFA2"));
            SetColor("ResuColorBrush", (Color)ColorConverter.ConvertFromString("White"));
            SetColor("ButtonColorBrush", (Color)ColorConverter.ConvertFromString("White"));
            SetColor("BorderColorBrush", (Color)ColorConverter.ConvertFromString("#33FFFFFF"));
            SetColor("ControlPageBrush", (Color)ColorConverter.ConvertFromString("#19FAFAFA"));
            SetColor("TitlePageBrush", (Color)ColorConverter.ConvertFromString("#0CFFFFFF"));
            SetColor("ContentPageBursh", (Color)ColorConverter.ConvertFromString("#0CFFFFFF"));
            SetColor("DataTopBrush", (Color)ColorConverter.ConvertFromString("#0CFFFFFF"));
            SetColor("TextX1ColorBrush", (Color)ColorConverter.ConvertFromString("White"));
        }
        public void unSkin() {
            skin = false;
            SetColor("ThemeColor", (Color)ColorConverter.ConvertFromString("#FF4EB7FB"));
            SetColor("ResuColorBrush", (Color)ColorConverter.ConvertFromString("#FF93A1AE"));
            SetColor("ButtonColorBrush", (Color)ColorConverter.ConvertFromString("#FF787878"));
            SetColor("BorderColorBrush", (Color)ColorConverter.ConvertFromString("#FFE7E8EC"));
            SetColor("ControlPageBrush", (Color)ColorConverter.ConvertFromString("#FFFAFAFA"));
            SetColor("TitlePageBrush", (Color)ColorConverter.ConvertFromString("White"));
            SetColor("ContentPageBursh", (Color)ColorConverter.ConvertFromString("White"));
            SetColor("DataTopBrush", (Color)ColorConverter.ConvertFromString("#FFFDFDFD"));
            SetColor("TextX1ColorBrush", (Color)ColorConverter.ConvertFromString("#FFA0A0A0"));
        }
        public SolidColorBrush GetThemeColorBrush() {
            return (SolidColorBrush)Resources["ThemeColor"];
        }

        public SolidColorBrush GetResuColorBrush() {
            return (SolidColorBrush)Resources["ResuColorBrush"];
        }

        public SolidColorBrush GetButtonColorBrush()
        {
            return (SolidColorBrush)Resources["ButtonColorBrush"];
        }

        LierdaCracker cracker = new LierdaCracker();
        protected override void OnStartup(StartupEventArgs e)
        {
            cracker.Cracker();
            base.OnStartup(e);
        }
        public App() {
//#if !DEBUG
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
//#endif
            BaseApp = this;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string i = "\n小萌账号:" + Settings.USettings.LemonAreeunIts + "\r\n小萌版本:"+EM + "\r\n" + ((Exception)e.ExceptionObject).Message + "\r\n 导致错误的对象名称:" + ((Exception)e.ExceptionObject).Source + "\r\n 引发异常的方法:" + ((Exception)e.ExceptionObject).TargetSite + "\r\n  帮助链接:" + ((Exception)e.ExceptionObject).HelpLink + "\r\n 调用堆:" + ((Exception)e.ExceptionObject).StackTrace;
            FileStream fs = new FileStream(Settings.USettings.CachePath + "Log.log", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(i);
            sw.Flush();
            sw.Close();
            fs.Close();
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            string i = "\n小萌账号:" + Settings.USettings.LemonAreeunIts + "\r\n小萌版本:"+EM+ "\r\n" + e.Exception.Message + "\r\n 导致错误的对象名称:" + e.Exception.Source + "\r\n 引发异常的方法:" + e.Exception.TargetSite + "\r\n  帮助链接:" + e.Exception.HelpLink + "\r\n 调用堆:" + e.Exception.StackTrace;
            FileStream fs = new FileStream(Settings.USettings.CachePath + "Log.log", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(i);
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        System.Threading.Mutex mut;
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            bool requestInitialOwnership = true;
            mut = new System.Threading.Mutex(requestInitialOwnership, "Lemon App", out bool mutexWasCreated);
            if (!(requestInitialOwnership && mutexWasCreated))
            {
                MsgHelper.SendMsg(MsgHelper.SEND_SHOW);
                Current.Shutdown();
            }
            else
            {
                if (!Directory.Exists(Settings.USettings.CachePath))
                    Directory.CreateDirectory(Settings.USettings.CachePath);
                if (!Directory.Exists(Settings.USettings.CachePath + "Skin"))
                    Directory.CreateDirectory(Settings.USettings.CachePath + "Skin");
                var qq = "";
                Settings.LoadLocaSettings();
                if (Settings.LSettings.qq != "EX")
                    qq = Settings.LSettings.qq;
                else
                {
                    try
                    {
                        qq = new DirectoryInfo(Directory.GetDirectories(Environment.ExpandEnvironmentVariables(@"%AppData%\Tencent\Users"))[0]).Name;
                    }
                    catch
                    {
                        qq = "2465759834";
                    }
                }
                Settings.LoadUSettings(qq);
                var sl = await HttpHelper.GetWebAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={qq}&reqfrom=1&reqtype=0", Encoding.UTF8);
                await HttpHelper.HttpDownloadFileAsync($"http://q2.qlogo.cn/headimg_dl?bs=qq&dst_uin={qq}&spec=100", Settings.USettings.CachePath + qq + ".jpg");
                Settings.USettings.UserName = JObject.Parse(sl)["data"]["creator"]["nick"].ToString();
                Settings.USettings.UserImage = Settings.USettings.CachePath + qq + ".jpg";
                Settings.USettings.LemonAreeunIts = qq;
                Settings.SaveSettings();
                Settings.LSettings.qq = qq;
                Settings.SaveLocaSettings();
                new MainWindow().Show();
            }
        }
     }
 }