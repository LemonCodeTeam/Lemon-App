using LemonLib;
using System;
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
        private WebBrowser wb;
        public LoginWindow(MainWindow m)
        {
            InitializeComponent();
            mw = m;
            wb = new WebBrowser();
            wf.Child = wb;
            Topmost = true;
        }
        public void Api_Login()
        {
            wb.Navigated += delegate
            {
                wb.Document.GetElementById("title_0").InnerText = "登录到 Lemon App";
                loading.Visibility = Visibility.Collapsed;
                wf.Visibility = Visibility.Visible;
            };
            wb.Navigate(new Uri("https://xui.ptlogin2.qq.com/cgi-bin/xlogin?appid=8000201&hide_border=1&style=33&theme=2&s_url=https%3A%2F%2Fvip.qq.com&daid=18&low_login=1&hln_autologin=e=2&login_text=%E6%8E%88%E6%9D%83%E5%B9%B6%E7%99%BB%E5%BD%95"));
            Activate();
            wb.DocumentTitleChanged += Wb_Dc_Login;
        }

        private async void Wb_Dc_Login(object sender, EventArgs e)
        {
            if (wb.Url.Host == "vip.qq.com")
            {
                await Task.Delay(500);
                //--------------暴露g_skey Cookies----------------------
                var cookie = wb.Document.Cookie;
                //-------------------------------------------------
                Console.WriteLine(cookie, "LoginData");
                string qq = TextHelper.FindTextByAB(cookie, "uin=o", ";", 0);
                LoginData send = new LoginData()
                {
                    qq = qq,
                    cookie = cookie
                };
                //-------------------------------------------------
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
                //---------------------------------------------------------
                mw.Dispatcher.Invoke(delegate { mw.Login(send); });
                wb.DocumentCompleted -= Wb_Dc_Login;
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
    }
}
