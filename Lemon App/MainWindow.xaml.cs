using LemonLibrary;
using LemonLibrary.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        DataItem MusicData = new DataItem(new Music());
        bool isplay = false;
        bool IsRadio = false;
        string RadioID = "";
        int ind = 0;//歌词页面是否打开
        bool xh = false;//false: lb true:dq  循环/单曲 播放控制
        bool issingerloaded = false;
        bool mod = true;//true : qq false : wy
        bool isLoading = false;
        bool IsFirstStart = true;
        bool isPlayasRun = false;
        bool isSearch = false;
        int ApiHandle = 0;
        #endregion
        #region 等待动画
        Thread tOL = null;
        LoadingWindow aw = null;
        public void RunThread(object dx) {
            try
            {
                double[] d = dx as double[];
                aw = new LoadingWindow();
                aw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                aw.Topmost = true;
                aw.Top = d[0];
                aw.Left = d[1];
                aw.Show();
                aw.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2)));
                System.Windows.Threading.Dispatcher.Run();
            }
            catch { }
        }

        public void OpenLoading()
        {
            if (!isLoading)
            {
               // var da = new DoubleAnimation(0, TimeSpan.FromSeconds(0));
         //       ContentPage.BeginAnimation(OpacityProperty, da);
                isLoading = true;
                tOL = new Thread(RunThread);
                tOL.SetApartmentState(ApartmentState.STA);
                double[] d = new double[2];
                d[0] = Top + Height / 2 - 90;
                d[1] = Left + Width / 2 - 50;
                tOL.Start(d);
            }
        }
        public async void CloseLoading()
        {
            if (isLoading)
            {
                await Task.Delay(100);
                isLoading = false;
                aw.Dispatcher.Invoke(() => {
                    var da = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
                    da.Completed +=delegate { aw.Close(); };
                    aw.BeginAnimation(OpacityProperty, da);});
              
             //   ContentPage.BeginAnimation(OpacityProperty, new DoubleAnimation(0.5, 1, TimeSpan.FromSeconds(0.3)));
            }
        }
        #endregion
        #region 窗口加载辅助
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private async void window_Loaded(object sender, RoutedEventArgs e)
        {
            var ani = Resources["Loading"] as Storyboard;
            ani.Begin();

            Settings.SaveWINDOW_HANDLE(new WindowInteropHelper(this).Handle.ToInt32());
            LoadSEND_SHOW();
            LoadHotDog();
            /////Timer user
            var ds = new System.Windows.Forms.Timer() { Interval = 2000 };
            ds.Tick += delegate { GC.Collect(); UIHelper.G(Page); };
            ds.Start();
            App.BaseApp.Apip.Start();

            await Task.Run(() => {
                Settings.LoadLocaSettings();
                string qq = Settings.LSettings.qq;
                Settings.LoadUSettings(qq);
            });

            LoadAfterLogin();

            /////top////
            await Task.Run(new Action(async delegate
            {
                var dt = await ml.GetTopIndexAsync();
                Dispatcher.Invoke(() => { topIndexList.Children.Clear(); });
                foreach (var d in dt)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var top = new TopControl(d.ID, d.Photo, d.Name);
                        top.MouseDown += async delegate (object seb, MouseButtonEventArgs ed)
                        {
                            isSearch = false;
                            OpenLoading();
                            var g = seb as TopControl;
                            NSPage(null, Data);
                            TXx.Background= new ImageBrush(await ImageCacheHelp.GetImageByUrl(g.pic));
                            TB.Text = g.name;
                            var ss = new Task(new Action(async delegate
                            {
                                var dta = await ml.GetToplistAsync(int.Parse(g.topID));
                                Dispatcher.Invoke(() => { DataItemsList.Children.Clear(); });
                                foreach (var j in dta)
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        var k = new DataItem(j) { Width = DataItemsList.ActualWidth };
                                        k.Play += PlayMusic;
                                        if (k.music.MusicID == MusicData.music.MusicID)
                                        {
                                            k.ShowDx();
                                            MusicData = k;
                                        }
                                        DataItemsList.Children.Add(k);
                                    });
                                }
                                isSearch = false;
                                Dispatcher.Invoke(() => { CloseLoading(); });
                            }));
                            ss.Start();
                        };
                        top.Margin = new Thickness(0, 0, 20, 20);
                        topIndexList.Children.Add(top);
                    });
                }
            }));
        }
        private void LoadAfterLogin(bool hasAnimation=true) {
            if (Settings.USettings.Skin_txt != "")
            {
                if (Settings.USettings.Skin_Path != "" && System.IO.File.Exists(Settings.USettings.Skin_Path))
                    Page.Background = new ImageBrush(new BitmapImage(new Uri(Settings.USettings.Skin_Path, UriKind.Absolute)));
                Color co;
                if (Settings.USettings.Skin_txt == "Black"){
                    co = Color.FromRgb(64, 64, 64); App.BaseApp.Skin_Black();
                    ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CFFFFFF"));
                }
                else{
                    co = Color.FromRgb(255, 255, 255); App.BaseApp.Skin();
                    ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26000000"));
                }
                App.BaseApp.SetColor("ThemeColor", Color.FromRgb(byte.Parse(Settings.USettings.Skin_Theme_R),
                    byte.Parse(Settings.USettings.Skin_Theme_G),
                    byte.Parse(Settings.USettings.Skin_Theme_B)));
                App.BaseApp.SetColor("ResuColorBrush", co);
                App.BaseApp.SetColor("ButtonColorBrush", co);
                App.BaseApp.SetColor("TextX1ColorBrush", co);
                ControlDownPage.BorderThickness = new Thickness(0);
                ControlPage.BorderThickness = new Thickness(0);
            }
            else { App.BaseApp.unSkin(); Page.Background = new SolidColorBrush(Colors.White); }

            LoadMusicData(hasAnimation);
        }
        private double now = 0;
        private string lastlyric = "";
        private Toast lyricTa = new Toast("", true);
        private bool isOpenGc = true;
        private void LoadMusicData(bool hasAnimation = true)
        {
            Updata();
            LoadSettings();
            if (Settings.USettings.UserName != string.Empty)
            {
                UserName.Text = Settings.USettings.UserName;
                if (System.IO.File.Exists(Settings.USettings.UserImage))
                {
                    var image = new System.Drawing.Bitmap(Settings.USettings.UserImage);
                    UserTX.Background = new ImageBrush(image.ToImageSource());
                }
            }
            (Resources["Closing"] as Storyboard).Completed += delegate { ShowInTaskbar = false; };
            ////////////load
            LyricView lv = new LyricView();
            lv.FoucsLrcColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            lv.NoramlLrcColor = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
            lv.TextAlignment = TextAlignment.Left;
            ly.Child = lv;
            lv.NextLyric += (text) =>
            {
                if (isOpenGc)
                {
                    if (lastlyric != text) if (text != "")
                            lyricTa.Updata(text);
                    lastlyric = text;
                }
            };
            ml = new MusicLib(lv, Settings.USettings.LemonAreeunIts);
            if (Settings.USettings.Playing.MusicName != "")
            {
                MusicData = new DataItem(Settings.USettings.Playing);
                PlayMusic(Settings.USettings.Playing.MusicID, Settings.USettings.Playing.ImageUrl, Settings.USettings.Playing.MusicName, Settings.USettings.Playing.Singer, false, false);
                string downloadpath = Settings.USettings.CachePath + "Music\\" + Settings.USettings.Playing.MusicID + ".mp3";
                MusicLib.pc.Open(downloadpath);
            }
            t.Interval = 500;
            t.Tick += async delegate
            {
                try
                {
                    now = await MusicLib.pc.Get();
                    if (isPlayasRun)
                    {
                        double all = await MusicLib.pc.GetAll();
                        string alls = TextHelper.TimeSpanToms(TimeSpan.FromMilliseconds(all));
                        if (Play_All.Text == alls && Play_All.Text != "00:") isPlayasRun = false;
                        Play_All.Text = alls;
                        jd.Maximum = all;
                    }
                    Play_Now.Text = TextHelper.TimeSpanToms(TimeSpan.FromMilliseconds(now));
                    if (canjd) jd.Value = now;
                    if (ind == 1)
                        ml.lv.LrcRoll(now, true);
                    else ml.lv.LrcRoll(now, false);
                }
                catch { }
            };
            MusicLib.pc.MediaEnded += delegate
            {
                t.Stop();
                MusicLib.pc.Pause();
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
                    {
                        if (DataItemsList.Children.IndexOf(MusicData) == DataItemsList.Children.Count - 1)
                            PlayMusic(DataItemsList.Children[0] as DataItem, null);
                        else PlayMusic(DataItemsList.Children[DataItemsList.Children.IndexOf(MusicData) + 1] as DataItem, null);
                    }
                }
            };
            if (Settings.USettings.Cookie == "" && Settings.USettings.g_tk == "")
                if (MessageBox.Show("未获取到g_tk与Cookie，请重新登录") == MessageBoxResult.OK)
                    UserTX_MouseDown(null, null);
        }

        private void exShow() {
                WindowState = WindowState.Normal;
                var ani = Resources["Loading"] as Storyboard;
                ani.Begin();
                Activate();
        }
        private void MaxBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MaxHeight = SystemParameters.WorkArea.Height + 10;
            if (WindowState == WindowState.Normal)
            {
                c.ResizeBorderThickness = new Thickness(0);
                Page.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(0), TimeSpan.FromSeconds(0)));
                WindowState = WindowState.Maximized;
                Page.Clip = new RectangleGeometry() { RadiusX = 0, RadiusY = 0, Rect = new Rect() { Width = Page.ActualWidth, Height = Page.ActualHeight } };
            }
            else
            {
                c.ResizeBorderThickness = new Thickness(10);
                Page.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(10), TimeSpan.FromSeconds(0)));
                WindowState = WindowState.Normal;
                Page.Clip = new RectangleGeometry() { RadiusX = 5, RadiusY = 5, Rect = new Rect() { Width = Page.ActualWidth, Height = Page.ActualHeight } };
            }
        }
        private void MinBtn_MouseDown(object sender, MouseButtonEventArgs e) { ShowInTaskbar = true; WindowState = WindowState.Minimized; }
        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Page.Clip = new RectangleGeometry() { RadiusX = 5, RadiusY = 5, Rect = new Rect() { Width = Page.ActualWidth, Height = Page.ActualHeight } };
            foreach (DataItem dx in DataItemsList.Children)
                dx.Width = DataItemsList.ActualWidth;
        }
        #endregion
        #region 设置
        public void LoadSettings() {
            CachePathTb.Text = Settings.USettings.CachePath;
            DownloadPathTb.Text = Settings.USettings.DownloadPath;
        }
        private void UserSendButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //KEY: xfttsuxaeivzdefd
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("lemon.app@qq.com");
            mailMessage.To.Add(new MailAddress("2728578956@qq.com"));
            mailMessage.Subject = "Lemon App用户反馈";
            mailMessage.Body = "UserID:" + Settings.USettings.LemonAreeunIts + "\r\n  \r\n"
                + UserSendText.Text;
            SmtpClient client = new SmtpClient();
            client.Host = "smtp.qq.com";
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("lemon.app@qq.com", "xfttsuxaeivzdefd");
            client.Send(mailMessage);
        }

        private void Run_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/TwilightLemon/Lemon-App");
        }
        private void SettingsBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LoadSettings();
            NSPage(null, SettingsPage);
        }
        private void SettingsPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 3)
            {
                if (hhh.Visibility == Visibility.Collapsed)
                    hhh.Visibility = Visibility.Visible;
                else hhh.Visibility = Visibility.Collapsed;
            }
        }
        private void CP_ChooseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var g = new System.Windows.Forms.FolderBrowserDialog();
            if (g.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DownloadPathTb.Text = g.SelectedPath;
                Settings.USettings.DownloadPath = g.SelectedPath;
            }
        }

        private void DP_ChooseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var g = new System.Windows.Forms.FolderBrowserDialog();
            if (g.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CachePathTb.Text = g.SelectedPath;
                Settings.USettings.DownloadPath = g.SelectedPath;
            }
        }

        private void CP_OpenBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer", CachePathTb.Text);
        }

        private void DP_OpenBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer", DownloadPathTb.Text);
        }
        #endregion
        #region 主题切换
        string TextColor_byChoosing = "Black";
        private void ColorThemeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChooseText.Visibility = Visibility.Visible;
        }

        private void Border_MouseDown_4(object sender, MouseButtonEventArgs e)
        {
            TextColor_byChoosing = "White";
        }

        private void Border_MouseDown_5(object sender, MouseButtonEventArgs e)
        {
            TextColor_byChoosing = "Black";
        }

        private void MDButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color co;
                if (TextColor_byChoosing == "Black")
                {
                    co = Color.FromRgb(64, 64, 64); App.BaseApp.Skin_Black();
                    ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CFFFFFF"));
                }
                else
                {
                    co = Color.FromRgb(255, 255, 255); App.BaseApp.Skin();
                    ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26000000"));
                }
                Color col = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                App.BaseApp.SetColor("ThemeColor", col);
                App.BaseApp.SetColor("ResuColorBrush", co);
                App.BaseApp.SetColor("ButtonColorBrush", co);
                App.BaseApp.SetColor("TextX1ColorBrush", co);
                ControlDownPage.BorderThickness = new Thickness(0);
                ControlPage.BorderThickness = new Thickness(0);
                Settings.USettings.Skin_txt = TextColor_byChoosing;
                Settings.USettings.Skin_Theme_R = col.R.ToString();
                Settings.USettings.Skin_Theme_G = col.G.ToString();
                Settings.USettings.Skin_Theme_B = col.B.ToString();
                Settings.SaveSettings();
            }
            ChooseText.Visibility = Visibility.Collapsed;
        }

        private void ColorThemeBtn_Copy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "图像文件(*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp|所有文件|*.*";
            ofd.ValidateNames = true;
            ofd.CheckPathExists = true;
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string strFileName = ofd.FileName;
                string file = Settings.USettings.CachePath + "Skin\\" + System.IO.Path.GetFileName(strFileName);
                System.IO.File.Move(strFileName, file);
                Page.Background = new ImageBrush(new System.Drawing.Bitmap(file).ToImageSource());
                Settings.USettings.Skin_Path = file;
            }
        }
        private async void SkinBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(null, SkinPage);
            SkinIndexList.Children.Clear();
            var json = JObject.Parse(await HttpHelper.GetWebAsync("https://gitee.com/TwilightLemon/ux/raw/master/SkinList.json"))["data"];
            int i = 1;
            foreach (var dx in json)
            {
                string name = dx["name"].ToString();
                Color color = Color.FromRgb(byte.Parse(dx["ThemeColor"]["R"].ToString()),
                    byte.Parse(dx["ThemeColor"]["G"].ToString()),
                    byte.Parse(dx["ThemeColor"]["B"].ToString()));
                if(!System.IO.File.Exists(Settings.USettings.CachePath + "Skin\\" + i + ".jpg"))
                     await HttpHelper.HttpDownloadFileAsync($"https://gitee.com/TwilightLemon/ux/raw/master/w{i}.jpg", Settings.USettings.CachePath + "Skin\\" + i + ".jpg");
                SkinControl sc = new SkinControl(i, name, color);
                sc.txtColor = dx["TextColor"].ToString();
                sc.MouseDown += async (s, n) => {
                    if (!System.IO.File.Exists(Settings.USettings.CachePath + "Skin\\" + sc.imgurl + ".png"))
                        await HttpHelper.HttpDownloadFileAsync($"https://gitee.com/TwilightLemon/ux/raw/master/{sc.imgurl}.png", Settings.USettings.CachePath + "Skin\\" + sc.imgurl + ".png");
                    Page.Background = new ImageBrush(new System.Drawing.Bitmap(Settings.USettings.CachePath + "Skin\\" + sc.imgurl + ".png").ToImageSource());
                    Color co;
                    if (sc.txtColor == "Black"){
                        co = Color.FromRgb(64, 64, 64); App.BaseApp.Skin_Black();
                        ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CFFFFFF"));
                    }
                    else
                    {
                        co = Color.FromRgb(255, 255, 255); App.BaseApp.Skin();
                        ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26000000"));
                    }
                    App.BaseApp.SetColor("ThemeColor", sc.theme);
                    App.BaseApp.SetColor("ResuColorBrush", co);
                    App.BaseApp.SetColor("ButtonColorBrush", co);
                    App.BaseApp.SetColor("TextX1ColorBrush", co);
                    ControlDownPage.BorderThickness = new Thickness(0);
                    ControlPage.BorderThickness = new Thickness(0);
                    Settings.USettings.Skin_Path = Settings.USettings.CachePath + "Skin\\" + +sc.imgurl + ".png";
                    Settings.USettings.Skin_txt = sc.txtColor;
                    Settings.USettings.Skin_Theme_R = sc.theme.R.ToString();
                    Settings.USettings.Skin_Theme_G = sc.theme.G.ToString();
                    Settings.USettings.Skin_Theme_B = sc.theme.B.ToString();
                    Settings.SaveSettings();
                };
                sc.Margin = new Thickness(10, 0, 0, 0);
                SkinIndexList.Children.Add(sc);
                i++;
            }
            SkinControl sxc = new SkinControl(-1, "默认主题", Color.FromArgb(0,0,0,0));
            sxc.MouseDown += (s, n) =>{
                ControlDownPage.BorderThickness = new Thickness(0,1,0,0);
                ControlPage.BorderThickness = new Thickness(0, 0, 1, 0);
                ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CFFFFFF"));
                Page.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                App.BaseApp.unSkin();
                Settings.USettings.Skin_txt = "";
                Settings.USettings.Skin_Path = "";
                Settings.SaveSettings();
            };
            sxc.Margin = new Thickness(10, 0, 0, 0);
            SkinIndexList.Children.Add(sxc);
        }
        #endregion
        #region 功能区
        #region Updata
        private async void Updata() {
            var o = JObject.Parse(await HttpHelper.GetWebAsync("https://gitee.com/TwilightLemon/UpdataForWindows/raw/master/WindowsUpdata.json"));
            string v = o["version"].ToString();
            string dt = o["description"].ToString().Replace("@32","\n");
            if (int.Parse(v) > int.Parse(App.EM)) {
                if (MyMessageBox.Show("小萌有更新啦", dt, "立即更新")) {
                    var xpath = Settings.USettings.CachePath + "win-release.exe";
                    await HttpHelper.HttpDownloadFileAsync("https://gitee.com/TwilightLemon/UpdataForWindows/raw/master/win-release.exe", xpath);
                    Process.Start(xpath);
                }
            }
        }
        #endregion
        #region N/S Page
        private Label LastClickLabel = null;
        private Grid LastPage = null;
        public void NSPage(Label ClickLabel,Grid TPage) {
            if (LastClickLabel == null) LastClickLabel = TopBtn;
            LastClickLabel.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            if (ClickLabel!=null)ClickLabel.SetResourceReference(ForegroundProperty, "ThemeColor");
            if (LastPage == null) LastPage = TopIndexPage;
            LastPage.Visibility = Visibility.Collapsed;
            TPage.Visibility = Visibility.Visible;
            TPage.BeginAnimation(OpacityProperty, new DoubleAnimation(0.5,1, TimeSpan.FromSeconds(0.2)));
            if (ClickLabel != null) LastClickLabel = ClickLabel;
            LastPage = TPage;
        }
        private void TopBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(TopBtn, TopIndexPage);
        }
        #endregion
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
                        }
                        Dispatcher.Invoke(() => { CloseLoading(); });
                    }));
                    s.Start();
                }
        }
        private void SingerBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(SingerBtn, SingerIndexPage);
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
            NSPage(ZJBtn, ZJIndexPage);
            if (FLGDIndexList.Children.Count == 0)
            {
                var sinx = new Task(new Action(async delegate
                {
                    Dispatcher.Invoke(() => { OpenLoading(); });
                    var wk = await ml.GetFLGDIndexAsync();
                    Dispatcher.Invoke(() => { 
                    RadioButton rb = new RadioButton()
                    {
                        Style = RadioMe.Style,
                        Background = RadioMe.Background,
                        Content = wk.Hot[0].name,
                        Uid = wk.Hot[0].id,
                        Margin = new Thickness(0, 0, 30, 10)
                    };
                        rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                        rb.SetResourceReference(BorderBrushProperty, "TextX1ColorBrush");
                        FLGDIndexList.Children.Add(rb); });
                    Dispatcher.Invoke(() => {
                        var rb = new TextBlock() { Text = "语种:" };
                        rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                        FLGDIndexList.Children.Add(rb);
                    });
                    foreach (var d in wk.Lauch)
                        Dispatcher.Invoke(() => {
                            var rb = new RadioButton()
                            {
                                Style = RadioMe.Style,
                                Background = RadioMe.Background,
                                Content = d.name,
                                Uid = d.id,
                                Margin = new Thickness(0, 0, 10, 10)
                            };
                            rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                            rb.SetResourceReference(BorderBrushProperty, "TextX1ColorBrush");
                            FLGDIndexList.Children.Add(rb); });
                    Dispatcher.Invoke(() => {
                        var rb = new TextBlock() { Text = "流派:" };
                        rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                        FLGDIndexList.Children.Add(rb);
                    });
                    foreach (var d in wk.LiuPai)
                        Dispatcher.Invoke(() => {
                            var rb = new RadioButton()
                            {
                                Style = RadioMe.Style,
                                Background = RadioMe.Background,
                                Content = d.name,
                                Uid = d.id,
                                Margin = new Thickness(0, 0, 10, 10)
                            };
                            rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                            rb.SetResourceReference(BorderBrushProperty, "TextX1ColorBrush");
                            FLGDIndexList.Children.Add(rb);
                        });
                    Dispatcher.Invoke(() => {
                        var rb = new TextBlock() { Text = "主题:" };
                        rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                        FLGDIndexList.Children.Add(rb);
                    });
                    foreach (var d in wk.Theme)
                        Dispatcher.Invoke(() => {
                            var rb = new RadioButton()
                            {
                                Style = RadioMe.Style,
                                Background = RadioMe.Background,
                                Content = d.name,
                                Uid = d.id,
                                Margin = new Thickness(0, 0, 10, 10)
                            };
                            rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                            rb.SetResourceReference(BorderBrushProperty, "TextX1ColorBrush");
                            FLGDIndexList.Children.Add(rb);
                        });
                    Dispatcher.Invoke(() => {
                        var rb = new TextBlock() { Text = "心情:" };
                        rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                        FLGDIndexList.Children.Add(rb);
                    });
                    foreach (var d in wk.Heart)
                        Dispatcher.Invoke(() => {
                            var rb = new RadioButton()
                            {
                                Style = RadioMe.Style,
                                Background = RadioMe.Background,
                                Content = d.name,
                                Uid = d.id,
                                Margin = new Thickness(0, 0, 10, 10)
                            };
                            rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                            rb.SetResourceReference(BorderBrushProperty, "TextX1ColorBrush");
                            FLGDIndexList.Children.Add(rb);
                        });
                    Dispatcher.Invoke(() => {
                        var rb = new TextBlock() { Text = "场景:" };
                        rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                        FLGDIndexList.Children.Add(rb);
                    });
                    foreach (var d in wk.Changjing)
                        Dispatcher.Invoke(() => {
                            var rb = new RadioButton()
                            {
                                Style = RadioMe.Style,
                                Background = RadioMe.Background,
                                Content = d.name,
                                Uid = d.id,
                                Margin = new Thickness(0, 0, 10, 10)
                            };
                            rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                            rb.SetResourceReference(BorderBrushProperty, "TextX1ColorBrush");
                            FLGDIndexList.Children.Add(rb);
                        });
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
                    foreach (var d in (await ml.GetRadioList()).Hot)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var a = new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) };
                            a.MouseDown += GetRadio;
                            RadioItemsList.Children.Add(a);
                        });
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
        public void GetGD(string id)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            isSearch = false;
            OpenLoading();
            MusicGDTask mt = new MusicGDTask();
            mt.Finished += async (dt) =>
            {
                He.MGData_Now = dt;
                await Dispatcher.Invoke(async () =>
                 {
                     TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.pic));
                     TB.Text = dt.name;
                 });
                foreach (var m in dt.Data)
                    Dispatcher.Invoke(new Action(() =>
                    {
                        var k = new DataItem(m, dt.IsOwn, this) { Width = DataItemsList.ActualWidth };
                        k.Play += PlayMusic;
                        if (k.music.MusicID == MusicData.music.MusicID)
                        {
                            k.ShowDx();
                            MusicData = k;
                        }
                        DataItemsList.Children.Add(k);
                    }));
                Dispatcher.Invoke(new Action(() =>
                {
                    sw.Stop();
                    Console.WriteLine("耗时:" + sw.Elapsed.TotalMilliseconds + "ms");
                    CloseLoading();
                }));
            };
            mt.GetGDAsync(id);
            NSPage(null, Data);
        }
        #endregion
        #region Radio
        private void RadioBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(RadioBtn, RadioIndexPage);
            RadioMe.IsChecked = true;
        }

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
                    PlayMusic(data.MusicID, data.ImageUrl, data.MusicName, data.Singer, true);
                });
                Dispatcher.Invoke(() => { CloseLoading(); });
            }));
            x.Start();
        }
        private void RadioPageChecked(object sender, RoutedEventArgs e)
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
        Music RadioData;
        #endregion
        #region ILike
        private void LikeBtnUp() {
            if (ind == 1)
                likeBtn_path.Fill = new SolidColorBrush(Colors.White);
            else
                likeBtn_path.SetResourceReference(Path.FillProperty, "ResuColorBrush");
        }

        private void LikeBtnDown() {
            likeBtn_path.Fill = new SolidColorBrush(Color.FromRgb(216, 30, 30));
        }
        private void likeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {//TODO:等待添加/删除功能完善..
            if (MusicName.Text != "MusicName")
            {
                if (IsRadio)
                {
                    if (Settings.USettings.MusicLike.ContainsKey(RadioData.MusicID))
                    {
                        LikeBtnUp();
                        Settings.USettings.MusicLike.Remove(RadioData.MusicID);
                        string a = MusicLib.DeleteMusicFromGD(RadioData.MusicID,MusicLib.MusicLikeGDid, MusicLib.MusicLikeGDdirid);
                        Toast.Send(a);
                    }
                    else
                    {
                        string[] a = MusicLib.AddMusicToGD(RadioData, MusicLib.MusicLikeGDdirid);
                        Toast.Send(a[1]+": "+a[0]);
                        Settings.USettings.MusicLike.Add(RadioData.MusicID, RadioData);
                        LikeBtnDown();
                    }
                }
                else
                {
                    if (Settings.USettings.MusicLike.ContainsKey(MusicData.music.MusicID))
                    {
                        LikeBtnUp();
                        Settings.USettings.MusicLike.Remove(MusicData.music.MusicID);
                        string a = MusicLib.DeleteMusicFromGD(MusicData.music.MusicID, MusicLib.MusicLikeGDid, MusicLib.MusicLikeGDdirid);
                        Toast.Send(a);
                    }
                    else
                    {
                        string[] a = MusicLib.AddMusicToGD(MusicData.music, MusicLib.MusicLikeGDdirid);
                        Toast.Send(a[1]+": "+a[0]);
                        Settings.USettings.MusicLike.Add(MusicData.music.MusicID, MusicData.music);
                        LikeBtnDown();
                    }
                }
                Settings.SaveSettings();
            }
        }
        private void LikeBtn_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            NSPage(LikeBtn, Data);
            TB.Text = "我喜欢";
            TXx.Background = Resources["LoveIcon"] as VisualBrush;
            DataItemsList.Children.Clear();
            OpenLoading();
            MusicGDTask mt = new MusicGDTask();
            mt.Finished += (dt) =>
            {
                He.MGData_Now = dt;
                if (dt.Data.Count == 0) Dispatcher.Invoke(()=> { NSPage(LikeBtn,NonePage); });
                foreach (var m in dt.Data)
                    Dispatcher.Invoke(new Action(() =>
                    {
                        var k = new DataItem(m, dt.IsOwn, this) { Width = DataItemsList.ActualWidth };
                        k.Play += PlayMusic;
                        if (k.music.MusicID == MusicData.music.MusicID)
                        {
                            k.ShowDx();
                            MusicData = k;
                        }
                        DataItemsList.Children.Add(k);
                    }));
                Dispatcher.Invoke(new Action(() =>
                {
                    sw.Stop();
                    Console.WriteLine("耗时:" + sw.Elapsed.TotalMilliseconds + "ms");
                    CloseLoading();

                    var ac = new Action<MusicGData>((m) =>
                    {
                        foreach (var ai in m.Data)
                        {
                            Settings.USettings.MusicLike.Add(ai.MusicID, ai);
                        }
                    });
                    ac(dt);
                }));
            };
            mt.GetGDAsync(MusicLib.MusicLikeGDid);
            isSearch = false;
        }
        #endregion
        #region DataPageBtn
        private void DataPlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PlayMusic(DataItemsList.Children[0] as DataItem, null);
        }

        private void DataDownloadBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            border5.Visibility = Visibility.Collapsed;
            DataDownloadPage.Visibility = Visibility.Visible;
            Download_Path.Text = Settings.USettings.DownloadPath;
            DownloadQx.IsChecked = true;
            DownloadQx.Content = "全不选";
            foreach (DataItem x in DataItemsList.Children) {
                x.MouseDown -= PlayMusic;
                x.NSDownload(true);
                x.Check();
            }
        }

        public void CloseDownloadPage() {
            border5.Visibility = Visibility.Visible;
            DataDownloadPage.Visibility = Visibility.Collapsed;
            foreach (DataItem x in DataItemsList.Children)
            {
                x.MouseDown += PlayMusic;
                x.NSDownload(false);
                x.Check();
            }
        }

        private void DataDownloadBtn_Copy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseDownloadPage();
        }
        #endregion
        #region SearchMusic
        private int ixPlay = 1;
        private void Datasv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Datasv.IsVerticalScrollBarAtButtom()&&isSearch) {
                ixPlay++;
                SearchMusic(SearchKey,ixPlay);
            }
        }
        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text.Trim() != string.Empty)
            {
                if (Search_SmartBox.Visibility != Visibility.Visible)
                    Search_SmartBox.Visibility = Visibility.Visible;
                var data = await ml.Search_SmartBoxAsync(SearchBox.Text);
                Search_SmartBoxList.Items.Clear();
                if (data.Count == 0)
                    Search_SmartBox.Visibility = Visibility.Collapsed;
                else foreach (var dt in data)
                        Search_SmartBoxList.Items.Add(dt);
            }
            else Search_SmartBox.Visibility = Visibility.Collapsed;
        }
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter&&SearchBox.Text.Trim() != string.Empty)
            { SearchMusic(SearchBox.Text); ixPlay = 1; Search_SmartBox.Visibility = Visibility.Collapsed; }
        }
        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
                Search_SmartBoxList.Focus();
        }

        private void Search_SmartBoxList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if (Search_SmartBoxList.SelectedIndex != -1){
                    SearchBox.Text = Search_SmartBoxList.SelectedItem.ToString().Replace("歌曲:", "").Replace("歌手:", "").Replace("专辑:", "");
                    Search_SmartBox.Visibility = Visibility.Collapsed;
                    SearchMusic(SearchBox.Text); ixPlay = 1;
                }
        }

        private void Bd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Search_SmartBoxList.SelectedIndex != -1)
            {
                SearchBox.Text = Search_SmartBoxList.SelectedItem.ToString().Replace("歌曲:", "").Replace("歌手:", "").Replace("专辑:", "");
                Search_SmartBox.Visibility = Visibility.Collapsed;
                SearchMusic(SearchBox.Text); ixPlay = 1;
            }
        }

        private void Search_SmartBox_MouseLeave(object sender, MouseEventArgs e)
        {
            Search_SmartBox.Visibility = Visibility.Collapsed;
        }
        private string SearchKey = "";
        public void SearchMusic(string key,int osx=0)
        {
            isSearch = true;
            SearchKey = key;
            OpenLoading();
            var xs = new Task(new Action(async delegate
            {
                List<Music> dt = null;
                if (osx == 0) Dispatcher.Invoke(() => { NSPage(null, Data); });
                if (osx==0)dt = await ml.SearchMusicAsync(key);
                else dt = await ml.SearchMusicAsync(key,osx);
                if(osx==0)
                   Dispatcher.Invoke(() => {
                       TB.Text = key;
                       DataItemsList.Children.Clear();
                       Datasv.ScrollToTop();
                });
                await Dispatcher.Invoke(async () =>
                {
                    TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.First().ImageUrl));
                    foreach (var j in dt)
                    {
                        var k = new DataItem(j) { Width = DataItemsList.ActualWidth };
                        if (k.music.MusicID == MusicData.music.MusicID)
                        {
                            k.ShowDx();
                            MusicData = k;
                        }
                        k.Play += PlayMusic;
                        DataItemsList.Children.Add(k);
                    }
                });
                Dispatcher.Invoke(() => {CloseLoading(); });
            }));
            xs.Start();
        }
        #endregion
        #region PlayMusic

        public void PlayMusic(object sender, MouseEventArgs e)
        {
            var dt = sender as DataItem;
            dt.ShowDx();
            MusicData = dt;
            PlayMusic(dt.music.MusicID, dt.music.ImageUrl, dt.music.MusicName, dt.music.Singer);
        }
        public void PlayMusic(DataItem dt)
        {
            dt.ShowDx();
            MusicData = dt;
            PlayMusic(dt.music.MusicID, dt.music.ImageUrl, dt.music.MusicName, dt.music.Singer);
        }
        private string LastPlay = "";
        public void PlayMusic(string id, string x, string name, string singer, bool isRadio = false, bool doesplay = true)
        {
            if (LastPlay == id)
            {
                if (doesplay)
                {
                    MusicLib.pc.To(0);
                    MusicLib.pc.Play();
                    (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Pause);
                    t.Start();
                    isplay = true;
                }
            }
            else
            {
                MusicName.Text = "连接资源中...";
                IsRadio = isRadio;
                isPlayasRun = true;
                t.Stop();
                MusicLib.pc.Pause();
                Settings.USettings.Playing = MusicData.music;
                Settings.SaveSettings();
                if (Settings.USettings.MusicLike.ContainsKey(id))
                    LikeBtnDown();
                else LikeBtnUp();
                ml.GetAndPlayMusicUrlAsync(id, true, MusicName, this,name+" - "+singer, doesplay);
                MusicImage.Background = new ImageBrush(new BitmapImage(new Uri(x)));
                Singer.Text = singer;
                if (doesplay)
                {
                    (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Pause);
                    t.Start();
                    isplay = true;
                }
                LastPlay = MusicData.music.MusicID;
            }
        }
        #endregion
        #region PlayControl
        bool canjd = true;
        private void jd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Abs((jd.Value + 50) - now) > 500)
            {
                if (canjd)
                {
                    canjd = false;
                    MusicLib.pc.ToAway += Pc_ToAway;
                }
                MusicLib.pc.To(jd.Value);
            }
        }

        private async void Pc_ToAway()
        {
            await Task.Delay(500);
            canjd = true;
            MusicLib.pc.ToAway -= Pc_ToAway;
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
        private void PlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isplay)
            {
                isplay = false;
                MusicLib.pc.Pause();
                t.Stop();
                (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Play);
            }
            else
            {
                isplay = true;
                MusicLib.pc.Play();
                t.Start();
                (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Pause);
            }
        }


        private void GcBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isOpenGc)
            {
                isOpenGc = false;
                lyricTa.Close();
                if (ind == 1)
                    path7.Fill = new SolidColorBrush(Colors.White);
                else
                    path7.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            }
            else
            {
                isOpenGc = true;
                lyricTa = new Toast("", true);
                path7.SetResourceReference(Path.FillProperty, "ThemeColor");
            }
        }
        private void Border_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            ind = 0;
            MusicName.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            Singer.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            Play_All.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            Play_Now.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            timetb.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            textBlock4.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            path1.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            path2.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            path3.SetResourceReference(Path.FillProperty, "ThemeColor");
            path4.SetResourceReference(Path.FillProperty, "ThemeColor");
            path5.SetResourceReference(Path.FillProperty, "ThemeColor");
            path6.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            if (!isOpenGc) path7.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            likeBtn_path.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            ControlDownPage.BorderThickness = new Thickness(0, 1, 0, 0);
            (Resources["CloseLyricPage"] as Storyboard).Begin();
        }

        private void MusicImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ind = 1;
            ControlDownPage.BorderThickness = new Thickness(0);
            MusicName.Foreground = new SolidColorBrush(Colors.White);
            Singer.Foreground = new SolidColorBrush(Colors.White);
            Play_All.Foreground = new SolidColorBrush(Colors.White);
            Play_Now.Foreground = new SolidColorBrush(Colors.White);
            timetb.Foreground = new SolidColorBrush(Colors.White);
            textBlock4.Foreground = new SolidColorBrush(Colors.White);
            path1.Fill = new SolidColorBrush(Colors.White);
            path2.Fill = new SolidColorBrush(Colors.White);
            path3.Fill = new SolidColorBrush(Colors.White);
            path4.Fill = new SolidColorBrush(Colors.White);
            path5.Fill = new SolidColorBrush(Colors.White);
            path6.Fill = new SolidColorBrush(Colors.White);
            if(!isOpenGc) path7.Fill = new SolidColorBrush(Colors.White);
            likeBtn_path.Fill = new SolidColorBrush(Colors.White);
            (Resources["OpenLyricPage"] as Storyboard).Begin();
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

        private void PlayLbBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(null, Data);
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
                List<MusicPL> data= await ml.GetPLByQQAsync(Settings.USettings.Playing.MusicID);
                //if (!isPos)
                //    data = await ml.GetPLByQQAsync(Settings.USettings.Playing.MusicID);
                //else
                //    data = await ml.GetPLAsync(m_Name.Text + "-" + m_Singer.Text);
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

        private void AddGDPage_DrBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mod)
            {
                MessageBox.Show(MusicLib.AddGDILike(AddGDPage_id.Text));
            }
            else
            {
                //添加网易云音乐歌单
            }
             (Resources["CloseAddGDPage"] as Storyboard).Begin();
            GDBtn_MouseDown(null, null);
        }
        #endregion
        #region Download

        private void ckFile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var g = new System.Windows.Forms.FolderBrowserDialog();
            if (g.ShowDialog() == System.Windows.Forms.DialogResult.OK){
                Download_Path.Text = g.SelectedPath;
                Settings.USettings.DownloadPath= g.SelectedPath;
            }
            
        }

        private void cb_color_Click(object sender, RoutedEventArgs e)
        {
            var d = sender as CheckBox;
            if (d.IsChecked == true)
            {
                d.Content = "全不选";
                foreach (DataItem x in DataItemsList.Children)
                    x.Check();
            }
            else
            {
                d.Content = "全选";
                foreach (DataItem x in DataItemsList.Children)
                    x.Check();
            }
        }

        private async void DownloadBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var data = new List<DataItem>();
            foreach (var x in DataItemsList.Children)
            {
                var f = x as DataItem;
                if (f.isChecked == true)
                    data.Add(f);
            }
            CloseDownloadPage();
            Msg msg = new Msg("正在下载全部歌曲(" + data.Count + ")","连接资源中.....");
            msg.Show();
            var DTimer = new System.Windows.Forms.Timer();
            await DownloadTaskAsync(msg, data,DTimer);
            DTimer.Interval = 3000;
            DTimer.Tick +=async delegate {
                if (DownloadIndex != data.Count){
                    string name = data[DownloadIndex].music.MusicName + " - " + data[DownloadIndex].music.Singer;
                    string file = Download_Path.Text + $"\\{name}.mp3";
                    System.IO.File.Delete(file);
                    await DownloadTaskAsync(msg, data, DTimer);
                }
            };
        }
        private int DownloadIndex=0;
        public async Task DownloadTaskAsync(Msg msg, List<DataItem> data, System.Windows.Forms.Timer dt) {
            dt.Start();
            string name = data[DownloadIndex].music.MusicName + " - " + data[DownloadIndex].music.Singer;
            string file = Download_Path.Text + $"\\{name}.mp3";
            if (!System.IO.File.Exists(file))
            {
                var cl = new WebClient();
                string mid = data[DownloadIndex].music.MusicID;
                string url = await ml.GetUrlAsync(mid);
                msg.tb.Text = "正在下载:" + (DownloadIndex + 1) + "  " + name;
                cl.DownloadFileAsync(new Uri(url), file);
                cl.DownloadProgressChanged += (s, e) =>
                {
                    dt.Stop();
                    msg.tb.Text = "正在下载：" + (DownloadIndex + 1) +"  ("+ e.ProgressPercentage + "%)"  + "  " + name;
                };
                cl.DownloadFileCompleted += async delegate
                {
                    if (!msg.IsClose)
                    {
                        if (DownloadIndex != data.Count)
                        {
                            DownloadIndex++;
                            await DownloadTaskAsync(msg, data,dt);
                        }
                        if (DownloadIndex + 1 == data.Count) {
                            if (!msg.IsClose)
                            {
                                msg.tb.Text = "已完成.";
                                await Task.Delay(5000);
                                msg.tbclose();
                            }
                            else
                            {
                                await Task.Delay(2000);
                                Msg msxg = new Msg("已取消下载","");
                                msxg.Show();
                                await Task.Delay(5000);
                                msxg.tbclose();
                            }
                        }
                    }
                    cl.Dispose();
                };
            }
            else {
                DownloadIndex++;
                await DownloadTaskAsync(msg, data,dt);
            }
        }

        #endregion
        #region User
        #region Login
        private async void UserTX_MouseDown(object sender, MouseButtonEventArgs e)
        {
            App.BaseApp.Apip.Start();
            await Task.Delay(1000);
            MsgHelper.SendMsg("Login", ApiHandle);
        }
        #endregion
        #region MyGD
        private List<string> GData_Now = new List<string>();
        private List<string> GLikeData_Now = new List<string>();
        private async void GDBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(GDBtn, MyGDIndexPage);
            OpenLoading();
            var GdData =await ml.GetGdListAsync();
            if (GdData.Count != GDItemsList.Children.Count)
            { GDItemsList.Children.Clear(); GData_Now.Clear(); }
            foreach (var jm in GdData)
            {
                if (!GData_Now.Contains(jm.Key))
                {
                    var ks = new FLGDIndexItem(jm.Key, jm.Value.name, jm.Value.pic) { Margin = new Thickness(20, 0, 0, 20) };
                    ks.MouseDown += FxGDMouseDown;
                    GDItemsList.Children.Add(ks);
                    GData_Now.Add(jm.Key);
                }
            }
            var GdLikeData = await ml.GetGdILikeListAsync();
            if (GdLikeData.Count != GDILikeItemsList.Children.Count)
            { GDILikeItemsList.Children.Clear(); GdLikeData.Clear(); }
            foreach (var jm in GdLikeData)
            {
                if (!GLikeData_Now.Contains(jm.Key))
                {
                    var ks = new FLGDIndexItem(jm.Key, jm.Value.name, jm.Value.pic) { Margin = new Thickness(20, 0, 0, 20) };
                    ks.MouseDown += FxGDMouseDown;
                    GDILikeItemsList.Children.Add(ks);
                    GLikeData_Now.Add(jm.Key);
                }
            }
            if (GdData.Count == 0 && GdLikeData.Count == 0)
                NSPage(GDBtn, NonePage);
            UIHelper.G(Page);
            CloseLoading();
        }

        private async void FxGDMouseDown(object sender, MouseButtonEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var da = new DoubleAnimation(0, TimeSpan.FromSeconds(0.3));
            ContentPage.BeginAnimation(OpacityProperty, da);
            OpenLoading();
            NSPage(null, Data);
            var dt = sender as FLGDIndexItem;
            TB.Text = dt.name.Text;
            DataItemsList.Children.Clear();
            TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.img));
            TB.Text = dt.name.Text;
            DataItemsList.Children.Clear();
            MusicGDTask mt = new MusicGDTask();
            mt.Finished += (md) => {
                He.MGData_Now = md;
                foreach(var m in md.Data)
                Dispatcher.Invoke(new Action(() =>
                {
                    var k = new DataItem(m, md.IsOwn, this) { Width = DataItemsList.ActualWidth };
                    k.Play += PlayMusic;
                    if (m.MusicID==MusicData.music.MusicID)
                    {
                        k.ShowDx();
                        MusicData = k;
                    }
                    DataItemsList.Children.Add(k);
                }));
                Dispatcher.Invoke(new Action(() => { sw.Stop();
                    Console.WriteLine("耗时:" + sw.Elapsed.TotalMilliseconds+"ms");
                    CloseLoading();
                }));
            };
            mt.GetGDAsync(dt.id);
            isSearch = false;
            ContentPage.BeginAnimation(OpacityProperty, new DoubleAnimation(0.5, 1, TimeSpan.FromSeconds(0.3)));
        }
        #endregion
        #endregion

        #endregion
        #region 快捷键
        private void LoadHotDog() {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            RegisterHotKey(handle, 124, 1, (uint)System.Windows.Forms.Keys.L);
            RegisterHotKey(handle, 125, 1, (uint)System.Windows.Forms.Keys.S);
            RegisterHotKey(handle, 126, 1, (uint)System.Windows.Forms.Keys.Space);
            RegisterHotKey(handle, 127, 1, (uint)System.Windows.Forms.Keys.Up);
            RegisterHotKey(handle, 128, 1, (uint)System.Windows.Forms.Keys.Down);
            InstallHotKeyHook(this);
            Closed += (s, e) => {
                IntPtr hd = new WindowInteropHelper(this).Handle;
                UnregisterHotKey(hd, 124);
                UnregisterHotKey(hd, 125);
                UnregisterHotKey(hd, 126);
                UnregisterHotKey(hd, 127);
                UnregisterHotKey(hd, 128);
            };
            //notifyIcon
            MusicLib.pc.notifyIcon = new System.Windows.Forms.NotifyIcon();
            MusicLib.pc.notifyIcon.Text = "小萌";
            MusicLib.pc.notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            MusicLib.pc.notifyIcon.Visible = true;
            //打开菜单项
            System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("打开");
            open.Click += delegate { exShow(); };
            //退出菜单项
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("关闭");
            exit.Click += delegate {
                MusicLib.pc.notifyIcon.Dispose();
                MusicLib.pc.Exit();
                if(!App.BaseApp.Apip.HasExited)
                    App.BaseApp.Apip.Kill();
                var dt = Resources["Closing"] as Storyboard;
                dt.Completed += async delegate { await Task.Delay(500); Settings.SaveSettings(); Environment.Exit(0); };
                dt.Begin();
            };
            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { open, exit };
            MusicLib.pc.notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

            MusicLib.pc.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, m) =>
            {
                if (m.Button == System.Windows.Forms.MouseButtons.Left) exShow();
            });
        }

        [DllImport("user32")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint controlKey, uint virtualKey);

        [DllImport("user32")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        public bool InstallHotKeyHook(Window window)
        {
            if (window == null)
                return false;
            WindowInteropHelper helper = new WindowInteropHelper(window);
            if (IntPtr.Zero == helper.Handle)
                return false;
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            if (source == null)
                return false;
            source.AddHook(HotKeyHook);
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
                else if (wParam.ToInt32() == 126)
                { PlayBtn_MouseDown(null, null); Toast.Send("已暂停/播放"); }
                else if (wParam.ToInt32() == 127)
                { Border_MouseDown(null, null); Toast.Send("成功切换到上一曲"); }
                else if (wParam.ToInt32() == 128)
                { Border_MouseDown_1(null, null); Toast.Send("成功切换到下一曲"); }
            }
            return IntPtr.Zero;
        }
        private const int WM_HOTKEY = 0x0312;
        #endregion
        #region 进程通信
        private void LoadSEND_SHOW() {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(hwnd);
            if (source != null) source.AddHook(WndProc);
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg ==MsgHelper.WM_COPYDATA)
            {
                MsgHelper.COPYDATASTRUCT cdata = new MsgHelper.COPYDATASTRUCT();
                Type mytype = cdata.GetType();
                cdata = (MsgHelper.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, mytype);
                if (cdata.lpData == MsgHelper.SEND_SHOW)
                    exShow();
                else if (cdata.lpData.Contains("Api#")){
                    ApiHandle = int.Parse(TextHelper.XtoYGetTo(cdata.lpData, "#", "*", 0));
                    if (IsFirstStart)
                    { IsFirstStart = false; MsgHelper.SendMsg("IsLogin", ApiHandle); }
                }
                else if (cdata.lpData.Contains("Login")){
                   App.BaseApp.Apip.Kill();
                   Console.WriteLine(cdata.lpData);
                    string qq = "2465759834";
                    if (cdata.lpData != "No Login")
                        qq = TextHelper.XtoYGetTo(cdata.lpData, "Login:", "###", 0);
                    Action a = new Action(async () =>
                    {
                        var sl = await HttpHelper.GetWebAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={qq}&reqfrom=1&reqtype=0", Encoding.UTF8);
                        await HttpHelper.HttpDownloadFileAsync($"http://q2.qlogo.cn/headimg_dl?bs=qq&dst_uin={qq}&spec=100", Settings.USettings.CachePath + qq + ".jpg");
                        await Task.Run(() => {
                            Settings.LoadUSettings(qq);
                            if (cdata.lpData.Contains("g_tk"))
                                Settings.USettings.g_tk = TextHelper.XtoYGetTo(cdata.lpData, "g_tk[", "]sk", 0);
                            Settings.USettings.Cookie = TextHelper.XtoYGetTo(cdata.lpData, "Cookie[", "]END", 0);
                            Settings.USettings.UserName = JObject.Parse(sl)["data"]["creator"]["nick"].ToString();
                            Settings.USettings.UserImage = Settings.USettings.CachePath + qq + ".jpg";
                            Settings.USettings.LemonAreeunIts = qq;
                            Settings.SaveSettings();
                            Settings.LSettings.qq = qq;
                            Settings.SaveLocaSettings();
                            Console.WriteLine(Settings.USettings.g_tk + "  " + Settings.USettings.Cookie);
                        });
                        LoadAfterLogin(false);
                    });
                    a();
                }
            }
            return IntPtr.Zero;
        }
        #endregion
    }
}