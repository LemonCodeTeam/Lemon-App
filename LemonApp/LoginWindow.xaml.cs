using LemonLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
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
            wb.ScriptErrorsSuppressed= true;
            wb.IsWebBrowserContextMenuEnabled = false;
            wb.WebBrowserShortcutsEnabled = false;
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
            wb.Navigate(new Uri("https://graph.qq.com/oauth2.0/show?which=Login&display=pc&response_type=code&client_id=100497308&redirect_uri=https%3A%2F%2Fy.qq.com%2Fportal%2Fwx_redirect.html%3Flogin_type%3D1%26surl%3Dhttps%3A%2F%2Fy.qq.com%2Fn%2Fryqq%2Fprofile&state=state&display=pc&scope=get_user_info%2Cget_app_friends"));
            Activate();
            wb.DocumentTitleChanged += Wb_Dc_Login;
        }
        string rCookie=null, rUrl=null;
        Dictionary<string, string> cookies = new Dictionary<string, string>();
        private async void Wb_Dc_Login(object sender, EventArgs e)
        {
            string url = wb.Url.ToString();
            var cookie = wb.Document.Cookie;
            foreach (string item in cookie.Split(';'))
            {
                string[] kv = item.Split('=');
                if (kv.Length == 2)
                {
                    var key = kv[0].Trim();
                    if(cookies.ContainsKey(key))
                        cookies[key] = kv[1].Trim();
                    else cookies.Add(key, kv[1].Trim());
                }
            }
            if (cookie.Contains("p_skey"))
                rCookie= cookie;
            if(url.Contains("https://y.qq.com/portal/wx_redirect.html")&&url.Contains("&code="))
                rUrl = url;
            if (rUrl != null && rCookie != null)
                Login();
        }
        private async void Login()
        {
            wb.DocumentTitleChanged -= Wb_Dc_Login;
            string g_tk = null;
            string l_code = TextHelper.FindTextByAB(rUrl, "&code=", "&", 0);
            //-------------------------------------------------
            string p_skey = cookies["p_skey"];
            long hash = 5381;
            for (int i = 0; i < p_skey.Length; i++)
            {
                hash += (hash << 5) + p_skey[i];
            }
            g_tk = (2147483647 & hash).ToString();
            //---------------------------------------------------------
            //POST music.fcg to Login:
            string postData = "{\"comm\":{\"g_tk\":" + g_tk + ",\"platform\":\"yqq\",\"ct\":24,\"cv\":0},\"req\":{\"module\":\"QQConnectLogin.LoginServer\",\"method\":\"QQLogin\",\"param\":{\"code\":\"" + l_code + "\"}}}";
            using var hc = new HttpClient(HttpHelper.GetSta());
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("AcceptLanguage", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            hc.DefaultRequestHeaders.Add("Referer", "https://y.qq.com/");
            hc.DefaultRequestHeaders.Host = "u.y.qq.com";
            hc.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");
            hc.DefaultRequestHeaders.Add("Cookie", rCookie);
            var result = await hc.PostAsync("https://u.y.qq.com/cgi-bin/musicu.fcg", new StringContent(postData, Encoding.UTF8));
            bool hasValue=result.Headers.TryGetValues("Set-Cookie", out var nc);
            if (hasValue) foreach (var item in nc)
                {
                    var temp = item.Split(';')[0].Split('=');
                    if (cookies.ContainsKey(temp[0]))
                        cookies[temp[0]] = temp[1];
                    else cookies.Add(temp[0], temp[1]);
                }
            string json = await result.Content.ReadAsStringAsync();
            JObject obj = JObject.Parse(json);
            string qq = cookies["uin"];
            //将dic转换为cookie字符串
            List<string> ck = new List<string>();
            foreach (var item in cookies)
            {
                ck.Add(item.Key + "=" + item.Value);
            }
            string cookie = string.Join("; ", ck);
            LoginData send = new LoginData()
            {
                qq = qq,
                cookie = cookie,
                g_tk = g_tk
            };
            LoginCallBack(send);
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
