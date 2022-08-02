using LemonLib;
using Lierda.WPFHelper;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace LemonApp
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static App BaseApp = null;
        /// <summary>
        /// 程序版本号 （用于检测更新）
        /// </summary>
        public static string EM = "1241";
        #region 启动时 进程检测 配置 登录
        //放在全局变量  防止GC回收  导致失效
        private System.Threading.Mutex mutex;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //控制单实例  并唤起
            mutex = new System.Threading.Mutex(true, "LemonApp", out bool mutexWasCreated);
            if (!mutexWasCreated)
            {
                MsgHelper.SendMsg(MsgHelper.SEND_SHOW);
                Current.Shutdown();
            }
            else
            {
                //To solve: HttpWebRequest The SSL connection could not be established
                ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
              
                if (!Directory.Exists(Settings.USettings.DataCachePath))
                    Directory.CreateDirectory(Settings.USettings.DataCachePath);
                if (!Directory.Exists(Settings.USettings.MusicCachePath))
                    Directory.CreateDirectory(Settings.USettings.MusicCachePath);
                if (!Directory.Exists(Settings.USettings.MusicCachePath + "Skin"))
                    Directory.CreateDirectory(Settings.USettings.MusicCachePath + "Skin");
                new MainWindow().Show();
            }
        }
        #endregion
        #region 主题颜色配置
        /// <summary>
        /// 指定当前主题背景色 default:0(white) 1(black)
        /// </summary>
        public int ThemeColor = 0;
        public void SetColor(string id, Color c)
        {
            var color = new SolidColorBrush() { Color = c };
            Resources[id] = color;
        }
        /// <summary>
        /// 适配白色字体的主题配置（默认）
        /// </summary>
        public void Skin()
        {
            ThemeColor = 1;
            SetColor("ThemeColor", (Color)ColorConverter.ConvertFromString("#FF31C27C"));
            SetColor("ResuColorBrush", (Color)ColorConverter.ConvertFromString("White"));
            SetColor("ControlPageBrush", (Color)ColorConverter.ConvertFromString("#19000000"));
   
            SetColor("PlayDLPage_Top", (Color)ColorConverter.ConvertFromString("#FF2D2D30"));
            SetColor("PlayDLPage_Bg", (Color)ColorConverter.ConvertFromString("#FF3E3E42"));
            SetColor("PlayDLPage_Border", (Color)ColorConverter.ConvertFromString("#FF252526"));
            SetColor("PlayDLPage_Font_Most", (Color)ColorConverter.ConvertFromString("#FFB9B9B9"));
            SetColor("PlayDLPage_Font_Low", (Color)ColorConverter.ConvertFromString("#FF979797"));
        }
        /// <summary>
        /// 适配黑色字体的主题配置
        /// </summary>
        public void Skin_Black()
        {
            ThemeColor = 0;
            SetColor("ThemeColor", (Color)ColorConverter.ConvertFromString("#FF31C27C"));
            SetColor("ResuColorBrush", (Color)ColorConverter.ConvertFromString("White"));
            SetColor("ControlPageBrush", (Color)ColorConverter.ConvertFromString("#19FFFFFF"));
 
            SetColor("PlayDLPage_Top", (Color)ColorConverter.ConvertFromString("#FFF9F9F9"));
            SetColor("PlayDLPage_Bg", (Color)ColorConverter.ConvertFromString("White"));
            SetColor("PlayDLPage_Border", (Color)ColorConverter.ConvertFromString("#FFF2F2F2"));
            SetColor("PlayDLPage_Font_Most", (Color)ColorConverter.ConvertFromString("#FF232323"));
            SetColor("PlayDLPage_Font_Low", (Color)ColorConverter.ConvertFromString("#FF707070"));
        }
        /// <summary>
        /// 恢复 默认主题  /卸载主题
        /// </summary>
        public void unSkin()
        {
            ThemeColor = 0;
            SetColor("ThemeColor", (Color)ColorConverter.ConvertFromString("#FF31C27C"));
            SetColor("ResuColorBrush", (Color)ColorConverter.ConvertFromString("#FF272727"));
            SetColor("ControlPageBrush", (Color)ColorConverter.ConvertFromString("#05000000"));
 
            SetColor("PlayDLPage_Top", (Color)ColorConverter.ConvertFromString("#FFF9F9F9"));
            SetColor("PlayDLPage_Bg", (Color)ColorConverter.ConvertFromString("White"));
            SetColor("PlayDLPage_Border", (Color)ColorConverter.ConvertFromString("#FFF2F2F2"));
            SetColor("PlayDLPage_Font_Most", (Color)ColorConverter.ConvertFromString("#FF232323"));
            SetColor("PlayDLPage_Font_Low", (Color)ColorConverter.ConvertFromString("#FF707070"));
        }
        #endregion
        #region lierda.WPFHelper 内存管控
        LierdaCracker cracker = new LierdaCracker();
        protected override void OnStartup(StartupEventArgs e)
        {
            cracker.Cracker();
            base.OnStartup(e);
        }
        #endregion
        #region 全局异常捕获/处理
        public App()
        {
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                args.SetObserved();
                var e = args.Exception;
                string i = "\nLemonApp账号:" + Settings.USettings.LemonAreeunIts
                + "\r\nLemonApp版本:" + EM
                + "\r\n" + e.Message
                + "\r\n 导致错误的对象名称:" + e.Source
                + "\r\n 引发异常的方法:" + e.TargetSite
                + "\r\n  帮助链接:" + e.HelpLink
                + "\r\n 调用堆:" + e.StackTrace;
                Console.WriteLine(i,"ERROR","red");
                FileStream fs = new FileStream(Settings.USettings.DataCachePath + "Log.log", FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(i);
                sw.Flush();
                sw.Close();
                fs.Close();
            };
            BaseApp = this;
        }
        public void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string i = "\nLemonApp账号:" + Settings.USettings.LemonAreeunIts + "\r\nLemonApp版本:" + EM + "\r\n" + ((Exception)e.ExceptionObject).Message + "\r\n 导致错误的对象名称:" + ((Exception)e.ExceptionObject).Source + "\r\n 引发异常的方法:" + ((Exception)e.ExceptionObject).TargetSite + "\r\n  帮助链接:" + ((Exception)e.ExceptionObject).HelpLink + "\r\n 调用堆:" + ((Exception)e.ExceptionObject).StackTrace;
            Console.WriteLine(i, "ERROR", "red");
            FileStream fs = new FileStream(Settings.USettings.DataCachePath + "Log.log", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(i);
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            string i = "\n(Dispatcher)LemonApp账号:" + Settings.USettings.LemonAreeunIts + "\r\nLemonApp版本:" + EM + "\r\n" + e.Exception.Message + "\r\n 导致错误的对象名称:" + e.Exception.Source + "\r\n 引发异常的方法:" + e.Exception.TargetSite + "\r\n  帮助链接:" + e.Exception.HelpLink + "\r\n 调用堆:" + e.Exception.StackTrace;
            Console.WriteLine(i, "ERROR", "red");
            FileStream fs = new FileStream(Settings.USettings.DataCachePath + "Log.log", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(i);
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        #endregion
    }
    #region Console 调试模式
    public class Console
    {
        private static Process p = null;

        public static void Open()
        {
            p = new Process();
            p.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "DebugConsole.exe";
            p.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <param name="color">blue white red yellow</param>
        public static async void WriteLine(object text,string title="",string color="blue")
        {
            if (p != null)
            {
                try
                {
                    string json = TextHelper.JSON.ToJSON(new DebugConsole.DebugData()
                    {
                        title = title,
                        data = text.ToString(),
                        color = color
                    });
                    Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    await clientSocket.ConnectAsync("127.0.0.1", 3239);
                    await clientSocket.SendAsync(Encoding.UTF8.GetBytes(json), SocketFlags.None);
                }
                catch { }
            }
        }
        public static void Close()
        {
            p.Kill();
        }
    }
    #endregion
}