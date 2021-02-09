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
        public LoginWindow(MainWindow m)
        {
            InitializeComponent();
            mw = m;
        }
        public void Api_Login()
        {
            wb.Navigate("https://xui.ptlogin2.qq.com/cgi-bin/xlogin?proxy_url=https%3A//qzs.qq.com&daid=5&&hide_title_bar=1&low_login=0&qlogin_auto_login=1&no_verifyimg=1&link_target=blank&appid=549000912&style=33&theme=2&target=self&s_url=https%3A%2F%2Fqzs.qq.com%2Fqzone%2Fv5%2Floginsucc.html%3Fpara%3Dizone&hide_border=1");
            Topmost = true;
            Activate();
            wb.Navigated+= Wb_Dc_Login;
        }

        private void Wb_Dc_Login(object sender, EventArgs e)
        {
            if (wb.Url.ToString().Contains("user.qzone.qq.com"))
            {
                Topmost = false;
                wb.Stop();
                string cookie = wb.Document.Cookie;
                string qq = TextHelper.FindTextByAB(cookie, " p_uin=o", ";", 0);
                LoginData send = new LoginData(){
                    qq = qq,
                    cookie = cookie
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
                mw.Dispatcher.Invoke(delegate { mw.Login(send); });
                wb.DocumentCompleted -= Wb_Dc_Login;
            }
            else
            {
                wb.Document.GetElementById("title_0").InnerText = "登录到 Lemon App";
                loading.Visibility = Visibility.Collapsed;
                wf.Visibility = Visibility.Visible;
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
            wb.ScriptErrorsSuppressed = true;
            Api_Login();
        }
    }
}
