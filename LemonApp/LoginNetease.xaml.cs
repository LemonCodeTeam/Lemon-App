using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// LoginNetease.xaml 的交互逻辑
    /// </summary>
    public partial class LoginNetease : Window
    {
        private System.Windows.Forms.WebBrowser wb;
        private Action<string> LoginCallBack;
        public LoginNetease(Action<string> loginCallBack)
        {
            InitializeComponent();
            wb = new();
            wb.ScriptErrorsSuppressed = true;
            wb.IsWebBrowserContextMenuEnabled = false;
            wb.WebBrowserShortcutsEnabled = false;
            wf.Child = wb;
            LoginCallBack = loginCallBack;
        }

        [DllImport("urlmon.dll", CharSet = CharSet.Ansi)]
        private static extern int UrlMkSetSessionOption(int dwOption, string pBuffer, int dwBufferLength, int dwReserved);
        const int URLMON_OPTION_USERAGENT = 0x10000001;

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton==MouseButtonState.Pressed)
                DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           string userAgent = "Mozilla/5.0 (Linux; Android 8.0.0; SM-G955U Build/R16NW) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Mobile Safari/537.36 Edg/119.0.0.0";
            UrlMkSetSessionOption(URLMON_OPTION_USERAGENT, userAgent, userAgent.Length, 0);
            wb.Navigated += delegate
            {
                loading.Visibility = Visibility.Collapsed;
                wf.Visibility = Visibility.Visible;
            };
            wb.Navigate(new Uri("https://music.163.com/#/login"));
            Activate();
            wb.DocumentTitleChanged += Wb_DocumentTitleChanged;
        }

        private void Wb_DocumentTitleChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
