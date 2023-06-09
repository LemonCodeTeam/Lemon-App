using LemonApp.ContentPage;
using LemonApp.ControlItems;
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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
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
        //-------Mini--------
        private MiniPlayer mini;
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
        public MusicLib ml;
        public PlayDLItem MusicData = new PlayDLItem(new Music());
        bool isplay = false;
        bool IsRadio = false;
        string RadioID = "";
        public static MusicPlayer mp;
        /// <summary>
        /// 歌词页面是否打开
        /// </summary>
        int IsLyricPageOpen = 0;
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
        private Storyboard LoadingAni = null;
        /*     Thread tOL = null;
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
        */
        public void OpenLoading()
        {
            if (!isLoading)
            {
                isLoading = true;
                LoadingAni.Stop();
                LoadingBar.BeginAnimation(OpacityProperty, null);
                LoadingBar.Visibility = Visibility.Visible;
                LoadingAni.Begin();
                /*
                tOL = new Thread(RunThread);
                tOL.SetApartmentState(ApartmentState.STA);
                tOL.Start();
                */
            }
        }
        public async void CloseLoading()
        {
            if (isLoading)
            {
                await Task.Delay(100);
                isLoading = false;
                var don = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
                don.Completed += delegate { LoadingAni.Stop(); LoadingBar.Visibility = Visibility.Collapsed; };
                LoadingBar.BeginAnimation(OpacityProperty, don);

                /*
                aw.Dispatcher.Invoke(() =>
                {
                    var da = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
                    da.Completed += delegate { aw.Close(); };
                    aw.BeginAnimation(OpacityProperty, da);
                });
                */
            }
        }
        #endregion
        #region 窗口加载时
        public MainWindow()
        {
            InitializeComponent();
        }

        #region 加载窗口时的基础配置 登录/播放组件
        /*
         主要分为三大组件的加载:
            1.窗口必要组件            ↓依次进行
            2.播放组件
            3.登录后的用户数据
         */
        public static void FixPopupBug()
        {
            var ifLeft = SystemParameters.MenuDropAlignment;
            if (ifLeft)
            {
                var t = typeof(SystemParameters);
                var field = t.GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
                field.SetValue(null, false);
            }

        }
        private async void window_Loaded(object sender, RoutedEventArgs e)
        {
            #region Part1
            //--------检测更新-------
            Update();
            //--------应用程序配置 热键和消息回调--------
            Settings.Handle.WINDOW_HANDLE = new WindowInteropHelper(this).Handle.ToInt32();
            Settings.Handle.ProcessId = Process.GetCurrentProcess().Id;
            Settings.SaveHandle();
            LoadSEND_SHOW();
            //-------注册个模糊效果------
            wac = new WindowAccentCompositor(this, (c) =>
            {
                Page.Background = new SolidColorBrush(c);
            });
            //---------Popup的移动事件
            LocationChanged += delegate
            {
                RUNPopup(Search_SmartBox);
                RUNPopup(SingerListPop);
                RUNPopup(MoreBtn_Meum);
                RUNPopup(Gdpop);
                RUNPopup(IntoGDPop);
                RUNPopup(AddGDPop);
            };
            FixPopupBug();//a disgusting & stupid thing...
            //---------任务栏 TASKBAR-----------
            //任务栏 缩略图 按钮
            TaskBarImg = new TabbedThumbnail(this, this, new Vector());
            TaskbarManager.Instance.TabbedThumbnail.AddThumbnailPreview(TaskBarImg);
            TaskBarImg.SetWindowIcon(Properties.Resources.icon);
            TaskBarImg.TabbedThumbnailActivated += delegate
            {
                WindowState = WindowState.Normal;
                Activate();
            };
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
            //-----------注册Debug回调-------
            MainClass.DebugCallBack = (a, b) =>
            {
                Console.WriteLine(b, a);
            };
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
            RotateTransform rtf = new RotateTransform();
            LyricBig.RenderTransform = rtf;
            DoubleAnimation dbAscending = new DoubleAnimation(0, new Duration
            (TimeSpan.FromSeconds(2)));
            rtf.BeginAnimation(RotateTransform.AngleProperty, dbAscending);
            //-----加载动画LoadingAnimation
            LoadingAni = Resources["LoadingAni"] as Storyboard;
            //-----Timer 清理与更新播放设备
            var ds = new System.Windows.Forms.Timer() { Interval = 5000 };
            ds.Tick += delegate { if (t.Enabled) mp.UpdateDevice(); GC.Collect(); };
            ds.Start();
            #endregion
            #region Part2
            //----播放组件-----------
            await MusicPlayer.PrepareDll();
            mp = new MusicPlayer(new WindowInteropHelper(this).Handle);
            //-----歌词显示 歌曲播放 等组件的加载
            lv = new LyricView();
            lv.ClickLyric += Lv_ClickLyric;
            lv.NormalLrcColor = new SolidColorBrush(Color.FromRgb(255, 255, 255)) { Opacity = 0.4 };
            lv.TextAlignment = TextAlignment.Center;
            ly.Child = lv;
            lv.NextLyric += async (lrc, trans) =>
            {
                //主要用于桌面歌词的显示
                if (lrc != "" && lastlyric != lrc)
                {
                    //有歌词更新
                    mini.lyric.Text = lrc;
                    if (Settings.USettings.DoesOpenDeskLyric)
                    {
                        var str = lrc + (Settings.USettings.TransLyric ? (trans == null ? "" : ("\r\n" + trans)) : "");
                        if (Settings.USettings.LyricAppBarOpen)
                            lyricTa.Update(str);
                        else lyricToast.Update(str);
                        lastlyric = lrc;
                    }
                    if (Settings.USettings.IsLyricImm)
                    {
                        lastlyric = lrc;
                        ImmTb_Lyric.Text = "";
                        ImmTb_Trans.Text = "";
                        string dt = lrc;
                        ImmTb_Trans.Text = Settings.USettings.TransLyric ? (trans ?? "") : "";
                        //分割划词
                        string[] dta = dt.Split(' ');
                        if (dta.Count() >= 3)
                        {
                            foreach (string a in dta)
                            {
                                ImmTb_Lyric.Text += a + " ";
                                await Task.Delay(150);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < dt.Length; i += 4)
                            {
                                string a = "";
                                if (i + 4 <= dt.Length)
                                    a = dt.Substring(i, 4);
                                else a = dt.Substring(i, dt.Length - i);
                                ImmTb_Lyric.Text += a;
                                await Task.Delay(150);
                            }
                        }
                    }
                }
            };
            ml = new MusicLib(Settings.USettings.LemonAreeunIts);
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
                        mini.jd.Value = now;
                        lyric_jd.Value = now;
                        Play_Now.Text = TimeSpan.FromMilliseconds(now).ToString(@"mm\:ss");
                    }
                    all = mp.GetLength.TotalMilliseconds;
                    string alls = TimeSpan.FromMilliseconds(all).ToString(@"mm\:ss");
                    Play_All.Text = alls;
                    jd.Maximum = all;
                    mini.jd.Maximum = all;
                    lyric_jd.Maximum = all;
                    if (IsLyricPageOpen == 1)
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
                                if (Settings.USettings.IsLyricImm && sv > 0.25)
                                {
                                    (Resources["LyricImm_High"] as Storyboard).Begin();
                                }
                                else
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
                        }
                        lv.LrcRoll(now + lyrictime_offset * 1000, true);
                    }
                    else lv.LrcRoll(now + lyrictime_offset * 1000, false);
                    //now does not necessarily equal to total...
                    if (Play_Now.Text.Equals(alls) && now > 2000 && all != 0)
                    {
                        now = 0;
                        all = 0;
                        mp.Position = TimeSpan.FromSeconds(0);
                        t.Stop();
                        //-----------播放完成时，判断单曲还是下一首
                        jd.Value = 0;
                        if (Settings.USettings.PlayXHMode == 1)//单曲循环
                        {
                            mp.Position = TimeSpan.FromMilliseconds(0);
                            mp.Play();
                            t.Start();
                        }
                        else if (Settings.USettings.PlayXHMode == 0 || Settings.USettings.PlayXHMode == 2) PlayControl_PlayNext(null, null);//下一曲
                    }
                }
                catch { }
            };
            //---------------------
            LP_ag1.MouseDown += LP_ag_MouseDown;
            LP_ag2.MouseDown += LP_ag_MouseDown;
            LP_ag3.MouseDown += LP_ag_MouseDown;
            //-------------------------
            PlayDLItem.Delete = new Action<PlayDLItem>((e) => this.PlayDL_List.Items.Remove(e));
            //----Load Mini-----------------
            mini = new MiniPlayer(this);
            //--------去除可恶的焦点边缘线
            UIHelper.G(Page);
            #endregion
            #region Part3
            //--------读取登录数据------
            await Settings.LoadLocaSettings();
            if (!string.IsNullOrEmpty(Settings.LSettings.qq))
                await Settings.LoadUSettings(Settings.LSettings.qq);
            else
            {
                string qq = "0";
                await Settings.LoadUSettings(qq);
                Settings.USettings.LemonAreeunIts = qq;
                Settings.SaveSettingsAsync();
                Settings.LSettings.qq = qq;
                await Settings.SaveLocaSettings();
            }
            LoadHotDog();
            Load_Theme();
            LoadMusicData();
            #endregion
            //组件加载完成................进入主页..
            //--------加载主页---------
            ClHomePage = new HomePage(this, TemplateSv.Template);
            ContentPage.Children.Add(ClHomePage);
            NSPage(new MeumInfo(ClHomePage, Meum_MusicKu), true, false);
        }

        private void Lv_ClickLyric(object sender, MouseButtonEventArgs e)
        {
            if (HasOpenLranslationPage) {
                var tb = sender as TextBlock;
                foreach (var i in tb.Inlines)
                    if (i.Name.Equals("main")) {
                        ta.Translation_ta.UpdateText((i as Run).Text);
                        return;
                    }
            }
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
            if (Settings.USettings.Skin_Type == 0)
            {
                //默认主题  （主要考虑到切换登录）
                if (wac.IsEnabled) wac.IsEnabled = false;
                Page.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                App.BaseApp.unSkin();
                App.BaseApp.SetColor("ThemeColor", Color.FromRgb(Settings.USettings.Skin_ThemeColor_R, Settings.USettings.Skin_ThemeColor_G, Settings.USettings.Skin_ThemeColor_B));
                DThemePage.Child = null;
            }
            else if (Settings.USettings.Skin_Type == 1)
            {
                //图片主题

                //    主题背景图片
                if (Settings.USettings.Skin_ImagePath != "" && System.IO.File.Exists(Settings.USettings.Skin_ImagePath))
                    Page.Background = new ImageBrush(new BitmapImage(new Uri(Settings.USettings.Skin_ImagePath, UriKind.Absolute)));
                else if (Settings.USettings.Skin_ImagePath == "[Dark]")
                {
                    Page.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2D2D30"));
                }

                //字体颜色
                Color co;
                if (Settings.USettings.Skin_FontColor == "Black")
                {
                    co = Color.FromRgb(64, 64, 64);
                    App.BaseApp.Skin_Black();
                }
                else
                {
                    co = Color.FromRgb(255, 255, 255);
                    App.BaseApp.Skin();
                }
                App.BaseApp.SetColor("ThemeColor", Color.FromRgb(Settings.USettings.Skin_ThemeColor_R, Settings.USettings.Skin_ThemeColor_G, Settings.USettings.Skin_ThemeColor_B));
                App.BaseApp.SetColor("ResuColorBrush", co);
            }
            else if (Settings.USettings.Skin_Type == 2)
            {
                //----新的[磨砂黑/白]主题---
                if (Settings.USettings.Skin_FontColor == "White")
                {
                    WdBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(32, 32, 32));
                    Page.Background = new SolidColorBrush(Colors.Transparent);
                    DThemePage.Child = null;
                    App.BaseApp.Skin();
                    App.BaseApp.SetColor("ThemeColor", Color.FromRgb(Settings.USettings.Skin_ThemeColor_R, Settings.USettings.Skin_ThemeColor_G, Settings.USettings.Skin_ThemeColor_B));
                    wac.Color = Color.FromArgb(200, 0, 0, 0);
                    wac.IsEnabled = true;
                }
                else
                {
                    WdBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180));
                    Page.Background = new SolidColorBrush(Colors.Transparent);
                    DThemePage.Child = null;
                    App.BaseApp.SetColor("ThemeColor", Color.FromRgb(Settings.USettings.Skin_ThemeColor_R, Settings.USettings.Skin_ThemeColor_G, Settings.USettings.Skin_ThemeColor_B));
                    App.BaseApp.Skin_Black();
                    Color co = Color.FromRgb(64, 64, 64);
                    App.BaseApp.SetColor("ResuColorBrush", co);
                    wac.Color = (Color.FromArgb(200, 255, 255, 255));
                    wac.IsEnabled = true;
                }
                DThemePage.Child = null;
            }
            else if (Settings.USettings.Skin_Type == 3)
            {
                //动态主题
                string NameSpace = TextHelper.FindTextByAB(Settings.USettings.Skin_ImagePath, "DTheme[", "]", 0);
                ThemeBase tb = null;
                if (NameSpace == Theme.Dtpp.Drawer.NameSpace)
                    tb = new Theme.Dtpp.Drawer(false);
                else if (NameSpace == Theme.TheFirstSnow.Drawer.NameSpace)
                    tb = new Theme.TheFirstSnow.Drawer(false);
                else if (NameSpace == Theme.YeStarLight.Drawer.NameSpace)
                    tb = new Theme.YeStarLight.Drawer(false);
                else if (NameSpace == Theme.TechDMusic.Drawer.NameSpace)
                    tb = new Theme.TechDMusic.Drawer(false);
                else if (NameSpace == Theme.FerrisWheel.Drawer.NameSpace)
                    tb = new Theme.FerrisWheel.Drawer(false);
                DThemePage.Child = tb;
                //字体颜色
                Color col;
                if (tb.FontColor == "Black")
                {
                    col = Color.FromRgb(64, 64, 64);
                    App.BaseApp.Skin_Black();
                }
                else
                {
                    col = Color.FromRgb(255, 255, 255);
                    App.BaseApp.Skin();
                }
                Color theme = Color.FromRgb(Settings.USettings.Skin_ThemeColor_R, Settings.USettings.Skin_ThemeColor_G, Settings.USettings.Skin_ThemeColor_B);
                App.BaseApp.SetColor("ThemeColor", theme);
                App.BaseApp.SetColor("ResuColorBrush", col);
                tb.Draw();
            }
        }
        private double now = 0;
        private double all = 0;
        private string lastlyric = "";
        private LyricBar lyricTa = null;
        private Toast lyricToast = null;
        private void LoadMusicData()
        {
            //-------[登录]用户的头像、名称等配置加载
            if (Settings.USettings.LemonAreeunIts != "0")
            {
                UserName.Text = Settings.USettings.UserName;
                if (System.IO.File.Exists(Settings.USettings.UserImage))
                {
                    var image = new System.Drawing.Bitmap(Settings.USettings.UserImage);
                    UserTX.Background = new ImageBrush(image.ToImageSource());
                    image.Dispose();
                }
                //检测登录是否已失效
                Thread t = new Thread(async () =>
                {
                    var sl = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={Settings.USettings.LemonAreeunIts}&reqfrom=1&reqtype=0");
                    Debug.WriteLine(sl);
                    JObject j = JObject.Parse(sl);
                    if (j["code"].ToString() == "0")
                    {
                        var sdc = JObject.Parse(sl)["data"]["creator"];
                        await HttpHelper.HttpDownloadFileAsync(sdc["headpic"].ToString().Replace("http://", "https://"), Settings.USettings.DataCachePath + Settings.USettings.LemonAreeunIts + ".jpg");
                        string name = sdc["nick"].ToString();
                        Settings.USettings.UserName = name;
                        var image = new System.Drawing.Bitmap(Settings.USettings.UserImage);
                        Dispatcher.Invoke(() =>
                        {
                            UserName.Text = name;
                            UserTX.Background = new ImageBrush(image.ToImageSource());
                        });
                        Dispatcher.Invoke(() =>
                        {
                            Toast.Send("登陆成功! o(*￣▽￣*)ブ  欢迎回来" + name);
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
            //-------是否打开了桌面歌词-----------
            OpenLyricAppBar.IsChecked= Settings.USettings.LyricAppBarOpen;
            if (Settings.USettings.DoesOpenDeskLyric)
            {
                if (Settings.USettings.LyricAppBarOpen)
                {
                    lyricTa = new LyricBar();
                    lyricTa.PopOut.MouseUp += PopOut_MouseUp;
                    lyricTa.Show();
                }
                else
                {
                    lyricToast = new Toast("", true);
                    lyricToast.Show();
                }
                path7.SetResourceReference(Path.FillProperty, "ThemeColor");
            }
            //------TransLyric Icon----------------
            if (Settings.USettings.TransLyric)
            {
                TransLyricIcon.SetResourceReference(Path.FillProperty, "ThemeColor");
            }
            else TransLyricIcon.Fill = new SolidColorBrush(Color.FromArgb(140, 255, 255, 255));
            //---------加载上一次播放-------与播放列表
            //如果已经保存过播放列表
            if (Settings.USettings.PlayingIndex != -1)
            {
                np = NowPage.GDItem;
                ListBox list = new ListBox();
                foreach (var a in Settings.USettings.MusicGDataPlayList)
                {
                    if (a.MusicID != "str.Null")
                    {
                        list.Items.Add(new DataItem(a));
                    }
                    else
                    {
                        list.Items.Add(new ListBoxItem() { Visibility = Visibility.Collapsed });
                    }
                }
                PushPlayMusic((DataItem)list.Items[Settings.USettings.PlayingIndex], list, false);
            }
            //没有的话就交给这位先生处理吧
            else if (Settings.USettings.Playing.MusicName != "")
            {
                MusicData = new PlayDLItem(Settings.USettings.Playing);
                PlayMusic(Settings.USettings.Playing, false);
            }
            //---------------登陆之后-同步"我喜欢"歌单ids----------
            if (Settings.USettings.LemonAreeunIts != "0")
            {
                Thread tx = new Thread(async () =>
                {
                    Dictionary<string, string> dt = new Dictionary<string, string>();
                    MusicLib.GetGDAsync(MusicLib.MusicLikeGDid ?? await ml.GetMusicLikeGDid(), new Action<string, string>((mid, id) =>
                    {
                        dt.Add(mid, id);
                    }));
                    Settings.USettings.MusicGDataLike.ids = dt;
                });
                tx.Start();
            }
            //-------------------
            AudioSlider.Value = 100;
            //---------载入沉浸歌词------
            if (Settings.USettings.IsLyricImm)
            {
                LyricNor.Visibility = Visibility.Collapsed;
                LyricImm.Visibility = Visibility.Visible;
            }
            //-----------是否打开了mini小窗-----------
            if (Settings.USettings.IsMiniOpen)
                mini.Show();
            //---------专辑图是圆的吗??-----
            MusicImage.CornerRadius = new CornerRadius(Settings.USettings.IsRoundMusicImage);
            //-----------QuickGoto列表加载---------------
            QuickGoToList.Children.Clear();
            foreach (var a in Settings.USettings.QuickGoToData)
                AddToQGT(a.Value, false);
            //----------播放循环模式------------------------
            mini.XHPath.Data = path6.Data = Geometry.Parse(Settings.USettings.PlayXHMode switch
            {
                0 => Properties.Resources.Lbxh,
                1 => Properties.Resources.Dqxh,
                2 => Properties.Resources.Random,
                _ => null
            });
        }

        private void PopOut_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenLyricAppBar.IsChecked = Settings.USettings.LyricAppBarOpen = false;
            lyricTa.Close();
            lyricToast = new Toast("", true);
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
        }
        /// <summary>
        /// 遍历调整宽度
        /// </summary>
        /// <param name="wp"></param>
        public void WidthUI(Panel wp, double? ContentWidth = null)
        {
            if (wp.Visibility == Visibility.Visible && wp.Children.Count > 0)
            {
                //每一行的元素数量
                int lineCount = int.Parse(wp.Uid);
                var uc = wp.Children[0] as UserControl;
                double max = uc.MaxWidth;
                double min = uc.MinWidth;
                ContentWidth ??= ContentPage.ActualWidth;
                //控制边距
                ContentWidth -= 5;
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
        public async void Login(LoginData data)
        {
            lw.Close();
            GC.Collect();
            //----------切换登录前先保存当前账号-------
            if (Settings.USettings.LemonAreeunIts != "0" && Settings.USettings.LemonAreeunIts != data.qq)
                await Settings.SaveSettingsTaskAsync();
            string qq = data.qq;
            Console.WriteLine("Login:" + data.g_tk + "\r\n Cookie:" + data.cookie, "LoginData");
            if (Settings.USettings.LemonAreeunIts == qq)
            {
                if (data.g_tk != null)
                {
                    Settings.USettings.g_tk = data.g_tk;
                    Settings.USettings.Cookie = data.cookie;
                    await Settings.SaveSettingsTaskAsync();
                }
            }
            else
            {
                //临时使用
                if (data.g_tk != null)
                {
                    Settings.USettings.g_tk = data.g_tk;
                    Settings.USettings.Cookie = data.cookie;
                }
                var sl = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={qq}&reqfrom=1&reqtype=0");
                Console.WriteLine(sl);
                var sdc = JObject.Parse(sl)["data"]["creator"];
                await HttpHelper.HttpDownloadFileAsync(sdc["headpic"].ToString().Replace("http://", "https://"), Settings.USettings.DataCachePath + qq + ".jpg");
                string name = sdc["nick"].ToString();
                await Settings.LoadUSettings(qq);
                if (data.g_tk != null)
                {
                    Settings.USettings.g_tk = data.g_tk;
                    Settings.USettings.Cookie = data.cookie;
                }
                Settings.USettings.UserName = name;
                Settings.USettings.UserImage = Settings.USettings.DataCachePath + qq + ".jpg";
                Settings.USettings.LemonAreeunIts = qq;
                await Settings.SaveSettingsTaskAsync();
                Settings.LSettings.qq = qq;
                await Settings.SaveLocaSettings();
                await ml.GetMusicLikeGDid();
                Toast.Send("登陆成功! o(*￣▽￣*)ブ  欢迎回来" + name);
                Console.WriteLine(Settings.USettings.g_tk + "  " + Settings.USettings.Cookie);
                Load_Theme();
                LoadMusicData();
            }
        }
        #endregion
        #region 设置
        private void ApplyHotKey_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Settings.USettings.HotKeys.Clear();
            var handle = UnHotKey();
            List<string> list = new List<string>();
            foreach (HotKeyChooser dt in KeysWrap.Children)
            {
                list.Add(dt.MainKey + dt.tKey.ToString());
            }
            if (list.GroupBy(i => i).Where(g => g.Count() > 1).Count() > 0)
            {
                Toast.Send("居然有重复的热键???");
                return;
            }
            foreach (HotKeyChooser dt in KeysWrap.Children)
            {
                HotKeyInfo hk = new HotKeyInfo();
                hk.desc = dt.desc;
                hk.KeyID = dt.KeyId;
                hk.tKey = dt.key;
                hk.MainKey = dt.MainKey;
                hk.MainKeyIndex = dt.index;
                Settings.USettings.HotKeys.Add(hk);
                RegisterHotKey(handle, hk.KeyID, (uint)hk.MainKey, (uint)(System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(hk.tKey));
                Toast.Send("设置热键成功！");
            }
        }
        public void LoadSettings()
        {
            CachePathTb.Text = Settings.USettings.MusicCachePath;
            DownloadPathTb.Text = Settings.USettings.DownloadPath;
            DownloadWithLyric.IsChecked = Settings.USettings.DownloadWithLyric;
            DownloadNameTb.Text = Settings.USettings.DownloadName;

            if (Settings.USettings.HotKeys.Count >= 0)
            {
                for (int i = 0; i < Settings.USettings.HotKeys.Count; i++)
                {
                    HotKeyChooser hk = KeysWrap.Children[i] as HotKeyChooser;
                    HotKeyInfo hi = Settings.USettings.HotKeys[i];
                    hk.desc = hi.desc;
                    hk.index = hi.MainKeyIndex;
                    hk.KeyId = hi.KeyID;
                    hk.key = hi.tKey;
                }
            }
        }
        private void UserSendButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //⚠警告!!!: 以下key仅供本开发者(TwilightLemon)使用,
            //               若发现滥用现象，将走法律程序解决!!!
            //KEY: xfttsuxaeivzdefd
            if (UserSendText.Text != "在此处输入你的建议或问题，或拖动附件到上方" && knowb.Text != string.Empty)
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
            NSPage(new MeumInfo(SettingsPage, null));
        }

        private void CP_ChooseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var g = new System.Windows.Forms.FolderBrowserDialog();
            if (g.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CachePathTb.Text = g.SelectedPath;
                Settings.USettings.MusicCachePath = g.SelectedPath;
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
            if (TextColor_byChoosing == "Black")
            {
                TextColor_byChoosing = "Black";
                Skin_ChooseBox_Font.Background = new SolidColorBrush(Colors.Black);
            }
            else
            {
                TextColor_byChoosing = "White";
                Skin_ChooseBox_Font.Background = new SolidColorBrush(Color.FromRgb(218, 218, 218));
            }
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
            //自定义主题  主题颜色/字体颜色
            Color co;
            if (TextColor_byChoosing == "Black")
            {
                co = Color.FromRgb(64, 64, 64);
                App.BaseApp.Skin_Black();
            }
            else
            {
                co = Color.FromRgb(255, 255, 255);
                App.BaseApp.Skin();
            }
            App.BaseApp.SetColor("ThemeColor", Theme_Choose_Color);
            App.BaseApp.SetColor("ResuColorBrush", co);
            Settings.USettings.Skin_FontColor = TextColor_byChoosing;
            Settings.USettings.Skin_ThemeColor_R = Theme_Choose_Color.R;
            Settings.USettings.Skin_ThemeColor_G = Theme_Choose_Color.G;
            Settings.USettings.Skin_ThemeColor_B = Theme_Choose_Color.B;
            Settings.SaveSettingsAsync();
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
                    string file = Settings.USettings.MusicCachePath + "Skin\\" + TextHelper.MD5.EncryptToMD5string(System.IO.File.ReadAllText(strFileName)) + System.IO.Path.GetExtension(strFileName);
                    System.IO.File.Copy(strFileName, file, true);
                    Dispatcher.Invoke(new Action(() =>
                    Page.Background = new ImageBrush(new System.Drawing.Bitmap(file).ToImageSource())));
                    Settings.USettings.Skin_ImagePath = file;
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
                Page.Background = null;
                Color co;
                if (sc.txtColor == "Black")
                {
                    co = Color.FromRgb(64, 64, 64);
                    App.BaseApp.Skin_Black();
                }
                else
                {
                    co = Color.FromRgb(255, 255, 255);
                    App.BaseApp.Skin();
                }
                App.BaseApp.SetColor("ThemeColor", sc.theme);
                App.BaseApp.SetColor("ResuColorBrush", co);
                Settings.USettings.Skin_Type = 3;
                Settings.USettings.Skin_ThemeColor_R = sc.theme.R;
                Settings.USettings.Skin_ThemeColor_G = sc.theme.G;
                Settings.USettings.Skin_ThemeColor_B = sc.theme.B;
                Settings.USettings.Skin_ImagePath = "DTheme[" + bg + "]";
                Settings.USettings.Skin_FontColor = ThemeName;
                Settings.SaveSettingsAsync();
            };
            SkinIndexList.Children.Add(sc);
        }
        private async void SkinBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(new MeumInfo(SkinPage, null));
            SkinIndexList.Children.Clear();
            #region 动态皮肤
            LoadDTheme(new Theme.FerrisWheel.Drawer());
            LoadDTheme(new Theme.TechDMusic.Drawer());
            LoadDTheme(new Theme.Dtpp.Drawer());
            LoadDTheme(new Theme.TheFirstSnow.Drawer());
            LoadDTheme(new Theme.YeStarLight.Drawer());
            #endregion
            #region 默认主题
            SkinControl sxc_black = new SkinControl("暗黑", new SolidColorBrush(Color.FromRgb(60, 60, 60)), Color.FromRgb(0, 0, 0));
            sxc_black.MouseDown += (s, n) =>
            {
                if (wac.IsEnabled) wac.IsEnabled = false;
                DThemePage.Child = null;
                App.BaseApp.Skin();
                Page.Background = new SolidColorBrush(Color.FromRgb(45, 45, 48));
                Color theme = (Color)ColorConverter.ConvertFromString("#FF31C27C");
                Settings.USettings.Skin_ThemeColor_R = theme.R;
                Settings.USettings.Skin_ThemeColor_G = theme.G;
                Settings.USettings.Skin_ThemeColor_B = theme.B;
                Settings.USettings.Skin_Type = 1;
                Settings.USettings.Skin_FontColor = "White";
                Settings.USettings.Skin_ImagePath = "[Dark]";
                Settings.SaveSettingsAsync();
            };
            sxc_black.Margin = new Thickness(12, 0, 12, 20);
            SkinIndexList.Children.Add(sxc_black);
            SkinControl sxc = new SkinControl("-1", "素白", Color.FromArgb(0, 0, 0, 0));
            sxc.MouseDown += (s, n) =>
            {
                if (wac.IsEnabled) wac.IsEnabled = false;
                Page.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                DThemePage.Child = null;
                App.BaseApp.unSkin();
                Color theme = (Color)ColorConverter.ConvertFromString("#FF31C27C");
                Settings.USettings.Skin_ThemeColor_R = theme.R;
                Settings.USettings.Skin_ThemeColor_G = theme.G;
                Settings.USettings.Skin_ThemeColor_B = theme.B;
                Settings.USettings.Skin_Type = 0;
                Settings.USettings.Skin_FontColor = "";
                Settings.USettings.Skin_ImagePath = "";
                Settings.SaveSettingsAsync();
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
                wac.Color = Color.FromArgb(200, 0, 0, 0);
                wac.IsEnabled = true;
                Color theme = (Color)ColorConverter.ConvertFromString("#FF31C27C");
                Settings.USettings.Skin_ThemeColor_R = theme.R;
                Settings.USettings.Skin_ThemeColor_G = theme.G;
                Settings.USettings.Skin_ThemeColor_B = theme.B;
                Settings.USettings.Skin_Type = 2;
                Settings.USettings.Skin_FontColor = "White";
                Settings.USettings.Skin_ImagePath = "";
                Settings.SaveSettingsAsync();
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
                wac.Color = (Color.FromArgb(200, 255, 255, 255));
                wac.IsEnabled = true;
                Color theme = (Color)ColorConverter.ConvertFromString("#FF31C27C");
                Settings.USettings.Skin_ThemeColor_R = theme.R;
                Settings.USettings.Skin_ThemeColor_G = theme.G;
                Settings.USettings.Skin_ThemeColor_B = theme.B;
                Settings.USettings.Skin_Type = 2;
                Settings.USettings.Skin_FontColor = "Black";
                Settings.USettings.Skin_ImagePath = "";
                Settings.SaveSettingsAsync();
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
                if (!System.IO.File.Exists(Settings.USettings.MusicCachePath + "Skin\\" + uri + ".jpg"))
                    await HttpHelper.HttpDownloadFileAsync($"https://gitee.com/TwilightLemon/ux/raw/master/w{uri}.jpg", Settings.USettings.MusicCachePath + "Skin\\" + uri + ".jpg");
                SkinControl sc = new SkinControl(uri, name, color);
                sc.txtColor = dx["TextColor"].ToString();
                sc.MouseDown += async (s, n) =>
                {
                    if (wac.IsEnabled) wac.IsEnabled = false;
                    if (!System.IO.File.Exists(Settings.USettings.MusicCachePath + "Skin\\" + sc.imgurl + ".png"))
                        await HttpHelper.HttpDownloadFileAsync($"https://gitee.com/TwilightLemon/ux/raw/master/{sc.imgurl}.png", Settings.USettings.MusicCachePath + "Skin\\" + sc.imgurl + ".png");
                    Page.Background = new ImageBrush(new System.Drawing.Bitmap(Settings.USettings.MusicCachePath + "Skin\\" + sc.imgurl + ".png").ToImageSource());
                    DThemePage.Child = null;
                    Color co;
                    if (sc.txtColor == "Black")
                    {
                        co = Color.FromRgb(64, 64, 64); App.BaseApp.Skin_Black();
                    }
                    else
                    {
                        co = Color.FromRgb(255, 255, 255); App.BaseApp.Skin();
                    }
                    App.BaseApp.SetColor("ThemeColor", sc.theme);
                    App.BaseApp.SetColor("ResuColorBrush", co);
                    Settings.USettings.Skin_Type = 1;
                    Settings.USettings.Skin_ImagePath = Settings.USettings.MusicCachePath + "Skin\\" + sc.imgurl + ".png";
                    Settings.USettings.Skin_FontColor = sc.txtColor;
                    Settings.USettings.Skin_ThemeColor_R = sc.theme.R;
                    Settings.USettings.Skin_ThemeColor_G = sc.theme.G;
                    Settings.USettings.Skin_ThemeColor_B = sc.theme.B;
                    Settings.SaveSettingsAsync();
                };
                sc.Margin = new Thickness(12, 0, 12, 20);
                SkinIndexList.Children.Add(sc);
            }
            #endregion
            WidthUI(SkinIndexList);
        }
        #endregion
        #region 功能区
        #region QuickGoTo 快速启动栏   preview
        public async void AddToQGT(QuickGoToData data, bool needSave = true)
        {
            if (needSave && Settings.USettings.QuickGoToData.ContainsKey(data.type + data.id))
                return;
            Border b = new Border() { Height = 25, Width = 25, CornerRadius = new CornerRadius(25), Margin = new Thickness(10, 0, 0, 0), Background = QGT_AddBtn.Background };
            b.MouseLeftButtonDown += QGTMouseDown;
            b.MouseRightButtonDown += QGTDelete;
            b.Tag = data;
            b.ToolTip = data.name;
            RenderOptions.SetBitmapScalingMode(b, BitmapScalingMode.Fant);
            b.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(data.imgurl, new int[2] { 50, 50 }));
            QuickGoToList.Children.Insert(0, b);
            if (needSave)
                Settings.USettings.QuickGoToData.Add(data.type + data.id, data);
        }

        private void QGTMouseDown(object sender, MouseEventArgs e)
        {
            var data = (sender as Border).Tag as QuickGoToData;
            if (data.type == "Singer")
                K_GetToSingerPage(new MusicSinger() { Mid = data.id, Name = data.name, Photo = data.imgurl });
            else if (data.type == "GD")
                LoadFxGDItems(new FLGDIndexItem() { data = new MusicGD() { ID = data.id, Name = data.name, Photo = data.imgurl, ListenCount = 0 } });
            else if (data.type == "TopList")
                GetTopItems(new TopControl(new MusicTop() { ID = data.id, Name = data.name, Photo = data.imgurl }));
            else if (data.type == "Radio")
                GetRadio(new RadioItem(new MusicRadioListItem() { ID = data.id, lstCount = 0, Name = data.name, Photo = data.imgurl }), null);
        }
        private void QGTDelete(object sender, MouseEventArgs e)
        {
            var b = sender as Border;
            var data = b.Tag as QuickGoToData;
            QuickGoToList.Children.Remove(b);
            Settings.USettings.QuickGoToData.Remove(data.type + data.id);
        }
        #endregion
        #region HomePage 主页
        //IFV的回调函数
        public async void IFVCALLBACK_LoadAlbum(string id, bool NeedSave = true)
        {
            np = NowPage.GDItem;
            DataCollectBtn.Visibility = Visibility.Collapsed;
            DataItemsList.Opacity = 0;
            NSPage(new MeumInfo(Data, null) { cmd = "[DataUrl]{\"type\":\"Album\",\"key\":\"" + id + "\"}" }, NeedSave, false);
            DataItemsList.Items.Clear();
            int count = (int)(DataItemsList.ActualHeight
            / 45);
            int index = 0;
            var dta = await MusicLib.GetAlbumSongListByIDAsync(id, new Action<Music, bool>((j, f) =>
            {
                var k = new DataItem(j, this, index);
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
            await Task.Yield();
            ContentAnimation(DataItemsList, new Thickness(0, 200, 0, 0));
        }
        #endregion
        #region Top 排行榜
        /// <summary>
        /// 加载TOP项
        /// </summary>
        /// <param name="g">Top ID</param>
        /// <param name="osx">页数</param>
        public async void GetTopItems(TopControl g, int osx = 1, bool NeedSave = true)
        {
            np = NowPage.Top;
            tc_now = g;
            ixTop = osx;
            OpenLoading();
            if (osx == 1)
            {
                DataCollectBtn.Visibility = Visibility.Collapsed;
                DataItemsList.Opacity = 0;
                NSPage(new MeumInfo(Data, null) { cmd = "[DataUrl]{\"type\":\"Top\",\"key\":\"" + g.Data.ID + "\",\"name\":\"" + g.Data.Name + "\",\"img\":\"" + g.Data.Photo + "\"}" }, NeedSave, false);
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
                var k = new DataItem(j, this, index);
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
            }), osx);
            if (osx == 1)
                ContentAnimation(DataItemsList, new Thickness(0, 200, 0, 0));
        }
        #endregion
        #region Update 检测更新
        private void Update()
        {
            //新开个线程，不需要占用加载时
            Thread t = new Thread(async () =>
            {
                var o = JObject.Parse(await HttpHelper.GetWebAsync("https://gitee.com/TwilightLemon/UpdataForWindows/raw/master/WindowsUpdata.json"));
                string v = o["version"].ToString();
                string dt = o["description"].ToString().Replace("#", "\n");
                if (int.Parse(v) > int.Parse(App.EM))
                {
                    Dispatcher.Invoke(() => { new UpdateBox(v, dt).Show(); });
                }
            });
            t.Start();
        }
        #endregion
        #region N/S Page 切换页面

        public void ContentAnimation(DependencyObject TPage, Thickness value = new Thickness())
        {
            var sb = Resources["LoadContentAnimation"] as Storyboard;
            foreach (Timeline ac in sb.Children)
            {
                Storyboard.SetTarget(ac, TPage);
                if (ac is ThicknessAnimationUsingKeyFrames)
                {
                    var ta = ac as ThicknessAnimationUsingKeyFrames;
                    ta.KeyFrames[0].Value = new Thickness(0, 200 + value.Top, 0, -200 + value.Bottom);
                    ta.KeyFrames[1].Value = value;
                }
            }
            sb.Begin();
        }
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

        private MainMeumItem LastClickLabel = null;
        private UIElement LastPage = null;
        private Border LastCom = null;
        public void NSPage(MeumInfo data, bool needSave = true, bool Check = true)
        {
            if (data.Page == Data)
                if (DataPage_ControlMod)
                    CloseDataControlPage();
            if (LastClickLabel == null) LastClickLabel = Meum_MusicKu;
            LastClickLabel.HasChecked = false;
            if (data.MeumItem != null) (data.MeumItem as MainMeumItem).HasChecked = true;
            if (LastPage == null) LastPage = ClHomePage;
            Border com = (Border)(data.MeumItem is MainMeumItem ? (data.MeumItem as MainMeumItem).ComBlock : null);

            //从其他页面跳转过来的没有COM
            double mar_left = 5;
            if (com != null && LastCom == null)
            {
                GeneralTransform generalTransform = com.TransformToAncestor(ControlPage);
                Point point = generalTransform.Transform(new Point(0, 0));
                AniCom.BeginAnimation(MarginProperty, null);
                AniCom.Margin = new Thickness(mar_left, point.Y, 0, 0);
            }
            //COM动画------------
            AniCom.Visibility = com == null ? Visibility.Hidden : Visibility.Visible;
            if (com != null && LastCom != null)
            {
                Storyboard ani = null;
                GeneralTransform generalTransform = LastCom.TransformToAncestor(ControlPage);
                Point point = generalTransform.Transform(new Point(0, 0));
                //准备动画:
                GeneralTransform gT = com.TransformToAncestor(ControlPage);
                Point p = gT.Transform(new Point(0, 0));
                //动画相对位置 
                double op = p.Y - point.Y;
                Console.WriteLine(op);
                if (op > 0)
                {
                    ani = Resources["ControlsAniDown"] as Storyboard;
                    (ani.Children[0] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = 20 + op;
                    var tau = ani.Children[1] as ThicknessAnimationUsingKeyFrames;
                    tau.KeyFrames[0].Value = new Thickness(mar_left, point.Y, 0, 0);
                    tau.KeyFrames[1].Value = new Thickness(mar_left, point.Y + op, 0, 0);
                }
                else
                {
                    ani = Resources["ControlsAniUp"] as Storyboard;
                    (ani.Children[0] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = 20 - op;
                    var tau = ani.Children[1] as ThicknessAnimationUsingKeyFrames;
                    tau.KeyFrames[0].Value = new Thickness(mar_left, point.Y + op, 0, 0);
                }
                ani.Begin();
            }
            LastPage.Visibility = Visibility.Collapsed;

            //-----cmd跳转处理----
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
                                    var a = new FLGDIndexItem();
                                    a.data.ID = key;
                                    a.data.Photo = img;
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
                    if (data.cmd == "SingerBig")
                        SetTopWhite(true);
                    if (singer_now != data.data)
                    {
                        GetSinger(new SingerItem((MusicSinger)data.data), false);
                        return;
                    }
                }
            }
            //------------------
            data.Page.Uid = data.cmd;
            data.Page.Visibility = Visibility.Visible;
            RunAnimation(data.Page, data.value);

            if (data.MeumItem != null) LastClickLabel = data.MeumItem as MainMeumItem;
            LastPage = data.Page;
            LastCom = com;
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
            NSPage(new MeumInfo(ClTopPage, Meum_Top), true, false);
        }
        private void MusicKuBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(new MeumInfo(ClHomePage, Meum_MusicKu));
            ClHomePage.LoadHomePage();
        }
        //前后导航
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
                MiniBtn.ColorDx = SearchBox.Foreground;
            }
            else
            {
                SearchBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0C000000"));
                SearchBox.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                (LastPageBtn.Child as Path).SetResourceReference(Path.FillProperty, "ResuColorBrush");
                (NextPageBtn.Child as Path).SetResourceReference(Path.FillProperty, "ResuColorBrush");
                SkinBtn.ColorDx = null;
                SettingsBtn.ColorDx = null;
                CloseBtn.ColorDx = null;
                MaxBtn.ColorDx = null;
                MinBtn.ColorDx = null;
                MiniBtn.ColorDx = null;
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
            msx.Photo = $"https://y.gtimg.cn/music/photo_new/T001R300x300M000{msx.Mid}.jpg?max_age=2592000";
            GetSinger(new SingerItem(msx));
        }
        public async void GetSinger(SingerItem si, bool NeedSavePage = true)
        {
            if (Settings.USettings.LemonAreeunIts == "0")
            {
                if (TwMessageBox.Show("官方限制需要登录才能访问\r\n是否现在登录？"))
                    UserTX_MouseDown(null, null);
                return;
            }
            np = NowPage.SingerItem;
            singer_now = si.data;
            OpenLoading();
            BtD.LastBt = null;
            Cisv.Content = null;
            var data = await MusicLib.GetSingerPageAsync(si.data.Mid);
            var cc = new SingerPage(si.data, data, this, new Action(async () =>
             {
                 if (data.HasBigPic)
                 {
                     await Task.Delay(100);
                     NSPage(new MeumInfo(SingerDataPage, Meum_Singer) { value = new Thickness(0, -50, 0, 0), cmd = "SingerBig", data = si.data }, NeedSavePage, false);
                 }
                 else
                 {
                     await Task.Delay(100);
                     NSPage(new MeumInfo(SingerDataPage, Meum_Singer) { cmd = "Singer", data = si.data }, NeedSavePage, false);
                 }
             }));
            Cisv.Content = cc;
            SingerDP_Top.Uid = "gun";
            Cisv.LastLocation = 0;
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
            NSPage(new MeumInfo(ClSingerIndexPage, Meum_Singer), true, false);
        }
        private void SingerGetToIFollow()
        {
            if (ClMyFollowSingerList == null)
            {
                ClMyFollowSingerList = new MyFollowSingerList(this, TemplateSv.Template);
                ContentPage.Children.Add(ClMyFollowSingerList);
            }
            else ClMyFollowSingerList.GetSingerList();
            NSPage(new MeumInfo(ClMyFollowSingerList, Meum_Singer), true, false);
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
            NSPage(new MeumInfo(ClFLGDIndexPage, Meum_GD), true, false);
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
            NSPage(new MeumInfo(ClRadioIndexPage, Meum_Radio), true, false);
        }

        public async void GetRadio(object sender, MouseEventArgs e)
        {
            var dt = sender as RadioItem;
            if (dt.data.Name == "个性电台" && Settings.USettings.LemonAreeunIts == "0")
            {
                if (TwMessageBox.Show("官方限制需要登录才能访问\r\n是否现在登录？"))
                    UserTX_MouseDown(null, null);
                return;
            }
            OpenLoading();
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
            Settings.USettings.PlayXHMode = 0;
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
            mini.likeBtn_path.SetResourceReference(Shape.FillProperty, "ResuColorBrush");
            likeBtn_path.SetResourceReference(Shape.FillProperty, "ResuColorBrush");
        }
        /// <summary>
        /// 添加喜欢 变红色
        /// </summary>
        private void LikeBtnDown()
        {
            likeBtn_path.Tag = true;
            likeBtn_path.Fill = new SolidColorBrush(Color.FromRgb(216, 30, 30));
            lyric_like.Fill = mini.likeBtn_path.Fill = likeBtn_path.Fill;
        }
        /// <summary>
        /// 添加/删除 我喜欢的歌曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void likeBtn_MouseDown(object sender, MouseButtonEventArgs e)
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
                            string a = await MusicLib.DeleteMusicFromGDAsync(new string[1] { ac.Value }, MusicLib.MusicLikeGDdirid);
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
            if (Settings.USettings.LemonAreeunIts == "0")
                NSPage(new MeumInfo(NonePage, Meum_ILike), NeedSave, false);
            else
            {
                NSPage(new MeumInfo(Data, Meum_ILike) { cmd = "DataUrl[ILike]" }, NeedSave, false);
                OpenLoading();
                TB.Text = "我喜欢";
                TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl("https://y.gtimg.cn/mediastyle/y/img/cover_love_300.jpg"));
                DataItemsList.Items.Clear();
                DataCollectBtn.Visibility = Visibility.Collapsed;
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
                        if (j.MusicID != null)
                        {
                            var k = new DataItem(j, this, i, b);
                            DataItemsList.Items[i] = k;
                            k.Play += PlayMusic;
                            k.Download += K_Download;
                            k.GetToSingerPage += K_GetToSingerPage;
                            if (j.MusicID == MusicData.Data.MusicID)
                            {
                                k.ShowDx();
                            }
                            Settings.USettings.MusicGDataLike.ids.Add(j.MusicID, j.Littleid);
                        }
                        else
                        {
                            //不可用的资源
                            var k = new DataItem(j, this, i, b);
                            k.Visibility = Visibility.Collapsed;
                            DataItemsList.Items[i] = k;
                        }
                    }
                    ), this, new Action<int>(i =>
                    {
                        while (DataItemsList.Items.Count != i)
                        {
                            DataItemsList.Items.Add(new ListBoxItem() { Visibility = Visibility.Collapsed });
                        }
                    }));
                CloseLoading();
                ContentAnimation(DataItemsList, new Thickness(0, 200, 0, 0));
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
                if (d.music.MusicID != null && d.isChecked)
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
                foreach (DataItem d in DataItemsList.Items)
                {
                    if (d.music.MusicID != null && d.isChecked)
                    {
                        ReadytoDelete.Add(d);
                        Musicid.Add(He.MGData_Now.ids[d.index]);
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
                da.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                Datasv.LastLocation = os;
                Datasv.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, da);
            }
        }

        private void DataPage_Top_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var da = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            Datasv.LastLocation = 0;
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
            foreach (DataItem x in DataItemsList.Items)
            {
                if (x.music.MusicID != null)
                {
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
            foreach (DataItem x in DataItemsList.Items)
            {
                if (x.music.MusicID != null)
                {
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
        int HB = 0;
        private void Datasv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Datasv == null) Datasv = (MyScrollView)DataItemsList.Template.FindName("Datasv", DataItemsList);
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

        [DllImport("user32")]
        public static extern IntPtr SetFocus(IntPtr hWnd);
        private async void SearchBox_LeftMouseUp(object sender, MouseButtonEventArgs e)
        {
            await Task.Yield();
            Search_SmartBox.IsOpen = true;
            await Task.Yield();
            var source = (HwndSource)PresentationSource.FromVisual(Search_SmartBox);
            SetFocus(source.Handle);
            Search_SmartBoxList.Items.Clear();
            var data = await MusicLib.SearchHotKey();
            var mdb = new ListBoxItem { Background = new SolidColorBrush(Colors.Transparent), Height = 30, Content = "热搜", Margin = new Thickness(0, 10, 0, 0) };
            Search_SmartBoxList.Items.Add(mdb);
            for (int i = 0; i < 5; i++)
            {
                var dt = data[i];
                var bd = new ListBoxItem { Background = new SolidColorBrush(Colors.Transparent), Height = 30, Content = dt, Margin = new Thickness(0, 10, 0, 0) };
                bd.PreviewMouseDown += Bd_MouseDown;
                bd.PreviewKeyDown += Search_SmartBoxList_KeyDown;
                Search_SmartBoxList.Items.Add(bd);
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBox.Text.Trim() != string.Empty)
            {
                await Task.Yield();
                if (!Search_SmartBox.IsOpen)
                    Search_SmartBox.IsOpen = true;
                var data = await ml.Search_SmartBoxAsync(SearchBox.Text);
                Search_SmartBoxList.Items.Clear();
                if (data.Count == 0)
                    Search_SmartBox.IsOpen = false;
                else foreach (var dt in data)
                    {
                        var mdb = new ListBoxItem { Background = new SolidColorBrush(Colors.Transparent), Height = 30, Content = dt, Margin = new Thickness(0, 10, 0, 0) };
                        mdb.PreviewMouseDown += Bd_MouseDown;
                        mdb.PreviewKeyDown += Search_SmartBoxList_KeyDown;
                        Search_SmartBoxList.Items.Add(mdb);
                    }
            }
            else Search_SmartBox.IsOpen = false;
        }
        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
                Search_SmartBoxList.Focus();
            else if (e.Key == Key.Enter && SearchBox.Text.Trim() != string.Empty)
            {
                SearchMusic(SearchBox.Text);
                ixPlay = 1;
                Search_SmartBox.IsOpen = false;
            }
        }

        private void Search_SmartBoxList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchBox.Text = (Search_SmartBoxList.SelectedItem as ListBoxItem).Content.ToString().Replace("歌曲:", "").Replace("歌手:", "").Replace("专辑:", "");
                Search_SmartBox.IsOpen = false;
                SearchMusic(SearchBox.Text); ixPlay = 1;
            }
        }

        private void Bd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchBox.Text = (sender as ListBoxItem).Content.ToString().Replace("歌曲:", "").Replace("歌手:", "").Replace("专辑:", "");
            Search_SmartBox.IsOpen = false;
            SearchMusic(SearchBox.Text); ixPlay = 1;
        }
        public async void SearchMusic(string key, int osx = 0, bool NeedSave = true)
        {
            if (Settings.USettings.LemonAreeunIts == "0")
            {
                if (TwMessageBox.Show("官方限制需要登录才能搜索\r\n是否现在登录？"))
                    UserTX_MouseDown(null, null);
                return;
            }
            np = NowPage.Search;
            SearchKey = key;
            OpenLoading();
            List<Music> dt = null;
            if (osx == 0) dt = await MusicLib.SearchMusicAsync(key);
            else dt = await MusicLib.SearchMusicAsync(key, osx);
            if (osx == 0)
            {
                TB.Text = key;
                DataItemsList.Opacity = 0;
                DataCollectBtn.Visibility = Visibility.Collapsed;
                DataItemsList.Items.Clear();
                if (Datasv != null)
                {
                    Datasv.LastLocation = 0;
                    Datasv.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0)));
                }
                HB = 1;
                (Resources["DataPage_Min"] as Storyboard).Begin();
                TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.First().ImageUrl));
            }
            if (osx == 0) NSPage(new MeumInfo(Data, null) { cmd = "[DataUrl]{\"type\":\"Search\",\"key\":\"" + key + "\"}" }, NeedSave, false);
            int i = 0;
            foreach (var j in dt)
            {
                var k = new DataItem(j, this, i);
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
                ContentAnimation(DataItemsList, new Thickness(0, 75, 0, 0));
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
        public void PushPlayMusic(DataItem dt, ListBox DataSource, bool doesplay = true)
        {
            AddPlayDL(dt, DataSource);
            dt.ShowDx();
            PlayMusic(dt.music, doesplay);
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
            foreach (DataItem e in source.Items)
            {
                //如果项目中有没有解析成功的数据
                //就用假数据填充,保证index一致
                if (e.music.MusicID != null)
                {
                    var k = new PlayDLItem(e.music);
                    k.MouseDoubleClick += K_MouseDoubleClick;
                    PlayDL_List.Items.Add(k);
                }
                else
                {
                    PlayDL_List.Items.Add(new PlayDLItem(new Music() { MusicID = null }) { Visibility = Visibility.Collapsed });
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
        private void MiniBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!Settings.USettings.IsMiniOpen)
            {
                Settings.USettings.IsMiniOpen = true;
                mini.Show();
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
                if (a.music.MusicID != null)
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
        public async Task LoadMusic(Music data, bool doesplay)
        {
            string downloadpath = Settings.USettings.MusicCachePath + "Music\\" + data.MusicID + ".mp3";
            MusicPlay_LoadProc.Value = 0;
            if (!System.IO.File.Exists(downloadpath))
            {
                MusicPlay_LoadProc.BeginAnimation(OpacityProperty, new DoubleAnimation(1, TimeSpan.FromSeconds(0)));
                var musicurl = await MusicLib.GetUrlAsync(data);
                Console.WriteLine("FROM:" + musicurl[1] + "\r\n" + musicurl[0]);
                mp.LoadUrl(downloadpath, musicurl[0], (max, value) =>
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
                Dispatcher.Invoke(() =>
                {
                    MusicName.Text = data.MusicName;
                    SongSource_tb.Text = musicurl[1];
                });
            }
            else
            {
                mp.Load(downloadpath);
                if (doesplay)
                    mp.Play();
                MusicName.Text = data.MusicName;
                SongSource_tb.Text = "Local";
            }
        }
        bool AbleToClick = true;
        Music ToPlayData = null;
        Color ThemeColor_ORD;
        Color AlbumColor;
        public async void PlayMusic(Music data, bool doesplay = true, bool force = false)
        {
            if (!force)
                ToPlayData = data;
            if (!AbleToClick && !force)
            {
                //操作频繁..
                Console.WriteLine("Dealed.", "PlayMusic");
                return;
            }
            AbleToClick = false;
            try
            {
                if (data.MusicID == null)
                {
                    PlayControl_PlayNext(null, null);
                    return;
                }
                t.Stop();
                if (mp.BassdlList.Count > 0)
                    mp.BassdlList.Last().SetClose();

                MusicName.Text = "连接资源中...";
                ImmTb_Lyric.Text = "";
                ImmTb_Trans.Text = "";
                mp.Pause();

                await LoadMusic(data, doesplay);

                Title = "Lemon App  " + data.MusicName + " - " + data.SingerText;
                Settings.USettings.Playing = MusicData.Data;
                Singer.Text = data.SingerText;
                mini.title.Text = data.MusicName + " - " + data.SingerText;
                mini.MusicName.Text = data.MusicName;
                mini.SingerText.Text = data.SingerText;
                lyrictime_offset = 0;
                LyricPage_TimeSetter.Text = "0.0s";

                #region 专辑图
                BitmapImage im = await ImageCacheHelp.GetImageByUrl(data.ImageUrl) ?? await ImageCacheHelp.GetImageByUrl("https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000");
                MusicImage.Background = new ImageBrush(im);
                mini.img.Background = MusicImage.Background;
                //模糊处理
                var rect = new System.Drawing.Rectangle(0, 0, im.PixelWidth, im.PixelHeight);
                var imb = im.ToBitmap();
                var col = imb.get_major_color();
                imb.GaussianBlur(ref rect, 70);
                //下面的做法会让取到的颜色灰化..
                //在模糊的基础上取主题色
                //var col = imb.get_major_color();

                int high = 230;
                int dark = 100;
                if (col.R >= high && col.G >= high && col.B >= high)
                {
                    col.R = (byte)(col.R * 0.6);
                    col.G = (byte)(col.G * 0.6);
                    col.B = (byte)(col.B * 0.6);
                }
                else if ((col.R+col.G+col.B)/3<dark)
                {
                    col.R = (byte)(col.R * 1.8);
                    col.G = (byte)(col.G * 1.8);
                    col.B = (byte)(col.B * 1.8);
                }
                if (col.R >= high && col.G >= high && col.B >= high)
                {
                    col.R -= 90; col.G -= 90; col.B -= 90;
                }
                else if ((col.R + col.G + col.B) / 3 < dark)
                {
                    col.R += 80; col.G += 80; col.B += 80;
                }
                AlbumColor = col;
                if (IsLyricPageOpen == 1) App.BaseApp.SetColor("ThemeColor", AlbumColor);
                Console.WriteLine(col.R + " " + col.G + " " + col.B, "ThemeColor of image");
                LyricPage_Background.Background = new ImageBrush(imb.ToBitmapImage()) { Stretch = Stretch.UniformToFill };
                #endregion

                LyricData dt = await MusicLib.GetLyric(Settings.USettings.Playing.MusicID);
                //用日语平假名来判断 基本避开中文字符干扰
                bool ldrm = Regex.Match(dt.lyric, @"[\u3040-\u309f]").Length > 0 && dt.HasTrans;
                Console.WriteLine(ldrm, "ISJAPANESE");
                lv.LoadLrc(dt, ldrm);
                RomajiLyric.Visibility = ldrm ? Visibility.Visible : Visibility.Collapsed;
                TransLyric.Visibility = dt.HasTrans ? Visibility.Visible : Visibility.Collapsed;
                if (doesplay)
                {
                    //开始播放
                    (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.MiniPause);
                    TaskBarBtn_Play.Icon = Properties.Resources.icon_pause;
                    lyric_playcontrol.Data = mini.play.Data = Geometry.Parse(Properties.Resources.MiniPause);
                    t.Start();
                    isplay = true;
                    if (Settings.USettings.LyricAnimationMode == 2)
                    {
                        LyricBigAniRound.Seek(TimeSpan.FromSeconds(0));
                        LyricBigAniRound.Resume();
                    }
                }

                try
                {
                    //Expection: Operations that change non-concurrent collections must have exclusive access. 
                    //A concurrent update was performed on this collection and
                    //corrupted its state. The collection's state is no longer correct.
                    if (Settings.USettings.MusicGDataLike.ids.ContainsKey(data.MusicID))
                        LikeBtnDown();
                    else LikeBtnUp();
                }
                catch { }
                try
                {
                    TaskBarImg.SetImage(im);
                    TaskBarImg.Title = data.MusicName + " - " + data.SingerText;
                }
                catch { }
                Console.WriteLine(ToPlayData.MusicName + "\r\n" + data.MusicName, "ToPlayData");
                if (ToPlayData != null && ToPlayData != data)
                    PlayMusic(ToPlayData, true, true);
            }
            finally
            {
                AbleToClick = true;
            }
            //-------加载歌曲相关歌单功能-------
            var gd = await MusicLib.GetSongListAboutSong(data.MusicID);
            if (gd.Count >= 1)
            {
                LP_ag1_img.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(gd[0].Photo, new int[2] { 80, 80 }));
                LP_ag1.Tag = new { id = gd[0].ID, name = gd[0].Name, img = gd[0].Photo };
                LP_ag1_tx.Text = gd[0].Name;
            }
            if (gd.Count >= 2)
            {
                LP_ag2_img.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(gd[1].Photo, new int[2] { 80, 80 }));
                LP_ag2.Tag = new { id = gd[1].ID, name = gd[1].Name, img = gd[1].Photo };
                LP_ag2_tx.Text = gd[1].Name;
            }
            if (gd.Count >= 3)
            {
                LP_ag3_img.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(gd[2].Photo, new int[2] { 80, 80 }));
                LP_ag3.Tag = new { id = gd[2].ID, name = gd[2].Name, img = gd[2].Photo };
                LP_ag3_tx.Text = gd[2].Name;
            }
        }

        private void LP_ag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (Resources["LP_AboutGD_MouseLeave"] as Storyboard).Begin();
            Border_MouseDown_2(null, null);
            dynamic data = (sender as FrameworkElement).Tag;
            LoadFxGDItems(new FLGDIndexItem(new MusicGD() { ID = data.id, Name = data.name, Photo = data.img }));
        }
        #endregion
        #region PlayControl
        private double lyrictime_offset = 0;
        private void LyricTimeSet_Up_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lyrictime_offset += 0.1;
            LyricPage_TimeSetter.Text = lyrictime_offset.ToString("0.0") + "s";
        }

        private void LyricTimeSet_Down_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lyrictime_offset -= 0.1;
            LyricPage_TimeSetter.Text = lyrictime_offset.ToString("0.0") + "s";
            Console.WriteLine(lyrictime_offset + "s", "LYRIC_OFFSET");
        }
        private bool isLyricPage_TimeSet_Open = false;
        private void LyricPage_TimeSet_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (Resources[isLyricPage_TimeSet_Open ? "LyricPage_TimeSetClose" : "LyricPage_TimeSetOpen"] as Storyboard).Begin();
            isLyricPage_TimeSet_Open = !isLyricPage_TimeSet_Open;
        }
        private async void FxDec_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                SaveFileDialog sf = new SaveFileDialog();
                sf.FileName = MusicData.Data.MusicName + "-" + MusicData.Data.SingerText + ".mp3";
                sf.Filter = "Mp3音频文件(*.mp3)|*.mp3";
                if ((bool)sf.ShowDialog())
                {
                    Toast.Send("保存中...");
                    string filename = sf.FileName;
                    mp.SaveToFile(filename, () => Dispatcher.Invoke(() => Toast.Send("成功保存音频文件！")));
                }
            }
            else
            {
                await Task.Yield();
                Pop_sp.HorizontalOffset = IsLyricPageOpen == 1 ? -285 : -40;
                Pop_sp.IsOpen = true;
                MusicPlay_sp.Value = mp.Speed;
                MusicPlay_pitch_sp.Value = mp.Pitch;
                Tempo_value.Text = (MusicPlay_sp.Value / 10).ToString("0.00") + "x";
                Pitch_value.Text = MusicPlay_pitch_sp.Value.ToString("0.00") + "F";
            }
        }
        private void MusicPlay_pitch_sp_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                mp.Pitch = (float)MusicPlay_pitch_sp.Value;
                Pitch_value.Text = MusicPlay_pitch_sp.Value.ToString("0.00") + "F";
            }
            catch { }
        }
        private void HzTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (Pitch_value.Text == "-2.50F")
                {
                    mp.Pitch = 0;
                    MusicPlay_pitch_sp.Value = 0;
                    Pitch_value.Text = "0F";
                }
                else
                {
                    mp.Pitch = -2.5F;
                    MusicPlay_pitch_sp.Value = -2.5;
                    Pitch_value.Text = "-2.50F";
                }
            }
        }
        private void Tempo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (Tempo_value.Text == "1.25x")
                {
                    //回到初始速度
                    mp.Speed = 0;
                    MusicPlay_sp.Value = 0;
                    Tempo_value.Text = "0x";
                }
                else
                {
                    //1.25倍速
                    mp.Speed = 25;
                    MusicPlay_sp.Value = 1.25;
                    Tempo_value.Text = "1.25x";
                }
            }
        }
        private void MusicPlay_sp_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                mp.Speed = (float)MusicPlay_sp.Value;
                Tempo_value.Text = (MusicPlay_sp.Value / 10).ToString("0.00") + "x";
            }
            catch { }
        }

        private async void AudioBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Task.Yield();
            Pop_voice.IsOpen = true;
        }
        private void AudioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mp != null)
                mp.VOL = (float)AudioSlider.Value / 100;
        }
        private void TaskBarBtn_Play_Click(object sender, EventArgs e)
        {
            PlayBtn_MouseDown(null, null);
        }
        public void Jd_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //若使用ValueChanged事件，在value改变时也会触发，而不单是拖动jd.
            mp.Position = TimeSpan.FromMilliseconds((sender as Slider).Value);
            CanJd = true;
        }
        bool CanJd = true;
        public void Jd_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CanJd = false;
        }
        public void Jd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (!CanJd)
                    Play_Now.Text = TimeSpan.FromMilliseconds((sender as Slider).Value).ToString(@"mm\:ss");
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

        public void PlayControl_PlayLast(object sender, MouseButtonEventArgs e)
        {
            PlayDLItem k = null;

            if (Settings.USettings.PlayXHMode == 0 || Settings.USettings.PlayXHMode == 1)
            {
                //如果已经到播放队列的第一首，那么上一首就是最后一首歌(列表循环 非电台)
                //如果已经到播放队列的第一首，没有上一首(电台)
                if (PlayDL_List.Items.IndexOf(MusicData) == 0)
                {
                    if (!IsRadio) k = PlayDL_List.Items[PlayDL_List.Items.Count - 1] as PlayDLItem;
                }
                else k = PlayDL_List.Items[PlayDL_List.Items.IndexOf(MusicData) - 1] as PlayDLItem;
            }
            else
            {
                int index = RandomIndexes.IndexOf(RandomOffset);
                if (index == 0)
                    return;
                RandomOffset = RandomIndexes[index - 1];
                k = PlayDL_List.Items[RandomOffset] as PlayDLItem;
            }

            if (k.Data.MusicID != null)
            {
                k.p(true);
                MusicData = k;
                PlayMusic(k.Data);
                bool find = false;
                foreach (DataItem a in DataItemsList.Items)
                {
                    if (a.music.MusicID != null)
                    {
                        if (a.music.MusicID.Equals(k.Data.MusicID))
                        {
                            find = true;
                            a.ShowDx();
                            break;
                        }
                    }
                }
                if (!find) new DataItem(new Music()).ShowDx();
            }
            else
            {
                MusicData = k;
                PlayControl_PlayLast(null, null);
            }
        }
        private List<int> RandomIndexes = new List<int>();
        private int RandomOffset = 0;
        public void PlayControl_PlayNext(object sender, MouseButtonEventArgs e)
        {
            PlayDLItem k = null;
            if (Settings.USettings.PlayXHMode == 0 || Settings.USettings.PlayXHMode == 1)
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
            else
            {
                //随机播放  TODO 待完善
                if (RandomIndexes.Count > 0)
                {
                    if (RandomOffset != RandomIndexes.Last())
                    {
                        //若当前index没到最后一个
                        RandomOffset = RandomIndexes[RandomIndexes.IndexOf(RandomOffset) + 1];
                        k = PlayDL_List.Items[RandomOffset] as PlayDLItem;
                    }
                    else
                    {
                        Random r = new Random(Guid.NewGuid().GetHashCode());
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

            if (k.Data.MusicID != null)
            {
                k.p(true);
                MusicData = k;
                PlayMusic(k.Data);
                bool find = false;
                foreach (DataItem a in DataItemsList.Items)
                {
                    if (a.music.MusicID != null)
                    {
                        if (a.music.MusicID.Equals(k.Data.MusicID))
                        {
                            find = true;
                            a.ShowDx();
                            break;
                        }
                    }
                }
                if (!find) new DataItem(new Music()).ShowDx();
            }
            else
            {
                MusicData = k;
                PlayControl_PlayNext(null, null);
            }
        }
        public void PlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isplay)
            {
                isplay = false;
                mp.Pause();
                if (Settings.USettings.LyricAnimationMode == 2)
                    LyricBigAniRound.Pause();
                TaskBarBtn_Play.Icon = Properties.Resources.icon_play;
                t.Stop();
                (PlayBtn.Child as Path).Data = lyric_playcontrol.Data = mini.play.Data = Geometry.Parse(Properties.Resources.MiniPlay);
            }
            else
            {
                isplay = true;
                mp.Play();
                if (Settings.USettings.LyricAnimationMode == 2)
                {
                    if (!IsBigAniRunning)
                    {
                        LyricBigAniRound.Begin();
                        IsBigAniRunning = true;
                    }
                    else
                    {
                        LyricBigAniRound.Resume();
                    }
                }
                TaskBarBtn_Play.Icon = Properties.Resources.icon_pause;
                t.Start();
                (PlayBtn.Child as Path).Data = lyric_playcontrol.Data = mini.play.Data = Geometry.Parse(Properties.Resources.MiniPause);
            }
        }


        private void GcBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Settings.USettings.DoesOpenDeskLyric)
            {
                Settings.USettings.DoesOpenDeskLyric = false;
                if (Settings.USettings.LyricAppBarOpen)
                    lyricTa.Close();
                else lyricToast.Close();
                lyric_opengc.Fill = new SolidColorBrush(Colors.White);
                path7.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            }
            else
            {
                Settings.USettings.DoesOpenDeskLyric = true;
                if (Settings.USettings.LyricAppBarOpen)
                {
                    lyricTa = new LyricBar();
                    lyricTa.PopOut.MouseUp += PopOut_MouseUp;
                    lyricTa.Show();
                }
                else {
                    lyricToast = new Toast("", true);
                    lyricToast.Show();
                }
                lyric_opengc.SetResourceReference(Path.FillProperty, "ThemeColor");
                path7.SetResourceReference(Path.FillProperty, "ThemeColor");
            }
        }
        private async void Border_MouseDown_2(object sender, MouseButtonEventArgs e)
        {
            IsLyricPageOpen = 0;
            CloseBtn.ColorDx = null;
            MaxBtn.ColorDx = null;
            MinBtn.ColorDx = null;
            var ol = Resources["CloseLyricPage"] as Storyboard;
            ol.Begin();
            await Task.Delay(300);
            App.BaseApp.SetColor("ThemeColor", ThemeColor_ORD);
        }
        private async void MusicImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsLyricPageOpen = 1;
            ThemeColor_ORD = (App.BaseApp.Resources["ThemeColor"] as SolidColorBrush).Color;
            var WhiteColorBrush = new SolidColorBrush(Colors.White);
            MinBtn.ColorDx = WhiteColorBrush;
            CloseBtn.ColorDx = WhiteColorBrush;
            MaxBtn.ColorDx = WhiteColorBrush;
            var ol = Resources["OpenLyricPage"] as Storyboard;
            ol.Begin();
            await Task.Delay(200);
            App.BaseApp.SetColor("ThemeColor", AlbumColor);
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
        private async void MoreBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await Task.Yield();
            MoreBtn_Meum.HorizontalOffset = IsLyricPageOpen == 1 ? -320 : -70;
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
            string json = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/splcloud/fcgi-bin/songlist_list.fcg?utf8=1&-=MusicJsonCallBack&uin={Settings.USettings.LemonAreeunIts}&rnd=0.693477705380313&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0");
            Console.WriteLine(json);
            JObject o = JObject.Parse(json);
            foreach (var a in o["list"])
            {
                string name = a["dirname"].ToString();
                if (MoreBtn_Meum_Add_List.ContainsKey(name))
                    return;
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
        public void XHBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //NOW:列表循环
            if (Settings.USettings.PlayXHMode == 0)
            {
                //切换为单曲循环
                Settings.USettings.PlayXHMode = 1;
                path6.Data = Geometry.Parse(Properties.Resources.Dqxh);
            }
            else if (Settings.USettings.PlayXHMode == 1)
            {
                //如果是电台播放则切换为顺序播放
                if (IsRadio)
                {
                    Settings.USettings.PlayXHMode = 0;
                    path6.Data = Geometry.Parse(Properties.Resources.Lbxh);
                }
                else
                {
                    if (MusicData.Data.MusicID != string.Empty)
                    {
                        RandomOffset = PlayDL_List.Items.IndexOf(MusicData);
                        RandomIndexes.Add(RandomOffset);
                    }
                    Settings.USettings.PlayXHMode = 2;
                    path6.Data = Geometry.Parse(Properties.Resources.Random);
                }
            }
            else if (Settings.USettings.PlayXHMode == 2)
            {
                Settings.USettings.PlayXHMode = 0;
                path6.Data = Geometry.Parse(Properties.Resources.Lbxh);
            }

            mini.XHPath.Data = path6.Data;
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
            NSPage(new MeumInfo(Data, null));
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
                da.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                PlayDLSV.LastLocation = os;
                PlayDLSV.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, da);
            }
        }

        private void PlayDL_Top_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var da = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            PlayDLSV.LastLocation = 0;
            PlayDLSV.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, da);
        }
        MyScrollView PlayDLSV = null;
        private void PlayDLDatasv_Loaded(object sender, RoutedEventArgs e)
        {
            PlayDLSV = sender as MyScrollView;
        }
        private void DataPlayAllBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var k = AddPlayDL_All(null, 0);
            (DataItemsList.Items[0] as DataItem).ShowDx();
            PlayMusic(k.Data);
        }
        #endregion
        #region Lyric & 评论加载
        private void LyricFontSize_UpBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Settings.USettings.LyricFontSize <= 28)
            {
                Settings.USettings.LyricFontSize += 2;
                lv.SetFontSize(Settings.USettings.LyricFontSize);
            }
        }

        private void LyricFontSize_DownBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Settings.USettings.LyricFontSize >= 14)
            {
                Settings.USettings.LyricFontSize -= 2;
                lv.SetFontSize(Settings.USettings.LyricFontSize);
            }
        }

        private void NorImm_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Settings.USettings.IsLyricImm = !Settings.USettings.IsLyricImm;
            if (LyricNor.Visibility == Visibility.Visible)
            {
                LyricNor.Visibility = Visibility.Collapsed;
                LyricImm.Visibility = Visibility.Visible;
            }
            else
            {
                LyricNor.Visibility = Visibility.Visible;
                LyricImm.Visibility = Visibility.Collapsed;
            }
        }
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
        private bool IsBigAniRunning = false;
        private void CheckLyricAnimation(int mode)
        {
            if (mode == 0)
            {
                LyricBigAniRound.Stop();
                IsBigAniRunning = false;
            }
            else if (mode == 2)
            {
                LyricBigAniRound.Begin();
                IsBigAniRunning = true;
            }
        }
        private void Border_MouseDown_3(object sender, MouseButtonEventArgs e)
        {
            Border_MouseDown_2(null, null);
            LoadPl();
        }
        private async void LoadPl()
        {
            NSPage(new MeumInfo(MusicPLPage, null));
            OpenLoading();
            MusicPL_tx.Background = MusicImage.Background;
            MusicPL_tb.Text = MusicName.Text + " - " + Singer.Text;
            bool cp = true;
            MusicPlList.Children.Clear();
            if (MusicPLPage_QQ.Visibility == Visibility.Visible)
            {
                MusicPlScrollViewer.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0)));
                var data = await MusicLib.GetPLByQQAsync(Settings.USettings.Playing.MusicID);
                if (data[0].Count > 0)
                {
                    var t = new TextBlock()
                    {
                        Text = "最近热评",
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(5, 5, 0, 0)
                    };
                    t.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                    MusicPlList.Children.Add(t);
                    foreach (var dt in data[0])
                    {
                        MusicPlList.Children.Add(new PlControl(dt) { couldpraise = cp, Margin = new Thickness(10, 0, 0, 20) });
                    }
                }
                var t1 = new TextBlock()
                {
                    Text = "精彩评论",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(5, 5, 0, 0)
                };
                t1.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                MusicPlList.Children.Add(t1);
                foreach (var dt in data[1])
                {
                    MusicPlList.Children.Add(new PlControl(dt) { couldpraise = cp, Margin = new Thickness(10, 0, 0, 20) });
                }

                var t2 = new TextBlock()
                {
                    Text = "最新评论",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(5, 5, 0, 0)
                };
                t2.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                MusicPlList.Children.Add(t2);
                foreach (var dt in data[2])
                {
                    MusicPlList.Children.Add(new PlControl(dt) { couldpraise = cp, Margin = new Thickness(10, 0, 0, 20) });
                }
            }
            else
            {
                cp = false;
                List<MusicPL> data = await MusicLib.GetPLByWyyAsync(MusicPL_tb.Text);
                MusicPlScrollViewer.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0)));
                foreach (var dt in data)
                {
                    MusicPlList.Children.Add(new PlControl(dt) { couldpraise = cp, Margin = new Thickness(10, 0, 0, 20) });
                }
            }
            MusicPlScrollViewer.LastLocation = 0;
            MusicPlScrollViewer.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0)));
            CloseLoading();
        }
        /// <summary>
        /// 加载网易云音乐评论
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
            MusicPlScrollViewer.LastLocation = 0;
            MusicPlScrollViewer.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0)));
            foreach (var dt in data)
            {
                MusicPlList.Children.Add(new PlControl(dt) { couldpraise = false, Margin = new Thickness(10, 0, 0, 20) });
            }
            CloseLoading();
        }

        /// <summary>
        /// 加载QQ音乐的评论
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MusicPLPage_Wy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MusicPLPage_Wy.Visibility = Visibility.Collapsed;
            MusicPLPage_QQ.Visibility = Visibility.Visible;
            OpenLoading();
            MusicPL_tb.Text = MusicName.Text + " - " + Singer.Text;
            MusicPlList.Children.Clear();
            var data = await MusicLib.GetPLByQQAsync(Settings.USettings.Playing.MusicID);
            if (data[0].Count > 0)
            {
                var t = new TextBlock()
                {
                    Text = "最近热评",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(5, 5, 0, 0)
                };
                t.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                MusicPlList.Children.Add(t);
                foreach (var dt in data[0])
                {
                    MusicPlList.Children.Add(new PlControl(dt) { Margin = new Thickness(10, 0, 0, 20) });
                }
            }
            var t1 = new TextBlock()
            {
                Text = "精彩评论",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 5, 0, 0)
            };
            t1.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            MusicPlList.Children.Add(t1);
            foreach (var dt in data[1])
            {
                MusicPlList.Children.Add(new PlControl(dt) { Margin = new Thickness(10, 0, 0, 20) });
            }

            var t2 = new TextBlock()
            {
                Text = "最新评论",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 5, 0, 0)
            };
            t2.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            MusicPlList.Children.Add(t2);
            foreach (var dt in data[2])
            {
                MusicPlList.Children.Add(new PlControl(dt) { Margin = new Thickness(10, 0, 0, 20) });
            }
            MusicPlScrollViewer.LastLocation = 0;
            MusicPlScrollViewer.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, new DoubleAnimation(0, TimeSpan.FromSeconds(0)));
            CloseLoading();
        }
        private void ly_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (lv != null)
                lv.RestWidth(e.NewSize.Width);
        }
        private void TransLyric_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Settings.USettings.TransLyric = !Settings.USettings.TransLyric;
            lv.SetTransLyric(Settings.USettings.TransLyric);
            if (Settings.USettings.TransLyric)
            {
                TransLyricIcon.SetResourceReference(Path.FillProperty, "ThemeColor");
            }
            else TransLyricIcon.Fill = new SolidColorBrush(Color.FromArgb(140, 255, 255, 255));
        }
        private void RomajiLyric_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Settings.USettings.RomajiLyric = !Settings.USettings.RomajiLyric;
            lv.SetRomajiLyric(Settings.USettings.RomajiLyric);
            if (Settings.USettings.RomajiLyric)
            {
                RomajiLyricIcon.SetResourceReference(Path.FillProperty, "ThemeColor");
            }
            else RomajiLyricIcon.Fill = new SolidColorBrush(Color.FromArgb(140, 255, 255, 255));
        }
        bool HasOpenLranslationPage = false;
        TransAirWindow ta = null;
        private void TransAir_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!HasOpenLranslationPage)
            {
                HasOpenLranslationPage = true;
                ta = new TransAirWindow();
                ta.Owner = this;
                ta.Top = ta.Owner.Top;
                ta.Left = ta.Owner.Left;
                ta.Height = ta.Owner.ActualHeight;
                ta.Closed += delegate { HasOpenLranslationPage = false; };
                ta.Owner.LocationChanged += delegate
                {
                    ta.Top = ta.Owner.Top;
                    ta.Left = ta.Owner.Left;
                    ta.Height = ta.Owner.ActualHeight;
                };
                ta.Show();
            }
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
            DownloadItem di = new DownloadItem(data, file, DownloadDL.Count());
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
                    int next = DownloadItemsList.Children.IndexOf(s) + 1;
                    if (DownloadItemsList.Children.Count != next)
                    {
                        DownloadItem d = DownloadItemsList.Children[next] as DownloadItem;
                        if (!d.finished) d.d.Download();
                    }
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
                foreach (DataItem x in DataItemsList.Items)
                    if (x.music.MusicID != null) (x as DataItem).Check(true);
            }
            else
            {
                d.Content = "全选";
                foreach (DataItem x in DataItemsList.Items)
                    if (x.music.MusicID != null) (x as DataItem).Check(false);
            }
        }
        private void Download_Btn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(new MeumInfo(DownloadPage, Meum_Download));
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
            foreach (DataItem f in DataItemsList.Items)
            {
                if (f.music.MusicID != null)
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
            if (Settings.USettings.LemonAreeunIts == "0")
                NSPage(new MeumInfo(NonePage, Meum_MYGD));
            else
            {
                NSPage(new MeumInfo(MyGDIndexPage, Meum_MYGD));
                OpenLoading();
                var GdData = await MusicLib.GetGdListAsync();
                bool renew_ = false;
                if (GdData.Count != GDItemsList.Children.Count)
                {
                    renew_ = true;
                    GDItemsList.Children.Clear();
                    GData_Now.Clear();
                }
                foreach (var jm in GdData)
                {
                    if (!GData_Now.Contains(jm.Key))
                    {
                        var ks = new FLGDIndexItem(new MusicGD() { ID = jm.Value.id, Name = jm.Value.name, Photo = jm.Value.pic, ListenCount = 0 }, true, jm.Value.subtitle) { Margin = new Thickness(12, 0, 12, 20) };
                        ks.DeleteEvent += async (fl) =>
                        {
                            if (TwMessageBox.Show("确定要删除吗?"))
                            {
                                string dirid = await MusicLib.GetGDdiridByNameAsync(fl.data.Name);
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
                if (renew_)
                {
                    await Task.Yield();
                    ContentAnimation(GDItemsList, GDItemsList.Margin);
                }

                var GdLikeData = await ml.GetGdILikeListAsync();
                renew_ = false;
                if (GdLikeData.Count != GDILikeItemsList.Children.Count)
                {
                    renew_ = true;
                    GDILikeItemsList.Children.Clear();
                    GLikeData_Now.Clear();
                }
                foreach (var jm in GdLikeData)
                {
                    if (!GLikeData_Now.Contains(jm.Key))
                    {
                        var ks = new FLGDIndexItem(new MusicGD() { ID = jm.Value.id, Name = jm.Value.name, Photo = jm.Value.pic, ListenCount = jm.Value.listenCount }, true) { Margin = new Thickness(12, 0, 12, 20) };
                        ks.DeleteEvent += async (fl) =>
                         {
                             if (TwMessageBox.Show("确定要删除吗?"))
                             {
                                 string a = await MusicLib.DelGDILikeAsync(fl.data.ID);
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
                    NSPage(new MeumInfo(MyGDIndexPage, Meum_MYGD));
                if (renew_)
                {
                    await Task.Yield();
                    ContentAnimation(GDILikeItemsList, GDILikeItemsList.Margin);
                }

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
            NSPage(new MeumInfo(Data, null) { cmd = "[DataUrl]{\"type\":\"GD\",\"key\":\"" + dt.data.ID + "\",\"name\":\"" + dt.data.Name + "\",\"img\":\"" + dt.data.Photo + "\"}" }, NeedSave, false);
            NowType = dt;
            TB.Text = dt.data.Name;
            DataItemsList.Items.Clear();
            TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.data.Photo));
            OpenLoading();
            He.MGData_Now = await MusicLib.GetGDAsync(dt.data.ID,
                (dt) =>
                {
                    Dispatcher.Invoke(async () =>
                    {
                        if (dt.Creater.Name == "QQ音乐官方歌单")
                            DataPage_TX.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl("https://y.qq.com/favicon.ico"));
                        else DataPage_TX.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.Creater.Photo, new int[2] { 50, 50 }));
                        DataPage_Creater.Text = dt.Creater.Name;
                        DataPage_Sim.Text = dt.desc;
                        DataCollectBtn.Visibility = dt.IsOwn ? Visibility.Collapsed : Visibility.Visible;
                    });
                },
                new Action<int, Music, bool>((i, j, b) =>
                {
                    if (j.MusicID != null)
                    {
                        var k = new DataItem(j, this, i, b);
                        DataItemsList.Items[i] = k;
                        k.Play += PlayMusic;
                        k.GetToSingerPage += K_GetToSingerPage;
                        k.Download += K_Download;
                        if (j.MusicID == MusicData.Data.MusicID)
                        {
                            k.ShowDx();
                        }
                    }
                    else
                    {
                        //不可用的资源
                        var k = new DataItem(j, this, i, b);
                        k.Visibility = Visibility.Collapsed;
                        DataItemsList.Items[i] = k;
                    }
                }), this,
            new Action<int>(i =>
            {
                while (DataItemsList.Items.Count != i)
                {
                    DataItemsList.Items.Add(new ListBoxItem() { Visibility = Visibility.Collapsed });
                }
            }));
            CloseLoading();
            await Task.Yield();
            ContentAnimation(DataItemsList, new Thickness(0, 200, 0, 0));
            np = NowPage.GDItem;
        }
        #endregion
        #endregion
        #region MV
        public void PlayMv(MVData mVData)
        {
            if (isplay) PlayBtn_MouseDown(null, null);
            new MVWindow(mVData).Show();
        }
        #endregion
        #endregion
        #region 快捷键
        private IntPtr UnHotKey()
        {
            IntPtr hd = new WindowInteropHelper(this).Handle;
            UnregisterHotKey(hd, 124);
            UnregisterHotKey(hd, 125);
            UnregisterHotKey(hd, 126);
            UnregisterHotKey(hd, 127);
            UnregisterHotKey(hd, 128);
            UnregisterHotKey(hd, 129);
            return hd;
        }
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private void LoadHotDog()
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            if (Settings.USettings.HotKeys.Count == 0)
            {
                RegisterHotKey(handle, 124, 1, (uint)System.Windows.Forms.Keys.L);
                RegisterHotKey(handle, 126, 1, (uint)System.Windows.Forms.Keys.Space);
                RegisterHotKey(handle, 127, 1, (uint)System.Windows.Forms.Keys.Up);
                RegisterHotKey(handle, 128, 1, (uint)System.Windows.Forms.Keys.Down);
                RegisterHotKey(handle, 129, 1, (uint)System.Windows.Forms.Keys.C);
                InstallHotKeyHook(this);
            }
            else
            {
                foreach (var hk in Settings.USettings.HotKeys)
                {
                    RegisterHotKey(handle, hk.KeyID, (uint)hk.MainKey, (uint)(System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(hk.tKey));
                }
                InstallHotKeyHook(this);
            }
            Closed += (s, e) =>
            {
                UnHotKey();
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
            exit.Click += async delegate
             {
                 if (Settings.USettings.DoesOpenDeskLyric)
                     if (Settings.USettings.LyricAppBarOpen)
                         lyricTa.Close();
                     else lyricToast.Close();
                 try
                 {
                     mp.Free();
                     notifyIcon.Dispose();
                 }
                 catch { }
                 Settings.USettings.MusicGDataPlayList.Clear();
                 foreach (object a in PlayDL_List.Items)
                 {
                     if (a is PlayDLItem)
                     {
                         var ab = a as PlayDLItem;
                         Settings.USettings.MusicGDataPlayList.Add(ab.Data);
                     }
                     else
                     {
                         Settings.USettings.MusicGDataPlayList.Add(new Music() { MusicID = "str.Null" });
                     }
                 }
                 Settings.USettings.PlayingIndex = PlayDL_List.Items.IndexOf(MusicData);
                 await Settings.SaveSettingsTaskAsync();
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

        /// <summary>
        /// 注册热键 None = 0, Alt = 1, Control = 2, Shift = 4, Windows = 8
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="id"></param>
        /// <param name="controlKey"></param>
        /// <param name="virtualKey"></param>
        /// <returns></returns>
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
                else if (wParam.ToInt32() == 126)
                { PlayBtn_MouseDown(null, null); Toast.Send("已暂停/播放"); }
                else if (wParam.ToInt32() == 127)
                { PlayControl_PlayLast(null, null); Toast.Send("成功切换到上一曲"); }
                else if (wParam.ToInt32() == 128)
                { PlayControl_PlayNext(null, null); Toast.Send("成功切换到下一曲"); }
                else if (wParam.ToInt32() == 129)
                {
                    IntPtr hx = MsgHelper.FindWindow(null, "LemonApp Debug Console");
                    if (hx == IntPtr.Zero)
                    {
                        Toast.Send("已进入调试模式🐱‍👤");
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

        private void Meum_Bought_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void OpenLyricAppBar_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.USettings.DoesOpenDeskLyric) {
                if (Settings.USettings.LyricAppBarOpen)
                {
                    lyricTa.Close();
                    lyricToast = new Toast("", true);
                    lyricToast.Show();
                }
                else {
                    lyricToast.Close();
                    lyricTa = new LyricBar();
                    lyricTa.PopOut.MouseUp += PopOut_MouseUp;
                    lyricTa.Show();
                }
            }
            Settings.USettings.LyricAppBarOpen = (bool)OpenLyricAppBar.IsChecked;
        }
    }
}