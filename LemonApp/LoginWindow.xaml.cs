using LemonLib;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

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
            wb.Navigate("https://xui.ptlogin2.qq.com/cgi-bin/xlogin?daid=384&pt_no_auth=1&style=40&hide_border=1&appid=1006102&s_url=https%3A%2F%2Fy.qq.com%2Fn%2Fyqq%2Fsong%2F000edOaL1WZOWq.html%23stat%3Dy_new.top.pop.logout&low_login=1&hln_css=&hln_title=&hln_acc=&hln_pwd=&hln_u_tips=&hln_p_tips=&hln_autologin=&hln_login=&hln_otheracc=&hide_close_icon=1&hln_qloginacc=&hln_reg=&hln_vctitle=&hln_verifycode=&hln_vclogin=&hln_feedback=");
            Topmost = true;
            Activate();
            wb.DocumentCompleted += Wb_Dc_Login;
        }

        private async void Wb_Dc_Login(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (wb.DocumentTitle.Contains("QQ音乐"))
            {

                Topmost = false;
                await Task.Delay(100);
                string cookie = wb.Document.Cookie;
                string qq = TextHelper.XtoYGetTo(cookie, "p_luin=o", ";", 0);
                string send = "Login:" + qq + "### 呱呱呱 Cookie[" + cookie + "]END";
                if (cookie.Contains("p_skey="))
                {
                    string p_skey = TextHelper.XtoYGetTo(cookie + ";", "p_skey=", ";", 0);
                    long hash = 5381;
                    for (int i = 0; i < p_skey.Length; i++)
                    {
                        hash += (hash << 5) + p_skey[i];
                    }
                    long g_tk = hash & 0x7fffffff;
                    send = "Login:" + qq + "### 呱呱呱 Cookie[" + cookie + "]END  叽里咕噜 g_tk[" + g_tk + "]sk";
                }
                mw.Dispatcher.Invoke(delegate { mw.Login(send); });
                wb.DocumentCompleted -= Wb_Dc_Login;
            }
            else
            {
                loading.Visibility = Visibility.Collapsed;
                wf.Visibility = Visibility.Visible;
                wb.Document.GetElementById("title_0").InnerText = "登录到 Lemon App";
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
