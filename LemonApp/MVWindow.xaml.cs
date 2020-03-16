using LemonLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// MVWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MVWindow : Window
    {
        public MVWindow(MVData Mv)
        {
            InitializeComponent();
            mVData = Mv;
            Loaded += MVWindow_Loaded;
        }

        private void MVWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PlayMv(mVData);
        }

        private MVData mVData;
        string mvpause = "M735.744 49.664c-51.2 0-96.256 44.544-96.256 95.744v733.184c0 51.2 45.056 95.744 96.256 95.744s96.256-44.544 96.256-95.744V145.408c0-51.2-45.056-95.744-96.256-95.744z m-447.488 0c-51.2 0-96.256 44.544-96.256 95.744v733.184c0 51.2 45.056 95.744 96.256 95.744S384 929.792 384 878.592V145.408c0-51.2-44.544-95.744-95.744-95.744z";
        string mvplay = "M766.464,448.170667L301.226667,146.944C244.394667,110.08,213.333333,126.293333,213.333333,191.146667L213.333333,832.853333C213.333333,897.706666,244.394666,913.834666,301.312,876.970667L766.378667,575.744C825.429334,537.514667,825.429334,486.314667,766.378667,448.085333z M347.733333,948.650667C234.666667,1021.781333,128,966.314667,128,832.938667L128,191.146667C128,57.6,234.752,2.218667,347.733333,75.349333L812.8,376.576C923.733333,448.426667,923.733333,575.658667,812.8,647.424L347.733333,948.650667z";
        bool MVplaying = false;
        System.Windows.Forms.Timer mvt = new System.Windows.Forms.Timer();
        public async void PlayMv(MVData mVData)
        {
            mvt.Interval = 1000;
            mvt.Tick += Mvt_Tick;
            MVplaying = true;
            Title = mVData.name;
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
            foreach (var dt in data)
            {
                MVPlList.Children.Add(new PlControl(dt) {Margin = new Thickness(10, 0, 0, 20) });
            }
            Activate();
            Topmost = false;
            double ps=ActualWidth/MvPlay_ME.NaturalVideoWidth;
            MV.Height = MvPlay_ME.NaturalVideoHeight * ps;
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

        private void CloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void MaxBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                MV.Height = MVScrollViewer.ActualHeight;
            }
            else
            {
                WindowState = WindowState.Normal;
                double ps = ActualWidth / MvPlay_ME.NaturalVideoWidth;
                MV.Height = MvPlay_ME.NaturalVideoHeight * ps;
            }
        }

        private void MinBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double ps = ActualWidth / MvPlay_ME.NaturalVideoWidth;
            MV.Height = MvPlay_ME.NaturalVideoHeight * ps;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) {
                Topmost = !Topmost;
                Toast.Send(Topmost ? "已置顶":"已取消置顶");
            }
        }
    }
}
