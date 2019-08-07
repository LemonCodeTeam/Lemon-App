using LemonLibrary;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        LoginWindow lw;
        MusicLib ml = new MusicLib();
        public PlayDLItem MusicData = new PlayDLItem(new Music());
        bool isplay = false;
        bool IsRadio = false;
        string RadioID = "";
        int ind = 0;//歌词页面是否打开
        bool xh = false;//false: lb true:dq  循环/单曲 播放控制
        bool issingerloaded = false;
        bool mod = true;//true : qq false : wy
        bool isLoading = false;
        bool isPlayasRun = false;
        public NowPage np;
        #endregion
        #region 任务栏 字段
        TabbedThumbnail TaskBarImg;
        ThumbnailToolBarButton TaskBarBtn_Last;
        ThumbnailToolBarButton TaskBarBtn_Play;
        ThumbnailToolBarButton TaskBarBtn_Next;
        #endregion
        #region 等待动画
        Thread tOL = null;
        LoadingWindow aw = null;
        public void RunThread(object dx)
        {
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
                aw.Dispatcher.Invoke(() =>
                {
                    var da = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
                    da.Completed += delegate { aw.Close(); };
                    aw.BeginAnimation(OpacityProperty, da);
                });

                //   ContentPage.BeginAnimation(OpacityProperty, new DoubleAnimation(0.5, 1, TimeSpan.FromSeconds(0.3)));
            }
        }
        #endregion
        #region 窗口加载辅助
        public MainWindow()
        {
            InitializeComponent();
        }
        #region 加载窗口时的基础配置 登录/播放组件
        private async void window_Loaded(object sender, RoutedEventArgs e)
        {
            //--------检测更新-------
            Updata();
            //--------应用程序配置 热键和消息回调--------
            Settings.Handle.WINDOW_HANDLE = new WindowInteropHelper(this).Handle.ToInt32();
            Settings.Handle.ProcessId = Process.GetCurrentProcess().Id;
            Settings.SaveHandle();
            LoadSEND_SHOW();
            LoadHotDog();
            //--------登录------
            Settings.LoadLocaSettings();
            if (Settings.LSettings.qq != "")
                Settings.LoadUSettings(Settings.LSettings.qq);
            else {
                string qq = "Public";
                Settings.LoadUSettings(qq);
                Settings.USettings.LemonAreeunIts = qq;
                Settings.SaveSettings();
                Settings.LSettings.qq = qq;
                Settings.SaveLocaSettings();
            }
            Load_Theme(false);
            //-----Timer user
            var ds = new System.Windows.Forms.Timer() { Interval = 2000 };
            ds.Tick += delegate { GC.Collect(); UIHelper.G(Page); };
            ds.Start();
            //---------Popup的移动事件
            LocationChanged += delegate
            {
                RUNPopup(Pop_sp);
                RUNPopup(SingerListPop);
                RUNPopup(MoreBtn_Meum);
                RUNPopup(Gdpop);
                RUNPopup(IntoGDPop);
                RUNPopup(AddGDPop);
            };
            SizeChanged += delegate
            {
                var offset = Pop_sp.HorizontalOffset;
                Pop_sp.HorizontalOffset = offset + 1;
                Pop_sp.HorizontalOffset = offset;
            };
            //---------专辑图是圆的吗??-----
            MusicImage.CornerRadius = new CornerRadius(Settings.USettings.IsRoundMusicImage);
            //---------任务栏 TASKBAR-----------
            //任务栏 缩略图 按钮
            TaskBarImg = new TabbedThumbnail(this, this, new Vector());
            TaskbarManager.Instance.TabbedThumbnail.AddThumbnailPreview(TaskBarImg);
            TaskBarImg.SetWindowIcon(Properties.Resources.icon);
            TaskBarImg.Title = Settings.USettings.Playing.MusicName + " - " + Settings.USettings.Playing.SingerText;
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
            TaskbarManager.Instance.ThumbnailToolBars.AddButtons(this, TaskBarBtn_Last,TaskBarBtn_Play,TaskBarBtn_Next);
            //---------加载TOP页面----- 等待移植
            TopLoadac();
            //--------加载主页---------
            LoadHomePage();
        }

        private void RUNPopup(Popup pp) {
            if (pp.IsOpen)
            {
                var offset = pp.HorizontalOffset;
                pp.HorizontalOffset = offset + 1;
                pp.HorizontalOffset = offset;
            }
        }

        /// <summary>
        /// 登录之后的主题配置 不同的账号可能使用不同的主题
        /// </summary>
        /// <param name="hasAnimation"></param>
        private void Load_Theme(bool hasAnimation = true)
        {
            if (Settings.USettings.Skin_Path == "BlurBlackTheme")
            {
                //----新的[磨砂黑]主题---
                Page.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#99000000"));
                App.BaseApp.Skin();
                ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C000000"));
                WindowBlur.SetIsEnabled(this, true);
            }
            else if (Settings.USettings.Skin_Path == "BlurWhiteTheme") {
                ControlDownPage.BorderThickness = new Thickness(0, 0, 0, 0);
                ControlPage.BorderThickness = new Thickness(0, 0, 0, 0);
                Page.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#99FFFFFF"));
                App.BaseApp.Skin_Black();
                Color co = Color.FromRgb(64, 64, 64);
                App.BaseApp.SetColor("ResuColorBrush", co);
                App.BaseApp.SetColor("ButtonColorBrush", co);
                App.BaseApp.SetColor("TextX1ColorBrush", co);
                ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CFFFFFF"));
                WindowBlur.SetIsEnabled(this, true);
            }
            else
            {
                if (Settings.USettings.Skin_txt != "")
                {//有主题配置 （非默认）
                    //    主题背景图片
                    if (Settings.USettings.Skin_Path != "" && System.IO.File.Exists(Settings.USettings.Skin_Path))
                    {
                        Page.Background = new ImageBrush(new BitmapImage(new Uri(Settings.USettings.Skin_Path, UriKind.Absolute)));
                        ControlDownPage.BorderThickness = new Thickness(0);
                        ControlPage.BorderThickness = new Thickness(0);
                    }
                    //字体颜色
                    Color co;
                    if (Settings.USettings.Skin_txt == "Black")
                    {
                        co = Color.FromRgb(64, 64, 64); App.BaseApp.Skin_Black();
                        ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CFFFFFF"));
                    }
                    else
                    {
                        co = Color.FromRgb(255, 255, 255); App.BaseApp.Skin();
                        ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26000000"));
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
                    if (WindowBlur.GetIsEnabled(this))
                        WindowBlur.SetIsEnabled(this, false);
                    ControlDownPage.BorderThickness = new Thickness(0, 1, 0, 0);
                    ControlPage.BorderThickness = new Thickness(0, 0, 1, 0);
                    ControlDownPage.SetResourceReference(BorderBrushProperty, "BorderColorBrush");
                    ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CFFFFFF"));
                    Page.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    App.BaseApp.unSkin();
                    Settings.USettings.Skin_txt = "";
                    Settings.USettings.Skin_Path = "";
                    Settings.SaveSettings();
                }
            }
            LoadMusicData(hasAnimation);
        }
        private double now = 0;
        private string lastlyric = "";
        private Toast lyricTa = new Toast("", true);
        private bool isOpenGc = true;
        private void LoadMusicData(bool hasAnimation = true)
        {
            LoadSettings();
            //-------用户的头像、名称等配置加载
            if (Settings.USettings.UserName != string.Empty)
            {
                UserName.Text = Settings.USettings.UserName;
                if (System.IO.File.Exists(Settings.USettings.UserImage))
                {
                    var image = new System.Drawing.Bitmap(Settings.USettings.UserImage);
                    UserTX.Background = new ImageBrush(image.ToImageSource());
                }
            }
            //-----歌词显示 歌曲播放 等组件的加载
            LyricView lv = new LyricView();
            lv.FoucsLrcColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            lv.NoramlLrcColor = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
            lv.TextAlignment = TextAlignment.Left;
            ly.Child = lv;
            lv.NextLyric += (text) =>
            {
                //主要用于桌面歌词的显示
                if (isOpenGc)
                {
                    if (lastlyric != text) if (text != "")
                            lyricTa.Updata(text);
                    lastlyric = text;
                }
            };
            ml = new MusicLib(lv, Settings.USettings.LemonAreeunIts);
            //---------加载上一次播放
            if (Settings.USettings.Playing.MusicName != "")
            {
                MusicData = new PlayDLItem(Settings.USettings.Playing);
                PlayMusic(Settings.USettings.Playing.MusicID, Settings.USettings.Playing.ImageUrl, Settings.USettings.Playing.MusicName, Settings.USettings.Playing.SingerText, false, false);
            }
            //--------播放时的Timer 进度/歌词
            t.Interval = 500;
            t.Tick += delegate
            {
                try
                {
                    now = MusicLib.mp.Position.TotalMilliseconds;
                    if (CanJd)
                    {
                        jd.Value = now;
                        Play_Now.Text = TextHelper.TimeSpanToms(TimeSpan.FromMilliseconds(now));
                    }
                    if (isPlayasRun && MusicLib.mp.NaturalDuration.HasTimeSpan)
                    {
                        double all = MusicLib.mp.NaturalDuration.TimeSpan.TotalMilliseconds;
                        string alls = TextHelper.TimeSpanToms(TimeSpan.FromMilliseconds(all));
                        if (Play_All.Text == alls && Play_All.Text != "00:") isPlayasRun = false;
                        Play_All.Text = alls;
                        jd.Maximum = all;
                    }
                    if (ind == 1)
                        ml.lv.LrcRoll(now, true);
                    else ml.lv.LrcRoll(now, false);
                }
                catch { }
            };
            //-----------播放完成时，判断单曲还是下一首
            MusicLib.mp.MediaEnded += delegate
            {
                Console.WriteLine("end");
                jd.Value = 0;
                if (xh)//单曲循环
                {
                    MusicLib.mp.Position = TimeSpan.FromMilliseconds(0);
                    MusicLib.mp.Play();
                }
                else PlayControl_PlayNext(null, null);//下一曲
            };
            //------检测key是否有效-----------
            if (Settings.USettings.LemonAreeunIts != "")
            {
                if (Settings.USettings.Cookie == "" || Settings.USettings.g_tk == "")
                    if (TwMessageBox.Show("(￣▽￣)\"登录失效了，请重新登录"))
                        UserTX_MouseDown(null, null);
            }
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
            WidthUI(SingerItemsList);
            WidthUI(RadioItemsList);
            WidthUI(GDItemsList);
            WidthUI(FLGDItemsList);
            WidthUI(GDILikeItemsList);
            WidthUI(MusicPlList);
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
        public void WidthUI(WrapPanel wp)
        {
            if (wp.Visibility == Visibility.Visible)
            {
                foreach (UserControl dx in wp.Children)
                    dx.Width = ContentPage.ActualWidth / 5;
            }
        }
        #endregion
        #endregion
        #region 设置
        public void LoadSettings()
        {
            CachePathTb.Text = Settings.USettings.CachePath;
            DownloadPathTb.Text = Settings.USettings.DownloadPath;
            DownloadWithLyric.IsChecked = Settings.USettings.DownloadWithLyric;
            DownloadNameTb.Text = Settings.USettings.DownloadName;

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

        private void DownloadWithLyric_Checked(object sender, RoutedEventArgs e)
        {
            Settings.USettings.DownloadWithLyric = (bool)DownloadWithLyric.IsChecked;
        }

        private void DownloadNameOK_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Settings.USettings.DownloadName = DownloadNameTb.Text;
        }
        #endregion
        #region 主题切换
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
                ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CFFFFFF"));
            }
            else
            {
                co = Color.FromRgb(255, 255, 255); App.BaseApp.Skin();
                ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26000000"));
            }
            App.BaseApp.SetColor("ThemeColor", Theme_Choose_Color);
            App.BaseApp.SetColor("ResuColorBrush", co);
            App.BaseApp.SetColor("ButtonColorBrush", co);
            App.BaseApp.SetColor("TextX1ColorBrush", co);
            ControlDownPage.BorderThickness = new Thickness(0);
            ControlPage.BorderThickness = new Thickness(0);
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
                string strFileName = ofd.FileName;
                string file = Settings.USettings.CachePath + "Skin\\" + System.IO.Path.GetFileName(strFileName);
                System.IO.File.Copy(strFileName, file, true);
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
                if (!System.IO.File.Exists(Settings.USettings.CachePath + "Skin\\" + i + ".jpg"))
                    await HttpHelper.HttpDownloadFileAsync($"https://gitee.com/TwilightLemon/ux/raw/master/w{i}.jpg", Settings.USettings.CachePath + "Skin\\" + i + ".jpg");
                SkinControl sc = new SkinControl(i, name, color);
                sc.txtColor = dx["TextColor"].ToString();
                sc.MouseDown += async (s, n) =>
                {
                    if (WindowBlur.GetIsEnabled(this))
                        WindowBlur.SetIsEnabled(this, false);
                    if (!System.IO.File.Exists(Settings.USettings.CachePath + "Skin\\" + sc.imgurl + ".png"))
                        await HttpHelper.HttpDownloadFileAsync($"https://gitee.com/TwilightLemon/ux/raw/master/{sc.imgurl}.png", Settings.USettings.CachePath + "Skin\\" + sc.imgurl + ".png");
                    Page.Background = new ImageBrush(new System.Drawing.Bitmap(Settings.USettings.CachePath + "Skin\\" + sc.imgurl + ".png").ToImageSource());
                    Color co;
                    if (sc.txtColor == "Black")
                    {
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
            SkinControl sxc = new SkinControl(-1, "默认主题", Color.FromArgb(0, 0, 0, 0));
            sxc.MouseDown += (s, n) =>
            {
                if (WindowBlur.GetIsEnabled(this))
                    WindowBlur.SetIsEnabled(this, false);
                ControlDownPage.BorderThickness = new Thickness(0, 1, 0, 0);
                ControlPage.BorderThickness = new Thickness(0, 0, 1, 0);
                ControlDownPage.SetResourceReference(BorderBrushProperty, "BorderColorBrush");
                ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CFFFFFF"));
                Page.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                App.BaseApp.unSkin();
                Settings.USettings.Skin_txt = "";
                Settings.USettings.Skin_Path = "";
                Settings.SaveSettings();
            };
            sxc.Margin = new Thickness(10, 0, 0, 0);
            SkinIndexList.Children.Add(sxc);

            SkinControl blur = new SkinControl(-2, "磨砂黑", Color.FromArgb(0, 0, 0, 0));
            blur.MouseDown += (s, n) =>
            {
                ControlDownPage.BorderThickness = new Thickness(0, 0, 0, 0);
                ControlPage.BorderThickness = new Thickness(0, 0, 0, 0);
                Page.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#99000000"));
                App.BaseApp.Skin();
                ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C000000"));
                WindowBlur.SetIsEnabled(this, true);
                Settings.USettings.Skin_txt = "";
                Settings.USettings.Skin_Path = "BlurBlackTheme";
                Settings.SaveSettings();
            };
            blur.Margin = new Thickness(10, 0, 0, 0);
            SkinIndexList.Children.Add(blur);
            SkinControl blurWhite = new SkinControl(-3, "亚克力白", Color.FromArgb(255, 240, 240, 240));
            blurWhite.MouseDown += (s, n) =>
            {
                ControlDownPage.BorderThickness = new Thickness(0, 0, 0, 0);
                ControlPage.BorderThickness = new Thickness(0, 0, 0, 0);
                Page.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#99FFFFFF"));
                App.BaseApp.Skin_Black();
                Color co = Color.FromRgb(64, 64, 64);
                App.BaseApp.SetColor("ResuColorBrush", co);
                App.BaseApp.SetColor("ButtonColorBrush", co);
                App.BaseApp.SetColor("TextX1ColorBrush", co);
                ControlDownPage.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CFFFFFF"));
                WindowBlur.SetIsEnabled(this, true);
                Settings.USettings.Skin_txt = "";
                Settings.USettings.Skin_Path = "BlurWhiteTheme";
                Settings.SaveSettings();
            };
            blurWhite.Margin = new Thickness(10, 0, 0, 0);
            SkinIndexList.Children.Add(blurWhite);
        }
        #endregion
        #region 功能区
        #region HomePage 主页
        private async void LoadHomePage() {
            //---------加载主页HomePage
            var data = await MusicLib.GetHomePageData();
            HomePage_IFV.Updata(data.focus, this);
            foreach (var a in data.Gdata)
            {
                var k = new FLGDIndexItem(a.ID, a.Name, a.Photo) { Width = 175, Height =175, Margin = new Thickness(20, 0, 0, 0) };
                k.StarEvent += (sx) =>
                {
                    MusicLib.AddGDILike(sx.id);
                    Toast.Send("收藏成功");
                };
                k.ImMouseDown += FxGDMouseDown;
                HomePage_Gdtj.Children.Add(k);
            }
            foreach (var a in data.NewMusic)
            {
                var k = new FLGDIndexItem(a.MusicID, a.MusicName + " - " + a.SingerText, a.ImageUrl) { Width = 175, Height =175, Margin = new Thickness(20, 0, 0, 0) };
                k.Tag = a;
                k.ImMouseDown += (object s, MouseButtonEventArgs es) => {
                    var sx = s as FLGDIndexItem;
                    Music dt = sx.Tag as Music;
                    //如果是歌单播放 那么将所有歌曲加入播放队列 
                    AddPlayDL_All(null, HomePage_Nm.Children.IndexOf(s as FLGDIndexItem));
                    PlayMusic(dt.MusicID, dt.ImageUrl, dt.MusicName, dt.SingerText);
                };
                HomePage_Nm.Children.Add(k);
            }
        }
        bool FLGDPage_Tag_IsOpen = false;
        private void FLGDPage_Tag_Turn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (FLGDPage_Tag_IsOpen)
            {
                FLGDPage_Tag_IsOpen = false;
                FLGDIndexList.Height = 130;
                FLGDPage_Tag_Open.Text = "展开";
            }
            else
            {
                FLGDPage_Tag_IsOpen = true;
                //相当于xaml中的 Height="Auto"
                FLGDIndexList.Height = double.NaN;
                FLGDPage_Tag_Open.Text = "收缩";
            }
        }
        //IFV的回调函数
        public async void IFVCALLBACK_LoadAlbum(string id) {
            np = NowPage.GDItem;
            var dta = await MusicLib.GetAlbumSongListByIDAsync(id);
            NSPage(null, Data);
            TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dta.pic));
            TB.Text = dta.name;
            DataItemsList.Items.Clear();
            foreach (var j in dta.Data)
            {
                var k = new DataItem(j) { Width = ContentPage.ActualWidth };
                k.GetToSingerPage += K_GetToSingerPage;
                k.Play += PlayMusic;
                k.Download += K_Download;
                if (k.music.MusicID == MusicData.Data.MusicID)
                {
                    k.ShowDx();
                }
                DataItemsList.Items.Add(k);
            }
        }
        #endregion
        #region Top 排行榜
        /// <summary>
        /// 加载TOP列表
        /// </summary>
        /// <returns></returns>
        private async void TopLoadac() {
            var dt = await ml.GetTopIndexAsync();
            topIndexList.Children.Clear();
            foreach (var d in dt)
            {
                var top = new TopControl(d);
                top.MouseDown += Top_MouseDown;
                top.Margin = new Thickness(0, 0, 10, 20);
                topIndexList.Children.Add(top);
            }
        }
        /// <summary>
        /// 选择了TOP项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Top_MouseDown(object sender, MouseButtonEventArgs e)
        {
            GetTopItems(sender as TopControl);
        }
        /// <summary>
        /// 加载TOP项
        /// </summary>
        /// <param name="g">Top ID</param>
        /// <param name="osx">页数</param>
        private async void GetTopItems(TopControl g, int osx = 1)
        {
            np = NowPage.Top;
            tc_now = g;
            ixTop = osx;
            OpenLoading();
            if (osx == 1)
            {
                NSPage(null, Data);
                TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(g.Data.Photo));
                TB.Text = g.Data.Name;
                DataItemsList.Items.Clear();
            }
            var dta = await ml.GetToplistAsync(g.Data.ID, osx);
            foreach (var j in dta)
            {
                var k = new DataItem(j) { Width = ContentPage.ActualWidth };
                k.GetToSingerPage += K_GetToSingerPage;
                k.Play += PlayMusic;
                k.Download += K_Download;
                if (k.music.MusicID == MusicData.Data.MusicID)
                {
                    k.ShowDx();
                }
                if (DataPage_DownloadMod)
                {
                    k.MouseDown -= PlayMusic;
                    k.NSDownload(true);
                    k.Check();
                }
                DataItemsList.Items.Add(k);
            }
            CloseLoading();
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
                if (MyMessageBox.Show("小萌有更新啦", dt, "立即更新"))
                {
                    var xpath = Settings.USettings.CachePath + "win-release.exe";
                    await HttpHelper.HttpDownloadFileAsync("https://gitee.com/TwilightLemon/UpdataForWindows/raw/master/win-release.exe", xpath);
                    Process.Start(xpath);
                }
            }
        }
        #endregion
        #region N/S Page 切换页面
        public void RunAnimation(DependencyObject TPage,Thickness value=new Thickness())
        {
            var sb = Resources["NSPageAnimation"] as Storyboard;
            foreach (Timeline ac in sb.Children)
            {
                Storyboard.SetTarget(ac, TPage);
                if (ac is ThicknessAnimationUsingKeyFrames) {
                    (ac as ThicknessAnimationUsingKeyFrames).KeyFrames[1].Value = value;
                }
            }
            sb.Begin();
        }

        private Label LastClickLabel = null;
        private Grid LastPage = null;
        public void NSPage(Label ClickLabel, Grid TPage,Thickness value =new Thickness())
        {
            if (TPage == Data)
                if (DataPage_DownloadMod)
                    CloseDownloadPage();
            if (LastClickLabel == null) LastClickLabel = MusicKuBtn;
            LastClickLabel.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            if (ClickLabel != null) ClickLabel.SetResourceReference(ForegroundProperty, "ThemeColor");
            if (LastPage == null) LastPage = HomePage;
            LastPage.Visibility = Visibility.Collapsed;
            TPage.Visibility = Visibility.Visible;
            RunAnimation(TPage,value);
            if (ClickLabel != null) LastClickLabel = ClickLabel;
            LastPage = TPage;

        }
        private void TopBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(TopBtn, TopIndexPage);
        }
        private void MusicKuBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(MusicKuBtn, HomePage);
        }
        #endregion
        #region Singer 歌手界面
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
        public async void GetSinger(SingerItem si, int osx = 1) {
            np = NowPage.SingerItem;
            singer_now = si.data;
            ixSinger = osx;
            OpenLoading();
            BtD.LastBt = null;
            var data =await MusicLib.GetSingerPageAsync(si.data.Mid);
            Cisv.Content = new SingerPage(data, this) {
                Width= ContentPage.ActualWidth
            };
            SingerDP_Top.Uid = "gun";
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

                NSPage(SingerBtn, SingerDataPage, new Thickness(0, -50, 0, 0));
            }
            else {
                SetTopWhite(false); ;
                SingerDP_Top.Visibility = Visibility.Collapsed;
                NSPage(SingerBtn, SingerDataPage); }
            Cisv.ScrollToVerticalOffset(0);
            CloseLoading();
            /*
            OpenLoading();
            List<Music> dt = await ml.GetSingerMusicByIdAsync(si.data.Mid, osx);
            if (osx == 1) {
                DataItemsList.Items.Clear();
                NSPage(null, Data);
                TB.Text = si.data.Name;
                TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(si.data.Photo));
            }
            foreach (var j in dt)
            {
                var k = new DataItem(j) { Width = ContentPage.ActualWidth };
                k.GetToSingerPage += K_GetToSingerPage;
                if (k.music.MusicID == MusicData.Data.MusicID)
                {
                    k.ShowDx();
                }
                k.Play += PlayMusic;
                k.Download += K_Download;
                if (DataPage_DownloadMod)
                {
                    k.MouseDown -= PlayMusic;
                    k.NSDownload(true);
                    k.Check();
                }
                DataItemsList.Items.Add(k);
            }
            if (osx == 1) Datasv.ScrollToTop();
            CloseLoading();
            */
        }
        private async void GetSingerList(string index = "-100", string area = "-100", string sex = "-100", string genre = "-100", int cur_page = 1) {
            if (cur_page == 1)
                SingerItemsList.Opacity = 0;
            string sin = (80 * (cur_page - 1)).ToString();
            OpenLoading();
            ixSingerList = cur_page;
            var data = await MusicLib.GetSingerListAsync(index, area, sex, genre, sin, cur_page);
            if (cur_page == 1) {
                SingerItemsList.Children.Clear();
            }
            foreach (var d in data)
            {
                var sinx = new SingerItem(d) { Margin = new Thickness(20, 0, 0, 20) };
                sinx.MouseDown += GetSinger;
                SingerItemsList.Children.Add(sinx);
            }
            if (cur_page == 1) SingerPage_sv.ScrollToTop();
            CloseLoading();
            if (cur_page == 1) {
                await Task.Delay(10);
                RunAnimation(SingerItemsList);
            }
        }
        private int ixSingerList = 1;
        private void SingerPage_sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (SingerPage_sv.IsVerticalScrollBarAtButtom())
            {
                ixSingerList++;
                GetSingerList(SingerTab_ABC.Uid, SingerTab_Area.Uid, SingerTab_Sex.Uid, SingerTab_Genre.Uid, ixSingerList);
            }
        }

        private void SingerBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(SingerBtn, SingerIndexPage);
            if (!issingerloaded)
            {
                issingerloaded = true;
                SingerTab_ABC = SingerABC.Children[0] as RbBox;
                SingerTab_ABC.Check(true);
                SingerTab_Area = SingerArea.Children[0] as RbBox;
                SingerTab_Area.Check(true);
                SingerTab_Sex = SingerSex.Children[0] as RbBox;
                SingerTab_Sex.Check(true);
                SingerTab_Genre = SingerGenre.Children[0] as RbBox;
                SingerTab_Genre.Check(true);
                foreach (var c in SingerABC.Children)
                    (c as RbBox).Checked += SingerTabChecked_ABC;
                foreach (var c in SingerArea.Children)
                    (c as RbBox).Checked += SingerTabChecked_Area;
                foreach (var c in SingerSex.Children)
                    (c as RbBox).Checked += SingerTabChecked_Sex;
                foreach (var c in SingerGenre.Children)
                    (c as RbBox).Checked += SingerTabChecked_Genre;
                GetSingerList();
            }
        }

        RbBox SingerTab_ABC = new RbBox() { Uid = "-100" };
        RbBox SingerTab_Area = new RbBox() { Uid = "-100" };
        RbBox SingerTab_Sex = new RbBox() { Uid = "-100" };
        RbBox SingerTab_Genre = new RbBox() { Uid = "-100" };
        private void SingerTabChecked_ABC(RbBox sender)
        {
            if (sender != null) {
                SingerTab_ABC.Check(false);
                SingerTab_ABC = sender;
                GetSingerList(SingerTab_ABC.Uid, SingerTab_Area.Uid, SingerTab_Sex.Uid, SingerTab_Genre.Uid, 1);
            }
        }

        private void SingerTabChecked_Genre(RbBox sender)
        {
            if (sender != null)
            {
                SingerTab_Genre.Check(false);
                SingerTab_Genre = sender;
                GetSingerList(SingerTab_ABC.Uid, SingerTab_Area.Uid, SingerTab_Sex.Uid, SingerTab_Genre.Uid, 1);
            }
        }

        private void SingerTabChecked_Sex(RbBox sender)
        {
            if (sender != null)
            {
                SingerTab_Sex.Check(false);
                SingerTab_Sex = sender;
                GetSingerList(SingerTab_ABC.Uid, SingerTab_Area.Uid, SingerTab_Sex.Uid, SingerTab_Genre.Uid, 1);
            }
        }

        private void SingerTabChecked_Area(RbBox sender)
        {
            if (sender != null)
            {
                SingerTab_Area.Check(false);
                SingerTab_Area = sender;
                GetSingerList(SingerTab_ABC.Uid, SingerTab_Area.Uid, SingerTab_Sex.Uid, SingerTab_Genre.Uid, 1);
            }
        }
        #endregion
        #region FLGD 分类歌单
        private async void ZJBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(ZJBtn, ZJIndexPage);
            if (FLGDPage_Tag_Lau.Children.Count == 0)
            {
                OpenLoading();
                //加载Tag标签
                var wk = await ml.GetFLGDIndexAsync();
                //--------语种------------
                foreach (var d in wk.Lauch) {
                    var tb = new RbBox()
                    {
                        Uid = d.id,
                        ContentText = d.name,
                        Margin = new Thickness(0, 0, 5, 5)
                    };
                    tb.Checked += FLGDPageChecked;
                    FLGDPage_Tag_Lau.Children.Add(tb);
                }
                //--------流派-------------
                foreach (var d in wk.LiuPai) {
                    var tb = new RbBox()
                    {
                        Uid = d.id,
                        ContentText = d.name.Replace("&#38;", "&"),
                        Margin = new Thickness(0, 0, 5, 5)
                    };
                    tb.Checked += FLGDPageChecked;
                    FLGDPage_Tag_LiuPai.Children.Add(tb);
                }
                //--------主题------------
                foreach (var d in wk.Theme) {
                    var tb = new RbBox()
                    {
                        Uid = d.id,
                        ContentText = d.name,
                        Margin = new Thickness(0, 0, 5, 5)
                    };
                    tb.Checked += FLGDPageChecked;
                    FLGDPage_Tag_Theme.Children.Add(tb);
                }
                //---------心情-----------
                foreach (var d in wk.Heart) {
                    var tb = new RbBox()
                    {
                        Uid = d.id,
                        ContentText = d.name,
                        Margin = new Thickness(0, 0, 5, 5)
                    };
                    tb.Checked += FLGDPageChecked;
                    FLGDPage_Tag_Heart.Children.Add(tb);
                }
                //--------场景-------
                foreach (var d in wk.Changjing) {
                    var tb = new RbBox()
                    {
                        Uid = d.id,
                        ContentText = d.name,
                        Margin = new Thickness(0, 0, 5, 5)
                    };
                    tb.Checked += FLGDPageChecked;
                    FLGDPage_Tag_Changjing.Children.Add(tb);
                }
                GetGDList("10000000");
                FLGDPage_Tag = FLGDPage_Tag_All;
                FLGDPage_Tag_All.Check(true);
                FLGDPage_Tag_All.Checked += FLGDPage_Tag_All_Checked;
                FLGDPage_SortId_Tj.Checked += FLGDPage_SortId_Tj_Checked;
                FLGDPage_SortId_Newest.Checked += FLGDPage_SortId_Newest_Checked;
            }
        }
        RbBox FLGDPage_Tag = new RbBox();
        string sortId;
        private void FLGDPageChecked(RbBox sender)
        {
            if (sender != null)
            {
                FLGDPage_Tag.Check(false);
                FLGDPage_Tag = sender;
                OpenLoading();
                GetGDList(sender.Uid);
            }
        }
        private void FLGDPage_Tag_All_Checked(RbBox sender)
        {
            FLGDPage_Tag.Check(false);
            FLGDPage_Tag = FLGDPage_Tag_All;
            OpenLoading();
            GetGDList(FLGDPage_Tag_All.Uid);
        }
        private void FLGDPage_SortId_Tj_Checked(RbBox sender)
        {
            sortId = "5";
            FLGDPage_SortId_Newest.Check(false);
            GetGDList(FLGDPage_Tag.Uid, ixFLGD);
        }

        private void FLGDPage_SortId_Newest_Checked(RbBox sender)
        {
            sortId = "2";
            FLGDPage_SortId_Tj.Check(false);
            GetGDList(FLGDPage_Tag.Uid, ixFLGD);
        }
        private int ixFLGD = 0;
        private void FLGDPage_sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (FLGDPage_sv.IsVerticalScrollBarAtButtom())
            {
                ixFLGD++;
                GetGDList(FLGDPage_Tag.Uid, ixFLGD);
            }
        }
        private async void GetGDList(string id, int osx = 1) {
            if (osx == 1)
                FLGDItemsList.Opacity = 0;
            FLGDPage_Tag.Uid = id;
            ixFLGD = osx;
            OpenLoading();
            var data = await ml.GetFLGDAsync(id, sortId, osx);
            if (osx == 1) FLGDItemsList.Children.Clear();
            foreach (var d in data)
            {
                var k = new FLGDIndexItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(20, 0, 0, 20) };
                k.StarEvent += (sx) =>
                {
                    MusicLib.AddGDILike(sx.id);
                    Toast.Send("收藏成功");
                };
                k.Width = ContentPage.ActualWidth / 5;
                k.ImMouseDown += FxGDMouseDown;
                FLGDItemsList.Children.Add(k);
            }
            if (osx == 1) FLGDPage_sv.ScrollToTop();
            CloseLoading();
            if (osx == 1) {
                await Task.Delay(10);
                RunAnimation(FLGDItemsList);
            }
        }
        #endregion
        #region Radio 电台
        private void RadioBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(RadioBtn, RadioIndexPage);
            RadioMe.Check(true);
            RadioPageChecked(RadioMe);
        }

        public async void GetRadio(object sender, MouseEventArgs e)
        {
            OpenLoading();
            var dt = sender as RadioItem;
            RadioID = dt.id;
            var data = await ml.GetRadioMusicAsync(dt.id);
            MusicData.Data = data;
            PlayMusic(data.MusicID, data.ImageUrl, data.MusicName, data.SingerText, true);
            CloseLoading();
        }
        private RbBox RadioPage_RbLast = null;
        private async void RadioPageChecked(RbBox sender)
        {
            RadioItemsList.Opacity = 0;
            if (sender != null)
            {
                OpenLoading();
                if (RadioPage_RbLast != null)
                    RadioPage_RbLast.Check(false);
                RadioPage_RbLast = sender;
                var data = await ml.GetRadioList();
                RadioItemsList.Children.Clear();
                List<MusicRadioListItem> dat = null;
                switch (sender.Uid)
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
                foreach (var d in dat)
                {
                    RadioItem a = new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) };
                    a.MouseDown += GetRadio;
                    a.Width = RadioItemsList.ActualWidth / 5;
                    RadioItemsList.Children.Add(a);
                }
                CloseLoading();
                await Task.Delay(10);
                RunAnimation(RadioItemsList);
            }
        }
        #endregion
        #region ILike 我喜欢 列表加载/数据处理
        /// <summary>
        /// 取消喜欢 变白色
        /// </summary>
        private void LikeBtnUp()
        {
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
            likeBtn_path.Fill = new SolidColorBrush(Color.FromRgb(216, 30, 30));
        }
        /// <summary>
        /// 添加/删除 我喜欢的歌曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void likeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MusicName.Text != "MusicName")
            {
                if (Settings.USettings.MusicLike.ContainsKey(MusicData.Data.MusicID))
                {
                    LikeBtnUp();
                    Settings.USettings.MusicLike.Remove(MusicData.Data.MusicID);
                    string a = MusicLib.DeleteMusicFromGD(MusicData.Data.MusicID, MusicLib.MusicLikeGDid, MusicLib.MusicLikeGDdirid);
                    Toast.Send(a);
                }
                else
                {
                    string[] a = MusicLib.AddMusicToGD(MusicData.Data.MusicID, MusicLib.MusicLikeGDdirid);
                    Toast.Send(a[1] + ": " + a[0]);
                    Settings.USettings.MusicLike.Add(MusicData.Data.MusicID, MusicData.Data);
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
        private async void LikeBtn_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (Settings.USettings.LemonAreeunIts == "Public")
                NSPage(LikeBtn, NonePage);
            else
            {
                NSPage(LikeBtn, Data);
                loadin.Value = 0;
                loadin.Opacity = 1;
                TB.Text = "我喜欢";
                TXx.Background = Resources["LoveIcon"] as VisualBrush;
                DataItemsList.Items.Clear();
                He.MGData_Now = await MusicLib.GetGDAsync(MusicLib.MusicLikeGDid, new Action<Music, bool>((j, b) =>
                {
                    var k = new DataItem(j, b, this);
                    DataItemsList.Items.Add(k);
                    k.Play += PlayMusic;
                    k.Width = DataItemsList.ActualWidth;
                    k.Download += K_Download;
                    k.GetToSingerPage += K_GetToSingerPage;
                    if (j.MusicID == MusicData.Data.MusicID)
                    {
                        k.ShowDx();
                    }
                    loadin.Value = DataItemsList.Items.Count;
                }), this,
                new Action<int>(i => loadin.Maximum = i));
                loadin.Opacity = 0;
                np = NowPage.GDItem;
            }
        }
        #endregion
        #region DataPageBtn 歌曲数据 DataPage 的逻辑处理
        private void DataPlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PlayMusic(DataItemsList.Items[0] as DataItem, null);
        }
        bool DataPage_DownloadMod = false;
        private void DataDownloadBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DataPage_DownloadMod = true;
            border5.Visibility = Visibility.Collapsed;
            DataDownloadPage.Visibility = Visibility.Visible;
            Download_Path.Text = Settings.USettings.DownloadPath;
            DownloadQx.IsChecked = true;
            DownloadQx.Content = "全不选";
            foreach (DataItem x in DataItemsList.Items)
            {
                x.MouseDown -= PlayMusic;
                x.NSDownload(true);
                x.Check();
            }
        }

        public void CloseDownloadPage()
        {
            DataPage_DownloadMod = false;
            border5.Visibility = Visibility.Visible;
            DataDownloadPage.Visibility = Visibility.Collapsed;
            foreach (DataItem x in DataItemsList.Items)
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
        #region SearchMusic  搜索音乐
        private int ixPlay = 1;
        private string SearchKey = "";

        private MusicSinger singer_now;
        private int ixSinger = 1;

        private TopControl tc_now;
        private int ixTop = 1;
        private ScrollViewer Datasv = null;
        private void Datasv_Loaded(object sender, RoutedEventArgs e)
        {
            Datasv = sender as ScrollViewer;
        }
        private void Datasv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Datasv.IsVerticalScrollBarAtButtom())
            {
                if (np == NowPage.Search)
                {
                    ixPlay++;
                    SearchMusic(SearchKey, ixPlay);
                }
                else if (np == NowPage.SingerItem)
                {
                    ixSinger++;
                    GetSinger(new SingerItem(singer_now), ixSinger);
                }
                else if (np == NowPage.Top) {
                    ixTop++;
                    GetTopItems(tc_now, ixTop);
                }
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
                else foreach (var dt in data) {
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
            if (e.Key == Key.Enter) {
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
        public async void SearchMusic(string key, int osx = 0)
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
                DataItemsList.Items.Clear();
                Datasv.ScrollToTop();
            }
            TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.First().ImageUrl));
            foreach (var j in dt)
            {
                var k = new DataItem(j) { Width = ContentPage.ActualWidth };
                if (k.music.MusicID == MusicData.Data.MusicID)
                {
                    k.ShowDx();
                }
                k.GetToSingerPage += K_GetToSingerPage;
                k.Play += PlayMusic;
                k.Download += K_Download;
                if (DataPage_DownloadMod)
                {
                    k.MouseDown -= PlayMusic;
                    k.NSDownload(true);
                    k.Check();
                }
                DataItemsList.Items.Add(k);
            }
            CloseLoading();
            if (osx == 0) NSPage(null, Data);
        }
        #endregion
        #region PlayMusic 播放时的逻辑处理

        public async void PlayMusic(object sender, MouseEventArgs e)
        {
            var dt = sender as DataItem;
            if (await MusicLib.GetUrlAsync(dt.music.MusicID) == null)
                new CannotPlay().ShowDialog();
            else
            {
                AddPlayDL(dt);
                dt.ShowDx();
                PlayMusic(dt.music.MusicID, dt.music.ImageUrl, dt.music.MusicName, dt.music.SingerText);
            }
        }
        public async void PlayMusic(DataItem dt, bool next = false)
        {
            if (await MusicLib.GetUrlAsync(dt.music.MusicID) == null)
                if (next) PlayControl_PlayNext(null, null);
                else new CannotPlay().ShowDialog();
            else
            {
                AddPlayDL(dt);
                dt.ShowDx();
                PlayMusic(dt.music.MusicID, dt.music.ImageUrl, dt.music.MusicName, dt.music.SingerText);
            }
        }
        public void PushPlayMusic(DataItem dt,ListBox DataSource)
        {
            AddPlayDL(dt,DataSource);
            dt.ShowDx();
            PlayMusic(dt.music.MusicID, dt.music.ImageUrl, dt.music.MusicName, dt.music.SingerText);
        }
        public async void PlayMusic(DataItem dt)
        {
            if (await MusicLib.GetUrlAsync(dt.music.MusicID) == null)
                new CannotPlay().ShowDialog();
            else
            {
                AddPlayDL(dt);
                dt.ShowDx();
                PlayMusic(dt.music.MusicID, dt.music.ImageUrl, dt.music.MusicName, dt.music.SingerText);
            }
        }
        public PlayDLItem AddPlayDL_All(DataItem dt,int index=-1,ListBox source=null) {
            if (source == null) source = DataItemsList;
            DLMode = false;
            PlayDL_List.Items.Clear();
            foreach (DataItem e in source.Items)
            {
                var k = new PlayDLItem(e.music);
                k.MouseDoubleClick += K_MouseDoubleClick;
                PlayDL_List.Items.Add(k);
            }
            if (index == -1)
                index = source.Items.IndexOf(dt);
            PlayDLItem dk = PlayDL_List.Items[index] as PlayDLItem;
            dk.p(true);
            MusicData = dk;
            return dk;
        }
        public void AddPlayDl_CR(DataItem dt) {
            DLMode = true;
            var k = new PlayDLItem(dt.music);
            k.MouseDoubleClick += K_MouseDoubleClick;
            int index = PlayDL_List.Items.IndexOf(MusicData) + 1;
            PlayDL_List.Items.Insert(index, k);
            k.p(true);
            MusicData = k;
        }
        public bool DLMode = false;
        public void AddPlayDL(DataItem dt,ListBox source=null) {
            if (np == NowPage.GDItem)
            {
                //本次为歌单播放 那么将所有歌曲加入播放队列 
                AddPlayDL_All(dt,-1,source);
            }
            else
            {
                //本次为其他播放，若上一次也是其他播放，那么添加所有，不是则插入当前的
                if (DLMode)
                    AddPlayDL_All(dt,-1,source);
                else AddPlayDl_CR(dt);
            }
        }
        public void K_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlayDLItem k = sender as PlayDLItem;
            k.p(true);
            MusicData = k;
            PlayMusic(k.Data.MusicID, k.Data.ImageUrl, k.Data.MusicName, k.Data.SingerText);
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

        private string LastPlay = "";
        public async void PlayMusic(string id, string x, string name, string singer, bool isRadio = false, bool doesplay = true)
        {
            if (await MusicLib.GetUrlAsync(id) == null)
                PlayControl_PlayNext(null, null);
            else
            {
                if (LastPlay == id)
                {
                    if (doesplay)
                    {
                        MusicLib.mp.Position = TimeSpan.FromMilliseconds(0);
                        MusicLib.mp.Play();
                        TaskBarBtn_Play.Icon = Properties.Resources.icon_pause;
                        (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Pause);
                        t.Start();
                        isplay = true;
                    }
                }
                else
                {
                    Title = name + " - " + singer;
                    MusicName.Text = "连接资源中...";
                    IsRadio = isRadio;
                    isPlayasRun = true;
                    t.Stop();
                    MusicLib.mp.Pause();
                    Settings.USettings.Playing = MusicData.Data;
                    Settings.SaveSettings();
                    if (Settings.USettings.MusicLike.ContainsKey(id))
                        LikeBtnDown();
                    else LikeBtnUp();
                    ml.GetAndPlayMusicUrlAsync(id, true, MusicName, this, name + " - " + singer, doesplay);
                    var im = await ImageCacheHelp.GetImageByUrl(x);
                    TaskBarImg.SetImage(im);
                    TaskBarImg.Title = name + " - " + singer;
                    MusicImage.Background = new ImageBrush(im);
                    var rect = new System.Drawing.Rectangle(0, 0, im.PixelWidth, im.PixelHeight);
                    var imb = im.ToBitmap();
                    imb.GaussianBlur(ref rect, 80);
                    LyricPage_Background.Background = new ImageBrush(imb.ToBitmapImage()) { Stretch = Stretch.Fill };
                    Singer.Text = singer;
                    if (doesplay)
                    {
                        (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Pause);
                        TaskBarBtn_Play.Icon = Properties.Resources.icon_pause;
                        t.Start();
                        isplay = true;
                    }
                    LastPlay = MusicData.Data.MusicID;
                }
            }
        }
        #endregion
        #region PlayControl
        private void Pop_sp_LostFocus(object sender, RoutedEventArgs e)
        {
            Pop_sp.IsOpen = false;
        }
        private void Border_MouseDown_6(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (MusicPlay_tb.Text == "1.25x")
                {
                    MusicLib.mp.SpeedRatio = 1d;
                    MusicPlay_tb.Text = "1x";
                }
                else
                {
                    MusicLib.mp.SpeedRatio = 1.25d;
                    MusicPlay_tb.Text = "1.25x";
                }
            }
            else Pop_sp.IsOpen = !Pop_sp.IsOpen;
        }

        private void MusicPlay_sp_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                MusicLib.mp.SpeedRatio = MusicPlay_sp.Value;
                MusicPlay_tb.Text = MusicPlay_sp.Value.ToString("0.00") + "x";
            }
            catch { }
        }
        private void TaskBarBtn_Play_Click(object sender, EventArgs e)
        {
            PlayBtn_MouseDown(null, null);
        }
        private void Jd_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {//若使用ValueChanged事件，在value改变时也会触发，而不单是拖动jd.
            MusicLib.mp.Position = TimeSpan.FromMilliseconds(jd.Value);
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
                    Play_Now.Text = TextHelper.TimeSpanToms(TimeSpan.FromMilliseconds(jd.Value));
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
            if (!IsRadio)
            {
                PlayDLItem k;
                //如果已经到播放队列的第一首，那么上一首就是最后一首歌(列表循环)
                if (PlayDL_List.Items.IndexOf(MusicData)==0)
                    k = PlayDL_List.Items[PlayDL_List.Items.Count-1] as PlayDLItem;
                else k = PlayDL_List.Items[PlayDL_List.Items.IndexOf(MusicData) - 1] as PlayDLItem;
                k.p(true);
                MusicData = k;
                PlayMusic(k.Data.MusicID, k.Data.ImageUrl, k.Data.MusicName, k.Data.SingerText);
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
        private void PlayControl_PlayNext(object sender, MouseButtonEventArgs e)
        {
            if (!IsRadio)
            {
                PlayDLItem k;
                //如果已到最后一首歌，那么下一首从头播放
                if (PlayDL_List.Items.IndexOf(MusicData) + 1==PlayDL_List.Items.Count)
                    k = PlayDL_List.Items[0] as PlayDLItem;
                else k = PlayDL_List.Items[PlayDL_List.Items.IndexOf(MusicData) + 1] as PlayDLItem;
                k.p(true);
                MusicData = k;
                PlayMusic(k.Data.MusicID, k.Data.ImageUrl, k.Data.MusicName, k.Data.SingerText);
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
            else GetRadio(new RadioItem(RadioID), null);
        }
        private void PlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isplay)
            {
                isplay = false;
                MusicLib.mp.Pause();
                TaskBarBtn_Play.Icon = Properties.Resources.icon_play;
                t.Stop();
                (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Play);
            }
            else
            {
                isplay = true;
                MusicLib.mp.Play();
                TaskBarBtn_Play.Icon = Properties.Resources.icon_pause;
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
            path10.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            App.BaseApp.SetColor("ButtonColorBrush", LastButtonColor);
            if (!isOpenGc) path7.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            likeBtn_path.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            ControlDownPage.BorderThickness = new Thickness(0, 1, 0, 0);
            var ol = Resources["CloseLyricPage"] as Storyboard;
            ol.Begin();
        }
        Color LastButtonColor;
        private void MusicImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ind = 1;
            ControlDownPage.BorderThickness = new Thickness(0);
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
            if (!isOpenGc) path7.Fill = new SolidColorBrush(Colors.White);
            likeBtn_path.Fill = new SolidColorBrush(Colors.White);
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
            /*
            if (MoreBtn_Meum.IsOpen)
            {
                MoreBtn_Meum.IsOpen = false;
                await Task.Delay(10);
                MoreBtn_Meum.StaysOpen = true;
            }
            else {
                MoreBtn_Meum.IsOpen = true;
                await Task.Delay(10);
                MoreBtn_Meum.StaysOpen = false;
            }
            */
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
        private void Mdb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MoreBtn_Meum.IsOpen = false;
            Gdpop.IsOpen = false;
            string name = (sender as ListBoxItem).Content.ToString();
            string id = MoreBtn_Meum_Add_List[name];
            string[] a = MusicLib.AddMusicToGD(MusicData.Data.MusicID, id);
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
        bool isOpenPlayDLPage = false;
        private void PlayLbBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isOpenPlayDLPage) {
                (Resources["ClosePlayDLPage"] as Storyboard).Begin();
                isOpenPlayDLPage = false;
            }
            else {
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
            NSPage(null, Data);
        }

        private void DataPlayAllBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var k = AddPlayDL_All(null, 0);
            (DataItemsList.Items[0] as DataItem).ShowDx();
            PlayMusic(k.Data.MusicID, k.Data.ImageUrl, k.Data.MusicName, k.Data.SingerText);
        }
        #endregion
        #region Lyric
        private void Border_MouseDown_3(object sender, MouseButtonEventArgs e)
        {
            Border_MouseDown_2(null,null);
            LoadPl();
        }
        private async void LoadPl() {
            NSPage(null, MusicPLPage);
            MusicPL_tb.Text = MusicName.Text + " - " + Singer.Text;
            List<MusicPL> data = await ml.GetPLByQQAsync(Settings.USettings.Playing.MusicID);
            MusicPlList.Children.Clear();
            foreach (var dt in data)
            {
                MusicPlList.Children.Add(new PlControl(dt) { Width = MusicPlList.ActualWidth - 10, Margin = new Thickness(10, 0, 0, 20) });
            }
        }
        private void ly_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ml.lv != null)
                ml.lv.RestWidth(e.NewSize.Width);
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
        private void IntoGDPage_DrBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mod)
            {
                MusicLib.AddGDILike(IntoGDPage_id.Text);
                TwMessageBox.Show("添加成功");
                IntoGDPop.IsOpen = false;
                 GDBtn_MouseDown(null, null);
            }
            else
            {
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

        private void AddGDPage_DrBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Toast.Send(MusicLib.AddNewGd(AddGDPage_name.Text, AddGDPage_ImgUrl));
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
        public void AddDownloadTask(Music data) {
            string name =TextHelper.MakeValidFileName (Settings.USettings.DownloadName
                .Replace("[I]", (DownloadDL.Count() + 1).ToString())
                .Replace("[M]", data.MusicName)
                .Replace("[S]", data.SingerText));
            string file = Settings.USettings.DownloadPath + $"\\{name}.mp3";
            DownloadItem di = new DownloadItem(data, file,DownloadDL.Count()){
                Width = ContentPage.ActualWidth
            };
            di.Delete += (s) =>{
                s.d.Stop();
                s.finished = true;
                s.zt.Text = "已取消";
            };
            di.Finished += (s) => {
                DownloadDL.Remove(s.MData);
                if (DownloadDL.Count != 0)
                {
                    DownloadItem d = DownloadItemsList.Children[DownloadItemsList.Children.IndexOf(s) + 1] as DownloadItem;
                    if (!d.finished) d.d.Download();
                }
                else {
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
                { x.isChecked = false; x.Check(); }
            }
            else
            {
                d.Content = "全选";
                foreach (DataItem x in DataItemsList.Items)
                { x.isChecked = true; x.Check(); }
            }
        }
        private void Download_Btn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(Download_Btn, DownloadPage);
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
        public void PushDownload(ListBox c) {
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
            CloseDownloadPage();
        }
        #endregion
        #region User
        #region Login
        private async void UserTX_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lw = new LoginWindow();
            lw.Show();
            await Task.Delay(500);
            MsgHelper.SendMsg("Login", lw.Handle.ToInt32());
        }
        #endregion
        #region MyGD
        private List<string> GData_Now = new List<string>();
        private List<string> GLikeData_Now = new List<string>();
        private async void GDBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Settings.USettings.LemonAreeunIts == "Public")
                NSPage(GDBtn, NonePage);
            else
            {
                NSPage(GDBtn, MyGDIndexPage);
                OpenLoading();
                var GdData = await ml.GetGdListAsync();
                if (GdData.Count != GDItemsList.Children.Count)
                { GDItemsList.Children.Clear(); GData_Now.Clear(); }
                foreach (var jm in GdData)
                {
                    if (!GData_Now.Contains(jm.Key))
                    {
                        var ks = new FLGDIndexItem(jm.Key, jm.Value.name, jm.Value.pic, true) { Margin = new Thickness(20, 0, 0, 20) };
                        ks.DeleteEvent += async (fl) =>
                        {
                            if (TwMessageBox.Show("确定要删除吗?"))
                            {
                                string dirid = await MusicLib.GetGDdiridByNameAsync(fl.sname);
                                string a = MusicLib.DeleteGdById(dirid);
                                GDBtn_MouseDown(null, null);
                            }
                        };
                        ks.Width = ContentPage.ActualWidth / 5;
                        ks.ImMouseDown += FxGDMouseDown;
                        GDItemsList.Children.Add(ks);
                        GData_Now.Add(jm.Key);
                    }
                }
                var GdLikeData = await ml.GetGdILikeListAsync();
                if (GdLikeData.Count != GDILikeItemsList.Children.Count)
                { GDILikeItemsList.Children.Clear(); GLikeData_Now.Clear(); }
                foreach (var jm in GdLikeData)
                {
                    if (!GLikeData_Now.Contains(jm.Key))
                    {
                        var ks = new FLGDIndexItem(jm.Key, jm.Value.name, jm.Value.pic, true) { Margin = new Thickness(20, 0, 0, 20) };
                        ks.DeleteEvent += (fl) =>
                        {
                            if (TwMessageBox.Show("确定要删除吗?"))
                            {
                                string a = MusicLib.DelGDILike(fl.id);
                                GDBtn_MouseDown(null, null);
                            }
                        };
                        ks.Width = ContentPage.ActualWidth / 5;
                        ks.ImMouseDown += FxGDMouseDown;
                        GDILikeItemsList.Children.Add(ks);
                        GLikeData_Now.Add(jm.Key);
                    }
                }
                if (GdData.Count == 0 && GdLikeData.Count == 0)
                    NSPage(GDBtn, NonePage);
                UIHelper.G(Page);
                CloseLoading();
            }
        }

        private async void FxGDMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadin.Value = 0;
            loadin.Opacity = 1;
            NSPage(null, Data);
            var dt = sender as FLGDIndexItem;
            TB.Text = dt.name.Text;
            DataItemsList.Items.Clear();
            TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.img));
            He.MGData_Now = await MusicLib.GetGDAsync(dt.id, new Action<Music, bool>((j, b) => {
                var k = new DataItem(j, b, this);
                DataItemsList.Items.Add(k);
                k.Play += PlayMusic;
                k.GetToSingerPage += K_GetToSingerPage;
                k.Download += K_Download;
                k.Width = DataItemsList.ActualWidth;
                if (j.MusicID == MusicData.Data.MusicID)
                {
                    k.ShowDx();
                }
                loadin.Value = DataItemsList.Items.Count;
            }), this,
            new Action<int>(i=>loadin.Maximum=i));
            loadin.Opacity = 0;
            CloseLoading();
            np = NowPage.GDItem;
        }
        #endregion
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
            notifyIcon.Text = "小萌";
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            notifyIcon.Visible = true;
            //打开菜单项
            System.Windows.Forms.MenuItem open = new System.Windows.Forms.MenuItem("打开");
            open.Click += delegate { exShow(); };
            //退出菜单项
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem("关闭");
            exit.Click += delegate
            {
                notifyIcon.Dispose();
                Settings.SaveSettings();
                Environment.Exit(0);
            };
            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { open, exit };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

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
        bool IsDebug = false;
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
                    if (!IsDebug)
                    {
                        IsDebug = true;
                        Toast.Send("已进入调试模式🐱‍👤");
                        ConsoleManager.Toggle();
                        Console.WriteLine("调试模式");
                    }
                    else
                    {
                        IsDebug = false;
                        ConsoleManager.Hide();
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
                else if (cdata.lpData.Contains("Login"))
                {
                    lw.Close();
                    Console.WriteLine(cdata.lpData);
                    string qq = "";
                    if (cdata.lpData != "No Login")
                        qq = TextHelper.XtoYGetTo(cdata.lpData, "Login:", "###", 0);
                    if (Settings.USettings.LemonAreeunIts == qq)
                    {
                        if (cdata.lpData.Contains("g_tk"))
                        {
                            Settings.USettings.g_tk = TextHelper.XtoYGetTo(cdata.lpData, "g_tk[", "]sk", 0);
                            Settings.USettings.Cookie = TextHelper.XtoYGetTo(cdata.lpData, "Cookie[", "]END", 0);
                            Settings.SaveSettings();
                        }
                    }
                    else
                    {
                        //此方法中不能使用Async异步，故使用Action
                        Action a = new Action(async () =>
                        {
                            if (cdata.lpData.Contains("g_tk"))
                            {
                                Settings.USettings.g_tk = TextHelper.XtoYGetTo(cdata.lpData, "g_tk[", "]sk", 0);
                                Settings.USettings.Cookie = TextHelper.XtoYGetTo(cdata.lpData, "Cookie[", "]END", 0);
                            }
                            var sl = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={qq}&reqfrom=1&reqtype=0", Encoding.UTF8);
                            Console.WriteLine(sl);
                            var sdc = JObject.Parse(sl)["data"]["creator"];
                            await HttpHelper.HttpDownloadFileAsync(sdc["headpic"].ToString().Replace("http://","https://"), Settings.USettings.CachePath + qq + ".jpg");
                            await Task.Run(() =>
                            {
                                Settings.LoadUSettings(qq);
                                if (cdata.lpData.Contains("g_tk"))
                                {
                                    Settings.USettings.g_tk = TextHelper.XtoYGetTo(cdata.lpData, "g_tk[", "]sk", 0);
                                    Settings.USettings.Cookie = TextHelper.XtoYGetTo(cdata.lpData, "Cookie[", "]END", 0);
                                }
                                Settings.USettings.UserName = sdc["nick"].ToString();
                                Settings.USettings.UserImage = Settings.USettings.CachePath + qq + ".jpg";
                                Settings.USettings.LemonAreeunIts = qq;
                                Settings.SaveSettings();
                                Settings.LSettings.qq = qq;
                                Settings.SaveLocaSettings();
                                Console.WriteLine(Settings.USettings.g_tk + "  " + Settings.USettings.Cookie);
                            });
                            Load_Theme(false);
                        });
                        a();
                    }
                }
            }
            return IntPtr.Zero;
        }
        #endregion

        private async void SearchBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Search_SmartBox.Visibility = Visibility.Visible;
            Search_SmartBoxList.Items.Clear();
            var data = await MusicLib.SearchHotKey();
            var mdb = new ListBoxItem { Background = new SolidColorBrush(Colors.Transparent), Height = 30, Content = "热搜", Margin = new Thickness(10, 0, 10, 0) };
            Search_SmartBoxList.Items.Add(mdb);
            for (int i = 0; i < 5; i++) {
                var dt = data[i];
                var bd = new ListBoxItem { Background = new SolidColorBrush(Colors.Transparent), Height = 30, Content = dt, Margin = new Thickness(10, 10, 10, 0) };
                bd.PreviewMouseDown += Bd_MouseDown;
                bd.PreviewKeyDown += Search_SmartBoxList_KeyDown;
                Search_SmartBoxList.Items.Add(bd);
            }
        }

        public void SetTopWhite(bool h) {
            if (h)
            {
                SearchBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#19000000"));
                SearchBox.Foreground = new SolidColorBrush(Colors.White);
                SkinBtn.ColorDx = SearchBox.Foreground;
                SettingsBtn.ColorDx = SearchBox.Foreground;
                CloseBtn.ColorDx = SearchBox.Foreground;
                MaxBtn.ColorDx = SearchBox.Foreground;
                MinBtn.ColorDx = SearchBox.Foreground;
            }
            else {
                SearchBox.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0C000000"));
                SearchBox.SetResourceReference(ForegroundProperty,"ResuColorBrush");
                SkinBtn.ColorDx = null;
                SettingsBtn.ColorDx = null;
                CloseBtn.ColorDx = null;
                MaxBtn.ColorDx = null;
                MinBtn.ColorDx = null;
            }
        }

        private void Cisv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Console.WriteLine("ScrollChanged:"+Cisv.VerticalOffset);
            if (SingerDP_Top.Uid == "ok") {
                if (Cisv.VerticalOffset >= 350){
                    if (SingerDP_Top.Visibility == Visibility.Collapsed) {
                        SingerDP_Top.Visibility = Visibility.Visible;
                    }
                }
                else {
                    SingerDP_Top.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SingerDataPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (SingerDataPage.Visibility == Visibility.Collapsed)
                SetTopWhite(false);
        }
    }
}