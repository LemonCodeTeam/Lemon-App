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
        private WebBrowser wb;
        private Action<LoginData> LoginCallBack;
        public LoginWindow(Action<LoginData> loginCallBack)
        {
            InitializeComponent();
            wb = new WebBrowser();
            wf.Child = wb;
            LoginCallBack = loginCallBack;
        }
        public void Api_Login()
        {
            wb.Navigated += delegate
            {
                loading.Visibility = Visibility.Collapsed;
                wf.Visibility = Visibility.Visible;
            };
            wb.Navigate(new Uri("https://graph.qq.com/oauth2.0/show?which=Login&display=pc&response_type=code&client_id=100497308&redirect_uri=https%3A%2F%2Fy.qq.com%2Fportal%2Fwx_redirect.html%3Flogin_type%3D1%26surl%3Dhttps%3A%2F%2Fy.qq.com%2Fn%2Fryqq%2Fprofile&state=state&display=pc&scope=get_user_info"));
            Activate();
            wb.DocumentTitleChanged += Wb_Dc_Login;
        }

        private async void Wb_Dc_Login(object sender, EventArgs e)
        {
            if (wb.Url.ToString()== "https://y.qq.com/n/ryqq/profile")
            {
                await Task.Delay(500);
                //--------------暴露g_skey Cookies----------------------
                var cookie = wb.Document.Cookie;
                //-------------------------------------------------
                Console.WriteLine(cookie, "LoginData");
                string qq = TextHelper.FindTextByAB(cookie, "luin=o", ";", 0);
                LoginData send = new LoginData()
                {
                    qq = qq,
                    cookie = cookie
                };
                //-------------------------------------------------
                if (cookie.Contains("qqmusic_key="))
                {
                    string p_skey = TextHelper.FindTextByAB(cookie + ";", "qqmusic_key=", ";", 0);
                    long hash = 5381;
                    for (int i = 0; i < p_skey.Length; i++)
                    {
                        hash += (hash << 5) + p_skey[i];
                    }
                    long g_tk =  2147483647 & hash;
                    send.g_tk = g_tk.ToString();
                }
                Console.WriteLine(send.g_tk, "LOGIN G_TK");
                //---------------------------------------------------------
                LoginCallBack(send);
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
