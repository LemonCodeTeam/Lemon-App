﻿using LemonLibrary;
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
        DataItem MusicData = new DataItem(new Music());
        bool isplay = false;
        bool IsRadio = false;
        string RadioID = "";
        int ind = 0;//歌词页面是否打开
        bool xh = false;//false: lb true:dq  循环/单曲 播放控制
        bool issingerloaded = false;
        bool mod = true;//true : qq false : wy
        bool isLoading = false;
        bool isPlayasRun = false;
        NowPage np;
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
            else Settings.LoadUSettings("Public");
            Load_Theme(false);
            //-----Timer user
            var ds = new System.Windows.Forms.Timer() { Interval = 2000 };
            ds.Tick += delegate { GC.Collect(); UIHelper.G(Page); };
            ds.Start();
            //---------切割机-----------
            //任务栏 => 正在播放的专辑图片
            Tasktb.ThumbnailClipMargin = new Thickness(LeftControl.ActualWidth, ActualHeight - MusicImage.ActualHeight, ControlDownPage.ActualWidth - LeftControl.ActualWidth - 74, 0);
            //---------加载TOP页面-----
            await Task.Run(TopLoadac());
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
                    //没有主题配置  （主要考虑到切换登录）
                    App.BaseApp.unSkin();
                    Page.Background = new SolidColorBrush(Colors.White);
                }
            }
            LoadMusicData(hasAnimation);
        }
        private double now = 0;
        private string lastlyric = "";
        private Toast lyricTa = new Toast("", true);
        private bool isOpenGc = true;
        private bool CanPlaysw = false;
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
                MusicData = new DataItem(Settings.USettings.Playing);
                PlayMusic(Settings.USettings.Playing.MusicID, Settings.USettings.Playing.ImageUrl, Settings.USettings.Playing.MusicName, Settings.USettings.Playing.SingerText, false, false);
                string downloadpath = Settings.USettings.CachePath + "Music\\" + Settings.USettings.Playing.MusicID + ".mp3";
                MusicLib.mp.Open(new Uri(downloadpath));
            }
            //--------播放时的Timer 进度/歌词
            t.Interval = 500;
            t.Tick += delegate
            {
                try
                {
                    Console.Write("- ");
                    now = MusicLib.mp.Position.TotalMilliseconds;
                    if (now == 0) { now = Play_sw.Elapsed.TotalMilliseconds; CanPlaysw = true; }
                    else { Play_sw.Reset(); CanPlaysw = false; }
                    Play_Now.Text = TextHelper.TimeSpanToms(TimeSpan.FromMilliseconds(now));
                    if (isPlayasRun && MusicLib.mp.NaturalDuration.HasTimeSpan)
                    {
                        double all = MusicLib.mp.NaturalDuration.TimeSpan.TotalMilliseconds;
                        string alls = TextHelper.TimeSpanToms(TimeSpan.FromMilliseconds(all));
                        if (Play_All.Text == alls && Play_All.Text != "00:") isPlayasRun = false;
                        Play_All.Text = alls;
                        jd.Maximum = all;
                    }
                    jd.Value = now;
                    if (ind == 1)
                        ml.lv.LrcRoll(now, true);
                    else ml.lv.LrcRoll(now, false);
                    Console.Write("- " + now);
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
                else Border_MouseDown_1(null, null);//下一曲
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
            ShowInTaskbar = true;
            Activate();
        }
        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowInTaskbar = false;
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
            //-------任务栏的专辑图片切割机----
            Console.WriteLine("SIZECHANGED");
            double tp = ActualWidth / 2;
            Thickness ab;
            if (ind == 0)//在主界面的
                ab = new Thickness(LeftControl.ActualWidth, ActualHeight - MusicImage.ActualHeight, ControlDownPage.ActualWidth - LeftControl.ActualWidth - 74, 0);
            //在歌词界面的
            else ab = new Thickness(ActualWidth - tp - 30 - border4.ActualWidth, border4.Margin.Top, tp + 30, border4.Margin.Bottom);
            Tasktb.ThumbnailClipMargin = ab;
            Console.WriteLine("Tasktb   LEFT:" + ab.Left + "   TOP" + ab.Top + "   RIGHT" + ab.Right + "   BOTTOM" + ab.Bottom);
            //------------调整大小时对控件进行伸缩---------------
            WidthUI(SingerItemsList);
            WidthUI(RadioItemsList);
            WidthUI(GDItemsList);
            WidthUI(FLGDItemsList);
            WidthUI(GDILikeItemsList);
            if (Data.Visibility == Visibility.Visible)
                foreach (DataItem dx in DataItemsList.Items)
                    dx.Width = ContentPage.ActualWidth;
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
        }
        #endregion
        #region 功能区
        #region Top 排行榜
        /// <summary>
        /// 加载TOP列表
        /// </summary>
        /// <returns></returns>
        private Action TopLoadac() {
            return new Action(async delegate
            {
                var dt = await ml.GetTopIndexAsync();
                Dispatcher.Invoke(() => { topIndexList.Children.Clear(); });
                foreach (var d in dt)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var top = new TopControl(d.ID, d.Photo, d.Name);
                        top.MouseDown += Top_MouseDown;
                        top.Margin = new Thickness(0, 0, 20, 20);
                        topIndexList.Children.Add(top);
                    });
                }
            });
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
                TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(g.pic));
                TB.Text = g.name;
                DataItemsList.Items.Clear();
            }
            var dta = await ml.GetToplistAsync(g.topID, osx);
            foreach (var j in dta)
            {
                var k = new DataItem(j) { Width = ContentPage.ActualWidth };
                k.GetToSingerPage += K_GetToSingerPage;
                k.Play += PlayMusic;
                k.Download += K_Download;
                if (k.music.MusicID == MusicData.music.MusicID)
                {
                    k.ShowDx();
                    MusicData = k;
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
        private Label LastClickLabel = null;
        private Grid LastPage = null;
        public void NSPage(Label ClickLabel, Grid TPage)
        {
            if (LastClickLabel == null) LastClickLabel = TopBtn;
            LastClickLabel.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            if (ClickLabel != null) ClickLabel.SetResourceReference(ForegroundProperty, "ThemeColor");
            if (LastPage == null) LastPage = TopIndexPage;
            LastPage.Visibility = Visibility.Collapsed;
            TPage.Visibility = Visibility.Visible;
            TPage.BeginAnimation(OpacityProperty, new DoubleAnimation(0.5, 1, TimeSpan.FromSeconds(0.2)));
            if (ClickLabel != null) LastClickLabel = ClickLabel;
            LastPage = TPage;
        }
        private void TopBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(TopBtn, TopIndexPage);
        }
        #endregion
        #region Singer 歌手界面
        string SingerKey1 = "all_all_";
        string SingerKey2 = "all";
        private void SingerPageChecked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                SingerKey1 = (sender as RadioButton).Uid;
                string sk = SingerKey1 + SingerKey2;
                if (sk == "all")
                    sk = "all_all_all";
                GetSingerList(sk);
            }
        }
        private void K_GetToSingerPage(MusicSinger ms)
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
        private async void GetSinger(SingerItem si,int osx=1) {
            np = NowPage.SingerItem;
            singer_now = si.data;
            ixSinger = osx;
            OpenLoading();
            List<Music> dt = await ml.GetSingerMusicByIdAsync(si.data.Mid,osx);
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
                if (k.music.MusicID == MusicData.music.MusicID)
                {
                    k.ShowDx();
                    MusicData = k;
                }
                k.Play += PlayMusic;
                k.Download += K_Download;
                DataItemsList.Items.Add(k);
            }
            if(osx==1)Datasv.ScrollToTop();
            CloseLoading();
        }
        private async void GetSingerList(string key,int osx=1) {
            OpenLoading();
            SingerPg_now = key;
            ixSingerList = osx;
            var sin = await ml.GetSingerAsync(key,osx);
            if (osx == 1){
                SingerItemsList.Children.Clear();
            }
            foreach (var d in sin)
            {
                    var sinx = new SingerItem(d) { Margin = new Thickness(20, 0, 0, 20) };
                    sinx.MouseDown += GetSinger;
                    SingerItemsList.Children.Add(sinx);
            }
            if (osx == 1) SingerPage_sv.ScrollToTop();
            CloseLoading();
        }
        private string SingerPg_now;
        private int ixSingerList = 1;
        private void SingerPage_sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (SingerPage_sv.IsVerticalScrollBarAtButtom())
            {
                ixSingerList++;
                GetSingerList(SingerPg_now, ixSingerList);
            }
        }
        private void SIngerPageChecked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                if (SingerKey1 == "")
                    SingerKey1 = "all_all_";
                SingerKey2 = (sender as RadioButton).Content.ToString().Replace("热门", "all").Replace("#", "9");
                GetSingerList(SingerKey1 + SingerKey2);
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
                GetSingerList("all_all_all");
            }
        }
        #endregion
        #region FLGD 分类歌单
        private void ZJBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(ZJBtn, ZJIndexPage);
            if (FLGDIndexList.Children.Count == 0)
            {
                var sinx = new Task(new Action(async delegate
                {
                    Dispatcher.Invoke(() => { OpenLoading(); });
                    var wk = await ml.GetFLGDIndexAsync();
                    Dispatcher.Invoke(() =>
                    {
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
                        FLGDIndexList.Children.Add(rb);
                    });
                    Dispatcher.Invoke(() =>
                    {
                        var rb = new TextBlock() { Text = "语种:" };
                        rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                        FLGDIndexList.Children.Add(rb);
                    });
                    foreach (var d in wk.Lauch)
                        Dispatcher.Invoke(() =>
                        {
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
                        var rb = new TextBlock() { Text = "流派:" };
                        rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                        FLGDIndexList.Children.Add(rb);
                    });
                    foreach (var d in wk.LiuPai)
                        Dispatcher.Invoke(() =>
                        {
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
                        var rb = new TextBlock() { Text = "主题:" };
                        rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                        FLGDIndexList.Children.Add(rb);
                    });
                    foreach (var d in wk.Theme)
                        Dispatcher.Invoke(() =>
                        {
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
                        var rb = new TextBlock() { Text = "心情:" };
                        rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                        FLGDIndexList.Children.Add(rb);
                    });
                    foreach (var d in wk.Heart)
                        Dispatcher.Invoke(() =>
                        {
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
                        var rb = new TextBlock() { Text = "场景:" };
                        rb.SetResourceReference(ForegroundProperty, "TextX1ColorBrush");
                        FLGDIndexList.Children.Add(rb);
                    });
                    foreach (var d in wk.Changjing)
                        Dispatcher.Invoke(() =>
                        {
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
                    Dispatcher.Invoke(() => { GetGDList(wk.Hot[0].id); CloseLoading(); });
                }));
                sinx.Start();
            }
        }
        private void FLGDPageChecked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                OpenLoading();
                var dt = sender as RadioButton;
                GetGDList(dt.Uid);
            }
        }
        private string FLGDId_now;
        private int ixFLGD = 0;
        private void FLGDPage_sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (FLGDPage_sv.IsVerticalScrollBarAtButtom())
            {
                ixFLGD++;
                GetGDList(FLGDId_now, ixFLGD);
            }
        }
        private async void GetGDList(string id, int osx = 1) {
            FLGDId_now = id;
            ixFLGD = osx;
            OpenLoading();
            var data = await ml.GetFLGDAsync(id, osx);
            if(osx==1)FLGDItemsList.Children.Clear();
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
        }
        #endregion
        #region Radio 电台
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
                    PlayMusic(data.MusicID, data.ImageUrl, data.MusicName, data.SingerText, true);
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
                        Dispatcher.Invoke(() =>
                        {
                            RadioItem a = new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) };
                            a.MouseDown += GetRadio;
                            a.Width = RadioItemsList.ActualWidth / 5;
                            RadioItemsList.Children.Add(a);
                        });
                    }
                    Dispatcher.Invoke(() => { CloseLoading(); });
                }));
                s.Start();
            }
        }
        Music RadioData;
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
                if (IsRadio)
                {
                    if (Settings.USettings.MusicLike.ContainsKey(RadioData.MusicID))
                    {
                        LikeBtnUp();
                        Settings.USettings.MusicLike.Remove(RadioData.MusicID);
                        string a = MusicLib.DeleteMusicFromGD(RadioData.MusicID, MusicLib.MusicLikeGDid, MusicLib.MusicLikeGDdirid);
                        Toast.Send(a);
                    }
                    else
                    {
                        string[] a = MusicLib.AddMusicToGD(RadioData.MusicID, MusicLib.MusicLikeGDdirid);
                        Toast.Send(a[1] + ": " + a[0]);
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
                        string[] a = MusicLib.AddMusicToGD(MusicData.music.MusicID, MusicLib.MusicLikeGDdirid);
                        Toast.Send(a[1] + ": " + a[0]);
                        Settings.USettings.MusicLike.Add(MusicData.music.MusicID, MusicData.music);
                        LikeBtnDown();
                    }
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
            NSPage(LikeBtn, Data);
            loadin.Value = 0;
            loadin.Opacity = 1;
            TB.Text = "我喜欢";
            TXx.Background = Resources["LoveIcon"] as VisualBrush;
            DataItemsList.Items.Clear();
            He.MGData_Now = await MusicLib.GetGDAsync(MusicLib.MusicLikeGDid, new Action<Music, bool>((j, b) => {
                var k = new DataItem(j, b, this);
                DataItemsList.Items.Add(k);
                k.Play += PlayMusic;
                k.Width = DataItemsList.ActualWidth;
                if (j.MusicID == MusicData.music.MusicID)
                {
                    k.ShowDx();
                    MusicData = k;
                }
                loadin.Value = DataItemsList.Items.Count;
            }), this,
            new Action<int>(i => loadin.Maximum = i));
            loadin.Opacity = 0;
            np = NowPage.GDItem;
        }
        #endregion
        #region DataPageBtn 歌曲数据 DataPage 的逻辑处理
        private void DataPlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PlayMusic(DataItemsList.Items[0] as DataItem, null);
        }

        private void DataDownloadBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
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
        private void Datasv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Datasv == null) Datasv = sender as ScrollViewer;
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
                else foreach (var dt in data)
                        Search_SmartBoxList.Items.Add(dt);
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
                if (Search_SmartBoxList.SelectedIndex != -1)
                {
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
        public void SearchMusic(string key, int osx = 0)
        {
            np = NowPage.Search;
            SearchKey = key;
            OpenLoading();
            var xs = new Task(new Action(async delegate
            {
                List<Music> dt = null;
                if (osx == 0) Dispatcher.Invoke(() => { NSPage(null, Data); });
                if (osx == 0) dt = await ml.SearchMusicAsync(key);
                else dt = await ml.SearchMusicAsync(key, osx);
                if (osx == 0)
                    Dispatcher.Invoke(() =>
                    {
                        TB.Text = key;
                        DataItemsList.Items.Clear();
                        Datasv.ScrollToTop();
                    });
                await Dispatcher.Invoke(async () =>
                {
                    TXx.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.First().ImageUrl));
                    foreach (var j in dt)
                    {
                        var k = new DataItem(j) { Width = ContentPage.ActualWidth };
                        if (k.music.MusicID == MusicData.music.MusicID)
                        {
                            k.ShowDx();
                            MusicData = k;
                        }
                        k.GetToSingerPage += K_GetToSingerPage;
                        k.Play += PlayMusic;
                        k.Download += K_Download;
                        DataItemsList.Items.Add(k);
                    }
                });
                Dispatcher.Invoke(() => { CloseLoading(); });
            }));
            xs.Start();
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
                dt.ShowDx();
                MusicData = dt;
                PlayMusic(dt.music.MusicID, dt.music.ImageUrl, dt.music.MusicName, dt.music.SingerText);
            }
        }
        public async void PlayMusic(DataItem dt, bool next = false)
        {
            MusicData = dt;
            if (await MusicLib.GetUrlAsync(dt.music.MusicID) == null)
                if (next) Border_MouseDown_1(null, null);
                else new CannotPlay().ShowDialog();
            else
            {
                dt.ShowDx();
                PlayMusic(dt.music.MusicID, dt.music.ImageUrl, dt.music.MusicName, dt.music.SingerText);
            }
        }
        public async void PlayMusic(DataItem dt)
        {
            if (await MusicLib.GetUrlAsync(dt.music.MusicID) == null)
                new CannotPlay().ShowDialog();
            else
            {
                dt.ShowDx();
                MusicData = dt;
                PlayMusic(dt.music.MusicID, dt.music.ImageUrl, dt.music.MusicName, dt.music.SingerText);
            }
        }
        private string LastPlay = "";
        private Stopwatch Play_sw = new Stopwatch();
        public async void PlayMusic(string id, string x, string name, string singer, bool isRadio = false, bool doesplay = true)
        {
            if (await MusicLib.GetUrlAsync(id) == null)
                Border_MouseDown_1(null, null);
            else
            {
                if (LastPlay == id)
                {
                    if (doesplay)
                    {
                        MusicLib.mp.Position = TimeSpan.FromMilliseconds(0);
                        MusicLib.mp.Play();
                        Tasktb_playBtn.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Image/pause.png", UriKind.Absolute));
                        (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Pause);
                        t.Start();
                        Play_sw.Restart();
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
                    Settings.USettings.Playing = MusicData.music;
                    Settings.SaveSettings();
                    if (Settings.USettings.MusicLike.ContainsKey(id))
                        LikeBtnDown();
                    else LikeBtnUp();
                    ml.GetAndPlayMusicUrlAsync(id, true, MusicName, this, name + " - " + singer, doesplay);
                    if (doesplay) Play_sw.Restart();
                    var im = await ImageCacheHelp.GetImageByUrl(x);
                    MusicImage.Background = new ImageBrush(im);
                    var rect = new System.Drawing.Rectangle(0, 0, im.PixelWidth, im.PixelHeight);
                    var imb = im.ToBitmap();
                    imb.GaussianBlur(ref rect, 80);
                    LyricPage_Background.Background = new ImageBrush(imb.ToBitmapImage()) { Stretch = Stretch.UniformToFill };
                    Singer.Text = singer;
                    if (doesplay)
                    {
                        (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Pause);
                        Tasktb_playBtn.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Image/pause.png", UriKind.Absolute));
                        t.Start();
                        isplay = true;
                    }
                    LastPlay = MusicData.music.MusicID;
                }
            }
        }
        #endregion
        #region PlayControl
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
        private void Tasktb_playBtn_Click(object sender, EventArgs e)
        {
            PlayBtn_MouseDown(null, null);
        }
        private void Jd_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {//若使用ValueChanged事件，在value改变时也会触发，而不单是拖动jd.
            MusicLib.mp.Position = TimeSpan.FromMilliseconds(jd.Value);
        }
        private void ThumbButtonInfo_Click(object sender, EventArgs e)
        {
            Border_MouseDown(null, null);
        }

        private void ThumbButtonInfo_Click_1(object sender, EventArgs e)
        {
            Border_MouseDown_1(null, null);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsRadio)
                PlayMusic(DataItemsList.Items[DataItemsList.Items.IndexOf(MusicData) - 1] as DataItem, null);
        }
        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (!IsRadio)
                PlayMusic(DataItemsList.Items[DataItemsList.Items.IndexOf(MusicData) + 1] as DataItem,true);
            else GetRadio(new RadioItem(RadioID), null);
        }
        private void PlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isplay)
            {
                if (CanPlaysw) Play_sw.Stop();
                isplay = false;
                MusicLib.mp.Pause();
                Tasktb_playBtn.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Image/play.png", UriKind.Absolute));
                t.Stop();
                (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Play);
            }
            else
            {
                isplay = true;
                if (CanPlaysw) Play_sw.Start();
                MusicLib.mp.Play();
                Tasktb_playBtn.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Image/pause.png", UriKind.Absolute));
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
            App.BaseApp.SetColor("ButtonColorBrush", LastButtonColor);
            if (!isOpenGc) path7.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            likeBtn_path.SetResourceReference(Path.FillProperty, "ResuColorBrush");
            ControlDownPage.BorderThickness = new Thickness(0, 1, 0, 0);
            var ol = Resources["CloseLyricPage"] as Storyboard;
            ol.Completed += async delegate
            {
                await Task.Delay(500);
                Thickness ab = new Thickness(LeftControl.ActualWidth + 10, ActualHeight - 10 - MusicImage.ActualHeight, ControlDownPage.ActualWidth - LeftControl.ActualWidth - 74 + 10, 10);
                Tasktb.ThumbnailClipMargin = ab;
            };
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
            if (!isOpenGc) path7.Fill = new SolidColorBrush(Colors.White);
            likeBtn_path.Fill = new SolidColorBrush(Colors.White);
            var ol = Resources["OpenLyricPage"] as Storyboard;
            ol.Completed += async delegate
            {
                await Task.Delay(500);
                double tp = ActualWidth / 2;
                Thickness ab = new Thickness(ActualWidth - tp - 30 - border4.ActualWidth, border4.Margin.Top + 10, tp + 30, border4.Margin.Bottom + 10);
                Tasktb.ThumbnailClipMargin = ab;
            };
            ol.Begin();
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
            Border_MouseDown_2(null,null);
            NSPage(null, MusicPLPage);
            MusicPL_tb.Text = MusicName.Text + " - " + Singer.Text;
            List<MusicPL> data = await ml.GetPLByQQAsync(Settings.USettings.Playing.MusicID);
            MusicPlList.Children.Clear();
            foreach (var dt in data)
            {
                MusicPlList.Children.Add(new PlControl(dt) { Width =MusicPlList.ActualWidth - 10, Margin = new Thickness(10, 0, 0, 20) });
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
                MusicLib.AddGDILike(AddGDPage_id.Text);
                TwMessageBox.Show("添加成功");
                (Resources["CloseAddGDPage"] as Storyboard).Begin();
                GDBtn_MouseDown(null, null);
            }
            else
            {
                ml.GetGDbyWYAsync(AddGDPage_id.Text, this, AddGDPage_ps_name, AddGDPage_ps_jd,
                    () =>
                    {
                        (Resources["CloseAddGDPage"] as Storyboard).Begin();
                        GDBtn_MouseDown(null, null);
                    });
            }
        }
        #endregion
        #region Download
        private void K_Download(DataItem sender)
        {
            var cc = (Resources["Downloading"] as Storyboard);
            if (DownloadIsFinish)
                cc.Begin();
            var f = sender;
            string name = f.music.MusicName + " - " + f.music.SingerText;
            Console.WriteLine(name);
            string file = Settings.USettings.DownloadPath + $"\\{name}.mp3";
            DownloadItem di = new DownloadItem(f.music, file, DownloadItemsList.Children.Count)
            {
                Width = ContentPage.ActualWidth
            };
            di.Delete += (s) =>
            {
                s.d.Pause();
                DownloadItemsList.Children.Remove(s);
            };
            di.Loadedd += () =>
            {
                di.d.Finished += () =>
                {
                    DownloadIsFinish = true;
                    Dispatcher.Invoke(() =>
                    {
                        foreach (var a in DownloadItemsList.Children)
                        {
                            DownloadItem dl = a as DownloadItem;
                            if (!dl.finished)
                            { DownloadIsFinish = false; dl.d.Download(); break; }
                        }
                        if (DownloadIsFinish) (Resources["Downloading"] as Storyboard).Stop();
                    });
                };
            };
            DownloadItemsList.Children.Add(di);
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
            if (DownloadIsFinish) (Resources["Downloading"] as Storyboard).Stop();
            DownloadItemsList.Children.Clear();
            NonePage_Copy.Visibility = Visibility.Visible;
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
                    string name = (f.music.MusicName + " - " + f.music.SingerText).Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""); ;
                    string file = Download_Path.Text + $"\\{name}.mp3";
                    DownloadItem di = new DownloadItem(f.music, file, DownloadItemsList.Children.Count)
                    {
                        Width = ContentPage.ActualWidth
                    };
                    di.Delete += (s) =>
                    {
                        s.d.Pause();
                        DownloadItemsList.Children.Remove(s);
                    };
                    di.Loadedd += () =>
                    {
                        di.d.Finished += () =>
                        {
                            DownloadIsFinish = true;
                            Dispatcher.Invoke(() =>
                            {
                                foreach (var a in DownloadItemsList.Children)
                                {
                                    DownloadItem dl = a as DownloadItem;
                                    if (!dl.finished)
                                    { DownloadIsFinish = false; dl.d.Download(); break; }
                                }
                            });
                            if (DownloadIsFinish) (Resources["Downloading"] as Storyboard).Stop();
                        };
                    };
                    DownloadItemsList.Children.Add(di);
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
                if (j.MusicID == MusicData.music.MusicID)
                {
                    k.ShowDx();
                    MusicData = k;
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
                { Border_MouseDown(null, null); Toast.Send("成功切换到上一曲"); }
                else if (wParam.ToInt32() == 128)
                { Border_MouseDown_1(null, null); Toast.Send("成功切换到下一曲"); }
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
                            await HttpHelper.HttpDownloadFileAsync(sdc["headpic"].ToString(), Settings.USettings.CachePath + qq + ".jpg");
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
    }
}