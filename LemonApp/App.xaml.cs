using LemonLib;
using Lierda.WPFHelper;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
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
        public static string EM = "1141";
        #region 启动时 进程检测 配置 登录
        System.Threading.Mutex mut;
        private void Application_Startup(object sender, StartupEventArgs e)
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
            SetColor("ButtonColorBrush", (Color)ColorConverter.ConvertFromString("White"));
            SetColor("BorderColorBrush", (Color)ColorConverter.ConvertFromString("#4C000000"));
            SetColor("ControlPageBrush", (Color)ColorConverter.ConvertFromString("#00FFFFFF"));
            SetColor("TextX1ColorBrush", (Color)ColorConverter.ConvertFromString("White"));

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
            SetColor("ButtonColorBrush", (Color)ColorConverter.ConvertFromString("White"));
            SetColor("BorderColorBrush", (Color)ColorConverter.ConvertFromString("#33FFFFFF"));
            SetColor("ControlPageBrush", (Color)ColorConverter.ConvertFromString("#00FFFFFF"));
            SetColor("TextX1ColorBrush", (Color)ColorConverter.ConvertFromString("White"));

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
            SetColor("ButtonColorBrush", (Color)ColorConverter.ConvertFromString("#FF7D7D7D"));
            SetColor("BorderColorBrush", (Color)ColorConverter.ConvertFromString("#FFF0F0F0"));
            SetColor("ControlPageBrush", (Color)ColorConverter.ConvertFromString("#FFFDFDFD"));
            SetColor("TextX1ColorBrush", (Color)ColorConverter.ConvertFromString("#FF535353"));

            SetColor("PlayDLPage_Top", (Color)ColorConverter.ConvertFromString("#FFF9F9F9"));
            SetColor("PlayDLPage_Bg", (Color)ColorConverter.ConvertFromString("White"));
            SetColor("PlayDLPage_Border", (Color)ColorConverter.ConvertFromString("#FFF2F2F2"));
            SetColor("PlayDLPage_Font_Most", (Color)ColorConverter.ConvertFromString("#FF232323"));
            SetColor("PlayDLPage_Font_Low", (Color)ColorConverter.ConvertFromString("#FF707070"));
        }
        public SolidColorBrush GetButtonColorBrush()
        {
            return (SolidColorBrush)Resources["ButtonColorBrush"];
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
                string i = "\n小萌账号:" + Settings.USettings.LemonAreeunIts
                + "\r\n小萌版本:" + EM
                + "\r\n" + e.Message
                + "\r\n 导致错误的对象名称:" + e.Source
                + "\r\n 引发异常的方法:" + e.TargetSite
                + "\r\n  帮助链接:" + e.HelpLink
                + "\r\n 调用堆:" + e.StackTrace;
                Console.WriteLine(i);
                FileStream fs = new FileStream(Settings.USettings.CachePath + "Log.log", FileMode.Append);
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
            string i = "\n小萌账号:" + Settings.USettings.LemonAreeunIts + "\r\n小萌版本:" + EM + "\r\n" + ((Exception)e.ExceptionObject).Message + "\r\n 导致错误的对象名称:" + ((Exception)e.ExceptionObject).Source + "\r\n 引发异常的方法:" + ((Exception)e.ExceptionObject).TargetSite + "\r\n  帮助链接:" + ((Exception)e.ExceptionObject).HelpLink + "\r\n 调用堆:" + ((Exception)e.ExceptionObject).StackTrace;
            Console.WriteLine(i);
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
            string i = "\n(Dispatcher)小萌账号:" + Settings.USettings.LemonAreeunIts + "\r\n小萌版本:" + EM + "\r\n" + e.Exception.Message + "\r\n 导致错误的对象名称:" + e.Exception.Source + "\r\n 引发异常的方法:" + e.Exception.TargetSite + "\r\n  帮助链接:" + e.Exception.HelpLink + "\r\n 调用堆:" + e.Exception.StackTrace;
            Console.WriteLine(i);
            FileStream fs = new FileStream(Settings.USettings.CachePath + "Log.log", FileMode.Append);
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
        private static StreamWriter sw = null;
        public static NamedPipeClientStream pipe = null;
        public static async void Open()
        {
            p = Process.Start(AppDomain.CurrentDomain.BaseDirectory + "DebugConsole.exe");
            pipe = new NamedPipeClientStream("localhost", "DebugConsolePipeForLemonApp", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None);
            await Task.Delay(500);
            await pipe.ConnectAsync();
            sw = new StreamWriter(pipe);
        }
        public static async void WriteLine(object text)
        {
            if (sw != null)
            {
                try
                {
                    await sw.WriteLineAsync(text.ToString());
                    sw.Flush();
                }
                catch
                {
                    Close();
                    Toast.Send("已退出调试模式🐱‍👤");
                }
            }
        }
        public static void Close()
        {
            p.Kill();
            sw.Close();
            sw.Dispose();
            pipe.Close();
            pipe.Dispose();
            sw = null;
            pipe = null;
        }
    }
    #endregion
}