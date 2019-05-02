using LemonLibrary;
using Lierda.WPFHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
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
        public static string EM = "1041";
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
            SetColor("BorderColorBrush", (Color)ColorConverter.ConvertFromString("#4C000000"));
            SetColor("ControlPageBrush", (Color)ColorConverter.ConvertFromString("#19000000"));
            SetColor("TitlePageBrush", (Color)ColorConverter.ConvertFromString("#0C000000"));
            SetColor("ContentPageBursh", (Color)ColorConverter.ConvertFromString("#0C000000"));
            SetColor("DataTopBrush", (Color)ColorConverter.ConvertFromString("#0C000000"));
            SetColor("TextX1ColorBrush", (Color)ColorConverter.ConvertFromString("White"));
        }
        public void Skin_Black()
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
            SetColor("ThemeColor", (Color)ColorConverter.ConvertFromString("#FF3ED38B"));
            SetColor("ResuColorBrush", (Color)ColorConverter.ConvertFromString("#FF7D7D7D"));
            SetColor("ButtonColorBrush", (Color)ColorConverter.ConvertFromString("#FF7D7D7D"));
            SetColor("BorderColorBrush", (Color)ColorConverter.ConvertFromString("#FFE1E1E2"));
            SetColor("ControlPageBrush", (Color)ColorConverter.ConvertFromString("#FFF5F5F7"));
            SetColor("TitlePageBrush", (Color)ColorConverter.ConvertFromString("#FFFAFAFA"));
            SetColor("ContentPageBursh", (Color)ColorConverter.ConvertFromString("#FFFAFAFA"));
            SetColor("DataTopBrush", (Color)ColorConverter.ConvertFromString("#FFFDFDFD"));
            SetColor("TextX1ColorBrush", (Color)ColorConverter.ConvertFromString("#FF7D7D7D"));
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
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                args.SetObserved();
                var e = args.Exception;
                string i = "\n小萌账号:" + Settings.USettings.LemonAreeunIts 
                +"\r\n小萌版本:" + EM 
                + "\r\n" + e.Message 
                + "\r\n 导致错误的对象名称:" + e.Source 
                + "\r\n 引发异常的方法:" + e.TargetSite 
                + "\r\n  帮助链接:" +e.HelpLink 
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

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string i = "\n小萌账号:" + Settings.USettings.LemonAreeunIts + "\r\n小萌版本:"+EM + "\r\n" + ((Exception)e.ExceptionObject).Message + "\r\n 导致错误的对象名称:" + ((Exception)e.ExceptionObject).Source + "\r\n 引发异常的方法:" + ((Exception)e.ExceptionObject).TargetSite + "\r\n  帮助链接:" + ((Exception)e.ExceptionObject).HelpLink + "\r\n 调用堆:" + ((Exception)e.ExceptionObject).StackTrace;
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
            string i = "\n(Dispatcher)小萌账号:" + Settings.USettings.LemonAreeunIts + "\r\n小萌版本:"+EM+ "\r\n" + e.Exception.Message + "\r\n 导致错误的对象名称:" + e.Exception.Source + "\r\n 引发异常的方法:" + e.Exception.TargetSite + "\r\n  帮助链接:" + e.Exception.HelpLink + "\r\n 调用堆:" + e.Exception.StackTrace;
            Console.WriteLine(i);
            FileStream fs = new FileStream(Settings.USettings.CachePath + "Log.log", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(i);
            sw.Flush();
            sw.Close();
            fs.Close();
        }
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
    }

    public class WindowWrapper
    {
        private App app;
        public void ShowMainWindow()
        {
            app = new App();

        }
    }

    [SuppressUnmanagedCodeSecurity]
    public static class ConsoleManager
    {
        private const string Kernel32_DllName = "kernel32.dll";

        [DllImport(Kernel32_DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32_DllName)]
        private static extern bool FreeConsole();

        [DllImport(Kernel32_DllName)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(Kernel32_DllName)]
        private static extern int GetConsoleOutputCP();

        public static bool HasConsole
        {
            get { return GetConsoleWindow() != IntPtr.Zero; }
        }

        /// <summary>  
        /// Creates a new console instance if the process is not attached to a console already.  
        /// </summary>  
        public static void Show()
        {
#if DEBUG
            if (!HasConsole)
            {
                AllocConsole();
                InvalidateOutAndError();
            }
#endif
        }

        /// <summary>  
        /// If the process has a console attached to it, it will be detached and no longer visible. Writing to the System.Console is still possible, but no output will be shown.  
        /// </summary>  
        public static void Hide()
        {
#if DEBUG
            if (HasConsole)
            {
                SetOutAndErrorNull();
                FreeConsole();
            }
#endif
        }

        public static void Toggle()
        {
            if (HasConsole)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        static void InvalidateOutAndError()
        {
            Type type = typeof(System.Console);

            System.Reflection.FieldInfo _out = type.GetField("_out",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            System.Reflection.FieldInfo _error = type.GetField("_error",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            System.Reflection.MethodInfo _InitializeStdOutError = type.GetMethod("InitializeStdOutError",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            Debug.Assert(_out != null);
            Debug.Assert(_error != null);

            Debug.Assert(_InitializeStdOutError != null);

            _out.SetValue(null, null);
            _error.SetValue(null, null);

            _InitializeStdOutError.Invoke(null, new object[] { true });
        }

        static void SetOutAndErrorNull()
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }
        static void SetOut(string A)
        {

        }
    }
}