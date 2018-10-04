using LemonLibrary;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoconutApi
{
    public class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == MsgHelper.WM_COPYDATA)
            {
                MsgHelper.COPYDATASTRUCT cdata = new MsgHelper.COPYDATASTRUCT();
                cdata = (MsgHelper.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, cdata.GetType());
                if (cdata.lpData == "IsLogin")
                    Api_IsLogin();
                else if (cdata.lpData == "Login")
                    Api_Login();
            }
            else base.DefWndProc(ref m);
        }
        #region Api_IsLogin
        private void Api_IsLogin()
        {
            wb.Navigate("https://y.qq.com/portal/profile.html");
            wb.DocumentCompleted += Wb_Dc_Api_IsLogin;
        }

        int wind = 0;
        private async void Wb_Dc_Api_IsLogin(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            await Task.Delay(1000);
            bool isfind = false;
            foreach (HtmlElement ele in wb.Document.All)
            {
                if (ele.InnerText == "立即登录")
                {
                    isfind = true;
                    MsgHelper.SendMsg("No Login", wind);
                    break;
                }
            }
            if (!isfind)
            {
                string qq = TextHelper.XtoYGetTo(wb.Document.Cookie, "pt2gguin=o", "; sajssdk", 0);
                MsgHelper.SendMsg("Login:" + qq + "###", wind);
            }
            wb.DocumentCompleted -= Wb_Dc_Api_IsLogin;
        }
        #endregion
        private void Api_Login()
        {
            wb.Navigate("https://xui.ptlogin2.qq.com/cgi-bin/xlogin?daid=384&pt_no_auth=1&style=40&appid=1006102&s_url=https%3A%2F%2Fy.qq.com%2F%23stat%3Dy_new.top.pop.logout&low_login=1&hln_css=&hln_title=&hln_acc=&hln_pwd=&hln_u_tips=&hln_p_tips=&hln_autologin=&hln_login=&hln_otheracc=&hide_close_icon=1&hln_qloginacc=&hln_reg=&hln_vctitle=&hln_verifycode=&hln_vclogin=&hln_feedback=");
            Opacity = 1;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            wb.DocumentCompleted += Wb_Dc_Login;
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            Opacity = 0;
            FormBorderStyle = FormBorderStyle.None;
            e.Cancel = true;
        }

        private async void Wb_Dc_Login(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (wb.DocumentTitle.Contains("QQ音乐"))
            {
                FormBorderStyle = FormBorderStyle.None;
                Opacity = 0;
                await Task.Delay(1000);
                Api_IsLogin();
                wb.DocumentCompleted -= Wb_Dc_Login;
            }
        }

        private WebBrowser wb;
        private void InitializeComponent()
        {
            this.wb = new System.Windows.Forms.WebBrowser();
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
            // MainWindow
            // 
            this.ClientSize = new System.Drawing.Size(530, 345);
            this.Controls.Add(this.wb);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainWindow";
            this.Opacity = 0;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.ResumeLayout(false);

        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            wind = MsgHelper.FindWindow(null, "LemonApp").ToInt32();
            MsgHelper.SendMsg("Api#" + this.Handle.ToInt32() + "*", wind);
        }
    }
}
