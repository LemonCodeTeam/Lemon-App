using LemonLibrary;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media;

namespace MatchaPlayer
{
    public class MainWindow : Form
    {
        Timer t = new Timer();
        public MainWindow(int handle)
        {
            InitializeComponent();
            wind = handle;
            Console.WriteLine(wind);
            MsgHelper.SendMsg("ok["+Handle.ToInt32()+"]", wind);
            mp.MediaEnded += (s, e) => {
                MsgHelper.SendMsg("MediaEnded", wind);
            };
            t.Interval = 1000;
            t.Tick += delegate {
                if(Process.GetProcessesByName("Lemon App").Length==0) {
                    mp.Stop();
                    mp.Close();
                    Environment.Exit(0);
                }
            };
            t.Start();
        }
        private MediaPlayer mp = new MediaPlayer();
        private bool IsToed = false;
        private double LastToValue = 0;
        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == MsgHelper.WM_COPYDATA)
            {
                MsgHelper.COPYDATASTRUCT cdata = new MsgHelper.COPYDATASTRUCT();
                cdata = (MsgHelper.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, cdata.GetType());
                string dt = cdata.lpData;
                Console.WriteLine(dt);
                if (dt.Contains("Open"))
                {
                    string url = TextHelper.XtoYGetTo(dt, "Open[", "]", 0);
                    mp.Open(new Uri(url, UriKind.Absolute));
                }
                else if (dt=="Play")
                {
                    mp.Play();
                }
                else if (dt=="Pause")
                {
                    mp.Pause();
                }
                else if (dt.Contains("To"))
                {
                    IsToed = true;
                    LastToValue = double.Parse(TextHelper.XtoYGetTo(dt, "To[", "]", 0));
                }
                else if (dt=="Get")
                {
                    MsgHelper.SendMsg("Ps[" + mp.Position.TotalMilliseconds + "]", wind);
                    if (IsToed) {
                        mp.Position = TimeSpan.FromMilliseconds(LastToValue);
                        IsToed = false;
                        MsgHelper.SendMsg("ToAway", wind);
                    }
                }
                else if (dt=="GetAll")
                {
                    if(mp.NaturalDuration.HasTimeSpan)
                        MsgHelper.SendMsg("PsAll[" + mp.NaturalDuration.TimeSpan.TotalMilliseconds + "]", wind);
                }
                else if (dt=="Exit")
                {
                    mp.Stop();
                    mp.Close();
                    Environment.Exit(0);
                }
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
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "MainWindow";
            this.Text = "(*/ω＼*) LemonApp Music Player";
            this.Opacity = 0D;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }

        int wind = 0;
    }
}
