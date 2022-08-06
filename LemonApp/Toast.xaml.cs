using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;

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

        #region Window styles
        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
        #endregion
    }
}
