using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace LemonLib
{
    public static class UIHelper
    {
        public static class ScrollViewerBehavior
        {
            public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior), new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));
            public static void SetVerticalOffset(FrameworkElement target, double value) => target.SetValue(VerticalOffsetProperty, value);
            public static double GetVerticalOffset(FrameworkElement target) => (double)target.GetValue(VerticalOffsetProperty);
            private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) => (target as ScrollViewer)?.ScrollToVerticalOffset((double)e.NewValue);
        }
        /// <summary>
        /// 子页面获取父类窗体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static T GetAncestor<T>(DependencyObject reference) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(reference);
            while (!(parent is T) && parent != null)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            if (parent != null)
                return (T)parent;
            else
                return null;
        }
        public static void G(FrameworkElement d)
        {
            try
            {
                if (d != null)
                {
                    d.FocusVisualStyle = null;
                    if (d is Panel)
                    {
                        foreach (var s in (d as Panel).Children)
                        {
                            G(s as FrameworkElement);
                        }
                    }
                    if (d is ScrollViewer)
                    {
                        G((d as ScrollViewer).Content as FrameworkElement);
                    }
                }
            }
            catch { }
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
