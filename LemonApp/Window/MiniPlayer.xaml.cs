using LemonLib;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LemonApp
{
    /// <summary>
    /// MiniPlayer.xaml 的交互逻辑
    /// </summary>
    public partial class MiniPlayer : Window
    {
        private MainWindow mw;
        public MiniPlayer(MainWindow m)
        {
            InitializeComponent();
            Loaded += MiniPlayer_Loaded;
            Closing += MiniPlayer_Closing;
            mw = m;
            jd.PreviewMouseDown += mw.Jd_PreviewMouseDown;
            jd.PreviewMouseUp += mw.Jd_PreviewMouseUp;
            jd.ValueChanged += mw.Jd_ValueChanged;
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.Fant);
        }

        private void MiniPlayer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            Settings.USettings.IsMiniOpen = false;
            e.Cancel = true;
        }

        private void MiniPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            WindowAccentCompositor wac = new WindowAccentCompositor(this, false, (c) =>
            {
                c.A = 255;
                Background = new SolidColorBrush(c);
            });
            wac.Color = App.BaseApp.ThemeColor == 0 ?
            Color.FromArgb(200, 255, 255, 255) :
            Color.FromArgb(200, 0, 0, 0);
            wac.IsEnabled = true;
        }

        private void likeBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mw.likeBtn_MouseDown(sender, e);
        }

        private void LastBtnDown(object sender, MouseButtonEventArgs e)
        {
            mw.PlayControl_PlayLast(sender, e);
        }

        private void NextBtnDown(object sender, MouseButtonEventArgs e)
        {
            mw.PlayControl_PlayNext(sender, e);
        }

        private void PlayBtnDown(object sender, MouseButtonEventArgs e)
        {
            mw.PlayBtn_MouseDown(sender, e);
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            CloseBtn.Visibility = Visibility.Visible;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseBtn.Visibility = Visibility.Collapsed;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void CloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Hide();
            Settings.USettings.IsMiniOpen = false;
        }

        private void XHBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mw.XHBtn_MouseDown(sender, e);
        }
    }
}
