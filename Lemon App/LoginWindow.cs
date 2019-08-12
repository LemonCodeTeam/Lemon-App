using LemonLibrary;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lemon_App
{
    public class LoginWindow : Form
    {
        private MainWindow mw;
        public LoginWindow(MainWindow m)
        {
            InitializeComponent();
            wb.Navigated += delegate { textBox1.Text = wb.Url.AbsoluteUri; };
            mw = m;
        }
        public void Api_Login()
        {
            wb.Navigate("https://xui.ptlogin2.qq.com/cgi-bin/xlogin?daid=384&pt_no_auth=1&style=40&appid=1006102&s_url=https%3A%2F%2Fy.qq.com%2Fn%2Fyqq%2Fsong%2F000edOaL1WZOWq.html%23stat%3Dy_new.top.pop.logout&low_login=1&hln_css=&hln_title=&hln_acc=&hln_pwd=&hln_u_tips=&hln_p_tips=&hln_autologin=&hln_login=&hln_otheracc=&hide_close_icon=1&hln_qloginacc=&hln_reg=&hln_vctitle=&hln_verifycode=&hln_vclogin=&hln_feedback=");
            Opacity = 1;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            TopMost = true;
            Activate();
            wb.DocumentCompleted += Wb_Dc_Login;
        }

        private async void Wb_Dc_Login(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (wb.DocumentTitle.Contains("QQ音乐"))
            {
                FormBorderStyle = FormBorderStyle.None;
                Opacity = 0;
                TopMost = false;
                await Task.Delay(100);
                string cookie = wb.Document.Cookie;
                string qq = TextHelper.XtoYGetTo(cookie, "p_luin=o", ";", 0);
                string send = "Login:" + qq + "### 呱呱呱 Cookie[" + cookie + "]END";
                if (cookie.Contains("p_skey=")) {
                    string p_skey = TextHelper.XtoYGetTo(cookie+";", "p_skey=", ";", 0);
                    long hash = 5381;
                    for (int i = 0; i < p_skey.Length; i++)
                    {
                        hash += (hash << 5) + p_skey[i];
                    }
                    long g_tk = hash & 0x7fffffff;
                    send = "Login:" + qq + "### 呱呱呱 Cookie[" + cookie + "]END  叽里咕噜 g_tk["+g_tk+"]sk";
                }
                mw.Login(send);
                wb.DocumentCompleted -= Wb_Dc_Login;
            }
        }



        private WebBrowser wb;
        private Button button1;
        private TextBox textBox1;
        private Button button2;
        private void InitializeComponent()
        {
            this.wb = new System.Windows.Forms.WebBrowser();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // wb
            // 
            this.wb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wb.Location = new System.Drawing.Point(0, 0);
            this.wb.MinimumSize = new System.Drawing.Size(20, 20);
            this.wb.Name = "wb";
            this.wb.Size = new System.Drawing.Size(530, 345);
            this.wb.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 21);
            this.textBox1.TabIndex = 0;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(0, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 0;
            // 
            // MainWindow
            // 
            this.ClientSize = new System.Drawing.Size(530, 345);
            this.Controls.Add(this.wb);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainWindow";
            this.Opacity = 0D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }
        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(wb.Document.Cookie);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            wb.Navigate(textBox1.Text);
        }
    }
}
