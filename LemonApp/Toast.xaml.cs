using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace LemonApp
{
    /// <summary>
    /// Toast.xaml 的交互逻辑
    /// </summary>
    public partial class Toast : Window
    {
        public Toast(string text)
        {
            InitializeComponent();
            Opacity = 0;
            tb.Text = text;
            Loaded += async delegate
            {
                if (tb.Text.Contains("\r\n"))
                    Height = 90;
                Width = SystemParameters.WorkArea.Width;
                Left = 0;
                Top = SystemParameters.WorkArea.Height - Height + 10;
                BeginAnimation(OpacityProperty, new DoubleAnimation(0.5, 1, TimeSpan.FromMilliseconds(300)));
                await Task.Delay(3000);
                BeginAnimation(OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(2000)));
                await Task.Delay(2500);
                Close();
            };
        }
        public static void Send(string text)
        {
            new Toast(text).Show();
        }
        public Toast(string text, bool staylonger)
        {
            InitializeComponent();
            tb.Text = text;
            Loaded += delegate
            {
                if (tb.Text.Contains("\r\n"))
                    Height = 90;
                if (text == "") Opacity = 0;
                Width = SystemParameters.WorkArea.Width;
                Left = 0;
                Top = SystemParameters.WorkArea.Height - Height - 30;
            };
            Show();
        }
        public void Updata(string Text)
        {
            tb.Text = Text;
            if (tb.Text.Contains("\r\n"))
                Height = 90;
            else Height = 45;
            if (Text == "") Opacity = 0;
            else Opacity = 1;
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Width = SystemParameters.WorkArea.Width;
            Left = 0;
            Top = SystemParameters.WorkArea.Height - Height - 30;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
