using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LemonApp
{
    /// <summary>
    /// TwMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class TwMessageBox : Window
    {
        public static bool Show(string tb)
        {
            return (bool)new TwMessageBox(tb).ShowDialog();
        }


        public TwMessageBox(string tb)
        {
            InitializeComponent();
            title.Text = tb;
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Background = null;
            WdBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180));
            title.Foreground = new SolidColorBrush(Color.FromRgb(75, 75, 75));
            candle.Theme = 0;
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
    }
}
