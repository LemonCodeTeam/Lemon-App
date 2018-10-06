using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;

namespace LemonLibrary.Helpers
{
    public class PlayerControl:Form
    {
        public PlayerControl()
        {
            InitializeComponent();
            Show();
        }
        private Process p;
        private int pHandle;
        private double Ps = 0;
        private double PsAll = 0;

        public delegate void FoxHandle();
        public event FoxHandle MediaEnded;
        public event FoxHandle ToAway;
        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == MsgHelper.WM_COPYDATA)
            {
                MsgHelper.COPYDATASTRUCT cdata = new MsgHelper.COPYDATASTRUCT();
                cdata = (MsgHelper.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, cdata.GetType());
                string dt = cdata.lpData;
                if(dt.Contains("ok"))
                    pHandle= int.Parse(TextHelper.XtoYGetTo(dt, "ok[", "]", 0));
                else if (dt.Contains("Ps["))
                    Ps = double.Parse(TextHelper.XtoYGetTo(dt, "Ps[", "]", 0));
                else if (dt.Contains("PsAll["))
                    PsAll = double.Parse(TextHelper.XtoYGetTo(dt, "PsAll[", "]", 0));
                else if (dt == "MediaEnded")
                    MediaEnded.Invoke();
                else if (dt == "ToAway")
                    ToAway.Invoke();
            }
            else base.DefWndProc(ref m);
        }
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainWindow
            // 
            this.ClientSize = new System.Drawing.Size(530, 345);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MainWindow";
            this.Opacity = 0D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.ResumeLayout(false);

        }        
        private void MainWindow_Load(object sender, EventArgs e)
        {
            p =Process.Start("MatchaPlayer", Handle.ToInt32().ToString());
        }

        public void Open(string url) {
            MsgHelper.SendMsg("Open[" + url + "]", pHandle);
        }
        public void Play()
        {
            MsgHelper.SendMsg("Play", pHandle);
        }
        public void Pause()
        {
            MsgHelper.SendMsg("Pause", pHandle);
        }
        public void To(double Milliseconds)
        {
            MsgHelper.SendMsg("To[" + Milliseconds + "]", pHandle);
        }
        public async Task<double> Get()
        {
            MsgHelper.SendMsg("Get", pHandle);
            await Task.Delay(10);
            return Ps;
        }
        public async Task<double> GetAll()
        {
            MsgHelper.SendMsg("GetAll", pHandle);
            await Task.Delay(10);
            return PsAll;
        }
        public void Exit()
        {
            MsgHelper.SendMsg("Exit", pHandle);
        }
    }
}
