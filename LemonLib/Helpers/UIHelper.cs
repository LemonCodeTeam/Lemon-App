using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Threading;

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
        /// <summary>
        /// 去除可恶的焦点边框虚线
        /// </summary>
        /// <param name="d"></param>
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
        /// <summary>
        /// ScrollViewer拉到底了吗?
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
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

    #region Window styles
    public class WindowStyles
    {
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
    }
    #endregion
    #region AppBar
    class Interop
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        [Flags]
        internal enum DWMWINDOWATTRIBUTE
        {
            DWMA_NCRENDERING_ENABLED = 1,
            DWMA_NCRENDERING_POLICY,
            DWMA_TRANSITIONS_FORCEDISABLED,
            DWMA_ALLOW_NCPAINT,
            DWMA_CPATION_BUTTON_BOUNDS,
            DWMA_NONCLIENT_RTL_LAYOUT,
            DWMA_FORCE_ICONIC_REPRESENTATION,
            DWMA_FLIP3D_POLICY,
            DWMA_EXTENDED_FRAME_BOUNDS,
            DWMA_HAS_ICONIC_BITMAP,
            DWMA_DISALLOW_PEEK,
            DWMA_EXCLUDED_FROM_PEEK,
            DWMA_LAST
        }

        [Flags]
        internal enum DWMNCRenderingPolicy
        {
            UseWindowStyle,
            Disabled,
            Enabled,
            Last
        }

        internal enum ABMsg : int
        {
            ABM_NEW = 0,
            ABM_REMOVE,
            ABM_QUERYPOS,
            ABM_SETPOS,
            ABM_GETSTATE,
            ABM_GETTASKBARPOS,
            ABM_ACTIVATE,
            ABM_GETAUTOHIDEBAR,
            ABM_SETAUTOHIDEBAR,
            ABM_WINDOWPOSCHANGED,
            ABM_SETSTATE
        }
        internal enum ABNotify : int
        {
            ABN_STATECHANGE = 0,
            ABN_POSCHANGED,
            ABN_FULLSCREENAPP,
            ABN_WINDOWARRANGE
        }

        internal enum MonitorDefaultTo
        {
            MONITOR_DEFAULTTONULL,
            MONITOR_DEFAULTTOPRIMARY,
            MONITOR_DEFAULTTONEAREST
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
        internal static extern uint SHAppBarMessage(int dwMessage, ref APPBARDATA pData);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int RegisterWindowMessage(string msg);

        [DllImport("dwmapi.dll")]
        internal static extern int DwmSetWindowAttribute(IntPtr hWnd, int attr, ref int attrValue, int attrSize);

        [DllImport("User32.dll")]
        internal static extern IntPtr MonitorFromWindow(IntPtr hWnd, MonitorDefaultTo dwFlags);

        [DllImport("User32.dll")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
    }

    public enum ABEdge : int
    {
        Left = 0,
        Top,
        Right,
        Bottom,
        None
    }

    public static class AppBarFunctions
    {

        private class RegisterInfo
        {
            public int CallbackId { get; set; }
            public bool IsRegistered { get; set; }
            public Window Window { get; set; }
            public ABEdge Edge { get; set; }
            public WindowStyle OriginalStyle { get; set; }
            public Point OriginalPosition { get; set; }
            public Size OriginalSize { get; set; }
            public ResizeMode OriginalResizeMode { get; set; }
            public bool OriginalTopmost { get; set; }
            public FrameworkElement ChildElement { get; set; }


            public Rect? DockedSize { get; set; }

            public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam,
                                    IntPtr lParam, ref bool handled)
            {
                if (msg == CallbackId)
                {
                    if (wParam.ToInt32() == (int)Interop.ABNotify.ABN_POSCHANGED)
                    {
                        ABSetPos(this, Window, ChildElement);
                        handled = true;
                    }
                }
                return IntPtr.Zero;
            }

        }
        private static readonly Dictionary<Window, RegisterInfo> RegisteredWindowInfo
            = new Dictionary<Window, RegisterInfo>();
        private static RegisterInfo GetRegisterInfo(Window appbarWindow)
        {
            RegisterInfo reg;
            if (RegisteredWindowInfo.ContainsKey(appbarWindow))
            {
                reg = RegisteredWindowInfo[appbarWindow];
            }
            else
            {
                reg = new RegisterInfo()
                {
                    CallbackId = 0,
                    Window = appbarWindow,
                    IsRegistered = false,
                    Edge = ABEdge.Top,
                    OriginalStyle = appbarWindow.WindowStyle,
                    OriginalPosition = new Point(appbarWindow.Left, appbarWindow.Top),
                    OriginalSize =
                        new Size(appbarWindow.ActualWidth, appbarWindow.ActualHeight),
                    OriginalResizeMode = appbarWindow.ResizeMode,
                    OriginalTopmost = appbarWindow.Topmost,
                    DockedSize = null

                };
                RegisteredWindowInfo.Add(appbarWindow, reg);
            }
            return reg;
        }

        private static void RestoreWindow(Window appbarWindow)
        {
            var info = GetRegisterInfo(appbarWindow);

            appbarWindow.WindowStyle = info.OriginalStyle;
            appbarWindow.ResizeMode = info.OriginalResizeMode;
            appbarWindow.Topmost = info.OriginalTopmost;

            info.DockedSize = null;

            var rect = new Rect(info.OriginalPosition.X, info.OriginalPosition.Y,
                info.OriginalSize.Width, info.OriginalSize.Height);
            appbarWindow.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                    new ResizeDelegate(DoResize), appbarWindow, rect);

        }

        public static void SetAppBar(Window appbarWindow, ABEdge edge, FrameworkElement childElement = null, bool topMost = true)
        {
            var info = GetRegisterInfo(appbarWindow);
            info.Edge = edge;
            info.ChildElement = childElement;

            var abd = new Interop.APPBARDATA();
            abd.cbSize = Marshal.SizeOf(abd);
            abd.hWnd = new WindowInteropHelper(appbarWindow).Handle;

            int renderPolicy;

            if (edge == ABEdge.None)
            {
                if (info.IsRegistered)
                {
                    Interop.SHAppBarMessage((int)Interop.ABMsg.ABM_REMOVE, ref abd);
                    info.IsRegistered = false;
                }
                RestoreWindow(appbarWindow);

                // Restore normal desktop window manager attributes
                renderPolicy = (int)Interop.DWMNCRenderingPolicy.UseWindowStyle;

                Interop.DwmSetWindowAttribute(abd.hWnd, (int)Interop.DWMWINDOWATTRIBUTE.DWMA_EXCLUDED_FROM_PEEK, ref renderPolicy, sizeof(int));
                Interop.DwmSetWindowAttribute(abd.hWnd, (int)Interop.DWMWINDOWATTRIBUTE.DWMA_DISALLOW_PEEK, ref renderPolicy, sizeof(int));

                return;
            }

            if (!info.IsRegistered)
            {
                info.IsRegistered = true;
                info.CallbackId = Interop.RegisterWindowMessage("AppBarMessage");
                abd.uCallbackMessage = info.CallbackId;

                var ret = Interop.SHAppBarMessage((int)Interop.ABMsg.ABM_NEW, ref abd);

                var source = HwndSource.FromHwnd(abd.hWnd);
                source.AddHook(info.WndProc);
            }

            appbarWindow.WindowStyle = WindowStyle.None;
            appbarWindow.ResizeMode = ResizeMode.NoResize;
            appbarWindow.Topmost = topMost;

            // Set desktop window manager attributes to prevent window
            // from being hidden when peeking at the desktop or when
            // the 'show desktop' button is pressed
            renderPolicy = (int)Interop.DWMNCRenderingPolicy.Enabled;

            Interop.DwmSetWindowAttribute(abd.hWnd, (int)Interop.DWMWINDOWATTRIBUTE.DWMA_EXCLUDED_FROM_PEEK, ref renderPolicy, sizeof(int));
            Interop.DwmSetWindowAttribute(abd.hWnd, (int)Interop.DWMWINDOWATTRIBUTE.DWMA_DISALLOW_PEEK, ref renderPolicy, sizeof(int));

            ABSetPos(info, appbarWindow, childElement);
        }

        private delegate void ResizeDelegate(Window appbarWindow, Rect rect);
        private static void DoResize(Window appbarWindow, Rect rect)
        {
            appbarWindow.Width = rect.Width;
            appbarWindow.Height = rect.Height;
            appbarWindow.Top = rect.Top;
            appbarWindow.Left = rect.Left;
        }

        private static void ABSetPos(RegisterInfo info, Window appbarWindow, FrameworkElement childElement)
        {
            var edge = info.Edge;
            var barData = new Interop.APPBARDATA();
            barData.cbSize = Marshal.SizeOf(barData);
            barData.hWnd = new WindowInteropHelper(appbarWindow).Handle;
            barData.uEdge = (int)edge;

            // Transforms a coordinate from WPF space to Screen space
            var toPixel = PresentationSource.FromVisual(appbarWindow).CompositionTarget.TransformToDevice;
            // Transforms a coordinate from Screen space to WPF space
            var toWpfUnit = PresentationSource.FromVisual(appbarWindow).CompositionTarget.TransformFromDevice;

            // Transform window size from wpf units (1/96 ") to real pixels, for win32 usage
            var sizeInPixels = childElement != null ?
                toPixel.Transform(new Vector(childElement.ActualWidth, childElement.ActualHeight)) :
                toPixel.Transform(new Vector(appbarWindow.ActualWidth, appbarWindow.ActualHeight));
            // Even if the documentation says SystemParameters.PrimaryScreen{Width, Height} return values in 
            // "pixels", they return wpf units instead.
            var actualWorkArea = GetActualWorkArea(info);
            var screenSizeInPixels =
                toPixel.Transform(new Vector(actualWorkArea.Width, actualWorkArea.Height));
            var workTopLeftInPixels =
                toPixel.Transform(new Point(actualWorkArea.Left, actualWorkArea.Top));
            var workAreaInPixelsF = new Rect(workTopLeftInPixels, screenSizeInPixels);

            if (barData.uEdge == (int)ABEdge.Left || barData.uEdge == (int)ABEdge.Right)
            {
                barData.rc.top = (int)workAreaInPixelsF.Top;
                barData.rc.bottom = (int)workAreaInPixelsF.Bottom;
                if (barData.uEdge == (int)ABEdge.Left)
                {
                    barData.rc.left = (int)workAreaInPixelsF.Left;
                    //Left might not always be zero so we need to accommodate for that.
                    //For example, if the Start Menu is docked LEFT, if we don't do the math, we'll end up with a negative size error
                    barData.rc.right = barData.rc.left + (int)Math.Round(sizeInPixels.X);
                }
                else
                {
                    barData.rc.right = (int)workAreaInPixelsF.Right;
                    barData.rc.left = barData.rc.right - (int)Math.Round(sizeInPixels.X);
                }
            }
            else
            {
                barData.rc.left = (int)workAreaInPixelsF.Left;
                barData.rc.right = (int)workAreaInPixelsF.Right;
                if (barData.uEdge == (int)ABEdge.Top)
                {
                    barData.rc.top = (int)workAreaInPixelsF.Top;
                    //Top might not always be zero so we need to accommodate for that.
                    //For example, if the Start Menu is docked TOP, if we don't do the math, we'll end up with a negative size error
                    barData.rc.bottom = barData.rc.top + (int)Math.Round(sizeInPixels.Y);
                }
                else
                {
                    barData.rc.bottom = (int)workAreaInPixelsF.Bottom;
                    barData.rc.top = barData.rc.bottom - (int)Math.Round(sizeInPixels.Y);
                }
            }

            Interop.SHAppBarMessage((int)Interop.ABMsg.ABM_QUERYPOS, ref barData);
            Interop.SHAppBarMessage((int)Interop.ABMsg.ABM_SETPOS, ref barData);

            // transform back to wpf units, for wpf window resizing in DoResize. 
            var location = toWpfUnit.Transform(new Point(barData.rc.left, barData.rc.top));
            var dimension = toWpfUnit.Transform(new Vector(
                barData.rc.right - barData.rc.left,
                barData.rc.bottom - barData.rc.top));

            var rect = new Rect(location, new Size(dimension.X, dimension.Y));
            info.DockedSize = rect;

            //This is done async, because WPF will send a resize after a new appbar is added.  
            //if we size right away, WPFs resize comes last and overrides us.
            appbarWindow.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                new ResizeDelegate(DoResize), appbarWindow, rect);
        }

        private static Rect GetActualWorkArea(RegisterInfo info)
        {
            var hWnd = new WindowInteropHelper(info.Window).Handle;
            var cwa = GetMonitorWorkArea(Interop.MonitorFromWindow(hWnd, Interop.MonitorDefaultTo.MONITOR_DEFAULTTONEAREST));
            var wa = new Rect(new Point(cwa.left, cwa.top), new Point(cwa.right, cwa.bottom));

            if (info.DockedSize != null)
            {
                wa.Union(info.DockedSize.Value);
            }
            return wa;
        }

        private static Interop.RECT GetMonitorWorkArea(IntPtr hMonitor)
        {
            var monitorInfo = new Interop.MONITORINFO();
            monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);
            Interop.GetMonitorInfo(hMonitor, ref monitorInfo);
            return monitorInfo.rcWork;
        }
    }
    #endregion
}
