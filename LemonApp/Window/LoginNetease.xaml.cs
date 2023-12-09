using LemonLib;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text.RegularExpressions;
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
        private Action<string,string> LoginCallBack;
        /// <summary>
        /// Log in Netease
        /// </summary>
        /// <param name="loginCallBack">string cookie,string NeteaseId</param>
        public LoginNetease(Action<string,string> loginCallBack)
        {
            InitializeComponent();
            LoginCallBack = loginCallBack;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton==MouseButtonState.Pressed)
                DragMove();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string core = null;
            try
            {
                core=CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch { }
            if(string.IsNullOrEmpty(core))
            {
                if(TwMessageBox.Show("没有检测到WebView2 Runtime 现在安装吗？")) {
                Process.Start("explorer","https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/");
                }
                Close();
                return;
            }
            var webView2Environment = await CoreWebView2Environment.CreateAsync(null, Settings.USettings.DataCachePath);
            await wb.EnsureCoreWebView2Async(webView2Environment);
            wb.CoreWebView2.CookieManager.DeleteAllCookies();
            wb.CoreWebView2.FrameNavigationCompleted += CoreWebView2_FrameNavigationCompleted;
            wb.CoreWebView2.Navigate("https://music.163.com/#/my/");
        }

        private async void CoreWebView2_FrameNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            string html = await wb.CoreWebView2.ExecuteScriptAsync("document.body.innerHTML");
            var regex = Regex.Match(html, @"(?<=/user/home\?id=)\d+");
            if (regex.Success)
            {
                string cookie = "";
                var a = await wb.CoreWebView2.CookieManager.GetCookiesAsync("https://music.163.com");
                foreach (var item in a)
                {
                    cookie += item.Name + "=" + item.Value + ";";
                }
                //去除首尾的双引号：
                cookie = cookie[..^1];
                Console.WriteLine(cookie);
                //用正则表达式提取用户ID：/user/home?id=323840418"
                string id = regex.Value;
                LoginCallBack(cookie, id);
                Close();
            }
        }

        private void CloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
