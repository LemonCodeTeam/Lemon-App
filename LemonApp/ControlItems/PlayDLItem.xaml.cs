using LemonLib;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// PlayDLItem.xaml 的交互逻辑
    /// </summary>
    public partial class PlayDLItem : UserControl
    {
        public Music Data;
        public PlayDLItem(Music m)
        {
            InitializeComponent();
            Data = m;
            SingerName.Text = m.SingerText;
            MusicName.Text = m.MusicName;
            Width = 300;
            var DeleteBtn = new TitlePageBtn() { Visibility = Visibility.Collapsed, Pathness = new Thickness(0), Height = 15, Width = 15, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 20, 0), PathData = Geometry.Parse("M880,240L704,240 704,176C704,123.2,660.8,80,608,80L416,80C363.2,80,320,123.2,320,176L320,240 144,240C126.4,240 112,254.4 112,272 112,289.6 126.4,304 144,304L192,304 192,816C192,886.4,249.6,944,320,944L704,944C774.4,944,832,886.4,832,816L832,304 880,304C897.6,304 912,289.6 912,272 912,254.4 897.6,240 880,240z M384,176C384,158.4,398.4,144,416,144L608,144C625.6,144,640,158.4,640,176L640,240 384,240 384,176z M768,816C768,851.2,739.2,880,704,880L320,880C284.8,880,256,851.2,256,816L256,304 768,304 768,816z M416 432c-17.6 0-32 14.4-32 32v256c0 17.6 14.4 32 32 32s32-14.4 32-32V464c0-17.6-14.4-32-32-32zM608 432c-17.6 0-32 14.4-32 32v256c0 17.6 14.4 32 32 32s32-14.4 32-32V464c0-17.6-14.4-32-32-32z") };
            DeleteBtn.MouseDown += delegate {
                Delete(this);
            };
            grid.Children.Add(DeleteBtn);
            MouseEnter += delegate {
                DeleteBtn.Visibility = Visibility.Visible;
            };
            MouseLeave += delegate {
                DeleteBtn.Visibility = Visibility.Collapsed;
            };
        }
        public static Action<PlayDLItem> Delete;
        public PlayDLItem(Music m, bool NeedImg, string imgUrl)
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
            Loaded += async delegate
            {
                if (NeedImg)
                {
                    img.Visibility = Visibility.Visible;
                    SingerName.Margin = new Thickness(66, 32, 10, 0);
                    MusicName.Margin = new Thickness(66, 12, 10, 0);
                    img.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(imgUrl, new int[2] { 100, 100 }));
                }
            };
            Data = m;
            SingerName.Text = m.SingerText;
            MusicName.Text = m.MusicName;
        }
        public void p(bool isp)
        {
            PD.LastPD.v(false);
            v(isp);
        }
        public bool pv;
        private void v(bool isp)
        {
            pv = isp;
            if (isp)
            {
                bk.Visibility = Visibility.Visible;
                MusicName.SetResourceReference(ForegroundProperty, "ThemeColor");
                SingerName.SetResourceReference(ForegroundProperty, "ThemeColor");

            }
            else
            {
                bk.Visibility = Visibility.Collapsed;
                MusicName.SetResourceReference(ForegroundProperty, "PlayDLPage_Font_Most");
                SingerName.SetResourceReference(ForegroundProperty, "PlayDLPage_Font_Low");
            }
            PD.LastPD = this;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Borderbg.Background = Resources["Touched"] as SolidColorBrush;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Borderbg.Background = null;
        }
    }

    public class PD
    {
        public static PlayDLItem LastPD = new PlayDLItem(new Music());
    }
}
