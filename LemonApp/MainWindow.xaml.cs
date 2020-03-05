using LemonApp.ContentPage;
using LemonApp.Theme;
using LemonLib;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Taskbar;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 一些字段
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
        public MusicLib ml;
        public PlayDLItem MusicData = new PlayDLItem(new Music());
        bool isplay = false;
        bool IsRadio = false;
        string RadioID = "";
        public static MusicPlayer mp;
        int ind = 0;//歌词页面是否打开
        /// <summary>
        /// 播放模式 0列表循环 1单曲循环 2随机播放
        /// </summary>
        int PlayMod = 0;
        bool mod = true;//true : qq false : wy
        LyricView lv;
        bool isLoading = false;
        public NowPage np;
        #endregion
        #region 任务栏 字段
        TabbedThumbnail TaskBarImg;
        ThumbnailToolBarButton TaskBarBtn_Last;
        ThumbnailToolBarButton TaskBarBtn_Play;
        ThumbnailToolBarButton TaskBarBtn_Next;
        #endregion
        #region 控件集
        private HomePage ClHomePage = null;
        private TopPage ClTopPage = null;
        private SingerIndexPage ClSingerIndexPage = null;
        private FLGDIndexPage ClFLGDIndexPage = null;
        private RadioIndexPage ClRadioIndexPage = null;
        private MyFollowSingerList ClMyFollowSingerList = null;
        #endregion
        #region 等待动画
        Thread tOL = null;
        LoadingWindow aw = null;
        public void RunThread()
        {
            try
            {
                aw = new LoadingWindow();
                aw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                aw.Topmost = true;
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
                isLoading = true;
                tOL = new Thread(RunThread);
                tOL.SetApartmentState(ApartmentState.STA);
                tOL.Start();
            }
        }
        public async void CloseLoading()
        {
            if (isLoading)
            {
                await Task.Delay(100);
                isLoading = false;
                aw.Dispatcher.Invoke(() =>
                {
                    var da = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
                    da.Completed += delegate { aw.Close(); };
                    aw.BeginAnimation(OpacityProperty, da);
                });
            }
        }
        #endregion
        #region 窗口加载时
        public MainWindow()
        {
            InitializeComponent();
        }

        #region 加载窗口时的基础配置 登录/播放组件
        private async void window_Loaded(object sender, RoutedEventArgs e)
        {
            //--------检测更新-------
            Updata();
            //----播放组件-----------
            mp = new MusicPlayer(new WindowInteropHelper(this).Handle);
            //--------应用程序配置 热键和消息回调--------
            Settings.Handle.WINDOW_HANDLE = new WindowInteropHelper(this).Handle.ToInt32();
            Settings.Handle.ProcessId = Process.GetCurrentProcess().Id;
            Settings.SaveHandle();
            LoadSEND_SHOW();
            LoadHotDog();
            //-------注册个模糊效果------
            wac = new WindowAccentCompositor(this, (c) =>
            {
                Page.Background = new SolidColorBrush(c);
            });
            //--------登录------
            Settings.LoadLocaSettings();
            if (Settings.LSettings.qq != "")
                await Settings.LoadUSettings(Settings.LSettings.qq);
            else
            {
                string qq = "Public";
                await Settings.LoadUSettings(qq);
                Settings.USettings.LemonAreeunIts = qq;
                Settings.SaveSettings();
                Settings.LSettings.qq = qq;
                Settings.SaveLocaSettings();
            }
            Load_Theme();
            //---------Popup的移动事件
            LocationChanged += delegate
            {
                RUNPopup(SingerListPop);
                RUNPopup(MoreBtn_Meum);
                RUNPopup(Gdpop);
                RUNPopup(IntoGDPop);
                RUNPopup(AddGDPop);
            };
            //---------专辑图是圆的吗??-----
            MusicImage.CornerRadius = new CornerRadius(Settings.USettings.IsRoundMusicImage);
            //---------任务栏 TASKBAR-----------
            //任务栏 缩略图 按钮
            TaskBarImg = new TabbedThumbnail(this, this, new Vector());
            TaskbarManager.Instance.TabbedThumbnail.AddThumbnailPreview(TaskBarImg);
            TaskBarImg.SetWindowIcon(Properties.Resources.icon);
            TaskBarImg.Title = Settings.USettings.Playing.MusicName + " - " + Settings.USettings.Playing.SingerText;
            TaskBarImg.TabbedThumbnailActivated += delegate
            {
                WindowState = WindowState.Normal;
                Activate();
            };
            if (Settings.USettings.Playing.ImageUrl != "")
                TaskBarImg.SetImage(await ImageCacheHelp.GetImageByUrl(Settings.USettings.Playing.ImageUrl));

            TaskBarBtn_Last = new ThumbnailToolBarButton(Properties.Resources.icon_left, "上一曲");
            TaskBarBtn_Last.Enabled = true;
            TaskBarBtn_Last.Click += TaskBarBtn_Last_Click;

            TaskBarBtn_Play = new ThumbnailToolBarButton(Properties.Resources.icon_play, "播放|暂停");
            TaskBarBtn_Play.Enabled = true;
            TaskBarBtn_Play.Click += TaskBarBtn_Play_Click;

            TaskBarBtn_Next = new ThumbnailToolBarButton(Properties.Resources.icon_right, "下一曲");
            TaskBarBtn_Next.Enabled = true;
            TaskBarBtn_Next.Click += TaskBarBtn_Next_Click;

            //添加按钮
            TaskbarManager.Instance.ThumbnailToolBars.AddButtons(this, TaskBarBtn_Last, TaskBarBtn_Play, TaskBarBtn_Next);
            //--------加载主页---------
            ClHomePage = new HomePage(this, TemplateSv.Template);
            ContentPage.Children.Add(ClHomePage);
            NSPage(new MeumInfo(MusicKuBtn, ClHomePage, MusicKuCom), true, false);
            //--------去除可恶的焦点边缘线
            UIHelper.G(Page);
        }
        private void RUNPopup(Popup pp)
        {
            if (pp.IsOpen)
            {
                var offset = pp.HorizontalOffset;
                pp.HorizontalOffset = offset + 1;
                pp.HorizontalOffset = offset;
            }
        }
        private WindowAccentCompositor wac = null;
        /// <summary>
        /// 登录之后的主题配置 不同的账号可能使用不同的主题
        /// </summary>
        /// <param name="hasAnimation"></param>
        private void Load_Theme()
        {
            if (Settings.USettings.Skin_Path == "BlurBlackTheme")
            {
                //----新的[磨砂黑]主题---
                WdBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(32, 32, 32));
                Page.Background = new SolidColorBrush(Colors.Transparent);
                DThemePage.Child = null;
                App.BaseApp.Skin();
                ControlDownPage.Background = new SolidColorBrush(Colors.Transparent);
                wac.Color = Color.FromArgb(200, 0, 0, 0);
                wac.IsEnabled = true;
            }
            else if (Settings.USettings.Skin_Path == "BlurWhiteTheme")
            {
                WdBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180));
                Page.Background = new SolidColorBrush(Colors.Transparent);
                DThemePage.Child = null;
                App.BaseApp.Skin_Black();
                Color co = Color.FromRgb(64, 64, 64);
                App.BaseApp.SetColor("ResuColorBrush", co);
                App.BaseApp.SetColor("ButtonColorBrush", co);
                App.BaseApp.SetColor("TextX1ColorBrush", co);
                ControlDownPage.Background = new SolidColorBrush(Colors.Transparent);
                wac.Color = (Color.FromArgb(200, 255, 255, 255));
                wac.IsEnabled = true;
            }
            else if (Settings.USettings.Skin_Path.Contains("DTheme"))
            {
                string NameSpace = TextHelper.XtoYGetTo(Settings.USettings.Skin_Path, "DTheme[", "]", 0);
                ThemeBase tb = null;
                if (NameSpace == Theme.Dtpp.Drawer.NameSpace)
                    tb = new Theme.Dtpp.Drawer();
                else if (NameSpace == Theme.TheFirstSnow.Drawer.NameSpace)
                    tb = new Theme.TheFirstSnow.Drawer();
                DThemePage.Child = tb;
                //字体颜色
                Color col;
                if (tb.FontColor == "Black")
                {
                    col = Color.FromRgb(64, 64, 64); App.BaseApp.Skin_Black();
                    ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFFFF"));
                }
                else
                {
                    col = Color.FromRgb(255, 255, 255); App.BaseApp.Skin();
                    ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000"));
                }
                Color theme = tb.ThemeColor;
                App.BaseApp.SetColor("ThemeColor", theme);
                App.BaseApp.SetColor("ResuColorBrush", col);
                App.BaseApp.SetColor("ButtonColorBrush", col);
                App.BaseApp.SetColor("TextX1ColorBrush", col);
            }
            else
            {
                if (Settings.USettings.Skin_Path != "")
                {//有主题配置 （非默认）
                    //    主题背景图片
                    if (Settings.USettings.Skin_Path != "" && System.IO.File.Exists(Settings.USettings.Skin_Path))
                    {
                        Page.Background = new ImageBrush(new BitmapImage(new Uri(Settings.USettings.Skin_Path, UriKind.Absolute)));
                    }
                    //字体颜色
                    Color co;
                    if (Settings.USettings.Skin_txt == "Black")
                    {
                        co = Color.FromRgb(64, 64, 64); App.BaseApp.Skin_Black();
                        ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFFFF"));
                    }
                    else
                    {
                        co = Color.FromRgb(255, 255, 255); App.BaseApp.Skin();
                        ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00000000"));
                    }
                    App.BaseApp.SetColor("ThemeColor", Color.FromRgb(byte.Parse(Settings.USettings.Skin_Theme_R),
                        byte.Parse(Settings.USettings.Skin_Theme_G),
                        byte.Parse(Settings.USettings.Skin_Theme_B)));
                    App.BaseApp.SetColor("ResuColorBrush", co);
                    App.BaseApp.SetColor("ButtonColorBrush", co);
                    App.BaseApp.SetColor("TextX1ColorBrush", co);
                }
                else
                {
                    //默认主题  （主要考虑到切换登录）
                    if (wac.IsEnabled) wac.IsEnabled = false;
                    ControlDownPage.SetResourceReference(BorderBrushProperty, "BorderColorBrush");
                    ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CFFFFFF"));
                    Page.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    App.BaseApp.unSkin();
                }
                DThemePage.Child = null;
            }
            //---------------歌词页专辑图转动
            LyricBigAniRound = new Storyboard();
            DependencyProperty[] propertyChain = new DependencyProperty[]{
                      RenderTransformProperty,
                      RotateTransform.AngleProperty };
            DoubleAnimationUsingKeyFrames us = new DoubleAnimationUsingKeyFrames();
            EasingDoubleKeyFrame edf = new EasingDoubleKeyFrame(360, TimeSpan.FromSeconds(15));
            us.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTarget(us, LyricBig);
            Storyboard.SetTargetProperty(us, new PropertyPath("(0).(1)", propertyChain));
            us.KeyFrames.Add(edf);
            LyricBigAniRound.Children.Add(us);
            LoadMusicData();
        }
        private double now = 0;
        private double all = 0;
        private string lastlyric = "";
        private Toast lyricTa = null;
        private void LoadMusicData()
        {
            LoadSettings();
            MainClass.DebugCallBack = (s) =>
            {
                Console.WriteLine(s);
            };
            //-------[登录]用户的头像、名称等配置加载
            if (Settings.USettings.UserName != string.Empty)
            {
                UserName.Text = Settings.USettings.UserName;
                if (System.IO.File.Exists(Settings.USettings.UserImage))
                {
                    var image = new System.Drawing.Bitmap(Settings.USettings.UserImage);
                    UserTX.Background = new ImageBrush(image.ToImageSource());
                    image.Dispose();
                }
                Thread t = new Thread(async () =>
                {
                    var sl = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={Settings.USettings.LemonAreeunIts}&reqfrom=1&reqtype=0", Encoding.UTF8);
                    Debug.WriteLine(sl);
                    JObject j = JObject.Parse(sl);
                    if (j["code"].ToString() == "0")
                    {
                        var sdc = JObject.Parse(sl)["data"]["creator"];
                        await HttpHelper.HttpDownloadFileAsync(sdc["headpic"].ToString().Replace("http://", "https://"), Settings.USettings.CachePath + Settings.USettings.LemonAreeunIts + ".jpg");
                        string name = sdc["nick"].ToString();
                        Settings.USettings.UserName = name;
                        var image = new System.Drawing.Bitmap(Settings.USettings.UserImage);
                        Dispatcher.Invoke(() =>
                        {
                            UserName.Text = name;
                            UserTX.Background = new ImageBrush(image.ToImageSource());
                        });
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if (TwMessageBox.Show("登录已失效，请重新登录！"))
                                UserTX_MouseDown(null, null);
                        });
                    }
                });
                t.Start();
            }
            //-----歌词显示 歌曲播放 等组件的加载
            lv = new LyricView();
            lv.NoramlLrcColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            lv.TextAlignment = TextAlignment.Center;
            ly.Child = lv;
            lv.NextLyric += (text) =>
            {
                //主要用于桌面歌词的显示
                if (Settings.USettings.DoesOpenDeskLyric)
                {
                    if (lastlyric != text) if (text != "")
                            lyricTa.Updata(text);
                    lastlyric = text;
                }
            };
            ml = new MusicLib(Settings.USettings.LemonAreeunIts);
            if (Settings.USettings.DoesOpenDeskLyric == true)
            {
                lyricTa = new Toast("", true);
                path7.SetResourceReference(Path.FillProperty, "ThemeColor");
            }
            //---------加载上一次播放
            if (Settings.USettings.Playing.MusicName != "")
            {
                MusicData = new PlayDLItem(Settings.USettings.Playing);
                PlayMusic(Settings.USettings.Playing, false);
            }
            //--------播放时的Timer 进度/歌词
            t.Interval = 500;
            t.Tick += delegate
            {
                try
                {
                    now = mp.Position.TotalMilliseconds;
                    if (CanJd)
                    {
                        jd.Value = now;
                        Play_Now.Text = TimeSpan.FromMilliseconds(now).ToString(@"mm\:ss");
                    }
                    all = mp.GetLength.TotalMilliseconds;
                    string alls = TimeSpan.FromMilliseconds(all).ToString(@"mm\:ss");
                    Play_All.Text = alls;
                    jd.Maximum = all;
                    if (ind == 1)
                    {
                        if (Settings.USettings.LyricAnimationMode == 0)
                        {
                            float[] data = mp.GetFFTData();
                            float sv = 0;
                            foreach (var c in data)
                                if (c > 0.06)
                                {
                                    sv = c;
                                    break;
                                }
                            if (sv != 0)
                            {
                                Border b = new Border();
                                b.BorderThickness = new Thickness(1);
                                b.BorderBrush = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255));
                                b.Height = LyricBig.ActualHeight;
                                b.Width = LyricBig.ActualWidth;
                                b.CornerRadius = LyricBig.CornerRadius;
                                b.HorizontalAlignment = HorizontalAlignment.Center;
                                b.VerticalAlignment = VerticalAlignment.Center;
                                var v = b.Height + sv * 500;
                                Storyboard s = (Resources["LyricAnit"] as Storyboard).Clone();
                                var f = s.Children[0] as DoubleAnimationUsingKeyFrames;
                                (f.KeyFrames[0] as SplineDoubleKeyFrame).Value = v;
                                Storyboard.SetTarget(f, b);
                                var f1 = s.Children[1] as DoubleAnimationUsingKeyFrames;
                                (f1.KeyFrames[0] as SplineDoubleKeyFrame).Value = v;
                                Storyboard.SetTarget(f1, b);
                                var f2 = s.Children[2] as DoubleAnimationUsingKeyFrames;
                                Storyboard.SetTarget(f2, b);
                                s.Completed += delegate { LyricAni.Children.Remove(b); };
                                LyricAni.Children.Add(b);
                                s.Begin();
                            }
                        }
                        lv.LrcRoll(now, true);
                    }
                    else lv.LrcRoll(now, false);
                    if (now == all && now > 2000 && all != 0)
                    {
                        now = 0;
                        all = 0;
                        mp.Position = TimeSpan.FromSeconds(0);
                        t.Stop();
                        //-----------播放完成时，判断单曲还是下一首
                        jd.Value = 0;
                        if (PlayMod == 1)//单曲循环
                        {
                            mp.Position = TimeSpan.FromMilliseconds(0);
                            mp.Play();
                            t.Start();
                        }
                        else if (PlayMod == 0 || PlayMod == 2) PlayControl_PlayNext(null, null);//下一曲
                    }
                }
                catch { }
            };
            //-----Timer 清理与更新播放设备
            var ds = new System.Windows.Forms.Timer() { Interval = 5000 };
            ds.Tick += delegate { if (t.Enabled) mp.UpdataDevice(); GC.Collect(); };
            ds.Start();
            //---------------MVPlayer Timer
            mvt.Interval = 1000;
            mvt.Tick += Mvt_Tick;
            //----------------同步"我喜欢"歌单ids
            Thread tx = new Thread(async ()=> {
                Dictionary<string, string> dt = new Dictionary<string, string>();
                MusicLib.GetGDAsync(MusicLib.MusicLikeGDid??await ml.GetMusicLikeGDid(),new Action<string,string>((mid,id) => {
                    dt.Add(mid,id);
                }));
                Settings.USettings.MusicGDataLike.ids = dt;
            });
            tx.Start();
        }
        #endregion
        #region 窗口控制 最大化/最小化/显示/拖动
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();//窗口移动
        }
        /// <summary>
        /// 显示窗口
        /// </summary>
        private void exShow()
        {
            WindowState = WindowState.Normal;
            Show();
            Activate();
        }
        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Hide();
        }
        /// <summary>
        /// 窗口最大化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaxBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }
        /// <summary>
        /// 窗口最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //------------调整大小时对控件进行伸缩---------------
            WidthUI(GDItemsList);
            WidthUI(GDILikeItemsList);
            WidthUI(SkinIndexList);

            if (MusicPlList.Visibility == Visibility.Visible)
                foreach (UserControl dx in MusicPlList.Children)
                    dx.Width = ContentPage.ActualWidth;
            if (Data.Visibility == Visibility.Visible)
                foreach (DataItem dx in DataItemsList.Items)
                    dx.Width = ContentPage.ActualWidth;
            if (SingerDataPage.Visibility == Visibility.Visible)
                (Cisv.Content as FrameworkElement).Width = SingerDataPage.ActualWidth;
        }
        /// <summary>
        /// 遍历调整宽度
        /// </summary>
        /// <param name="wp"></param>
        public void WidthUI(Panel wp, double? ContentWidth = null)
        {
            if (wp.Visibility == Visibility.Visible && wp.Children.Count > 0)
            {
                int lineCount = int.Parse(wp.Uid);
                var uc = wp.Children[0] as UserControl;
                double max = uc.MaxWidth;
                double min = uc.MinWidth;
                ContentWidth = ContentWidth ?? ContentPage.ActualWidth;
                if (ContentWidth > (24 + max) * lineCount)
                    lineCount++;
                else if (ContentWidth < (24 + min) * lineCount)
                    lineCount--;
                WidTX(wp, lineCount, (double)ContentWidth);
            }
        }

        private void WidTX(Panel wp, int lineCount, double ContentWidth)
        {
            foreach (UserControl dx in wp.Children)
                dx.Width = (ContentWidth - 24 * lineCount) / lineCount;
        }
        #endregion
        #endregion
        #region Login 登录
        public void Login(string cdata)
        {
            lw.Close();
            Console.WriteLine(cdata);
            string qq = "";
            if (cdata != "No Login")
                qq = TextHelper.XtoYGetTo(cdata, "Login:", "###", 0);
            if (Settings.USettings.LemonAreeunIts == qq)
            {
                if (cdata.Contains("g_tk"))
                {
                    Settings.USettings.g_tk = TextHelper.XtoYGetTo(cdata, "g_tk[", "]sk", 0);
                    Settings.USettings.Cookie = TextHelper.XtoYGetTo(cdata, "Cookie[", "]END", 0);
                    Settings.SaveSettings();
                }
            }
            else
            {
                //此方法中不能使用Async异步，故使用Action
                Action a = new Action(async () =>
                {
                    if (cdata.Contains("g_tk"))
                    {
                        Settings.USettings.g_tk = TextHelper.XtoYGetTo(cdata, "g_tk[", "]sk", 0);
                        Settings.USettings.Cookie = TextHelper.XtoYGetTo(cdata, "Cookie[", "]END", 0);
                    }
                    var sl = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={qq}&reqfrom=1&reqtype=0", Encoding.UTF8);
                    Console.WriteLine(sl);
                    var sdc = JObject.Parse(sl)["data"]["creator"];
                    await HttpHelper.HttpDownloadFileAsync(sdc["headpic"].ToString().Replace("http://", "https://"), Settings.USettings.CachePath + qq + ".jpg");
                    string name = sdc["nick"].ToString();
                    await Task.Run(async () =>
                    {
                        await Settings.LoadUSettings(qq);
                        if (cdata.Contains("g_tk"))
                        {
                            Settings.USettings.g_tk = TextHelper.XtoYGetTo(cdata, "g_tk[", "]sk", 0);
                            Settings.USettings.Cookie = TextHelper.XtoYGetTo(cdata, "Cookie[", "]END", 0);
                        }
                        Settings.USettings.UserName = name;
                        Settings.USettings.UserImage = Settings.USettings.CachePath + qq + ".jpg";
                        Settings.USettings.LemonAreeunIts = qq;
                        Settings.SaveSettings();
                        Settings.LSettings.qq = qq;
                        Settings.SaveLocaSettings();
                        Console.WriteLine(Settings.USettings.g_tk + "  " + Settings.USettings.Cookie);
                        this.Dispatcher.Invoke(() => { Load_Theme(); });
                    });
                });
                a();
            }
        }
        #endregion
        #region 设置
        public void LoadSettings()
        {
            CachePathTb.Text = Settings.USettings.CachePath;
            DownloadPathTb.Text = Settings.USettings.DownloadPath;
            DownloadWithLyric.IsChecked = Settings.USettings.DownloadWithLyric;
            DownloadNameTb.Text = Settings.USettings.DownloadName;

        }
        private void SettingsPage_URLink_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer", SettingsPage_URLink.Text);
        }
        private void UserSendButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //⚠警告!!!: 以下key仅供本开发者(TwilightLemon)使用,
            //               若发现滥用现象，将走法律程序解决!!!
            //KEY: xfttsuxaeivzdefd
            if (UserSendText.Text != "在此处输入你的建议或问题")
            {
                va.Text = "发送中...";
                string body = "Lemon App 版本号:" + App.EM +
                    "\r\nUserAddress:" + knowb.Text +
                    "\r\nUserID:" + Settings.USettings.LemonAreeunIts +
                    "\r\n  \r\n"
                    + UserSendText.Text;
                Task.Run(new Action(() =>
                {
                    MailMessage mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress("lemon.app@qq.com");
                    mailMessage.To.Add(new MailAddress("2728578956@qq.com"));
                    mailMessage.Subject = "Lemon App用户反馈";
                    mailMessage.Body = body;
                    //添加附件...
                    if (HasFJ)
                        foreach (var file in USFJFilePath)
                            mailMessage.Attachments.Add(new Attachment(file));
                    SmtpClient client = new SmtpClient();
                    client.Host = "smtp.qq.com";
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("lemon.app@qq.com", "xfttsuxaeivzdefd");
                    client.Send(mailMessage);
                    Dispatcher.Invoke(() => va.Text = "发送成功!");
                }));
            }
            else va.Text = "请输入";
        }
        private string[] USFJFilePath = null;
        private bool HasFJ = false;
        private void UserSend_fj_Drag(object sender, DragEventArgs e)
        {
            HasFJ = true;
            USFJFilePath = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (USFJFilePath.Count() > 1)
                UserSend_fj.TName = "已选多个文件";
            else
            {
                System.IO.FileInfo f = new System.IO.FileInfo(USFJFilePath[0]);
                UserSend_fj.TName = f.Name;
            }
        }
        private void UserSend_fj_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Multiselect = true;
            o.ShowDialog();
            USFJFilePath = o.FileNames;
            if (USFJFilePath.Count() > 1)
                UserSend_fj.TName = "已选多个文件";
            else
            {
                System.IO.FileInfo f = new System.IO.FileInfo(USFJFilePath[0]);
                UserSend_fj.TName = f.Name;
            }
        }

        private void SettingsBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LoadSettings();
            NSPage(new MeumInfo(null, SettingsPage, null));
        }

        private void CP_ChooseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var g = new System.Windows.Forms.FolderBrowserDialog();
            if (g.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CachePathTb.Text = g.SelectedPath;
                Settings.USettings.CachePath = g.SelectedPath;
            }
        }

        private void DP_ChooseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var g = new System.Windows.Forms.FolderBrowserDialog();
            if (g.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DownloadPathTb.Text = g.SelectedPath;
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

        private void DownloadWithLyric_Click(object sender, RoutedEventArgs e)
        {
            Settings.USettings.DownloadWithLyric = (bool)DownloadWithLyric.IsChecked;
        }

        private void DownloadNameOK_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Settings.USettings.DownloadName = DownloadNameTb.Text;
        }
        #endregion
        #region 主题切换
        #region 自定义主题
        private void SkinPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SkinIndexList.Children.Clear();
        }
        string TextColor_byChoosing = "Black";
        private void ColorThemeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChooseText.Visibility = Visibility.Visible;
            Theme_Choose_Color = (Skin_ChooseBox_Theme.Background as SolidColorBrush).Color;
        }
        private void Border_MouseDown_4(object sender, MouseButtonEventArgs e)
        {
            TextColor_byChoosing = "White";
            Skin_ChooseBox_Font.Background = (sender as Border).BorderBrush;
        }
        private void Border_MouseDown_5(object sender, MouseButtonEventArgs e)
        {
            TextColor_byChoosing = "Black";
            Skin_ChooseBox_Font.Background = new SolidColorBrush(Colors.Black);
        }
        private void MDButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Color co;
            if (TextColor_byChoosing == "Black")
            {
                co = Color.FromRgb(64, 64, 64); App.BaseApp.Skin_Black();
                ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFFFF"));
            }
            else
            {
                co = Color.FromRgb(255, 255, 255); App.BaseApp.Skin();
                ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFFFF"));
            }
            App.BaseApp.SetColor("ThemeColor", Theme_Choose_Color);
            App.BaseApp.SetColor("ResuColorBrush", co);
            App.BaseApp.SetColor("ButtonColorBrush", co);
            App.BaseApp.SetColor("TextX1ColorBrush", co);
            Settings.USettings.Skin_txt = TextColor_byChoosing;
            Settings.USettings.Skin_Theme_R = Theme_Choose_Color.R.ToString();
            Settings.USettings.Skin_Theme_G = Theme_Choose_Color.G.ToString();
            Settings.USettings.Skin_Theme_B = Theme_Choose_Color.B.ToString();
            Settings.SaveSettings();
            ChooseText.Visibility = Visibility.Collapsed;
        }
        private void ThemeChooseCloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChooseText.Visibility = Visibility.Collapsed;
        }
        Color Theme_Choose_Color;
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Theme_Choose_Color = Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
                Skin_ChooseBox_Theme.Background = new SolidColorBrush(Theme_Choose_Color);
            }
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
                Task.Run(new Action(() =>
                {
                    string strFileName = ofd.FileName;
                    string file = Settings.USettings.CachePath + "Skin\\" + TextHelper.MD5.EncryptToMD5string(System.IO.File.ReadAllText(strFileName)) + System.IO.Path.GetExtension(strFileName);
                    System.IO.File.Copy(strFileName, file, true);
                    Dispatcher.Invoke(new Action(() =>
                    Page.Background = new ImageBrush(new System.Drawing.Bitmap(file).ToImageSource())));
                    Settings.USettings.Skin_Path = file;
                }));
            }
        }
        #endregion
        private void LoadDTheme(ThemeBase bg)
        {
            string ThemeName = bg.ThemeName;
            bg.Clip = new RectangleGeometry(new Rect() { Height = 450, Width = 800 });
            bg.Width = 800;
            bg.Height = 450;
            VisualBrush vb = new VisualBrush(bg);
            vb.Stretch = Stretch.Fill;
            string font = bg.FontColor;
            Color theme = bg.ThemeColor;
            SkinControl sc = new SkinControl(ThemeName, vb, theme);
            sc.txtColor = font;
            sc.Margin = new Thickness(12, 0, 12, 20);
            sc.MouseDown += (s, n) =>
            {
                if (wac.IsEnabled) wac.IsEnabled = false;
                var bgl = bg.GetPage();
                DThemePage.Child = bgl;
                Color co;
                if (sc.txtColor == "Black")
                {
                    co = Color.FromRgb(64, 64, 64); App.BaseApp.Skin_Black();
                    ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFFFF"));
                }
                else
                {
                    co = Color.FromRgb(255, 255, 255); App.BaseApp.Skin();
                    ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFFFF"));
                }
                App.BaseApp.SetColor("ThemeColor", sc.theme);
                App.BaseApp.SetColor("ResuColorBrush", co);
                App.BaseApp.SetColor("ButtonColorBrush", co);
                App.BaseApp.SetColor("TextX1ColorBrush", co);
                Settings.USettings.Skin_Path = "DTheme[" + bg + "]";
                Settings.USettings.Skin_txt = ThemeName;
                Settings.SaveSettings();
            };
            SkinIndexList.Children.Add(sc);
        }
        private async void SkinBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(new MeumInfo(null, SkinPage, null));
            SkinIndexList.Children.Clear();
            #region 动态皮肤
            LoadDTheme(new Theme.Dtpp.Drawer());
            LoadDTheme(new Theme.TheFirstSnow.Drawer());
            #endregion
            #region 默认主题
            SkinControl sxc = new SkinControl("-1", "默认主题", Color.FromArgb(0, 0, 0, 0));
            sxc.MouseDown += (s, n) =>
            {
                if (wac.IsEnabled) wac.IsEnabled = false;
                ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CFFFFFF"));
                Page.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                DThemePage.Child = null;
                App.BaseApp.unSkin();
                Settings.USettings.Skin_txt = "";
                Settings.USettings.Skin_Path = "";
                Settings.SaveSettings();
            };
            sxc.Margin = new Thickness(12, 0, 12, 20);
            SkinIndexList.Children.Add(sxc);
            #endregion
            #region 磨砂主题
            SkinControl blur = new SkinControl("-2", "磨砂黑", Color.FromArgb(0, 0, 0, 0));
            blur.MouseDown += (s, n) =>
            {
                Page.Background = new SolidColorBrush(Colors.Transparent);
                WdBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(32, 32, 32));
                DThemePage.Child = null;
                App.BaseApp.Skin();
                ControlDownPage.Background = new SolidColorBrush(Colors.Transparent);
                wac.Color = Color.FromArgb(200, 0, 0, 0);
                wac.IsEnabled = true;
                Settings.USettings.Skin_txt = "";
                Settings.USettings.Skin_Path = "BlurBlackTheme";
                Settings.SaveSettings();
            };
            blur.Margin = new Thickness(12, 0, 12, 20);
            SkinIndexList.Children.Add(blur);
            SkinControl blurWhite = new SkinControl("-3", "亚克力白", Color.FromArgb(255, 240, 240, 240));
            blurWhite.MouseDown += (s, n) =>
            {
                Page.Background = new SolidColorBrush(Colors.Transparent);
                WdBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180));
                DThemePage.Child = null;
                App.BaseApp.Skin_Black();
                Color co = Color.FromRgb(64, 64, 64);
                App.BaseApp.SetColor("ResuColorBrush", co);
                App.BaseApp.SetColor("ButtonColorBrush", co);
                App.BaseApp.SetColor("TextX1ColorBrush", co);
                ControlDownPage.Background = new SolidColorBrush(Colors.Transparent);
                wac.Color = (Color.FromArgb(200, 255, 255, 255));
                wac.IsEnabled = true;
                Settings.USettings.Skin_txt = "";
                Settings.USettings.Skin_Path = "BlurWhiteTheme";
                Settings.SaveSettings();
            };
            blurWhite.Margin = new Thickness(12, 0, 12, 20);
            SkinIndexList.Children.Add(blurWhite);
            #endregion
            #region 在线主题
            var json = JObject.Parse(await HttpHelper.GetWebAsync("https://gitee.com/TwilightLemon/ux/raw/master/SkinList.json"))["dataV2"];
            foreach (var dx in json)
            {
                string name = dx["name"].ToString();
                string uri = dx["uri"].ToString();
                Color color = Color.FromRgb(byte.Parse(dx["ThemeColor"]["R"].ToString()),
                    byte.Parse(dx["ThemeColor"]["G"].ToString()),
                    byte.Parse(dx["ThemeColor"]["B"].ToString()));
                if (!System.IO.File.Exists(Settings.USettings.CachePath + "Skin\\" + uri + ".jpg"))
                    await HttpHelper.HttpDownloadFileAsync($"https://gitee.com/TwilightLemon/ux/raw/master/w{uri}.jpg", Settings.USettings.CachePath + "Skin\\" + uri + ".jpg");
                SkinControl sc = new SkinControl(uri, name, color);
                sc.txtColor = dx["TextColor"].ToString();
                sc.MouseDown += async (s, n) =>
                {
                    if (wac.IsEnabled) wac.IsEnabled = false;
                    if (!System.IO.File.Exists(Settings.USettings.CachePath + "Skin\\" + sc.imgurl + ".png"))
                        await HttpHelper.HttpDownloadFileAsync($"https://gitee.com/TwilightLemon/ux/raw/master/{sc.imgurl}.png", Settings.USettings.CachePath + "Skin\\" + sc.imgurl + ".png");
                    Page.Background = new ImageBrush(new System.Drawing.Bitmap(Settings.USettings.CachePath + "Skin\\" + sc.imgurl + ".png").ToImageSource());
                    DThemePage.Child = null;
                    Color co;
                    if (sc.txtColor == "Black")
                    {
                        co = Color.FromRgb(64, 64, 64); App.BaseApp.Skin_Black();
                        ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFFFF"));
                    }
                    else
                    {
                        co = Color.FromRgb(255, 255, 255); App.BaseApp.Skin();
                        ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFFFF"));
                    }
                    App.BaseApp.SetColor("ThemeColor", sc.theme);
                    App.BaseApp.SetColor("ResuColorBrush", co);
                    App.BaseApp.SetColor("ButtonColorBrush", co);
                    App.BaseApp.SetColor("TextX1ColorBrush", co);
                    Settings.USettings.Skin_Path = Settings.USettings.CachePath + "Skin\\" + sc.imgurl + ".png";
                    Settings.USettings.Skin_txt = sc.txtColor;
                    Settings.USettings.Skin_Theme_R = sc.theme.R.ToString();
                    Settings.USettings.Skin_Theme_G = sc.theme.G.ToString();
                    Settings.USettings.Skin_Theme_B = sc.theme.B.ToString();
                    Settings.SaveSettings();
                };
                sc.Margin = new Thickness(12, 0, 12, 20);
                SkinIndexList.Children.Add(sc);
            }
            #endregion
            WidthUI(SkinIndexList);
        }
        #endregion
        #region 功能区
        #region HomePage 主页
        //IFV的回调函数
        public async void IFVCALLBACK_LoadAlbum(string id, bool NeedSave = true)
        {
            np = NowPage.GDItem;
            DataCollectBtn.Visibility = Visibility.Collapsed;
            DataItemsList.Opacity = 0;
            NSPage(new MeumInfo(null, Data, null) { cmd = "[DataUrl]{\"type\":\"Album\",\"key\":\"" + id + "\"}" }, NeedSave, false);
            DataItemsList.Items.Clear();
            int count = (int)(DataItemsList.ActualHeight
            / 45);
            int index = 0;
            var dta = await MusicLib.GetAlbumSongListByIDAsync(id, new Action<Music, bool>((j, f) =>
            {
                var k = new DataItem(j, this, index) { Width = ContentPage.ActualWidth };
                DataItemsList.Items.Add(k);
                k.GetToSingerPage += K_GetToSingerPage;
                k.Play += PlayMusic;
                k.Download += K_Download;
                if (k.music.MusicID == MusicData.Data.MusicID)
                    k.ShowDx();
                index++;
            }), this, async (md) =>
            {
                DataPage_TX.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(md.Creater.Photo, new int[2] { 36, 36 }));
                DataPage_Creater.Text = md.Creater.Name;
                DataPage_Sim.Text = md.desc;
                TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(md.pic));
                TB.Text = md.name;
            }, count);
            RunAnimation(DataItemsList,new Thickness(0, 200, 0, 0));
        }
        #endregion
        #region Top 排行榜
        /// <summary>
        /// 选择了TOP项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Top_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GetTopItems(sender as TopControl);
        }
        /// <summary>
        /// 加载TOP项
        /// </summary>
        /// <param name="g">Top ID</param>
        /// <param name="osx">页数</param>
        private async void GetTopItems(TopControl g, int osx = 1, bool NeedSave = true)
        {
            np = NowPage.Top;
            tc_now = g;
            ixTop = osx;
            OpenLoading();
            if (osx == 1)
            {
                DataCollectBtn.Visibility = Visibility.Collapsed;
                DataItemsList.Opacity = 0;
                NSPage(new MeumInfo(null, Data, null) { cmd = "[DataUrl]{\"type\":\"Top\",\"key\":\"" + g.Data.ID + "\",\"name\":\"" + g.Data.Name + "\",\"img\":\"" + g.Data.Photo + "\"}" }, NeedSave, false);
                DataPage_TX.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl("https://y.qq.com/favicon.ico"));
                DataPage_Creater.Text = "QQ音乐官方";
                DataPage_Sim.Text = g.Data.desc;
                TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(g.Data.Photo));
                TB.Text = g.Data.Name;
                DataItemsList.Items.Clear();
            }
            int index = 0;
            var dta = await ml.GetToplistAsync(g.Data.ID, new Action<Music, bool>((j, f) =>
            {
                var k = new DataItem(j, this, index) { Width = ContentPage.ActualWidth };
                DataItemsList.Items.Add(k);
                k.GetToSingerPage += K_GetToSingerPage;
                k.Play += PlayMusic;
                k.Download += K_Download;
                if (k.music.MusicID == MusicData.Data.MusicID)
                    k.ShowDx();
                if (DataPage_ControlMod)
                {
                    k.MouseDown -= PlayMusic;
                    k.NSDownload(true);
                    k.Check(true);
                }
                index++;
            }), this, new Action(() =>
            {
                CloseLoading();
            }),osx);
            if (osx == 1)
                RunAnimation(DataItemsList, new Thickness(0, 200, 0, 0));
        }
        #endregion
        #region Updata 检测更新
        private async void Updata()
        {
            var o = JObject.Parse(await HttpHelper.GetWebAsync("https://gitee.com/TwilightLemon/UpdataForWindows/raw/master/WindowsUpdata.json"));
            string v = o["version"].ToString();
            string dt = o["description"].ToString().Replace("@32", "\n");
            if (int.Parse(v) > int.Parse(App.EM))
            {
                new UpdataBox(v, dt).ShowDialog();
            }
        }
        #endregion
        #region N/S Page 切换页面
        public void RunAnimation(DependencyObject TPage, Thickness value = new Thickness())
        {
            var sb = Resources["NSPageAnimation"] as Storyboard;
            foreach (Timeline ac in sb.Children)
            {
                Storyboard.SetTarget(ac, TPage);
                if (ac is ThicknessAnimationUsingKeyFrames)
                {
                    var ta = ac as ThicknessAnimationUsingKeyFrames;
                    ta.KeyFrames[0].Value = new Thickness(200, value.Top, -200, value.Bottom);
                    ta.KeyFrames[1].Value = value;
                }
            }
            sb.Begin();
        }

        private TextBlock LastClickLabel = null;
        private UIElement LastPage = null;
        private Border LastCom = null;
        public void NSPage(MeumInfo data, bool needSave = true, bool Check = true)
        {
            if (data.Page == Data)
                if (DataPage_ControlMod)
                    CloseDataControlPage();
            if (LastClickLabel == null) LastClickLabel = MusicKuBtn;
            LastClickLabel.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            if (data.tb != null) data.tb.SetResourceReference(ForegroundProperty, "ThemeColor");
            if (LastPage == null) LastPage = ClHomePage;
            if (LastCom == null) LastCom = MusicKuCom;
            LastCom.Visibility = Visibility.Collapsed;
            LastPage.Visibility = Visibility.Collapsed;
            //-----cmd处理----
            if (Check)
            {
                //歌曲页 items
                if (data.cmd.Contains("DataUrl"))
                {
                    if (data.Page.Uid != data.cmd)
                    {
                        if (data.cmd == "DataUrl[ILike]")
                            LoadILikeItems(false);
                        else
                        {
                            JObject o = JObject.Parse(data.cmd.Replace("[DataUrl]", ""));
                            string type = o["type"].ToString();
                            string key = o["key"].ToString();
                            switch (type)
                            {
                                case "Search":
                                    SearchMusic(key, 0, false);
                                    break;
                                case "GD":
                                    string name = o["name"].ToString();
                                    string img = o["img"].ToString();
                                    var a = new FLGDIndexItem() { id = key, img = img };
                                    a.name.Text = name;
                                    LoadFxGDItems(a, false);
                                    break;
                                case "Album":
                                    IFVCALLBACK_LoadAlbum(key, false);
                                    break;
                                case "Top":
                                    string nam = o["name"].ToString();
                                    string im = o["img"].ToString();
                                    var b = new TopControl(new MusicTop() { ID = key, Name = nam, Photo = im });
                                    GetTopItems(b, 0, false);
                                    break;
                            }
                        }
                    }
                }
                //歌手详细页
                else if (data.cmd.Contains("Singer"))
                {
                    if(data.cmd=="SingerBig")
                        SetTopWhite(true);
                    if (singer_now != data.data)
                    {
                        GetSinger(new SingerItem((MusicSinger)data.data),false);
                        return;
                    }
                }
            }
            //------------------
            data.Page.Uid = data.cmd;
            if (data.Com != null) data.Com.Visibility = Visibility.Visible;
            data.Page.Visibility = Visibility.Visible;
            RunAnimation(data.Page, data.value);
            if (data.tb != null) LastClickLabel = data.tb;
            LastPage = data.Page;
            LastCom = data.Com;
            if (needSave)
            {
                AddPage(data);
            }
        }
        private void TopBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ClTopPage == null)
            {
                ClTopPage = new TopPage(this, TemplateSv.Template);
                ContentPage.Children.Add(ClTopPage);
            }
            else { ClTopPage.LoadTopData(); }
            NSPage(new MeumInfo(TopBtn, ClTopPage, TopCom), true, false);
        }
        private void MusicKuBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(new MeumInfo(MusicKuBtn, ClHomePage, MusicKuCom));
            ClHomePage.LoadHomePage();
        }
        //前后导航仪
        int QHNowPageIndex = 0;
        List<MeumInfo> PageData = new List<MeumInfo>();
        public void AddPage(MeumInfo data)
        {
            if (PageData.Count != 0)
                while (!(PageData.Count - 1).Equals(QHNowPageIndex))
                {
                    PageData.RemoveAt(PageData.Count - 1);
                }
            PageData.Add(data);
            QHNowPageIndex = PageData.Count - 1;
            foreach (var a in PageData)
            {
                Console.WriteLine(PageData.IndexOf(a) + " " + a.Page);
            }
            Console.WriteLine(QHNowPageIndex);
        }

        private void LastPageBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (QHNowPageIndex != 0)
            {
                QHNowPageIndex--;
                var a = PageData[QHNowPageIndex];
                NSPage(a, false, true);
            }
            Console.WriteLine(QHNowPageIndex);
        }

        private void NextPageBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (QHNowPageIndex != PageData.Count - 1)
            {
                QHNowPageIndex++;
                var a = PageData[QHNowPageIndex];
                NSPage(a, false, true);
            }
            Console.WriteLine(QHNowPageIndex);
        }
        #endregion
        #region Singer 歌手界面
        public void SetTopWhite(bool h)
        {
            if (h)
            {
                SearchBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#19000000"));
                SearchBox.Foreground = new SolidColorBrush(Colors.White);
                (LastPageBtn.Child as Path).Fill = SearchBox.Foreground;
                (NextPageBtn.Child as Path).Fill = SearchBox.Foreground;
                SkinBtn.ColorDx = SearchBox.Foreground;
                SettingsBtn.ColorDx = SearchBox.Foreground;
                CloseBtn.ColorDx = SearchBox.Foreground;
                MaxBtn.ColorDx = SearchBox.Foreground;
                MinBtn.ColorDx = SearchBox.Foreground;
            }
            else
            {
                SearchBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0C000000"));
                SearchBox.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                (LastPageBtn.Child as Path).SetResourceReference(Path.FillProperty, "ButtonColorBrush");
                (NextPageBtn.Child as Path).SetResourceReference(Path.FillProperty, "ButtonColorBrush");
                SkinBtn.ColorDx = null;
                SettingsBtn.ColorDx = null;
                CloseBtn.ColorDx = null;
                MaxBtn.ColorDx = null;
                MinBtn.ColorDx = null;
            }
        }

        private void Cisv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (SingerDP_Top.Uid == "ok")
            {
                if (Cisv.VerticalOffset >= 350)
                {
                    if (SingerDP_Top.Visibility == Visibility.Collapsed)
                    {
                        SingerDP_Top.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    SingerDP_Top.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SingerDataPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (SingerDataPage.Visibility == Visibility.Collapsed)
                SetTopWhite(false);
        }

        public void K_GetToSingerPage(MusicSinger ms)
        {
            var msx = ms;
            Console.WriteLine(ms.Mid);
            msx.Photo = $"https://y.gtimg.cn/music/photo_new/T001R300x300M000{msx.Mid}.jpg?max_age=2592000";
            GetSinger(new SingerItem(msx), null);
        }
        public void GetSinger(object sender, MouseEventArgs e)
        {
            SingerItem si = sender as SingerItem;
            GetSinger(si);
        }
        public async void GetSinger(SingerItem si, bool NeedSavePage=true)
        {
            np = NowPage.SingerItem;
            singer_now = si.data;
            OpenLoading();
            BtD.LastBt = null;
            Cisv.Content = null;
            var data = await MusicLib.GetSingerPageAsync(si.data.Mid);
            var cc = new SingerPage(data, this, new Action(async () =>
            {
                if (data.HasBigPic)
                {
                    await Task.Delay(100);
                    NSPage(new MeumInfo(SingerBtn, SingerDataPage, SingerCom) { value = new Thickness(0, -50, 0, 0), cmd = "SingerBig",data= si.data }, NeedSavePage, false);
                }
                else
                {
                    await Task.Delay(100);
                    NSPage(new MeumInfo(SingerBtn, SingerDataPage, SingerCom) {cmd = "Singer", data= si.data },NeedSavePage,false);
                    }
            }))
            {
                Width = ContentPage.ActualWidth
            };
            Cisv.Content = cc;
            SingerDP_Top.Uid = "gun";
            Cisv.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0)));
            if (data.HasBigPic)
            {
                SetTopWhite(true);
                SingerDP_Top.Visibility = Visibility.Visible;

                var im = await ImageCacheHelp.GetImageByUrl(data.mSinger.Photo);
                var rect = new System.Drawing.Rectangle(0, 0, im.PixelWidth, im.PixelHeight);
                var imb = im.ToBitmap();
                imb.GaussianBlur(ref rect, 80);
                SingerDP_Top.Background = new ImageBrush(imb.ToBitmapImage()) { Stretch = Stretch.UniformToFill };
                SingerDP_Top.Uid = "ok";
            }
            else
            {
                SetTopWhite(false);
                SingerDP_Top.Visibility = Visibility.Collapsed;
            }
            cc.Load();
            CloseLoading();
        }

        private void SingerBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ClSingerIndexPage == null)
            {
                ClSingerIndexPage = new SingerIndexPage(this, TemplateSv.Template, SingerGetToIFollow);
                ContentPage.Children.Add(ClSingerIndexPage);
            }
            NSPage(new MeumInfo(SingerBtn, ClSingerIndexPage, SingerCom), true, false);
        }
        private void SingerGetToIFollow()
        {
            if (ClMyFollowSingerList == null)
            {
                ClMyFollowSingerList = new MyFollowSingerList(this, TemplateSv.Template);
                ContentPage.Children.Add(ClMyFollowSingerList);
            }
            else ClMyFollowSingerList.GetSingerList();
            NSPage(new MeumInfo(SingerBtn, ClMyFollowSingerList, SingerCom), true, false);
        }
        #endregion
        #region FLGD 分类歌单
        private void ZJBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ClFLGDIndexPage == null)
            {
                ClFLGDIndexPage = new FLGDIndexPage(this, TemplateSv.Template);
                ContentPage.Children.Add(ClFLGDIndexPage);
            }
            NSPage(new MeumInfo(ZJBtn, ClFLGDIndexPage, GDCom), true, false);
        }
        #endregion
        #region Radio 电台
        private void RadioBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ClRadioIndexPage == null)
            {
                ClRadioIndexPage = new RadioIndexPage(this, TemplateSv.Template);
                ContentPage.Children.Add(ClRadioIndexPage);
            }
            NSPage(new MeumInfo(RadioBtn, ClRadioIndexPage, RadioCom), true, false);
        }

        public async void GetRadio(object sender, MouseEventArgs e)
        {
            OpenLoading();
            var dt = sender as RadioItem;
            RadioID = dt.data.ID;
            var data = await MusicLib.GetRadioMusicAsync(dt.data.ID);
            DLMode = false;
            PlayDL_List.Items.Clear();
            foreach (var s in data)
            {
                var kx = new PlayDLItem(s);
                kx.MouseDoubleClick += K_MouseDoubleClick;
                PlayDL_List.Items.Add(kx);
            }
            PlayDLItem k = PlayDL_List.Items[0] as PlayDLItem;
            k.p(true);
            MusicData = k;
            IsRadio = true;
            PlayMod = 0;
            (XHBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Lbxh);
            PlayMusic(k.Data);
            CloseLoading();
        }
        #endregion
        #region ILike 我喜欢 列表加载/数据处理
        /// <summary>
        /// 取消喜欢 变白色
        /// </summary>
        private void LikeBtnUp()
        {
            likeBtn_path.Tag = false;
            if (ind == 1)
                likeBtn_path.Fill = new SolidColorBrush(Colors.White);
            else
                likeBtn_path.SetResourceReference(Shape.FillProperty, "ResuColorBrush");
        }
        /// <summary>
        /// 添加喜欢 变红色
        /// </summary>
        private void LikeBtnDown()
        {
            likeBtn_path.Tag = true;
            likeBtn_path.Fill = new SolidColorBrush(Color.FromRgb(216, 30, 30));
        }
        /// <summary>
        /// 添加/删除 我喜欢的歌曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void likeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MusicName.Text != "MusicName")
            {
                if (Settings.USettings.MusicGDataLike.ids.ContainsKey(MusicData.Data.MusicID))
                {
                    LikeBtnUp();
                    foreach (var ac in Settings.USettings.MusicGDataLike.ids)
                    {
                        if (ac.Key == MusicData.Data.MusicID)
                        {
                            string a = await MusicLib.DeleteMusicFromGDAsync(new string[1] { ac.Value}, MusicLib.MusicLikeGDdirid);
                            Settings.USettings.MusicGDataLike.ids.Remove(MusicData.Data.MusicID);
                            Toast.Send(a);
                        }
                    }
                }
                else
                {
                    string[] a = await MusicLib.AddMusicToGDAsync(MusicData.Data.MusicID, MusicLib.MusicLikeGDdirid);
                    Toast.Send(a[1] + ": " + a[0]);
                    Settings.USettings.MusicGDataLike.ids.Add(MusicData.Data.MusicID, MusicData.Data.Littleid);
                    LikeBtnDown();
                }
                Settings.SaveSettings();
            }
        }
        /// <summary>
        /// 加载我喜欢的列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LikeBtn_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            LoadILikeItems();
        }
        private async void LoadILikeItems(bool NeedSave = true)
        {
            if (Settings.USettings.LemonAreeunIts == "Public")
                NSPage(new MeumInfo(ILikeBtn, NonePage, ILikeCom), NeedSave, false);
            else
            {
                NSPage(new MeumInfo(ILikeBtn, Data, ILikeCom) { cmd = "DataUrl[ILike]" }, NeedSave, false);
                OpenLoading();
                TB.Text = "我喜欢";
                TXx.Background = Resources["LoveIcon"] as VisualBrush;
                DataItemsList.Items.Clear();
                DataCollectBtn.Visibility =Visibility.Collapsed;
                string id = MusicLib.MusicLikeGDid ?? await ml.GetMusicLikeGDid();
                Settings.USettings.MusicGDataLike.ids.Clear();
                He.MGData_Now = await MusicLib.GetGDAsync(id,
                   (dt) =>
                   {
                       Dispatcher.Invoke(async () =>
                       {
                           DataPage_TX.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.Creater.Photo, new int[2] { 36, 36 }));
                           DataPage_Creater.Text = dt.Creater.Name;
                           DataPage_Sim.Text = dt.desc;
                       });
                   },
                    new Action<int, Music, bool>((i, j, b) =>
                    {
                        var k = new DataItem(j, this, i, b);
                        DataItemsList.Items[i] = k;
                        k.Play += PlayMusic;
                        k.Width = DataItemsList.ActualWidth;
                        k.Download += K_Download;
                        k.GetToSingerPage += K_GetToSingerPage;
                        if (j.MusicID == MusicData.Data.MusicID)
                        {
                            k.ShowDx();
                        }
                        Settings.USettings.MusicGDataLike.ids.Add(j.MusicID, j.Littleid);
                    }), this, new Action<int>(i =>
                    {
                        while (DataItemsList.Items.Count != i)
                        {
                            DataItemsList.Items.Add("");
                        }
                    }));
                CloseLoading();
                RunAnimation(DataItemsList, new Thickness(0, 200, 0, 0));
                np = NowPage.GDItem;
            }
        }
        #endregion
        #region DataPageBtn 歌曲数据 DataPage 的逻辑处理
        private void DataShareBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(np switch
            {
                NowPage.GDItem => $"https://y.qq.com/n/yqq/playsquare/{He.MGData_Now.id}.html#stat=y_new.index.playlist.pic",
                NowPage.Top => $"https://y.qq.com/n/yqq/toplist/{tc_now.Data.ID}.html",
                NowPage.Search => $"https://y.qq.com/portal/search.html#page=1&searchid=1&remoteplace=txt.yqq.top&t=song&w={HttpUtility.HtmlDecode(SearchKey)}",
                _ => null
            });
            Toast.Send("链接已复制到剪切板");
        }

        private async void DataCollectBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await MusicLib.AddGDILikeAsync(He.MGData_Now.id);
            Toast.Send("收藏成功");
        }
        private async void Md_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _Gdpop.IsOpen = false;
            string name = (sender as ListBoxItem).Content.ToString();
            string id = _ListData[name];
            string Musicid = "";
            string types = "";
            foreach (DataItem d in DataItemsList.Items)
            {
                if (d.isChecked)
                {
                    types += "3,";
                    Musicid += d.music.MusicID + ",";
                }
            }
            Musicid = Musicid[0..^1];
            types = types[0..^1];
            string[] a = await MusicLib.AddMusicToGDPLAsync(Musicid, id, types);
            Toast.Send(a[1] + ": " + a[0]);
        }
        private Popup _Gdpop = null;
        private ListBox _Add_Gdlist = null;
        private Dictionary<string, string> _ListData = new Dictionary<string, string>();//name,id
        private async void DataPage_PLCZ_AddTo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_Gdpop == null)
            {
                string Gdpopxaml = @"<Popup xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" x:Name=""Gdpop"" AllowsTransparency=""True"" Placement=""Mouse"">
                <Border Background=""{DynamicResource PlayDLPage_Bg}"" CornerRadius=""5"" Margin=""10"" BorderBrush=""{DynamicResource PlayDLPage_Border}"" BorderThickness=""1"">
                    <Grid>
                        <ListBox x:Name=""Add_Gdlist""  VirtualizingPanel.VirtualizationMode=""Recycling""
                            VirtualizingPanel.IsVirtualizing=""True""  Background=""{x:Null}"" Style=""{DynamicResource ListBoxStyle1}"" ScrollViewer.HorizontalScrollBarVisibility=""Disabled"" ItemContainerStyle=""{DynamicResource ListBoxItemStyle1}"" Margin=""5"" Foreground=""{DynamicResource PlayDLPage_Font_Most}"" >
                            <ListBoxItem Content=""我喜欢的歌单""/>
                        </ListBox>
                    </Grid>
                </Border>
            </Popup>";
                _Gdpop = (Popup)XamlReader.Parse(Gdpopxaml);
                _Add_Gdlist = (ListBox)((Grid)((Border)_Gdpop.Child).Child).Children[0];
                grid.Children.Add(_Gdpop);
            }
            _Add_Gdlist.Items.Clear();
            _ListData.Clear();
            JObject o = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/splcloud/fcgi-bin/songlist_list.fcg?utf8=1&-=MusicJsonCallBack&uin={Settings.USettings.LemonAreeunIts}&rnd=0.693477705380313&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0"));
            foreach (var a in o["list"])
            {
                string name = a["dirname"].ToString();
                _ListData.Add(name, a["dirid"].ToString());
                var mdb = new ListBoxItem { Background = new SolidColorBrush(Colors.Transparent), Height = 30, Content = name, Margin = new Thickness(10, 10, 10, 0) };
                mdb.PreviewMouseDown += Md_MouseDown;
                _Add_Gdlist.Items.Add(mdb);
            }
            var md = new ListBoxItem { Background = new SolidColorBrush(Colors.Transparent), Height = 30, Content = "取消", Margin = new Thickness(10, 10, 10, 0) };
            md.PreviewMouseDown += delegate { _Gdpop.IsOpen = false; };
            _Add_Gdlist.Items.Add(md);
            _Gdpop.IsOpen = true;
        }

        private async void DataPage_PLCZ_Delete_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (TwMessageBox.Show("确定要删除这些歌曲吗?"))
            {
                List<DataItem> ReadytoDelete = new List<DataItem>();
                List<string> Musicid = new List<string>();
                foreach (var dx in DataItemsList.Items)
                {
                    if (dx is DataItem)
                    {
                        var d = dx as DataItem;
                        if (d.isChecked)
                        {
                            ReadytoDelete.Add(d);
                            Musicid.Add(He.MGData_Now.ids[d.index]);
                        }
                    }
                }
                string dirid = await MusicLib.GetGDdiridByNameAsync(He.MGData_Now.name);
                Toast.Send(await MusicLib.DeleteMusicFromGDAsync(Musicid.ToArray(), dirid));
                foreach (var d in ReadytoDelete)
                {
                    He.MGData_Now.Data.Remove(d.music);
                    DataItemsList.Items.Remove(d);
                }
            }
        }

        private void DataPLCZBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DataPage_CMType = 1;
            OpenDataControlPage();
            DataPage_PLCZ_Delete.Visibility = He.MGData_Now.IsOwn ? Visibility.Visible : Visibility.Collapsed;
        }
        private void DataPage_GOTO_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int index = -1;
            for (int i = 0; i < DataItemsList.Items.Count; i++)
            {
                if ((DataItemsList.Items[i] as DataItem).pv)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                int p = (index + 1) * 45;
                double os = p - (DataItemsList.ActualHeight / 2) + 10;
                Console.WriteLine(os);
                var da = new DoubleAnimation(os, TimeSpan.FromMilliseconds(300));
                da.EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut };
                Datasv.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, da);
            }
        }

        private void DataPage_Top_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var da = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut };
            Datasv.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, da);
        }

        private void DataPlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PlayMusic(DataItemsList.Items[0] as DataItem, null);
        }
        bool DataPage_ControlMod = false;
        int DataPage_CMType = 0;//0:Download 1:批量操作
        private void DataDownloadBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DataPage_CMType = 0;
            OpenDataControlPage();
        }
        private void OpenDataControlPage()
        {
            DataPage_ControlMod = true;
            DataPage_MainInfo.Visibility = Visibility.Collapsed;
            DataControlPage.Visibility = Visibility.Visible;
            if (DataPage_CMType == 0)
            {
                DataDownloadPage.Visibility = Visibility.Visible;
                DataPLCZPage.Visibility = Visibility.Collapsed;
                Download_Path.Text = Settings.USettings.DownloadPath;
                DownloadQx.IsChecked = true;
                DownloadQx.Content = "全不选";
            }
            else
            {
                DataDownloadPage.Visibility = Visibility.Collapsed;
                DataPLCZPage.Visibility = Visibility.Visible;
                DataPage_PLCZChoose.IsChecked = true;
                DataPage_PLCZChoose.Content = "全不选";
            }
            DataItemsList.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(0, 50, 0, 0), TimeSpan.FromSeconds(0)));
            foreach (var xs in DataItemsList.Items)
            {
                if (xs is DataItem)
                {
                    var x = xs as DataItem;
                    x.MouseDown -= PlayMusic;
                    x.NSDownload(true);
                    x.Check(true);
                }
            }
        }
        public void CloseDataControlPage()
        {
            DataPage_ControlMod = false;
            DataPage_MainInfo.Visibility = Visibility.Visible;
            if (HB == 1)
                DataItemsList.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(0, 80, 0, 0), TimeSpan.FromSeconds(0)));
            else DataItemsList.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(0, 200, 0, 0), TimeSpan.FromSeconds(0)));
            DataControlPage.Visibility = Visibility.Collapsed;
            foreach (var xs in DataItemsList.Items)
            {
                if (xs is DataItem)
                {
                    var x = xs as DataItem;
                    x.MouseDown += PlayMusic;
                    x.NSDownload(false);
                    x.Check(false);
                }
            }
        }

        private void DataDownloadBtn_Back_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseDataControlPage();
        }
        #endregion
        #region SearchMusic  搜索音乐
        private int ixPlay = 1;
        private string SearchKey = "";

        private MusicSinger singer_now;

        private TopControl tc_now;
        private int ixTop = 1;
        private MyScrollView Datasv = null;
        private void Datasv_Loaded(object sender, RoutedEventArgs e)
        {
            if (Datasv == null)
                Datasv = sender as MyScrollView;
        }
        int HB = 0;
        private void Datasv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double offset = Datasv.ContentVerticalOffset;
            if (!DataPage_ControlMod && np != NowPage.Search)
                if (offset > 0)
                {
                    if (HB == 0)
                    {
                        HB = 1;
                        (Resources["DataPage_Min"] as Storyboard).Begin();
                    }
                }
                else
                {
                    if (HB == 1)
                    {
                        HB = 0;
                        (Resources["DataPage_Max"] as Storyboard).Begin();
                    }
                }
            if (Datasv.IsVerticalScrollBarAtButtom())
            {
                if (np == NowPage.Search)
                {
                    ixPlay++;
                    SearchMusic(SearchKey, ixPlay);
                }
                else if (np == NowPage.Top)
                {
                    ixTop++;
                    GetTopItems(tc_now, ixTop);
                }
            }
        }

        private async void SearchBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Search_SmartBox.Visibility = Visibility.Visible;
            Search_SmartBoxList.Items.Clear();
            var data = await MusicLib.SearchHotKey();
            var mdb = new ListBoxItem { Background = new SolidColorBrush(Colors.Transparent), Height = 30, Content = "热搜", Margin = new Thickness(10, 0, 10, 0) };
            Search_SmartBoxList.Items.Add(mdb);
            for (int i = 0; i < 5; i++)
            {
                var dt = data[i];
                var bd = new ListBoxItem { Background = new SolidColorBrush(Colors.Transparent), Height = 30, Content = dt, Margin = new Thickness(10, 10, 10, 0) };
                bd.PreviewMouseDown += Bd_MouseDown;
                bd.PreviewKeyDown += Search_SmartBoxList_KeyDown;
                Search_SmartBoxList.Items.Add(bd);
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
                    {
                        var mdb = new ListBoxItem { Background = new SolidColorBrush(Colors.Transparent), Height = 30, Content = dt, Margin = new Thickness(10, 10, 10, 0) };
                        mdb.PreviewMouseDown += Bd_MouseDown;
                        mdb.PreviewKeyDown += Search_SmartBoxList_KeyDown;
                        Search_SmartBoxList.Items.Add(mdb);
                    }
            }
            else Search_SmartBox.Visibility = Visibility.Collapsed;
        }
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SearchBox.Text.Trim() != string.Empty)
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
            {
                SearchBox.Text = (Search_SmartBoxList.SelectedItem as ListBoxItem).Content.ToString().Replace("歌曲:", "").Replace("歌手:", "").Replace("专辑:", "");
                Search_SmartBox.Visibility = Visibility.Collapsed;
                SearchMusic(SearchBox.Text); ixPlay = 1;
            }
        }

        private void Bd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchBox.Text = (sender as ListBoxItem).Content.ToString().Replace("歌曲:", "").Replace("歌手:", "").Replace("专辑:", "");
            Search_SmartBox.Visibility = Visibility.Collapsed;
            SearchMusic(SearchBox.Text); ixPlay = 1;
        }

        private void Search_SmartBox_MouseLeave(object sender, MouseEventArgs e)
        {
            Search_SmartBox.Visibility = Visibility.Collapsed;
        }
        public async void SearchMusic(string key, int osx = 0, bool NeedSave = true)
        {
            try
            {
                np = NowPage.Search;
                SearchKey = key;
                OpenLoading();
                List<Music> dt = null;
                if (osx == 0) dt = await ml.SearchMusicAsync(key);
                else dt = await ml.SearchMusicAsync(key, osx);
                if (osx == 0)
                {
                    TB.Text = key;
                    DataItemsList.Opacity = 0;
                    DataCollectBtn.Visibility = Visibility.Collapsed;
                    DataItemsList.Items.Clear();
                    if (Datasv != null) Datasv.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0)));
                    HB = 1;
                    (Resources["DataPage_Min"] as Storyboard).Begin();
                    TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.First().ImageUrl));
                }
                if (osx == 0) NSPage(new MeumInfo(null, Data, null) { cmd = "[DataUrl]{\"type\":\"Search\",\"key\":\"" + key + "\"}" }, NeedSave, false);
                int i = 0;
                foreach (var j in dt)
                {
                    var k = new DataItem(j, this, i) { Width = ContentPage.ActualWidth };
                    DataItemsList.Items.Add(k);
                    if (k.music.MusicID == MusicData.Data.MusicID)
                    {
                        k.ShowDx();
                    }
                    k.GetToSingerPage += K_GetToSingerPage;
                    k.Play += PlayMusic;
                    k.Download += K_Download;
                    if (DataPage_ControlMod)
                    {
                        k.MouseDown -= PlayMusic;
                        k.NSDownload(true);
                        k.Check(true);
                    }
                    i++;
                }
                CloseLoading();
                if (osx == 0) 
                    RunAnimation(DataItemsList, new Thickness(0, 75, 0, 0));
            }
            catch { }
        }
        #endregion
        #region PlayMusic 播放时的逻辑处理

        public void PlayMusic(object sender, MouseEventArgs e)
        {
            var dt = sender as DataItem;
            AddPlayDL(dt);
            dt.ShowDx();
            PlayMusic(dt.music);
        }
        public void PlayMusic(DataItem dt, bool next = false)
        {
            AddPlayDL(dt);
            dt.ShowDx();
            PlayMusic(dt.music);
        }
        public void PushPlayMusic(DataItem dt, ListBox DataSource)
        {
            AddPlayDL(dt, DataSource);
            dt.ShowDx();
            PlayMusic(dt.music);
        }
        public void PlayMusic(DataItem dt)
        {
            AddPlayDL(dt);
            dt.ShowDx();
            PlayMusic(dt.music);
        }
        public PlayDLItem AddPlayDL_All(DataItem dt, int index = -1, ListBox source = null)
        {
            if (source == null) source = DataItemsList;
            DLMode = false;
            PlayDL_List.Items.Clear();
            foreach (Object e in source.Items)
            {
                if (e is DataItem)
                {
                    var ae = e as DataItem;
                    var k = new PlayDLItem(ae.music);
                    k.MouseDoubleClick += K_MouseDoubleClick;
                    PlayDL_List.Items.Add(k);
                }
            }
            if (index == -1)
                index = source.Items.IndexOf(dt);
            PlayDLItem dk = PlayDL_List.Items[index] as PlayDLItem;
            dk.p(true);
            MusicData = dk;
            return dk;
        }
        public void AddPlayDl_CR(DataItem dt)
        {
            DLMode = true;
            var k = new PlayDLItem(dt.music);
            k.MouseDoubleClick += K_MouseDoubleClick;
            int index = PlayDL_List.Items.IndexOf(MusicData) + 1;
            PlayDL_List.Items.Insert(index, k);
            k.p(true);
            MusicData = k;
        }
        public bool DLMode = false;
        public void AddPlayDL(DataItem dt, ListBox source = null)
        {
            if (np == NowPage.GDItem)
            {
                //本次为歌单播放 那么将所有歌曲加入播放队列 
                AddPlayDL_All(dt, -1, source);
            }
            else
            {
                //本次为其他播放，若上一次也是其他播放，那么添加所有，不是则插入当前的
                if (DLMode)
                    AddPlayDL_All(dt, -1, source);
                else AddPlayDl_CR(dt);
            }
        }
        public void K_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlayDLItem k = sender as PlayDLItem;
            k.p(true);
            MusicData = k;
            PlayMusic(k.Data);
            bool find = false;
            foreach (DataItem a in DataItemsList.Items)
            {
                if (a.music.MusicID.Equals(k.Data.MusicID))
                {
                    find = true;
                    a.ShowDx();
                    break;
                }
            }
            if (!find) new DataItem(new Music()).ShowDx();
        }
        private ProgressBar MusicPlay_LoadProc;
        private void MusicPlay_LoadProc_Loaded(object sender, RoutedEventArgs e)
        {
            MusicPlay_LoadProc = sender as ProgressBar;
        }
        public async void LoadMusic(Music data, bool doesplay)
        {
            string downloadpath = Settings.USettings.CachePath + "Music\\" + data.MusicID + ".mp3";
            MusicPlay_LoadProc.Value = 0;
            if (!System.IO.File.Exists(downloadpath))
            {
                MusicPlay_LoadProc.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0)));
                var musicurl = await MusicLib.GetUrlAsync(data.MusicID);
                Console.WriteLine(musicurl);
                mp.LoadUrl(downloadpath, musicurl, (max, value) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MusicPlay_LoadProc.Maximum = max;
                        MusicPlay_LoadProc.Value = value;
                    });
                }, () =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MusicPlay_LoadProc.BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5)));
                    });
                });
                if (doesplay)
                    mp.Play();
                MusicName.Text = data.MusicName;
            }
            else
            {
                mp.Load(downloadpath);
                if (doesplay)
                    mp.Play();
                MusicName.Text = data.MusicName;
            }
        }
        public async void PlayMusic(Music data, bool doesplay = true)
        {
            t.Stop();
            if (mp.BassdlList.Count > 0)
                mp.BassdlList.Last().SetClose();

            MusicName.Text = "连接资源中...";
            mp.Pause();

            LoadMusic(data, doesplay);

            Title = data.MusicName + " - " + data.SingerText;
            Settings.USettings.Playing = MusicData.Data;
            Settings.SaveSettings();
            if (Settings.USettings.MusicGDataLike.ids.ContainsKey(data.MusicID))
                LikeBtnDown();
            else LikeBtnUp();

            var im = await ImageCacheHelp.GetImageByUrl(data.ImageUrl);
            MusicImage.Background = new ImageBrush(im);
            var rect = new System.Drawing.Rectangle(0, 0, im.PixelWidth, im.PixelHeight);
            var imb = im.ToBitmap();
            imb.GaussianBlur(ref rect, 20);
            LyricPage_Background.Background = new ImageBrush(imb.ToBitmapImage()) { Stretch = Stretch.UniformToFill };
            Singer.Text = data.SingerText;

            string dt = await MusicLib.GetLyric(data.MusicID);
            lv.LoadLrc(dt);

            if (doesplay)
            {
                (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Pause);
                TaskBarBtn_Play.Icon = Properties.Resources.icon_pause;
                t.Start();
                isplay = true;
                if (Settings.USettings.LyricAnimationMode == 2)
                    LyricBigAniRound.Begin();
            }
            try
            {
                TaskBarImg.SetImage(im);
                TaskBarImg.Title = data.MusicName + " - " + data.SingerText;
            }
            catch { }

            //-------加载歌曲相关歌单功能-------
            var gd = await MusicLib.GetSongListAboutSong(data.MusicID);
            LP_ag1_img.Background= new ImageBrush(await ImageCacheHelp.GetImageByUrl(gd[0].Photo, new int[2] { 80,80 }));
            LP_ag2_img.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(gd[1].Photo, new int[2] { 80, 80 }));
            LP_ag3_img.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(gd[2].Photo, new int[2] { 80, 80 }));

            LP_ag1_tx.Text = gd[0].Name; LP_ag2_tx.Text = gd[1].Name; LP_ag3_tx.Text = gd[2].Name;
            LP_ag1.Tag = new { id = gd[0].ID, name = gd[0].Name, img = gd[0].Photo };
            LP_ag2.Tag = new { id = gd[1].ID, name = gd[1].Name, img = gd[1].Photo };
            LP_ag3.Tag = new { id = gd[2].ID, name = gd[2].Name, img = gd[2].Photo };

            LP_ag1.MouseDown += LP_ag_MouseDown;
            LP_ag2.MouseDown += LP_ag_MouseDown;
            LP_ag3.MouseDown += LP_ag_MouseDown;
        }

        private void LP_ag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (Resources["LP_AboutGD_MouseLeave"] as Storyboard).Begin();
            Border_MouseDown_2(null, null);
            dynamic data= (sender as FrameworkElement).Tag;
            LoadFxGDItems(new FLGDIndexItem(data.id, data.name, data.img, 0));
        }
        #endregion
        #region PlayControl
        //private void Pop_sp_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    Pop_sp.IsOpen = false;
        //}
        //private void Border_MouseDown_6(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ClickCount == 2)
        //    {
        //        if (MusicPlay_tb.Text == "1.25x")
        //        {
        //            MusicLib.mp.SpeedRatio = 1d;
        //            MusicPlay_tb.Text = "倍速";
        //        }
        //        else
        //        {
        //            MusicLib.mp.SpeedRatio = 1.25d;
        //            MusicPlay_tb.Text = "1.25x";
        //        }
        //    }
        //    else Pop_sp.IsOpen = !Pop_sp.IsOpen;
        //}
        //private void MusicPlay_sp_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    try
        //    {
        //        MusicLib.mp.SpeedRatio = MusicPlay_sp.Value;
        //        MusicPlay_tb.Text = MusicPlay_sp.Value.ToString("0.00") + "x";
        //    }
        //    catch { }
        //}

        private void TaskBarBtn_Play_Click(object sender, EventArgs e)
        {
            PlayBtn_MouseDown(null, null);
        }
        private void Jd_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {//若使用ValueChanged事件，在value改变时也会触发，而不单是拖动jd.
            mp.Position = TimeSpan.FromMilliseconds(jd.Value);
            CanJd = true;
        }
        bool CanJd = true;
        private void Jd_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CanJd = false;
        }
        private void Jd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (!CanJd)
                    Play_Now.Text = TimeSpan.FromMilliseconds(jd.Value).ToString(@"mm\:ss");
            }
            catch { }
        }
        private void TaskBarBtn_Last_Click(object sender, EventArgs e)
        {
            PlayControl_PlayLast(null, null);
        }

        private void TaskBarBtn_Next_Click(object sender, EventArgs e)
        {
            PlayControl_PlayNext(null, null);
        }

        private void PlayControl_PlayLast(object sender, MouseButtonEventArgs e)
        {
            PlayDLItem k = null;

            if (PlayMod == 0)
            {
                //如果已经到播放队列的第一首，那么上一首就是最后一首歌(列表循环 非电台)
                //如果已经到播放队列的第一首，没有上一首(电台)
                if (PlayDL_List.Items.IndexOf(MusicData) == 0)
                {
                    if (!IsRadio) k = PlayDL_List.Items[PlayDL_List.Items.Count - 1] as PlayDLItem;
                }
                else k = PlayDL_List.Items[PlayDL_List.Items.IndexOf(MusicData) - 1] as PlayDLItem;
            }
            else {
                int index = RandomIndexes.IndexOf(RandomOffset);
                if (index == 0)
                    return;
                RandomOffset = RandomIndexes[index - 1];
                k = PlayDL_List.Items[RandomOffset] as PlayDLItem;
            }
            
            if (k != null)
            {
                k.p(true);
                MusicData = k;
                PlayMusic(k.Data);
                bool find = false;
                foreach (DataItem a in DataItemsList.Items)
                {
                    if (a.music.MusicID.Equals(k.Data.MusicID))
                    {
                        find = true;
                        a.ShowDx();
                        break;
                    }
                }
                if (!find) new DataItem(new Music()).ShowDx();
            }
        }
        private List<int> RandomIndexes = new List<int>();
        private int RandomOffset = 0;
        private void PlayControl_PlayNext(object sender, MouseButtonEventArgs e)
        {
            PlayDLItem k = null;

            if (PlayMod == 0)
            {
                //如果已到最后一首歌，那么下一首从头播放(列表循环 非电台)
                //已经到最后一首歌，下一首需要重新查询电台列表
                if (PlayDL_List.Items.IndexOf(MusicData) + 1 == PlayDL_List.Items.Count)
                {
                    if (IsRadio)
                        GetRadio(new RadioItem(RadioID), null);
                    else
                        k = PlayDL_List.Items[0] as PlayDLItem;
                }
                else k = PlayDL_List.Items[PlayDL_List.Items.IndexOf(MusicData) + 1] as PlayDLItem;
            }
            else {
                //随机播放  TODO 待完善
                if (RandomIndexes.Count > 0)
                {
                    if (RandomOffset != RandomIndexes.Last())
                    {
                        //若当前index没到最后一个
                        RandomOffset = RandomIndexes[RandomIndexes.IndexOf(RandomOffset) + 1];
                        k = PlayDL_List.Items[RandomOffset] as PlayDLItem;
                    }
                    else {
                        Random r = new Random();
                        int index = r.Next(0, PlayDL_List.Items.Count - 1);
                        RandomIndexes.Add(index);
                        RandomOffset = index;
                        k = PlayDL_List.Items[index] as PlayDLItem;
                    }
                }
                else
                {
                    Random r = new Random();
                    int index = r.Next(0, PlayDL_List.Items.Count - 1);
                    RandomIndexes.Add(index);
                    RandomOffset = index;
                    k = PlayDL_List.Items[index] as PlayDLItem;
                }
            }
            
            if (k != null)
            {
                k.p(true);
                MusicData = k;
                PlayMusic(k.Data);
                bool find = false;
                foreach (DataItem a in DataItemsList.Items)
                {
                    if (a.music.MusicID.Equals(k.Data.MusicID))
                    {
                        find = true;
                        a.ShowDx();
                        break;
                    }
                }
                if (!find) new DataItem(new Music()).ShowDx();
            }
        }
        private void PlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isplay)
            {
                isplay = false;
                mp.Pause();
                if (Settings.USettings.LyricAnimationMode == 2)
                    LyricBigAniRound.Pause();
                TaskBarBtn_Play.Icon = Properties.Resources.icon_play;
                t.Stop();
                (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Play);
            }
            else
            {
                isplay = true;
                mp.Play();
                if (Settings.USettings.LyricAnimationMode == 2)
                    LyricBigAniRound.Begin();
                TaskBarBtn_Play.Icon = Properties.Resources.icon_pause;
                t.Start();
                (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Pause);
            }
        }


        private void GcBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Settings.USettings.DoesOpenDeskLyric)
            {
                Settings.USettings.DoesOpenDeskLyric = false;
                lyricTa.Close();
                if (ind == 1)
                    path7.Fill = new SolidColorBrush(Colors.White);
                else
                    path7.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            }
            else
            {
                Settings.USettings.DoesOpenDeskLyric = true;
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
            path10.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            App.BaseApp.SetColor("ButtonColorBrush", LastButtonColor);
            if (!Settings.USettings.DoesOpenDeskLyric) path7.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            if (!(bool)likeBtn_path.Tag) likeBtn_path.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            var ol = Resources["CloseLyricPage"] as Storyboard;
            ol.Begin();
        }
        Color LastButtonColor;
        private void MusicImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ind = 1;
            LastButtonColor = App.BaseApp.GetButtonColorBrush().Color;
            App.BaseApp.SetColor("ButtonColorBrush", Colors.White);
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
            path10.Fill = new SolidColorBrush(Colors.White);
            if (!Settings.USettings.DoesOpenDeskLyric) path7.Fill = new SolidColorBrush(Colors.White);
            if(!(bool)likeBtn_path.Tag) likeBtn_path.Fill = new SolidColorBrush(Colors.White);
            var ol = Resources["OpenLyricPage"] as Storyboard;
            ol.Begin();
        }
        private void MusicImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MusicImage.CornerRadius.Equals(new CornerRadius(5)))
            {
                MusicImage.CornerRadius = new CornerRadius(100);
                Settings.USettings.IsRoundMusicImage = 100;
            }
            else
            {
                MusicImage.CornerRadius = new CornerRadius(5);
                Settings.USettings.IsRoundMusicImage = 5;
            }
        }
        private void MoreBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MoreBtn_Meum.IsOpen = !MoreBtn_Meum.IsOpen;
        }
        private void MoreBtn_Meum_DL_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MoreBtn_Meum.IsOpen = false;
            K_Download(new DataItem(MusicData.Data));
        }
        private Dictionary<string, string> MoreBtn_Meum_Add_List = new Dictionary<string, string>();
        private async void MoreBtn_Meum_Add_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Add_Gdlist.Items.Clear();
            MoreBtn_Meum_Add_List.Clear();
            JObject o = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/splcloud/fcgi-bin/songlist_list.fcg?utf8=1&-=MusicJsonCallBack&uin={Settings.USettings.LemonAreeunIts}&rnd=0.693477705380313&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0"));
            foreach (var a in o["list"])
            {
                string name = a["dirname"].ToString();
                MoreBtn_Meum_Add_List.Add(name, a["dirid"].ToString());
                var mdb = new ListBoxItem
                {
                    Background = new SolidColorBrush(Colors.Transparent),
                    Height = 30,
                    Content = name,
                    Margin = new Thickness(10, 10, 10, 0)
                };
                mdb.PreviewMouseDown += Mdb_MouseDown;
                Add_Gdlist.Items.Add(mdb);
            }
            var md = new ListBoxItem
            {
                Background = new SolidColorBrush(Colors.Transparent),
                Height = 30,
                Content = "取消",
                Margin = new Thickness(10, 10, 10, 0)
            };
            md.PreviewMouseDown += delegate { Gdpop.IsOpen = false; };
            Add_Gdlist.Items.Add(md);
            Gdpop.IsOpen = true;
        }
        private async void Mdb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MoreBtn_Meum.IsOpen = false;
            Gdpop.IsOpen = false;
            string name = (sender as ListBoxItem).Content.ToString();
            string id = MoreBtn_Meum_Add_List[name];
            string[] a = await MusicLib.AddMusicToGDAsync(MusicData.Data.MusicID, id);
            Toast.Send(a[1] + ": " + a[0]);
        }
        private void MoreBtn_Meum_PL_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MoreBtn_Meum.IsOpen = false;
            LoadPl();
        }
        private void MoreBtn_Meum_Singer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MusicData.Data.Singer.Count == 1)
            {
                MoreBtn_Meum.IsOpen = false;
                K_GetToSingerPage(MusicData.Data.Singer[0]);
            }
            else
            {
                Add_SLP.Items.Clear();
                foreach (var a in MusicData.Data.Singer)
                {
                    string name = a.Name;
                    var mdbs = new ListBoxItem
                    {
                        Background = new SolidColorBrush(Colors.Transparent),
                        Height = 30,
                        Tag = MusicData.Data.Singer.IndexOf(a),
                        Content = name,
                        Margin = new Thickness(10, 10, 10, 0)
                    };
                    mdbs.PreviewMouseDown += Mdbs_MouseDown;
                    Add_SLP.Items.Add(mdbs);
                }
                var md = new ListBoxItem
                {
                    Background = new SolidColorBrush(Colors.Transparent),
                    Height = 30,
                    Content = "取消",
                    Margin = new Thickness(10, 10, 10, 0)
                };
                md.PreviewMouseDown += delegate { SingerListPop.IsOpen = false; };
                Add_SLP.Items.Add(md);
                SingerListPop.IsOpen = true;
            }
        }
        private void Mdbs_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MoreBtn_Meum.IsOpen = false;
            SingerListPop.IsOpen = false;
            K_GetToSingerPage(MusicData.Data.Singer[(int)((sender as ListBoxItem).Tag)]);
        }
        private void XHBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //NOW:列表循环
            if (PlayMod == 0)
            {
                //切换为单曲循环
                PlayMod = 1;
                (XHBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Dqxh);
            }
            else if (PlayMod == 1)
            {
                //如果是电台播放则切换为顺序播放
                if (IsRadio)
                {
                    PlayMod = 0;
                    (XHBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Lbxh);
                }
                else
                {
                    if (MusicData.Data.MusicID!=string.Empty) {
                        RandomOffset = PlayDL_List.Items.IndexOf(MusicData);
                        RandomIndexes.Add(RandomOffset);
                    }
                    PlayMod = 2;
                    (XHBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Random);
                }
            }
            else if (PlayMod == 2)
            {
                PlayMod = 0;
                (XHBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Lbxh);
            }
        }
        bool isOpenPlayDLPage = false;
        private void PlayLbBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isOpenPlayDLPage)
            {
                (Resources["ClosePlayDLPage"] as Storyboard).Begin();
                isOpenPlayDLPage = false;
            }
            else
            {
                (Resources["OpenPlayDLPage"] as Storyboard).Begin();
                isOpenPlayDLPage = true;
            }
        }
        private void PlayDLPage_ClosePage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (Resources["ClosePlayDLPage"] as Storyboard).Begin();
            isOpenPlayDLPage = false;
        }

        private void PlayDLPage_IntoDataPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (Resources["ClosePlayDLPage"] as Storyboard).Begin();
            isOpenPlayDLPage = false;
            NSPage(new MeumInfo(null, Data, null));
        }
        private void PlayDL_GOTO_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int index = -1;
            for (int i = 0; i < PlayDL_List.Items.Count; i++)
            {
                if ((PlayDL_List.Items[i] as PlayDLItem).pv)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                int p = (index + 1) * 60;
                double os = p - (PlayDL_List.ActualHeight / 2) + 10;
                Console.WriteLine(os);
                var da = new DoubleAnimation(os, TimeSpan.FromMilliseconds(300));
                da.EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut };
                PlayDLSV.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, da);
            }
        }

        private void PlayDL_Top_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var da = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseOut };
            PlayDLSV.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, da);
        }
        ScrollViewer PlayDLSV = null;
        private void PlayDLDatasv_Loaded(object sender, RoutedEventArgs e)
        {
            PlayDLSV = sender as ScrollViewer;
        }
        private void DataPlayAllBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var k = AddPlayDL_All(null, 0);
            (DataItemsList.Items[0] as DataItem).ShowDx();
            PlayMusic(k.Data);
        }
        #endregion
        #region Lyric & 评论加载

        private void LyricBig_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Settings.USettings.LyricAnimationMode == 0)
                Settings.USettings.LyricAnimationMode = 1;
            else if (Settings.USettings.LyricAnimationMode == 1)
                Settings.USettings.LyricAnimationMode = 2;
            else if (Settings.USettings.LyricAnimationMode == 2)
                Settings.USettings.LyricAnimationMode = 0;
            CheckLyricAnimation(Settings.USettings.LyricAnimationMode);
        }
        private Storyboard LyricBigAniRound = null;
        private void CheckLyricAnimation(int mode)
        {
            if (mode == 0)
            {
                LyricBigAniRound.Stop();
                RotateTransform rtf = new RotateTransform();
                LyricBig.RenderTransform = rtf;
                DoubleAnimation dbAscending = new DoubleAnimation(0, new Duration
                (TimeSpan.FromSeconds(2)));
                rtf.BeginAnimation(RotateTransform.AngleProperty, dbAscending);
            }
            else if (mode == 2)
            {
                LyricBigAniRound.Begin();
            }
        }
        private void Border_MouseDown_3(object sender, MouseButtonEventArgs e)
        {
            Border_MouseDown_2(null, null);
            LoadPl();
        }
        private async void LoadPl()
        {
            NSPage(new MeumInfo(null, MusicPLPage, null));
            OpenLoading();
            MusicPL_tb.Text = MusicName.Text + " - " + Singer.Text;
            List<MusicPL> data;
            bool cp = true;
            if (MusicPLPage_QQ.Visibility == Visibility.Visible)
                data = await MusicLib.GetPLByQQAsync(Settings.USettings.Playing.MusicID);
            else
            {
                cp = false;
                data = await MusicLib.GetPLByWyyAsync(MusicPL_tb.Text);
            }
            MusicPlList.Children.Clear();
            MusicPlScrollViewer.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, new DoubleAnimation(0,TimeSpan.FromSeconds(0)));
            foreach (var dt in data)
            {
                MusicPlList.Children.Add(new PlControl(dt) { couldpraise = cp, Width = MusicPlList.ActualWidth - 10, Margin = new Thickness(10, 0, 0, 20) });
            }
            CloseLoading();
        }
        /// <summary>
        /// 加载QQ音乐评论
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MusicPLPage_QQ_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MusicPLPage_Wy.Visibility = Visibility.Visible;
            MusicPLPage_QQ.Visibility = Visibility.Collapsed;
            OpenLoading();
            MusicPL_tb.Text = MusicName.Text + " - " + Singer.Text;
            List<MusicPL> data = await MusicLib.GetPLByWyyAsync(MusicPL_tb.Text);
            MusicPlList.Children.Clear();
            MusicPlScrollViewer.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0)));
            foreach (var dt in data)
            {
                MusicPlList.Children.Add(new PlControl(dt) { couldpraise = false, Width = MusicPlList.ActualWidth - 10, Margin = new Thickness(10, 0, 0, 20) });
            }
            CloseLoading();
        }

        /// <summary>
        /// 加载网易云音乐的评论
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MusicPLPage_Wy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MusicPLPage_Wy.Visibility = Visibility.Collapsed;
            MusicPLPage_QQ.Visibility = Visibility.Visible;
            OpenLoading();
            MusicPL_tb.Text = MusicName.Text + " - " + Singer.Text;
            List<MusicPL> data = await MusicLib.GetPLByQQAsync(Settings.USettings.Playing.MusicID);
            MusicPlList.Children.Clear();
            MusicPlScrollViewer.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0)));
            foreach (var dt in data)
            {
                MusicPlList.Children.Add(new PlControl(dt) { Width = MusicPlList.ActualWidth - 10, Margin = new Thickness(10, 0, 0, 20) });
            }
            CloseLoading();
        }
        private void ly_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (lv != null)
                lv.RestWidth(e.NewSize.Width);
        }
        #endregion
        #region IntoGD 导入歌单
        private void IntoGDPage_CloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IntoGDPop.IsOpen = false;
        }
        private void IntoGDPage_qqmod_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!mod)
            {
                mod = true;
                QPath_Bg.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8C913"));
                QPath_Ic.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF02B053"));
                IntoGDPage_wymod.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB2B2B2"));
            }
        }

        private void IntoGDPage_wymod_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mod)
            {
                mod = false;
                QPath_Bg.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF1F1F1"));
                QPath_Ic.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB2B2B2"));
                IntoGDPage_wymod.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE72D2C"));
            }
        }
        private void IntoGDPage_OpenBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IntoGDPop.IsOpen = !IntoGDPop.IsOpen;
        }
        private async void IntoGDPage_DrBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mod)
            {
                await MusicLib.AddGDILikeAsync(IntoGDPage_id.Text);
                TwMessageBox.Show("添加成功");
                IntoGDPop.IsOpen = false;
                GDBtn_MouseDown(null, null);
            }
            else
            {
                IntoGDPage_main.Visibility = Visibility.Collapsed;
                IntoGDPage_loading.Visibility = Visibility.Visible;
                ml.GetGDbyWYAsync(IntoGDPage_id.Text, this, IntoGDPage_ps_name, IntoGDPage_ps_jd,
                    () =>
                    {
                        IntoGDPop.IsOpen = false;
                        GDBtn_MouseDown(null, null);
                    });
            }
        }
        #endregion
        #region AddGD 创建歌单
        private string AddGDPage_ImgUrl = "";
        private async void AddGDPage_ImgBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "图像文件(*.png;*.jpg)|*.png;*.jpg";
            ofd.ValidateNames = true;
            ofd.CheckPathExists = true;
            ofd.CheckFileExists = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AddGDPage_ImgUrl = await MusicLib.UploadAFile(ofd.FileName);
                AddGDPage_Img.Background = new ImageBrush(new BitmapImage(new Uri(AddGDPage_ImgUrl)));
            }
        }

        private async void AddGDPage_DrBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Toast.Send(await MusicLib.AddNewGdAsync(AddGDPage_name.Text, AddGDPage_ImgUrl));
            AddGDPop.IsOpen = false;
            GDBtn_MouseDown(null, null);
        }

        private void AddGDPage_OpenBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AddGDPop.IsOpen = !AddGDPop.IsOpen;
        }

        private void AddGDPage_CloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AddGDPop.IsOpen = false;
        }
        #endregion
        #region Download
        private List<Music> DownloadDL = new List<Music>();
        public void AddDownloadTask(Music data)
        {
            string name = TextHelper.MakeValidFileName(Settings.USettings.DownloadName
                .Replace("[I]", (DownloadDL.Count() + 1).ToString())
                .Replace("[M]", data.MusicName)
                .Replace("[S]", data.SingerText));
            string file = Settings.USettings.DownloadPath + $"\\{name}.mp3";
            DownloadItem di = new DownloadItem(data, file, DownloadDL.Count())
            {
                Width = ContentPage.ActualWidth
            };
            di.Delete += (s) =>
            {
                s.d.Stop();
                s.finished = true;
                s.zt.Text = "已取消";
            };
            di.Finished += (s) =>
            {
                DownloadDL.Remove(s.MData);
                if (DownloadDL.Count != 0)
                {
                    DownloadItem d = DownloadItemsList.Children[DownloadItemsList.Children.IndexOf(s) + 1] as DownloadItem;
                    if (!d.finished) d.d.Download();
                }
                else
                {
                    DownloadIsFinish = true;
                    (Resources["Downloading"] as Storyboard).Stop();
                }
            };
            DownloadItemsList.Children.Add(di);
            DownloadDL.Add(data);
            DownloadIsFinish = false;
        }
        public void K_Download(DataItem sender)
        {
            var cc = (Resources["Downloading"] as Storyboard);
            if (DownloadIsFinish)
                cc.Begin();
            AddDownloadTask(sender.music);
        }
        bool DownloadIsFinish = true;
        private void ckFile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var g = new System.Windows.Forms.FolderBrowserDialog();
            if (g.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Download_Path.Text = g.SelectedPath;
                Settings.USettings.DownloadPath = g.SelectedPath;
            }

        }

        private void cb_color_Click(object sender, RoutedEventArgs e)
        {
            var d = sender as CheckBox;
            if (d.IsChecked == true)
            {
                d.Content = "全不选";
                foreach (var x in DataItemsList.Items)
                    if (x is DataItem) (x as DataItem).Check(true);
            }
            else
            {
                d.Content = "全选";
                foreach (var x in DataItemsList.Items)
                    if (x is DataItem) (x as DataItem).Check(false);
            }
        }
        private void Download_Btn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(new MeumInfo(DownloadMGBtn, DownloadPage, DownloadMGCom));
            if (DownloadItemsList.Children.Count == 0)
                NonePage_Copy.Visibility = Visibility.Visible;
            else NonePage_Copy.Visibility = Visibility.Collapsed;
        }

        private void Download_pause_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Download_pause.TName == "暂停")
            {
                Download_pause.TName = "开始";
                foreach (var a in DownloadItemsList.Children)
                {
                    DownloadItem dl = a as DownloadItem;
                    dl.d.Pause();
                }
            }
            else
            {
                Download_pause.TName = "暂停";
                foreach (var a in DownloadItemsList.Children)
                {
                    DownloadItem dl = a as DownloadItem;
                    dl.d.Start();
                }
            }
        }

        private void Download_clear_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var a in DownloadItemsList.Children)
            {
                DownloadItem dl = a as DownloadItem;
                dl.d.Pause();
                dl.d.Stop();
            }
            (Resources["Downloading"] as Storyboard).Stop();
            DownloadItemsList.Children.Clear();
            NonePage_Copy.Visibility = Visibility.Visible;
        }
        public void PushDownload(ListBox c)
        {
            var cc = (Resources["Downloading"] as Storyboard);
            if (DownloadIsFinish)
                cc.Begin();
            foreach (var x in c.Items)
            {
                var f = x as DataItem;
                if (f.isChecked == true)
                {
                    AddDownloadTask(f.music);
                }
            }
        }
        private void DownloadBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var cc = (Resources["Downloading"] as Storyboard);
            if (DownloadIsFinish)
                cc.Begin();
            foreach (var x in DataItemsList.Items)
            {
                var f = x as DataItem;
                if (f.isChecked == true)
                {
                    AddDownloadTask(f.music);
                }
            }
            CloseDataControlPage();
        }
        #endregion
        #region User
        #region Login
        LoginWindow lw;
        private void UserTX_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lw = new LoginWindow(this);
            lw.Show();
        }
        #endregion
        #region MyGD
        private List<string> GData_Now = new List<string>();
        private List<string> GLikeData_Now = new List<string>();
        private async void GDBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Settings.USettings.LemonAreeunIts == "Public")
                NSPage(new MeumInfo(MYGDBtn, NonePage, MYGDCom));
            else
            {
                NSPage(new MeumInfo(MYGDBtn, MyGDIndexPage, MYGDCom));
                OpenLoading();
                var GdData = await ml.GetGdListAsync();
                if (GdData.Count != GDItemsList.Children.Count)
                { GDItemsList.Children.Clear(); GData_Now.Clear(); }
                foreach (var jm in GdData)
                {
                    if (!GData_Now.Contains(jm.Key))
                    {
                        var ks = new FLGDIndexItem(jm.Key, jm.Value.name, jm.Value.pic, 0, true, jm.Value.subtitle) { Margin = new Thickness(12, 0, 12, 20) };
                        ks.DeleteEvent += async (fl) =>
                        {
                            if (TwMessageBox.Show("确定要删除吗?"))
                            {
                                string dirid = await MusicLib.GetGDdiridByNameAsync(fl.sname);
                                string a = await MusicLib.DeleteGdByIdAsync(dirid);
                                GDBtn_MouseDown(null, null);
                            }
                        };
                        ks.Width = ContentPage.ActualWidth / 5;
                        ks.ImMouseDown += FxGDMouseDown;
                        GDItemsList.Children.Add(ks);
                        GData_Now.Add(jm.Key);
                    }
                }
                WidthUI(GDItemsList);
                var GdLikeData = await ml.GetGdILikeListAsync();
                if (GdLikeData.Count != GDILikeItemsList.Children.Count)
                { GDILikeItemsList.Children.Clear(); GLikeData_Now.Clear(); }
                foreach (var jm in GdLikeData)
                {
                    if (!GLikeData_Now.Contains(jm.Key))
                    {
                        var ks = new FLGDIndexItem(jm.Key, jm.Value.name, jm.Value.pic, jm.Value.listenCount, true) { Margin = new Thickness(12, 0, 12, 20) };
                        ks.DeleteEvent += async (fl) =>
                         {
                             if (TwMessageBox.Show("确定要删除吗?"))
                             {
                                 string a = await MusicLib.DelGDILikeAsync(fl.id);
                                 GDBtn_MouseDown(null, null);
                             }
                         };
                        ks.Width = ContentPage.ActualWidth / 5;
                        ks.ImMouseDown += FxGDMouseDown;
                        GDILikeItemsList.Children.Add(ks);
                        GLikeData_Now.Add(jm.Key);
                    }
                }
                WidthUI(GDILikeItemsList);
                if (GdData.Count == 0 && GdLikeData.Count == 0)
                    NSPage(new MeumInfo(MYGDBtn, MyGDIndexPage, MYGDCom));
                CloseLoading();
            }
        }

        public void FxGDMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dt = sender as FLGDIndexItem;
            LoadFxGDItems(dt);
        }
        private FLGDIndexItem NowType;
        private async void LoadFxGDItems(FLGDIndexItem dt, bool NeedSave = true)
        {
            NSPage(new MeumInfo(null, Data, null) { cmd = "[DataUrl]{\"type\":\"GD\",\"key\":\"" + dt.id + "\",\"name\":\"" + dt.name.Text + "\",\"img\":\"" + dt.img + "\"}" }, NeedSave, false);
            NowType = dt;
            TB.Text = dt.name.Text;
            DataItemsList.Items.Clear();
            TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.img));
            OpenLoading();
            He.MGData_Now = await MusicLib.GetGDAsync(dt.id,
                (dt) =>
                {
                    Dispatcher.Invoke(async () =>
                    {
                        if(dt.Creater.Name=="QQ音乐官方歌单")
                            DataPage_TX.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl("https://y.qq.com/favicon.ico"));
                        else DataPage_TX.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.Creater.Photo, new int[2] { 50,50 }));
                        DataPage_Creater.Text = dt.Creater.Name;
                        DataPage_Sim.Text = dt.desc;
                        DataCollectBtn.Visibility = dt.IsOwn ? Visibility.Collapsed : Visibility.Visible;
                    });
                },
                new Action<int, Music, bool>((i, j, b) =>
                {
                    var k = new DataItem(j, this, i, b);
                    DataItemsList.Items[i] = k;
                    k.Play += PlayMusic;
                    k.GetToSingerPage += K_GetToSingerPage;
                    k.Download += K_Download;
                    k.Width = DataItemsList.ActualWidth;
                    if (j.MusicID == MusicData.Data.MusicID)
                    {
                        k.ShowDx();
                    }
                }), this,
            new Action<int>(i =>
            {
                while (DataItemsList.Items.Count != i)
                {
                    DataItemsList.Items.Add("");
                }
            }));
            CloseLoading();
            RunAnimation(DataItemsList, new Thickness(0, 200, 0, 0));
            np = NowPage.GDItem;
        }
        #endregion
        #endregion
        #region MV
        string mvpause = "M735.744 49.664c-51.2 0-96.256 44.544-96.256 95.744v733.184c0 51.2 45.056 95.744 96.256 95.744s96.256-44.544 96.256-95.744V145.408c0-51.2-45.056-95.744-96.256-95.744z m-447.488 0c-51.2 0-96.256 44.544-96.256 95.744v733.184c0 51.2 45.056 95.744 96.256 95.744S384 929.792 384 878.592V145.408c0-51.2-44.544-95.744-95.744-95.744z";
        string mvplay = "M766.464,448.170667L301.226667,146.944C244.394667,110.08,213.333333,126.293333,213.333333,191.146667L213.333333,832.853333C213.333333,897.706666,244.394666,913.834666,301.312,876.970667L766.378667,575.744C825.429334,537.514667,825.429334,486.314667,766.378667,448.085333z M347.733333,948.650667C234.666667,1021.781333,128,966.314667,128,832.938667L128,191.146667C128,57.6,234.752,2.218667,347.733333,75.349333L812.8,376.576C923.733333,448.426667,923.733333,575.658667,812.8,647.424L347.733333,948.650667z";
        bool MVplaying = false;
        System.Windows.Forms.Timer mvt = new System.Windows.Forms.Timer();
        public async void PlayMv(MVData mVData)
        {
            NSPage(new MeumInfo(null, MvPage, null));
            MVplaying = true;
            MvPlay_Tb.Text = mVData.name;
            MvPlay_Tb.Uid = mVData.id;
            MvPlay_Desc.Text = await MusicLib.GetMVDesc(mVData.id);
            MvPlay_ME.Source = new Uri(await MusicLib.GetMVUrl(mVData.id));
            MvPlay_ME.Play();
            mvpath.Data = Geometry.Parse(mvpause);
            mvt.Start();
            //加载评论
            List<MusicPL> data = await MusicLib.GetMVPL(MvPlay_Tb.Uid);
            MVPlList.Children.Clear();
            MVScrollViewer.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0)));
            foreach (var dt in data)
            {
                MVPlList.Children.Add(new PlControl(dt) { Width = MvPage.ActualWidth - 10, Margin = new Thickness(10, 0, 0, 20) });
            }
            if (isplay) PlayBtn_MouseDown(null, null);
        }
        private void Mvt_Tick(object sender, EventArgs e)
        {
            var jd_all = MvPlay_ME.NaturalDuration.HasTimeSpan ? MvPlay_ME.NaturalDuration.TimeSpan : TimeSpan.FromMilliseconds(0);
            Mvplay_jd.Maximum = jd_all.TotalMilliseconds;
            Mvplay_jdtb_all.Text = jd_all.ToString(@"mm\:ss");
            var jd_now = MvPlay_ME.Position;
            Mvplay_jdtb_now.Text = jd_now.ToString(@"mm\:ss");
            Mvplay_jd.Value = jd_now.TotalMilliseconds;
        }

        private void Mvplay_plps_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MVplaying)
            {
                MVplaying = false;
                MvPlay_ME.Pause();
                mvpath.Data = Geometry.Parse(mvplay);
            }
            else
            {
                MVplaying = true;
                MvPlay_ME.Play();
                mvpath.Data = Geometry.Parse(mvpause);
            }
        }
        private void MvPlay_ME_MouseEnter(object sender, MouseEventArgs e)
        {
            mvct.Height = 30;
        }

        private void MvPlay_ME_MouseLeave(object sender, MouseEventArgs e)
        {
            mvct.Height = 0;
        }
        private void MvJd_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            MvPlay_ME.Position = TimeSpan.FromMilliseconds(Mvplay_jd.Value);
            MvCanJd = true;
        }
        bool MvCanJd = true;
        private void MvJd_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MvCanJd = false;
        }
        private void MvJd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (!MvCanJd)
                    Mvplay_jdtb_now.Text = TimeSpan.FromMilliseconds(Mvplay_jd.Value).ToString(@"mm\:ss");
            }
            catch { }
        }

        private void MvPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (MvPage.Visibility == Visibility.Collapsed)
            {
                MvPlay_ME.Stop();
                MvPlay_ME.Close();
            }
        }
        #endregion
        #endregion
        #region 快捷键
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private void LoadHotDog()
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            RegisterHotKey(handle, 124, 1, (uint)System.Windows.Forms.Keys.L);
            RegisterHotKey(handle, 125, 1, (uint)System.Windows.Forms.Keys.S);
            RegisterHotKey(handle, 126, 1, (uint)System.Windows.Forms.Keys.Space);
            RegisterHotKey(handle, 127, 1, (uint)System.Windows.Forms.Keys.Up);
            RegisterHotKey(handle, 128, 1, (uint)System.Windows.Forms.Keys.Down);
            RegisterHotKey(handle, 129, 1, (uint)System.Windows.Forms.Keys.C);
            InstallHotKeyHook(this);
            Closed += (s, e) =>
            {
                IntPtr hd = new WindowInteropHelper(this).Handle;
                UnregisterHotKey(hd, 124);
                UnregisterHotKey(hd, 125);
                UnregisterHotKey(hd, 126);
                UnregisterHotKey(hd, 127);
                UnregisterHotKey(hd, 128);
                UnregisterHotKey(hd, 129);
            };
            //notifyIcon
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = "Lemon App";
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            notifyIcon.Visible = true;
            //打开菜单项
            System.Windows.Forms.ToolStripMenuItem open = new System.Windows.Forms.ToolStripMenuItem("打开");
            open.Click += delegate { exShow(); };
            //退出菜单项
            System.Windows.Forms.ToolStripMenuItem exit = new System.Windows.Forms.ToolStripMenuItem("关闭");
            exit.Click += delegate
            {
                try
                {
                    mp.Free();
                    notifyIcon.Dispose();
                }
                catch { }
                Settings.SaveSettings();
                Environment.Exit(0);
            };
            //关联托盘控件
            var a = new System.Windows.Forms.ContextMenuStrip();
            a.Items.Add(open);
            a.Items.Add(exit);
            notifyIcon.ContextMenuStrip = a;
            notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, m) =>
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
                { PlayControl_PlayLast(null, null); Toast.Send("成功切换到上一曲"); }
                else if (wParam.ToInt32() == 128)
                { PlayControl_PlayNext(null, null); Toast.Send("成功切换到下一曲"); }
                else if (wParam.ToInt32() == 129)
                {
                    IntPtr hx = MsgHelper.FindWindow(null, "LemonApp Debug Console");
                    if (hx== IntPtr.Zero)
                    {
                        Toast.Send("已进入调试模式🐱‍👤");
                        if (Console.pipe != null)
                            Console.Close();
                        Console.Open();
                        Console.WriteLine("调试模式");
                    }
                    else
                    {
                        Console.Close();
                        Toast.Send("已退出调试模式🐱‍👤");
                    }
                }
            }
            return IntPtr.Zero;
        }
        private const int WM_HOTKEY = 0x0312;
        #endregion
        #region 进程通信
        private void LoadSEND_SHOW()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource source = HwndSource.FromHwnd(hwnd);
            if (source != null) source.AddHook(WndProc);
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == MsgHelper.WM_COPYDATA)
            {
                MsgHelper.COPYDATASTRUCT cdata = new MsgHelper.COPYDATASTRUCT();
                Type mytype = cdata.GetType();
                cdata = (MsgHelper.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, mytype);
                if (cdata.lpData == MsgHelper.SEND_SHOW)
                    exShow();
            }
            return IntPtr.Zero;
        }
        #endregion
    }
}