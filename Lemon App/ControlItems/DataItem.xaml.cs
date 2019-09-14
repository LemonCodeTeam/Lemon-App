using LemonLibrary;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
        public delegate void MouseDownHandle(DataItem sender);
        public event MouseDownHandle Play;
        public event MouseDownHandle Download;
        public delegate void MouseDownHandle_sm(MusicSinger ms);
        public event MouseDownHandle_sm GetToSingerPage;

        public Music music;
        private MainWindow Mainwindow = null;
        private bool needb = false;

        private int BtnWidth = 0;
        public DataItem(Music dat, MainWindow mw,bool needDeleteBtn=false)
        {
            try
            {
                InitializeComponent();
                Mainwindow = mw;
                music = dat;
                Loaded += delegate {
                    ser.Inlines.Clear();
                    needb = needDeleteBtn;
                    name.Text = dat.MusicName;
                    if(dat.Album.Name!=null)
                    ab.Text = dat.Album.Name.Replace("空","");
                    foreach (MusicSinger a in dat.Singer) {
                        Ran r = new Ran() { Text=a.Name,data=a};
                        r.MouseDown += R_MouseDown;
                        ser.Inlines.Add(r);
                        ser.Inlines.Add(new Run(" / "));
                    }
                    ser.Inlines.Remove(ser.Inlines.LastInline);
                    mss.Text = dat.MusicName_Lyric;

                    int BtnCount = 0;
                    if (dat.Mvmid != "") { 
                        MV.Visibility = Visibility.Visible;
                        BtnCount++;
                    }
                    if (dat.Pz == "HQ") { 
                        HQ.Visibility = Visibility.Visible;
                        BtnCount++;
                    }
                    else if (dat.Pz == "SQ") { 
                        SQ.Visibility = Visibility.Visible;
                        BtnCount++;
                    }
                    BtnWidth = 32 * BtnCount + 5;
                    if (namss.ActualWidth > wpl.ActualWidth - BtnWidth)
                        namss.Width = wpl.ActualWidth - 101;
                };
            }
            catch { }
        }
        public DataItem(Music dat) {
            InitializeComponent();
            music = dat;
        }
        private void R_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ran r = sender as Ran;
            MusicSinger ms = r.data as MusicSinger;
            GetToSingerPage(ms);
        }

        bool ns = false;
        public bool isChecked = false;
        public void ShowDx() {
            if (He.LastItem != null)
            {
                mss.Opacity = 0.6;
                ser.Opacity = 0.8;
                He.LastItem.bg.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                He.LastItem.namss.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                He.LastItem.ser.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                He.LastItem.color.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
            bg.Background = new SolidColorBrush(Color.FromArgb(10, 0, 0, 0));
            ser.SetResourceReference(ForegroundProperty, "ThemeColor");
            namss.SetResourceReference(ForegroundProperty, "ThemeColor");
            color.SetResourceReference(BackgroundProperty, "ThemeColor");
            mss.Opacity = 1;
            ser.Opacity = 1;

            He.LastItem = this;
        }
        public void NSDownload(bool ns) {
            this.ns = ns;
            if (ns)
            {
                wpl.Margin = new System.Windows.Thickness(60, 0, 10, 0);
                CheckView.Visibility = System.Windows.Visibility.Visible;
                MouseDown += CheckView_MouseDown;
            }
            else {
                wpl.Margin = new System.Windows.Thickness(10, 0, 10, 0);
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
                GO.Visibility = Visibility.Visible;
                CheckView.SetResourceReference(BorderBrushProperty, "ThemeColor");
                isChecked = true;
            }
            else {
                GO.Visibility = Visibility.Collapsed;
                CheckView.SetResourceReference(BorderBrushProperty, "TextX1ColorBrush");
                isChecked = false;
            }
        }

        private void PlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Play(this);
        }
        private Dictionary<string, string> ListData = new Dictionary<string, string>();//name,id
        private async void AddBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Add_Gdlist.Items.Clear();
            ListData.Clear();
            JObject o = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/splcloud/fcgi-bin/songlist_list.fcg?utf8=1&-=MusicJsonCallBack&uin={Settings.USettings.LemonAreeunIts}&rnd=0.693477705380313&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0"));
            foreach (var a in o["list"]) {
                string name = a["dirname"].ToString();
                ListData.Add(name, a["dirid"].ToString());
                var mdb = new ListBoxItem { Background = new SolidColorBrush(Colors.Transparent), Height =30, Content = name,Margin=new Thickness(10, 10, 10, 0) };
                mdb.PreviewMouseDown += Mdb_MouseDown;
                Add_Gdlist.Items.Add(mdb);
            }
            var md = new ListBoxItem { Background = new SolidColorBrush(Colors.Transparent), Height = 30, Content = "取消", Margin = new Thickness(10, 10,10, 0) };
            md.PreviewMouseDown += delegate { Gdpop.IsOpen = false; };
            Add_Gdlist.Items.Add(md);
            Gdpop.IsOpen = true;
        }

        private void Mdb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Gdpop.IsOpen = false;
            string name = (sender as ListBoxItem).Content.ToString();
            string id = ListData[name];
            string[] a = MusicLib.AddMusicToGD(music.MusicID,id);
            Toast.Send(a[1]+": "+a[0]);
        }

        private void DownloadBtn_MouseDown(object sender, MouseButtonEventArgs e) => Download(this);

        private async void DeleteBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (TwMessageBox.Show("确定要删除此歌曲吗?"))
            {
                int index = He.MGData_Now.Data.IndexOf(music);
                string dirid = await MusicLib.GetGDdiridByNameAsync(He.MGData_Now.name);
                string Musicid = He.MGData_Now.ids[index];
                Mainwindow.DataItemsList.Items.RemoveAt(index);
                Toast.Send(MusicLib.DeleteMusicFromGD(Musicid, He.MGData_Now.id, dirid));
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Buttons.Visibility = Visibility.Visible;
            if (isChecked)
                wpl.Margin = new Thickness(60, 10, 80, 10);
            else wpl.Margin = new Thickness(10, 10, 80, 10);
            if (needb)DeleteBtn.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Buttons.Visibility = Visibility.Collapsed;
            DeleteBtn.Visibility = Visibility.Collapsed;
            if (isChecked)
                wpl.Margin = new Thickness(60, 10, 10, 10);
            else wpl.Margin = new Thickness(10, 10, 10, 10);
        }
        /// <summary>
        /// 没啥用，留着懒得改模板了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Datasv_ScrollChanged(object sender, ScrollChangedEventArgs e) { }

        private void Ab_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine(music.Album.ID);
            Mainwindow.IFVCALLBACK_LoadAlbum(music.Album.ID);
        }

        private void MV_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mainwindow .PlayMv(new MVData() {
                id=music.Mvmid,
                name=music.MusicName+" - "+music.SingerText
            });
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (namss.ActualWidth > wpl.ActualWidth - BtnWidth)
                namss.Width = wpl.ActualWidth - BtnWidth;
            else namss.Width = double.NaN;
        }
    }


    public class He
    {
        public static DataItem LastItem = null;
        public static MusicGData MGData_Now = null;
    }
}
