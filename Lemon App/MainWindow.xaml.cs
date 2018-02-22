using LemonLibrary;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static LemonLibrary.InfoHelper;
/*
 TODO: 
    no Image.
     */
namespace Lemon_App
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
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
        public MainWindow()
        {
            InitializeComponent();
        }
        LemonLibrary.MusicLib ml = new MusicLib();
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            var ani = Resources["Loading"] as Storyboard;
            ani.Completed += delegate
            {
                OpenLoading();
                var ds = new System.Windows.Forms.Timer() { Interval = 2000 };
                ds.Tick += delegate { GC.Collect(); UIHelper.G(Page); };
                ds.Start();
                FullScreenManager.RepairWpfWindowFullScreenBehavior(this);
                if (System.IO.File.Exists(Settings.USettings.UserImage))
                {
                    var image = new System.Drawing.Bitmap(Settings.USettings.UserImage);
                    UserTX.Background = new ImageBrush(image.ToImageSource());
                }
                ////////////
                LyricView lv = new LyricView();
                lv.FoucsLrcColor = new SolidColorBrush(Color.FromRgb(48, 195, 124));
                lv.NoramlLrcColor = new SolidColorBrush(Color.FromRgb(199, 199, 199));
                lv.TextAlignment = TextAlignment.Left;
                lv.tbWidth = 470;
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
                (Resources["Closing"] as Storyboard).Completed += delegate { Settings.SaveSettings(); Environment.Exit(0); };
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
                                var file = AppDomain.CurrentDomain.BaseDirectory + "Cache\\Top" + g.topID + ".jpg";
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
                                      }
                                      Dispatcher.Invoke(() => { CloseLoading(); });
                                  }));
                                ss.Start();
                            };
                            top.Margin = new Thickness(0, 0, 20, 20);
                            topIndexList.Children.Add(top);
                        });
                    }
                }));
                de.Start();
                var sinx = new Task(new Action(async delegate
                {
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
                    }
                    Dispatcher.Invoke(() => { CloseLoading(); });
                }));
                sinx.Start();
            };
            ani.Begin();
        }
        string SingerKey1 = "all_all_";
        string SingerKey2 = "all";
        public void GDMouseDown(object s, MouseButtonEventArgs se)
        {
            GetGD((s as FLGDIndexItem).id);
        }
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
            if (sender != null)
            {
                OpenLoading();
                if (SingerKey1 == "")
                    SingerKey1 = "all_all_";
                SingerKey2 = (sender as RadioButton).Content.ToString().Replace("热门", "all").Replace("#", "9");
                var s=new Task(new Action(async delegate
                {
                    var mx = await ml.GetSingerAsync(SingerKey1 + SingerKey2);
                    this.Dispatcher.Invoke(() =>
                    {
                        singerItemsList.Children.Clear();
                        foreach (var d in mx)
                        {
                            var sinx = new SingerItem(d.Photo, d.Name) { Margin = new Thickness(20, 0, 0, 20) };
                            sinx.MouseDown += GetSinger;
                            singerItemsList.Children.Add(sinx);
                        }
                        CloseLoading();
                    });
                }));
                s.Start();
            }
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
            var file = AppDomain.CurrentDomain.BaseDirectory + "Cache\\GD" + id + ".jpg";
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
                Dispatcher.Invoke(()=> { CloseLoading(); });
            }));
            sx.Start();
        }
        private void RadioPageChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                {
                    OpenLoading();
                    var s = new Task(new Action(async delegate
                    {
                        var dt = sender as RadioButton;
                        var data = await ml.GetRadioList();
                        Dispatcher.Invoke(() =>
                        {
                            RadioItemsList.Children.Clear();
                            switch (dt.Uid)
                            {
                                case "0":
                                    foreach (var d in data.Hot)
                                    {
                                        RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                                    }
                                    break;
                                case "1":

                                    foreach (var d in data.Evening)
                                    {
                                        RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                                    }
                                    break;
                                case "2":
                                    foreach (var d in data.Love)
                                    {
                                        RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                                    }
                                    break;
                                case "3":
                                    foreach (var d in data.Theme)
                                    {
                                        RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                                    }
                                    break;
                                case "4":
                                    foreach (var d in data.Changjing)
                                    {
                                        RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                                    }
                                    break;
                                case "5":
                                    foreach (var d in data.Style)
                                    {
                                        RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                                    }
                                    break;
                                case "6":
                                    foreach (var d in data.Lauch)
                                    {
                                        RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                                    }
                                    break;
                                case "7":
                                    foreach (var d in data.People)
                                    {
                                        RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                                    }
                                    break;
                                case "8":
                                    foreach (var d in data.Diqu)
                                    {
                                        RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                                    }
                                    break;
                            }
                            foreach (var i in RadioItemsList.Children)
                            {
                                (i as RadioItem).MouseDown += GetRadio;
                            }
                        });
                        Dispatcher.Invoke(() => { CloseLoading(); });
                    }));
                    s.Start();
                }
            }
            catch { }
        }
        DataItem MusicData;
        InfoHelper.Music RadioData;
        public void PlayMusic(object sender, MouseEventArgs e)
        {
            var dt = sender as DataItem;
            MusicData = dt;
            PlayMusic(dt.ID, dt.Image, dt.SongName, dt.Singer);
        }
        bool IsRadio = false;
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
                ml.mldata.Add(id, name + " - " + singer);
            ml.GetAndPlayMusicUrlAsync(id, true, MusicName, this, doesplay);
            MusicImage.Background = new ImageBrush(new BitmapImage(new Uri(x)));
            Singer.Text = singer;
            if (doesplay)
            {
                (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Pause);
                t.Start();
            }
        }
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchMusic(SearchBox.Text);
            }
        }
        public void SearchMusic(string key)
        {
            OpenLoading();
            var xs = new Task(new Action(async delegate
            {
                var dt = await ml.SearchMusicAsync(key);
                var file = AppDomain.CurrentDomain.BaseDirectory + "Cache\\Search" + key + ".jpg";
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
        string RadioID = "";
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
                    ml.mldata.Add(data.MusicID, data.MusicName + " - " + data.Singer);
                    PlayMusic(data.MusicID, data.ImageUrl, data.MusicName, data.Singer, true);
                });
                Dispatcher.Invoke(() => { CloseLoading(); });
            }));
            x.Start();
        }
        bool isplay = false;
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
        int ind = 0;
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
        bool xh = false;//false: lb true:dq
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

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Page.Clip = new RectangleGeometry() { RadiusX = 5, RadiusY = 5, Rect = new Rect() { Width = Page.ActualWidth, Height = Page.ActualHeight } };
        }

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

        private void DataPlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PlayMusic(DataItemsList.Children[0] as DataItem, null);
        }

        private void DataDownloadBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            new DownloadMarger(DataItemsList.Children).Show();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(Settings.USettings.UserImage))
            {
                var image = new System.Drawing.Bitmap(Settings.USettings.UserImage);
                TX.Background = new ImageBrush(image.ToImageSource());
            }
            NM.Text = Settings.USettings.UserName;
        }

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

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            new FC().Show();
        }
        private void ZJBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void GDBtn_MouseDown(object sender, MouseButtonEventArgs e)
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
            var sx = new Task(new Action(delegate
            {
                var dt = sender as FLGDIndexItem;
                var file = AppDomain.CurrentDomain.BaseDirectory + "Cache\\GD" + dt.id + ".jpg";
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
                    foreach (var j in Settings.USettings.MusicGD[dt.id].Data)
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

        bool issingerloaded = false;
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
                    this.Dispatcher.Invoke(() =>
                    {
                        singerItemsList.Children.Clear();
                        foreach (var d in mx)
                        {
                            var sinx = new SingerItem(d.Photo, d.Name) { Margin = new Thickness(20, 0, 0, 20) };
                            sinx.MouseDown += GetSinger;
                            singerItemsList.Children.Add(sinx);
                        }
                    });
                    Dispatcher.Invoke(() => { CloseLoading(); });
                }));
                s.Start();
            }
        }

        private void Border_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            new AddGDWindow().ShowDialog();
            GDBtn_MouseDown(null, null);
        }

        private async void Border_MouseDown_3(object sender, MouseButtonEventArgs e)
        {
            if (m_Name.Visibility == Visibility.Visible)
            {
                m_Singer.Visibility = Visibility.Collapsed;
                m_Name.Visibility = Visibility.Collapsed;
                ly.Visibility = Visibility.Collapsed;
                pl.Visibility = Visibility.Visible;
                var data = await ml.GetPLAsync(m_Name.Text + "-" + m_Singer.Text);
                pldata.Children.Clear();
                foreach (var dt in data)
                {
                    pldata.Children.Add(new PlControl(dt.img, dt.name, dt.text) { Width = pldata.ActualWidth });
                }
            }
            else {
                m_Singer.Visibility = Visibility.Visible;
                m_Name.Visibility = Visibility.Visible;
                ly.Visibility = Visibility.Visible;
                pl.Visibility = Visibility.Collapsed;
            }
        }
    }
}