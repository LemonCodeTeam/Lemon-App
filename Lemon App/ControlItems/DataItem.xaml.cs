using LemonLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static LemonLibrary.InfoHelper;

namespace Lemon_App
{
    /// <summary>
    /// DataItem.xaml 的交互逻辑
    /// </summary>
    public partial class DataItem : UserControl
    {
        public delegate void MouseDownHandle(object sender, MouseButtonEventArgs e);
        public event MouseDownHandle Play;
        public event MouseDownHandle Add;
        public event MouseDownHandle Download;


        public string ID { set; get; }
        public string SongName { set; get; }
        public string Singer { set; get; }
        public string Image { set; get; }
        public Music music;
        public DataItem(Music dat)
        {
            try
            {
                InitializeComponent();
                music = dat;
                ID = dat.MusicID;
                SongName = dat.MusicName;
                Singer = dat.Singer;
                Image = dat.ImageUrl;
                name.Text = SongName;
                ser.Text = Singer;
                mss.Text = dat.MusicName_Lyric;
            }
            catch { }
        }
        bool ns = false;
        public bool isChecked = false;
        public void ShowDx() {
            if (He.LastItem != null)
            {
                mss.Opacity = 0.6;
                ser.Opacity = 0.8;
                He.LastItem.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                He.LastItem.namss.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                He.LastItem.ser.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                He.LastItem.color.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0));
            ser.SetResourceReference(ForegroundProperty, "ThemeColor");
            namss.SetResourceReference(ForegroundProperty, "ThemeColor");
            color.SetResourceReference(BackgroundProperty, "ThemeColor");
            mss.Opacity = 1;
            ser.Opacity = 1;

            He.LastItem = this;
        }
        public bool isPlay(string name) {
            if (this.name.Text == name)
                return true;
            else return false;
        }
        public void NSDownload(bool ns) {
            this.ns = ns;
            if (ns)
            {
                namss.Margin = new System.Windows.Thickness(60, 0, 10, 0);
                CheckView.Visibility = System.Windows.Visibility.Visible;
                MouseDown += CheckView_MouseDown;
            }
            else {
                namss.Margin = new System.Windows.Thickness(10, 0, 10, 0);
                CheckView.Visibility = System.Windows.Visibility.Collapsed;
                MouseDown -= CheckView_MouseDown;
            }
        }

        private void CheckView_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Check();
        }

        public void Check() {
            if (!isChecked)
            {
                GO.Visibility = System.Windows.Visibility.Visible;
                CheckView.SetResourceReference(BorderBrushProperty, "ThemeColor");
                isChecked = true;
            }
            else {
                GO.Visibility = System.Windows.Visibility.Collapsed;
                CheckView.SetResourceReference(BorderBrushProperty, "TextX1ColorBrush");
                isChecked = false;
            }
        }

        private void PlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Play(this, e);
            if (!ns)
                ShowDx();
        }
        private Dictionary<string, string> ListData = new Dictionary<string, string>();//name,id
        private async void AddBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Add_Gdlist.Children.Clear();
            JObject o = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/splcloud/fcgi-bin/songlist_list.fcg?utf8=1&-=MusicJsonCallBack&uin={Settings.USettings.LemonAreeunIts}&rnd=0.693477705380313&g_tk=1803226462&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0", null,
                "yqq_stat=0; pgv_info=ssid=s9030869079; ts_last=y.qq.com/; pgv_pvid=6655025332; ts_uid=7057611058; pgv_pvi=8567083008; pgv_si=s9362499584; _qpsvr_localtk=0.4296063820147471; uin=o2728578956; skey=@uvgfbeYR4; ptisp=cm; RK=sKKMfg2M0M; ptcz=7b132d6e799e806b8e425c58966902006b53fc86e1ecb0d2678db33d44f7239d; luin=o2728578956; lskey=000100007afb4fb9a74df33c89945350f16ebce7ef0faca9f0da0aa18246d750b53e68077ae6f83ba3901761; p_uin=o2728578956; pt4_token=-HKvK3MM2TlBjBsPDdO*3Iw6shscAWOJEz-pf5eTH2g_; p_skey=rW8tk6zH9QhmEIOjVvvBXdIeyFQbGGi2xnYiT8f2Ioo_; p_luin=o2728578956; p_lskey=000400008417d0c8d018dff398f9512dcee49d8e75aeae7d975e77c39a169d1e17a25b0dfecfb63bebf56cba; ts_refer=xui.ptlogin2.qq.com/cgi-bin/xlogin"));
            foreach (var a in o["list"]) {
                string name = a["dirname"].ToString();
                ListData.Add(name, a["dirid"].ToString());
                var mdb = new MDButton() { TName = name,Margin=new System.Windows.Thickness(0, 10, 0, 0) };
                mdb.MouseDown += Mdb_MouseDown;
                Add_Gdlist.Children.Add(mdb);
            }
            Gdpop.IsOpen = true;
            Add(this, e);
        }

        private void Mdb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Gdpop.IsOpen = false;
            string name = (sender as MDButton).TName;
            string id = ListData[name];
            var header = new WebHeaderCollection();
            header.Add(HttpRequestHeader.Accept, "*/*");
            header.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9");
            header.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8");
            header.Add(HttpRequestHeader.Cookie, "pgv_pvi=2664902656; RK=5KKMai2UwO; ptcz=bb12287513538861cab0c6bb27b34e8319763d96415cc0ac1bbaaccf627c7e34; pgv_pvid=2128817280; ts_uid=3468067346; tvfe_boss_uuid=e53d7e2db72f04a4; o_cookie=2728578956; ts_refer=xui.ptlogin2.qq.com/cgi-bin/xlogin; yq_index=0; pgv_si=s8161463296; _qpsvr_localtk=0.5790436313333647; ptisp=cm; pgv_info=ssid=s1625262774; uin=o2728578956; skey=@YkWJWMuMT; luin=o2728578956; lskey=00010000def5b4c5e09cd29ff523925acaa692d2a08eccf27b2a1da71eb02ae7f83da1e8625a8c173dfa9c35; p_uin=o2728578956; pt4_token=B19wvfTueaeuFZjzD2FckuQ-s4SDBPl-T9YdwFvGufg_; p_skey=XZNHdIp*r9EWDz*qseJhcNrOpIEmuhqA*1EMlN6kolk_; p_luin=o2728578956; p_lskey=000400005748931815ffe7d6baa313c5f4961a1f18a5fb9752b6259d2e7901b3ef0756fc66cde4f481d37c11; yqq_stat=0; ts_last=y.qq.com/n/yqq/playlist/2591355982.html");
            header.Add(HttpRequestHeader.Referer, "https://y.qq.com/n/yqq/singer/0020PeOh4ZaCw1.html");
            header.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
            header.Add(HttpRequestHeader.Host, "c.y.qq.com");
            string result=HttpHelper.PostWeb("https://c.y.qq.com/splcloud/fcgi-bin/fcg_music_add2songdir.fcg?g_tk=1864465719",
                $"loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.post&needNewCode=0&uin={Settings.USettings.LemonAreeunIts}&midlist={ID}&typelist=13&dirid={id}&addtype=&formsender=4&source=153&r2=0&r3=1&utf8=1&g_tk=1864465719",header);
            System.Windows.Forms.MessageBox.Show(result);
            foreach (var a in Settings.USettings.MusicGD) {
                if (a.Value.name.Contains(name)) {
                    Settings.USettings.MusicGD[a.Key].Data.Insert(0,music);
                }
            }
        }

        private void DownloadBtn_MouseDown(object sender, MouseButtonEventArgs e) => Download(this, e);
    }


    public class He
    {
        public static DataItem LastItem = null;
    }
}
