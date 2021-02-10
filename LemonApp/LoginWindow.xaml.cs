using LemonLib;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        private MainWindow mw;
        public LoginWindow(MainWindow m)
        {
            InitializeComponent();
            mw = m;
        }
        public async void Api_Login()
        {
            wb.NavigationCompleted +=async delegate {
                Console.WriteLine("", "Arrive");
                await wb.ExecuteScriptAsync("document.getElementsByClassName(\"lay_top\")[0].remove();");
                await wb.ExecuteScriptAsync("document.getElementsByClassName(\"page_accredit combine_page_children align\")[0].remove();");
                await wb.ExecuteScriptAsync("document.getElementById(\"title_0\").innerText=\"登录到Lemon App\";");
                loading.Visibility = Visibility.Collapsed;
                wf.Visibility = Visibility.Visible;
            };
            wb.Source=new Uri("https://graph.qq.com/oauth2.0/show?which=Login&display=pc&response_type=code&client_id=100497308&redirect_uri=https%3A%2F%2Fy.qq.com%2Fportal%2Fwx_redirect.html%3Flogin_type%3D1%26surl%3Dhttps%253A%252F%252Fy.qq.com%252Fportal%252Fprofile.html%26use_customer_cb%3D0&state=state&display=pc");
            Topmost = true;
            Activate();
            wb.ContentLoading+= Wb_Dc_Login;
            await wb.EnsureCoreWebView2Async();
        }

        private async void Wb_Dc_Login(object sender, EventArgs e)
        {
            if (wb.Source.ToString().Contains("y.qq.com/portal/profile.html"))
            {
                Topmost = false;
                wb.Stop();
                var cookies =(await wb.CoreWebView2.CookieManager.GetCookiesAsync("https://graph.qq.com/oauth2.0/authorize"));
                string cookie = "";
                foreach (var i in cookies) {
                    cookie += i.Name + "=" + i.Value + "; ";
                }
                var ycookies = (await wb.CoreWebView2.CookieManager.GetCookiesAsync("https://y.qq.com/portal/profile.html"));
                string ycookie = "";
                foreach (var i in ycookies)
                {
                    ycookie += i.Name + "=" + i.Value + "; ";
                }
                Console.WriteLine(cookie, "LoginData");
                string qq = TextHelper.FindTextByAB(cookie + ";", "ptui_loginuin=", ";", 0);
                LoginData send = new LoginData()
                {
                    qq = qq,
                    cookie = ycookie
                };
                if (cookie.Contains("p_skey="))
                {
                    string p_skey = TextHelper.FindTextByAB(cookie + ";", "p_skey=", ";", 0);
                    long hash = 5381;
                    for (int i = 0; i < p_skey.Length; i++)
                    {
                        hash += (hash << 5) + p_skey[i];
                    }
                    long g_tk = hash & 0x7fffffff;
                    send.g_tk = g_tk.ToString();
                }
                Console.WriteLine(send.g_tk, "LOGIN G_TK");
                   mw.Dispatcher.Invoke(delegate { mw.Login(send); });
                wb.ContentLoading -= Wb_Dc_Login;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void CloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Api_Login();
        }

        bool downloading = false;
        private async void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!downloading)
            {
                Toast.Send("正在下载WebView2运行时...");
                downloading = true;
                string url = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";
                string path = System.IO.Path.Combine(Settings.USettings.DownloadPath, "WebViewRuntimeSetup.exe");
                await HttpHelper.HttpDownloadFileAsync(url, path);
                Toast.Send("完成.");
                Process.Start(path);
            }
        }
    }
}
