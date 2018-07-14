using LemonLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static LemonLibrary.InfoHelper;
/*
 TODO: 
    ?no Image.
    GD AutoResurt:
    .changer the download file :
            user cache: D:/Lemon App/Cache/User/username/music'pro'gd....
            music /download   changerst.
     */
namespace Lemon_App
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 一些字段
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
        MusicLib ml = new MusicLib();
        private System.Windows.Forms.NotifyIcon notifyIcon;
        DataItem MusicData;
        bool isplay = false;
        bool IsRadio = false;
        string RadioID = "";
        int ind = 0;//歌词页面是否打开
        bool xh = false;//false: lb true:dq  循环/单曲 播放控制
        bool issingerloaded = false;
        bool isPos = false;//true:Wy false:QQ 播放控制
        bool mod = true;//true : qq false : wy
        #endregion
        #region 等待动画
        public void OpenLoading()
        {
            var s = Resources["OpenLoadingFx"] as Storyboard;
            s.Completed += delegate { (Resources["FxLoading"] as Storyboard).Begin(); };
            s.Begin();
        }
        public void CloseLoading()
        {
            var s = Resources["CloseLoadingFx"] as Storyboard;
            s.Completed += delegate { (Resources["FxLoading"] as Storyboard).Stop(); };
            s.Begin();
        }
        #endregion
        #region 窗口加载辅助
        public MainWindow()
        {
            InitializeComponent();
            FullScreenManager.RepairWpfWindowFullScreenBehavior(this);
            if (Settings.USettings.skin == 0)
                (Resources["Skin"] as Storyboard).Begin();
            else
                Settings.USettings.skin = 0;
                (Resources["unSkin"] as Storyboard).Begin();
            Closed += delegate { notifyIcon.Visible = false; };
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            var ani = Resources["Loading"] as Storyboard;
            ani.Completed += Ani_Completed;
            ani.Begin();
        }

        private void Ani_Completed(object sender, EventArgs e)
        {
            OpenLoading();
            IntPtr handle = new WindowInteropHelper(this).Handle;
            RegisterHotKey(handle, 124, 4, (uint)System.Windows.Forms.Keys.L);
            RegisterHotKey(handle, 125, 4, (uint)System.Windows.Forms.Keys.S);
            InstallHotKeyHook(this);
            //notifyIcon
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = "小萌";
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            notifyIcon.Visible = true;
            //打开菜单项
            System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("打开");
            open.Click += delegate { exShow(); };
            //退出菜单项
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("关闭");
            exit.Click += delegate {
                var dt = Resources["Closing"] as Storyboard;
                dt.Completed += delegate { Settings.SaveSettings(); Environment.Exit(0); };
                dt.Begin();
            };
            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { open, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

            notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, m) =>
            {
                if (m.Button == System.Windows.Forms.MouseButtons.Left) exShow();
            });
            /////Timer user
            var ds = new System.Windows.Forms.Timer() { Interval = 2000 };
            ds.Tick += delegate { GC.Collect(); UIHelper.G(Page); };
            ds.Start();
            if (System.IO.File.Exists(Settings.USettings.UserImage))
            {
                var image = new System.Drawing.Bitmap(Settings.USettings.UserImage);
                UserTX.Background = new ImageBrush(image.ToImageSource());
            }
                (Resources["Closing"] as Storyboard).Completed += delegate { ShowInTaskbar = false; };
            ////////////load
            LyricView lv = new LyricView();
            lv.FoucsLrcColor = new SolidColorBrush(Color.FromRgb(78, 183, 251));
            lv.NoramlLrcColor = new SolidColorBrush(Color.FromRgb(254, 254, 254));
            lv.TextAlignment = TextAlignment.Left;
            ly.Child = lv;
            ml = new MusicLib(lv);
            if (Settings.USettings.Playing.MusicName != "")
            {
                PlayMusic(Settings.USettings.Playing.MusicID, Settings.USettings.Playing.ImageUrl, Settings.USettings.Playing.MusicName, Settings.USettings.Playing.Singer, false, false);
                jd.Maximum = Settings.USettings.alljd;
                jd.Value = Settings.USettings.jd;
                ml.m.Position = TimeSpan.FromMilliseconds(Settings.USettings.jd);
            }
            t.Interval = 500;
            t.Tick += delegate
            {
                try
                {
                    jd.Maximum = ml.m.NaturalDuration.TimeSpan.TotalMilliseconds;
                    jd.Value = ml.m.Position.TotalMilliseconds;
                    if (ind == 1)
                        ml.lv.LrcRoll(ml.m.Position.TotalMilliseconds);
                    Settings.USettings.alljd = jd.Maximum;
                    Settings.USettings.jd = jd.Value;
                }
                catch { }
            };
            ml.m.MediaEnded += delegate
            {
                t.Stop();
                jd.Value = 0;
                if (xh)
                    if (IsRadio)
                        PlayMusic(RadioData.MusicID, RadioData.ImageUrl, RadioData.MusicName, RadioData.Singer, true);
                    else PlayMusic(MusicData, null);
                else
                {
                    if (IsRadio)
                        GetRadio(new RadioItem(RadioID), null);
                    else
                        PlayMusic(DataItemsList.Children[DataItemsList.Children.IndexOf(MusicData) + 1] as DataItem, null);
                }
            };
            /////top////
            var de = new Task(new Action(async delegate
            {
                var dt = await ml.GetTopIndexAsync();

                foreach (var d in dt)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var top = new TopControl(d.ID, d.Photo, d.Name);
                        top.MouseDown += delegate (object seb, MouseButtonEventArgs ed)
                        {
                            OpenLoading();
                            var g = seb as TopControl;
                            (Resources["ClickTop"] as Storyboard).Begin();
                            var file = InfoHelper.GetPath() + "Cache\\Top" + g.topID + ".jpg";
                            if (!System.IO.File.Exists(file))
                            {
                                var s = new WebClient();
                                s.DownloadFileAsync(new Uri(g.pic), file);
                                s.DownloadFileCompleted += delegate { TXx.Background = new ImageBrush(new BitmapImage(new Uri(file, UriKind.Relative))); };
                            }
                            else TXx.Background = new ImageBrush(new BitmapImage(new Uri(file, UriKind.Relative)));
                            TB.Text = g.name;
                            var ss = new Task(new Action(async delegate
                            {
                                var dta = await ml.GetToplistAsync(int.Parse(g.topID));
                                Dispatcher.Invoke(() => { DataItemsList.Children.Clear(); });
                                foreach (var j in dta)
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        var k = new DataItem(j.MusicID, j.MusicName, j.Singer, j.ImageUrl) { Margin = new Thickness(20, 0, 0, 20) };
                                        k.MouseDown += PlayMusic;
                                        DataItemsList.Children.Add(k);
                                    });
                                    await Task.Delay(1);
                                }
                                Dispatcher.Invoke(() => { CloseLoading(); });
                            }));
                            ss.Start();
                        };
                        top.Margin = new Thickness(0, 0, 20, 20);
                        topIndexList.Children.Add(top);
                    });
                }
                Dispatcher.Invoke(() => { CloseLoading(); });
            }));
            de.Start();
            CloseLoading();
        }

        private void exShow() {
            try
            {
                this.WindowState = WindowState.Normal;
                var ani = Resources["Loading"] as Storyboard;
                ani.Completed -= Ani_Completed;
                ani.Begin();
                this.Activate();
            }
            catch { }
        }
        private void BigBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                c.ResizeBorderThickness = new Thickness(0);
                Page.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(0), TimeSpan.FromSeconds(0)));
                WindowState = WindowState.Maximized;
                Page.Clip = new RectangleGeometry() { RadiusX = 0, RadiusY = 0, Rect = new Rect() { Width = Page.ActualWidth, Height = Page.ActualHeight } };
            }
            else
            {
                c.ResizeBorderThickness = new Thickness(30);
                Page.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(30), TimeSpan.FromSeconds(0)));
                WindowState = WindowState.Normal;
                Page.Clip = new RectangleGeometry() { RadiusX = 5, RadiusY = 5, Rect = new Rect() { Width = Page.ActualWidth, Height = Page.ActualHeight } };
            }
        }
        private void SmallBtn_MouseDown(object sender, MouseButtonEventArgs e) { ShowInTaskbar = true; WindowState = WindowState.Minimized; }
        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Page.Clip = new RectangleGeometry() { RadiusX = 5, RadiusY = 5, Rect = new Rect() { Width = Page.ActualWidth, Height = Page.ActualHeight } };
        }
        #endregion
        #region 主题切换
        private void ControlPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Settings.USettings.skin == 0)
            {
                Settings.USettings.skin = 1;
                (Resources["Skin"] as Storyboard).Begin();
            }
            else
            {
                Settings.USettings.skin = 0;
                (Resources["unSkin"] as Storyboard).Begin();
            }
        }
        #endregion
        #region 功能区
        #region Singer
        string SingerKey1 = "all_all_";
        string SingerKey2 = "all";
        private void SingerPageChecked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                OpenLoading();
                SingerKey1 = (sender as RadioButton).Uid;
                string sk = SingerKey1 + SingerKey2;
                if (sk == "all")
                    sk = "all_all_all";
                var s = new Task(new Action(async delegate
                {
                    var sin = await ml.GetSingerAsync(sk);
                    Dispatcher.Invoke(() => { singerItemsList.Children.Clear(); });
                    foreach (var d in sin)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var sinx = new SingerItem(d.Photo, d.Name) { Margin = new Thickness(20, 0, 0, 20) };
                            sinx.MouseDown += GetSinger;
                            singerItemsList.Children.Add(sinx);
                        });
                        await Task.Delay(1);
                    }
                    Dispatcher.Invoke(() => { CloseLoading(); });
                }));
                s.Start();
            }
        }
        public void GetSinger(object sender, MouseEventArgs e)
        {
            SearchMusic((sender as SingerItem).singer);
        }
        private void SIngerPageChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                {
                    OpenLoading();
                    if (SingerKey1 == "")
                        SingerKey1 = "all_all_";
                    SingerKey2 = (sender as RadioButton).Content.ToString().Replace("热门", "all").Replace("#", "9");
                    var s = new Task(new Action(async delegate
                    {
                        var mx = await ml.GetSingerAsync(SingerKey1 + SingerKey2);
                        Dispatcher.Invoke(() => { singerItemsList.Children.Clear(); });
                        foreach (var d in mx)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                var sinx = new SingerItem(d.Photo, d.Name) { Margin = new Thickness(20, 0, 0, 20) };
                                sinx.MouseDown += GetSinger;
                                singerItemsList.Children.Add(sinx);
                            });
                            await Task.Delay(1);
                        }
                        Dispatcher.Invoke(() => { CloseLoading(); });
                    }));
                    s.Start();
                }
            }
            catch { }
        }
        private void SingerBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!issingerloaded)
            {
                issingerloaded = true;
                foreach (var c in Singerws.Children)
                    (c as RadioButton).Checked += SingerPageChecked;
                foreach (var c in Singersx.Children)
                    (c as RadioButton).Checked += SIngerPageChecked;

                OpenLoading();
                var s = new Task(new Action(async delegate
                {
                    var mx = await ml.GetSingerAsync("all_all_all");
                    Dispatcher.Invoke(() => { singerItemsList.Children.Clear(); });
                    foreach (var d in mx)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var sinx = new SingerItem(d.Photo, d.Name) { Margin = new Thickness(20, 0, 0, 20) };
                            sinx.MouseDown += GetSinger;
                            singerItemsList.Children.Add(sinx);
                        });
                        await Task.Delay(1);
                    }
                    Dispatcher.Invoke(() => { CloseLoading(); });
                }));
                s.Start();
            }
        }
        #endregion
        #region FLGD
        private void ZJBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (FLGDIndexList.Children.Count == 0)
            {
                var sinx = new Task(new Action(async delegate
                {
                    Dispatcher.Invoke(() => { OpenLoading(); });
                    var wk = await ml.GetFLGDIndexAsync();
                    Dispatcher.Invoke(() => { FLGDIndexList.Children.Add(new RadioButton() { Content = wk.Hot[0].name, Uid = wk.Hot[0].id, Margin = new Thickness(0, 0, 30, 10) }); });
                    Dispatcher.Invoke(() => { FLGDIndexList.Children.Add(new TextBlock() { Text = "语种:" }); });
                    foreach (var d in wk.Lauch)
                    {
                        Dispatcher.Invoke(() => { FLGDIndexList.Children.Add(new RadioButton() { Content = d.name, Uid = d.id, Margin = new Thickness(0, 0, 10, 10) }); });
                    }
                    Dispatcher.Invoke(() => { FLGDIndexList.Children.Add(new TextBlock() { Text = "流派:" }); });
                    foreach (var d in wk.LiuPai)
                    {
                        Dispatcher.Invoke(() => { FLGDIndexList.Children.Add(new RadioButton() { Content = d.name, Uid = d.id, Margin = new Thickness(0, 0, 10, 10) }); });
                    }
                    Dispatcher.Invoke(() => { FLGDIndexList.Children.Add(new TextBlock() { Text = "主题:" }); });
                    foreach (var d in wk.Theme)
                    {
                        Dispatcher.Invoke(() => { FLGDIndexList.Children.Add(new RadioButton() { Content = d.name, Uid = d.id, Margin = new Thickness(0, 0, 10, 10) }); });
                    }
                    Dispatcher.Invoke(() => { FLGDIndexList.Children.Add(new TextBlock() { Text = "心情:" }); });
                    foreach (var d in wk.Heart)
                    {
                        Dispatcher.Invoke(() => { FLGDIndexList.Children.Add(new RadioButton() { Content = d.name, Uid = d.id, Margin = new Thickness(0, 0, 10, 10) }); });
                    }
                    Dispatcher.Invoke(() => { FLGDIndexList.Children.Add(new TextBlock() { Text = "场景:" }); });
                    foreach (var d in wk.Changjing)
                    {
                        Dispatcher.Invoke(() => { FLGDIndexList.Children.Add(new RadioButton() { Content = d.name, Uid = d.id, Margin = new Thickness(0, 0, 10, 10) }); });
                    }
                    Dispatcher.Invoke(() =>
                    {
                        foreach (var d in FLGDIndexList.Children)
                        {
                            if (d is RadioButton)
                                (d as RadioButton).Checked += FLGDPageChecked;
                        }
                    });
                    var dat = await ml.GetFLGDAsync(int.Parse(wk.Hot[0].id));
                    Dispatcher.Invoke(() => { FLGDItemsList.Children.Clear(); });
                    foreach (var d in dat)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var kss = new FLGDIndexItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(20, 0, 0, 20) };
                            kss.MouseDown += GDMouseDown;
                            FLGDItemsList.Children.Add(kss);
                        });
                        await Task.Delay(1);
                    }
                    foreach (var d in (await ml.GetRadioList()).Hot)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var a = new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) };
                            a.MouseDown += GetRadio;
                            RadioItemsList.Children.Add(a);
                        }); await Task.Delay(1);
                    }
                    Dispatcher.Invoke(() => { CloseLoading(); });
                }));
                sinx.Start();
            }
        }
        public void GDMouseDown(object s, MouseButtonEventArgs se)
        {
            GetGD((s as FLGDIndexItem).id);
        }
        private void FLGDPageChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                {
                    OpenLoading();
                    var dt = sender as RadioButton;
                    int xs = int.Parse(dt.Uid);
                    var s = new Task(new Action(async delegate
                    {
                        var data = await ml.GetFLGDAsync(xs);
                        Dispatcher.Invoke(() =>
                        {
                            FLGDItemsList.Children.Clear();
                            foreach (var d in data)
                            {
                                var k = new FLGDIndexItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(20, 0, 0, 20) };
                                k.MouseDown += GDMouseDown;
                                FLGDItemsList.Children.Add(k);
                            }
                            CloseLoading();
                        });
                    }));
                    s.Start();
                }
            }
            catch { }
        }
        public void GetGD(string id)
        {
            OpenLoading();
            var sx = new Task(new Action(async delegate {
                var dt = await ml.GetGDAsync(id);
                var file = InfoHelper.GetPath() + "Cache\\GD" + id + ".jpg";
                if (!System.IO.File.Exists(file))
                {
                    var s = new WebClient();
                    s.DownloadFileAsync(new Uri(dt.pic), file);
                    s.DownloadFileCompleted += delegate { Dispatcher.Invoke(() => { TXx.Background = new ImageBrush(new BitmapImage(new Uri(file, UriKind.Relative))); }); };
                }
                else Dispatcher.Invoke(() => { TXx.Background = new ImageBrush(new BitmapImage(new Uri(file, UriKind.Relative))); });
                Dispatcher.Invoke(() =>
                {
                    TB.Text = dt.name;
                    DataItemsList.Children.Clear();
                    foreach (var j in dt.Data)
                    {
                        var k = new DataItem(j.MusicID, j.MusicName, j.Singer, j.ImageUrl) { Margin = new Thickness(20, 0, 0, 20) };
                        k.MouseDown += PlayMusic;
                        DataItemsList.Children.Add(k);
                    }
               (Resources["OpenDataPage"] as Storyboard).Begin();
                });
                Dispatcher.Invoke(() => { CloseLoading(); });
            }));
            sx.Start();
        }
        #endregion
        #region Radio
        private void RadioBtn_MouseDown(object sender, MouseButtonEventArgs e) =>RadioMe.IsChecked = true;
        public void GetRadio(object sender, MouseEventArgs e)
        {
            OpenLoading();
            var x = new Task(new Action(async delegate
            {
                var dt = sender as RadioItem;
                RadioID = dt.id;
                var data = await ml.GetRadioMusicAsync(dt.id);
                RadioData = data;
                Dispatcher.Invoke(() =>
                {
                    ml.mldata.Add(data.MusicID, (data.MusicName + " - " + data.Singer).Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""));
                    PlayMusic(data.MusicID, data.ImageUrl, data.MusicName, data.Singer, true);
                });
                Dispatcher.Invoke(() => { CloseLoading(); });
            }));
            x.Start();
        }
        private void RadioPageChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                {
                    OpenLoading();
                    var dt = sender as RadioButton;
                    var s = new Task(new Action(async delegate
                    {
                        var data = await ml.GetRadioList();
                        Dispatcher.Invoke(() => { RadioItemsList.Children.Clear(); });
                        List<MusicRadioListItem> dat = null;
                        Dispatcher.Invoke(() =>
                        {
                            switch (dt.Uid)
                            {
                                case "0":
                                    dat = data.Hot;
                                    break;
                                case "1":
                                    dat = data.Evening;
                                    break;
                                case "2":
                                    dat = data.Love;
                                    break;
                                case "3":
                                    dat = data.Theme;
                                    break;
                                case "4":
                                    dat = data.Changjing;
                                    break;
                                case "5":
                                    dat = data.Style;
                                    break;
                                case "6":
                                    dat = data.Lauch;
                                    break;
                                case "7":
                                    dat = data.People;
                                    break;
                                case "8":
                                    dat = data.Diqu;
                                    break;
                            }
                        });
                        foreach (var d in dat)
                        {
                            Dispatcher.Invoke(() => { RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) }); });
                            await Task.Delay(1);
                        }
                        Dispatcher.Invoke(() => {
                            foreach (var i in RadioItemsList.Children)
                                (i as RadioItem).MouseDown += GetRadio;
                        });
                        Dispatcher.Invoke(() => { CloseLoading(); });
                    }));
                    s.Start();
                }
            }
            catch { }
        }
        Music RadioData;
        #endregion
        #region ILike
        private void likeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MusicName.Text != "MusicName")
            {
                if (IsRadio)
                {
                    if (Settings.USettings.MusicLike.ContainsKey(RadioData.MusicID))
                    {
                        (Resources["LikeBtnUp"] as Storyboard).Begin();
                        Settings.USettings.MusicLike.Remove(RadioData.MusicID);
                    }
                    else
                    {
                        Settings.USettings.MusicLike.Add(RadioData.MusicID, RadioData);
                        (Resources["LikeBtnDown"] as Storyboard).Begin();
                    }
                }
                else
                {
                    if (Settings.USettings.MusicLike.ContainsKey(MusicData.ID))
                    {
                        (Resources["LikeBtnUp"] as Storyboard).Begin();
                        Settings.USettings.MusicLike.Remove(MusicData.ID);
                    }
                    else
                    {
                        Settings.USettings.MusicLike.Add(MusicData.ID, new InfoHelper.Music()
                        {
                            GC = MusicData.ID,
                            Singer = MusicData.Singer,
                            ImageUrl = MusicData.Image,
                            MusicID = MusicData.ID,
                            MusicName = MusicData.SongName
                        });
                        (Resources["LikeBtnDown"] as Storyboard).Begin();
                    }
                }
                Settings.SaveSettings();
            }
        }
        private void LikeBtn_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            TB.Text = "我喜欢";
            TXx.Background = Resources["LoveIcon"] as VisualBrush;
            DataItemsList.Children.Clear();
            foreach (var dt in Settings.USettings.MusicLike.Values)
            {
                var jm = new DataItem(dt.MusicID, dt.MusicName, dt.Singer, dt.ImageUrl) { Margin = new Thickness(20, 0, 0, 20) };
                jm.MouseDown += PlayMusic;
                DataItemsList.Children.Add(jm);
            }
        }
        #region LikePage(Data)

        private void DataPlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PlayMusic(DataItemsList.Children[0] as DataItem, null);
        }

        private void DataDownloadBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DownloadList.Children.Clear();
            filepath.Text = InfoHelper.GetPath() + "Download";
            foreach (DataItem x in DataItemsList.Children)
                DownloadList.Children.Add(new CheckBox() { Width = 370, Content = x.SongName + " - " + x.Singer, Uid = x.ID, FocusVisualStyle = null, Foreground = cb_color.Foreground, Style = cb_color.Style });
        }
        #endregion
        #endregion
        #region SearchMusic
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchMusic(SearchBox.Text);
        }
        public void SearchMusic(string key)
        {
            OpenLoading();
            var xs = new Task(new Action(async delegate
            {
                var dt = await ml.SearchMusicAsync(key);
                var file = InfoHelper.GetPath() + "Cache\\Search" + key + ".jpg";
                if (!System.IO.File.Exists(file))
                {
                    var s = new WebClient();
                    s.DownloadFileAsync(new Uri(dt.First().ImageUrl), file);
                    s.DownloadFileCompleted += delegate { Dispatcher.Invoke(() => { TXx.Background = new ImageBrush(new BitmapImage(new Uri(file, UriKind.Relative))); }); };
                }
                else Dispatcher.Invoke(() => { TXx.Background = new ImageBrush(new BitmapImage(new Uri(file, UriKind.Relative))); });
                Dispatcher.Invoke(() => {
                    TB.Text = key;
                    DataItemsList.Children.Clear();
                    foreach (var j in dt)
                    {
                        var k = new DataItem(j.MusicID, j.MusicName, j.Singer, j.ImageUrl) { Margin = new Thickness(20, 0, 0, 20) };
                        k.MouseDown += PlayMusic;
                        DataItemsList.Children.Add(k);
                    }
               (Resources["OpenDataPage"] as Storyboard).Begin();
                });
                Dispatcher.Invoke(() => { CloseLoading(); });
            }));
            xs.Start();
        }
        #endregion
        #region PlayMusic
        public void PlayMusic(object sender, MouseEventArgs e)
        {
            var dt = sender as DataItem;
            MusicData = dt;
            PlayMusic(dt.ID, dt.Image, dt.SongName, dt.Singer);
        }
        public void PlayMusic(string id, string x, string name, string singer, bool isRadio = false, bool doesplay = true)
        {
            MusicName.Text = "";
            IsRadio = isRadio;
            Settings.USettings.Playing.GC = id;
            Settings.USettings.Playing.ImageUrl = x;
            Settings.USettings.Playing.MusicID = id;
            Settings.USettings.Playing.MusicName = name;
            Settings.USettings.Playing.Singer = singer;
            Settings.SaveSettings();
            if (Settings.USettings.MusicLike.ContainsKey(id))
                (Resources["LikeBtnDown"] as Storyboard).Begin();
            else (Resources["LikeBtnUp"] as Storyboard).Begin();
            if (!ml.mldata.ContainsKey(id))
                ml.mldata.Add(id, (name + " - " + singer).Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""));
            ml.GetAndPlayMusicUrlAsync(id, true, MusicName, this, isPos, doesplay);
            MusicImage.Background = new ImageBrush(new BitmapImage(new Uri(x)));
            Singer.Text = singer;
            if (doesplay)
            {
                (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Pause);
                t.Start();
            }
        }
        #endregion
        #region PlayControl
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsRadio)
                PlayMusic(DataItemsList.Children[DataItemsList.Children.IndexOf(MusicData) - 1] as DataItem, null);
        }
        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (!IsRadio)
                PlayMusic(DataItemsList.Children[DataItemsList.Children.IndexOf(MusicData) + 1] as DataItem, null);
            else GetRadio(new RadioItem(RadioID), null);
        }
        private void PlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isplay)
            {
                isplay = false;
                ml.m.Pause();
                t.Stop();
                (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Play);
            }
            else
            {
                isplay = true;
                ml.m.Play();
                t.Start();
                (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Pause);
            }
        }
        private void jd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ml.m.Position = TimeSpan.FromMilliseconds(jd.Value);
        }
        private void MusicImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ind == 0)
            {
                ind = 1;
                (Resources["OpenLyricPage"] as Storyboard).Begin();
            }
            else
            {
                ind = 0;
                (Resources["CloseLyricPage"] as Storyboard).Begin();
            }
        }
        private void XHBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (xh)
            {
                xh = false;
                (XHBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Lbxh);
            }
            else
            {
                xh = true;
                (XHBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Dqxh);
            }
        }
        #endregion
        #region Lyric
        private async void Border_MouseDown_3(object sender, MouseButtonEventArgs e)
        {
            if (m_Name.Visibility == Visibility.Visible)
            {
                m_Singer.Visibility = Visibility.Collapsed;
                m_Name.Visibility = Visibility.Collapsed;
                ly.Visibility = Visibility.Collapsed;
                pl.Visibility = Visibility.Visible;
                List<MusicPL> data;
                if (!isPos)
                    data = await ml.GetPLByQQAsync(Settings.USettings.Playing.MusicID);
                else
                    data = await ml.GetPLAsync(m_Name.Text + "-" + m_Singer.Text);
                pldata.Children.Clear();
                foreach (var dt in data)
                {
                    pldata.Children.Add(new PlControl(dt.img, dt.name, dt.text) { Width = pldata.ActualWidth - 100 });
                }
            }
            else
            {
                m_Singer.Visibility = Visibility.Visible;
                m_Name.Visibility = Visibility.Visible;
                ly.Visibility = Visibility.Visible;
                pl.Visibility = Visibility.Collapsed;
            }
        }
        private void ly_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ml.lv != null)
                ml.lv.RestWidth(e.NewSize.Width);
        }
        private void qq_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isPos = false;
            ml.GetAndPlayMusicUrlAsync(Settings.USettings.Playing.MusicID, true, MusicName, this, isPos);
        }

        private void wy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isPos = true;
            ml.GetAndPlayMusicUrlAsync(Settings.USettings.Playing.MusicID, true, MusicName, this, isPos);
        }
        #endregion
        #region AddGD
        private void AddGDPage_qqmod_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!mod)
            {
                AddGDPage_qqmod.Effect = new DropShadowEffect() { BlurRadius = 10, Opacity = 0.4, ShadowDepth = 0 };
                AddGDPage_wymod.Effect = null;
                mod = true;
            }
        }

        private void AddGDPage_wymod_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mod)
            {
                AddGDPage_qqmod.Effect = null;
                AddGDPage_wymod.Effect = new DropShadowEffect() { BlurRadius = 10, Opacity = 0.4, ShadowDepth = 0 };
                mod = false;
            }
        }

        private async void AddGDPage_DrBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mod)
            {
                if (!Settings.USettings.MusicGD.ContainsKey(AddGDPage_id.Text))
                    Settings.USettings.MusicGD.Add(AddGDPage_id.Text, await ml.GetGDAsync(AddGDPage_id.Text));
            }
            else
            {
                if (!Settings.USettings.MusicGD.ContainsKey(AddGDPage_id.Text))
                    Settings.USettings.MusicGD.Add(AddGDPage_id.Text, await ml.GetGDbyWYAsync(AddGDPage_id.Text, this, AddGDPage_ps_name, AddGDPage_ps_jd));
            }
             (Resources["CloseAddGDPage"] as Storyboard).Begin();
            GDBtn_MouseDown(null, null);
        }
        #endregion
        #region Download

        private void ckFile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var g = new System.Windows.Forms.FolderBrowserDialog();
            if (g.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                filepath.Text = g.SelectedPath;
        }

        private void cb_color_Click(object sender, RoutedEventArgs e)
        {
            var d = sender as CheckBox;
            if (d.IsChecked == true)
            {
                d.Content = "全不选";
                foreach (CheckBox x in DownloadList.Children)
                    x.IsChecked = true;
            }
            else
            {
                d.Content = "全选";
                foreach (CheckBox x in DownloadList.Children)
                    x.IsChecked = false;
            }
        }

        private async void Dmp_DownloadBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var data = new List<CheckBox>();
            foreach (var x in DownloadList.Children)
            {
                var f = x as CheckBox;
                if (f.IsChecked == true)
                    data.Add(f);
            }
            int index = 0;
            (Resources["CloseDmp"] as Storyboard).Begin();
            Msg msg = new Msg("正在下载全部歌曲(" + data.Count + ")");
            msg.Show();
            for (index = 0; index < data.Count; index++)
            {
                if (msg.IsClose)
                    break;
                var cl = new WebClient();
                string mid = data[index].Uid;
                string url = new LemonLibrary.MusicLib().GetUrlAsync(mid);
                string name = data[index].Content.ToString();
                msg.tb.Text = "正在下载全部歌曲(" + data.Count + ")\n已完成:" + (index + 1) + "  " + name;
                string file = filepath.Text + $"\\{name}.mp3";
                cl.DownloadFile(new Uri(url), file);
                cl.DownloadFileCompleted += delegate { cl.Dispose(); };
            }
            if (!msg.IsClose)
            {
                msg.tb.Text = "已完成.";
                await Task.Delay(5000);
                msg.tbclose();
            }
            else
            {
                await Task.Delay(2000);
                Msg msxg = new Msg("已取消下载");
                msxg.Show();
                await Task.Delay(5000);
                msxg.tbclose();
            }
        }
        #endregion
        #region User
        private void TX_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog o = new Microsoft.Win32.OpenFileDialog();
            if (o.ShowDialog() == true)
            {
                var image = new System.Drawing.Bitmap(o.FileName);
                TX.Background = new ImageBrush(image.ToImageSource());
                Settings.USettings.UserImage = o.FileName;
                Settings.SaveSettings();
            }
        }
        #region MyGD
        private void GDBtn_MouseDown(object sender, EventArgs e)
        {
            GDItemsList.Children.Clear();
            foreach (var jm in Settings.USettings.MusicGD)
            {
                var ks = new FLGDIndexItem(jm.Key, jm.Value.name, jm.Value.pic) { Margin = new Thickness(20, 0, 0, 20) };
                ks.MouseDown += FxGDMouseDown;
                GDItemsList.Children.Add(ks);
            }
            UIHelper.G(Page);
        }

        private void FxGDMouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenLoading();
            var sx = new Task(new Action(async delegate
            {
                var dt = sender as FLGDIndexItem;
                Dispatcher.Invoke(() =>
                {
                    (Resources["OpenDataPage"] as Storyboard).Begin();
                    OpenLoading();
                    TB.Text = dt.name.Text;
                    DataItemsList.Children.Clear();
                });
                await Task.Delay(500);
                var file = InfoHelper.GetPath() + "Cache\\GD" + dt.id + ".jpg";
                if (!System.IO.File.Exists(file))
                {
                    var s = new WebClient();
                    s.DownloadFileAsync(new Uri(dt.img), file);
                    s.DownloadFileCompleted += delegate { Dispatcher.Invoke(() => { TXx.Background = new ImageBrush(new BitmapImage(new Uri(file, UriKind.Relative))); }); };
                }
                else Dispatcher.Invoke(() => { TXx.Background = new ImageBrush(new BitmapImage(new Uri(file, UriKind.Relative))); });
                Dispatcher.Invoke(() =>
                {
                    TB.Text = dt.name.Text;
                    DataItemsList.Children.Clear();
                });
                foreach (var j in Settings.USettings.MusicGD[dt.id].Data)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var k = new DataItem(j.MusicID, j.MusicName, j.Singer, j.ImageUrl) { Margin = new Thickness(20, 0, 0, 20) };
                        k.MouseDown += PlayMusic;
                        DataItemsList.Children.Add(k);
                    });
                    await Task.Delay(1);
                }
                Dispatcher.Invoke(() => { CloseLoading(); });
            }));
            sx.Start();
        }
        #endregion

        #endregion

        #endregion
        #region 快捷键
        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint controlKey, uint virtualKey);

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        public bool InstallHotKeyHook(Window window)
        {
            if (window == null)
                return false;
            System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(window);
            if (IntPtr.Zero == helper.Handle)
                return false;
            System.Windows.Interop.HwndSource source = System.Windows.Interop.HwndSource.FromHwnd(helper.Handle);
            if (source == null)
                return false;
            source.AddHook(this.HotKeyHook);
            return true;
        }
        private IntPtr HotKeyHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                if (wParam.ToInt32() == 124)
                    exShow();
                else if (wParam.ToInt32() == 125)
                    new SearchWindow().Show();
            }
            return IntPtr.Zero;
        }
        private const int WM_HOTKEY = 0x0312;
        #endregion
    }
}