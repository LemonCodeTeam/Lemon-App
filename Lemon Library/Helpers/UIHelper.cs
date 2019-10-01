using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace LemonLibrary
{
    public static class UIHelper {
        public static class ScrollViewerBehavior
        {
            public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(ScrollViewerBehavior), new UIPropertyMetadata(0.0, OnHorizontalOffsetChanged));
            public static void SetHorizontalOffset(FrameworkElement target, double value) => target.SetValue(HorizontalOffsetProperty, value);
            public static double GetHorizontalOffset(FrameworkElement target) => (double)target.GetValue(HorizontalOffsetProperty);
            private static void OnHorizontalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) => (target as ScrollViewer)?.ScrollToHorizontalOffset((double)e.NewValue);

            public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior), new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));
            public static void SetVerticalOffset(FrameworkElement target, double value) => target.SetValue(VerticalOffsetProperty, value);
            public static double GetVerticalOffset(FrameworkElement target) => (double)target.GetValue(VerticalOffsetProperty);
            private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) => (target as ScrollViewer)?.ScrollToVerticalOffset((double)e.NewValue);
        }
        public static bool IsVerticalScrollBarAtButtom(this ScrollViewer ex)
        {
            try
            {
                bool isAtButtom = false;
                double dVer = ex.VerticalOffset;
                double dViewport = ex.ViewportHeight;
                double dExtent = ex.ExtentHeight;
                if (dVer != 0)
                {
                    if (dVer + dViewport == dExtent)
                        isAtButtom = true;
                    else
                        isAtButtom = false;
                }
                else
                    isAtButtom = false;
                if (ex.VerticalScrollBarVisibility == ScrollBarVisibility.Disabled || ex.VerticalScrollBarVisibility == ScrollBarVisibility.Hidden)
                    isAtButtom = true;
                return isAtButtom;
            }
            catch { return false; }
        }
    }
    public class MyPopup : Popup
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetActiveWindow(IntPtr hWnd);

        static MyPopup()
        {
            EventManager.RegisterClassHandler(
                typeof(MyPopup),
                Popup.PreviewGotKeyboardFocusEvent,
                new KeyboardFocusChangedEventHandler(OnPreviewGotKeyboardFocus),
                true);
        }

        private static void OnPreviewGotKeyboardFocus(Object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = e.NewFocus as TextBoxBase;
            if (textBox != null)
            {
                var hwndSource = PresentationSource.FromVisual(textBox) as HwndSource;
                if (hwndSource != null)
                {
                    SetActiveWindow(hwndSource.Handle);
                }
            }
        }
    }
}
