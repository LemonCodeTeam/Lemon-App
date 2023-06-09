using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using static LemonLib.WindowStyles;

namespace LemonApp
{
    /// <summary>
    /// 发送一个Toast通知   或作为桌面歌词
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
        public void Update(string Text)
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
        }
    }
}
