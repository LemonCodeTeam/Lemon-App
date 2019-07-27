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
        public static void G(FrameworkElement d)
        {
            d.FocusVisualStyle = null;
            RenderOptions.SetBitmapScalingMode(d, BitmapScalingMode.Fant);
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
        public static bool IsVerticalScrollBarAtButtom(this ScrollViewer ex)
        {
            bool isAtButtom = false;
            double dVer = ex.VerticalOffset;
            double dViewport = ex.ViewportHeight;
            double dExtent = ex.ExtentHeight;
            if (dVer != 0){
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
    }

    public class CusPopup : Popup
    {
        /// <summary>
        /// 应用状态
        /// </summary>
        private bool? _appliedTopMost;
        /// <summary>
        /// 是否已经加载
        /// </summary>
        private bool _alreadyLoaded;
        /// <summary>
        /// popup所在的窗体
        /// </summary>
        private Window _parentWindow;

        /// <summary>
        /// 是否顶置
        /// </summary>
        public bool IsTopmost
        {
            get { return (bool)GetValue(IsTopmostProperty); }
            set { SetValue(IsTopmostProperty, value); }
        }
        /// <summary>
        /// 是否顶置依赖属性（默认不顶置）
        /// </summary>
        public static readonly DependencyProperty IsTopmostProperty = DependencyProperty.Register("IsTopmost", typeof(bool), typeof(CusPopup), new FrameworkPropertyMetadata(false, OnIsTopmostChanged));

        /// <summary>
        /// 是否跟随父窗体移动
        /// </summary>
        public bool IsMove
        {
            get { return (bool)GetValue(IsMoveProperty); }
            set { SetValue(IsMoveProperty, value); }
        }
        public static readonly DependencyProperty IsMoveProperty =
            DependencyProperty.Register("IsMove", typeof(bool), typeof(CusPopup), new PropertyMetadata(false));


        /// <summary>
        /// 构造函数
        /// </summary>
        public CusPopup()
        {
            Loaded += OnPopupLoaded;
            Unloaded += OnPopupUnloaded;
        }

        /// <summary>
        /// popup加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnPopupLoaded(object sender, RoutedEventArgs e)
        {
            if (_alreadyLoaded)
                return;

            _alreadyLoaded = true;

            if (Child != null)
            {
                Child.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnChildPreviewMouseLeftButtonDown), true);
            }

            _parentWindow = Window.GetWindow(this);
            if (IsMove)
            {
                _parentWindow.LocationChanged += delegate
                {
                    var offset = this.HorizontalOffset;
                    this.HorizontalOffset = offset + 1;
                    this.HorizontalOffset = offset;
                };
                _parentWindow.SizeChanged += delegate
                {
                    var offset = this.HorizontalOffset;
                    this.HorizontalOffset = offset + 1;
                    this.HorizontalOffset = offset;
                };
            }

            if (_parentWindow == null)
                return;

            _parentWindow.Activated += OnParentWindowActivated;
            _parentWindow.Deactivated += OnParentWindowDeactivated;
        }

        private void OnPopupUnloaded(object sender, RoutedEventArgs e)
        {
            if (_parentWindow == null)
                return;
            _parentWindow.Activated -= OnParentWindowActivated;
            _parentWindow.Deactivated -= OnParentWindowDeactivated;
        }
        /// <summary>
        /// 主窗体激活事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnParentWindowActivated(object sender, EventArgs e)
        {
            Debug.WriteLine("Parent Window Activated");
            SetTopmostState(true);
        }
        /// <summary>
        /// 主窗体不在激活状态事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnParentWindowDeactivated(object sender, EventArgs e)
        {
            if (IsTopmost == false)
            {
                SetTopmostState(IsTopmost);
            }
        }
        /// <summary>
        /// 子元素的鼠标左键按下事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnChildPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SetTopmostState(true);

            if (!_parentWindow.IsActive && IsTopmost == false)
            {
                _parentWindow.Activate();
            }
        }
        /// <summary>
        /// IsTopmost属性改变事件
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private static void OnIsTopmostChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var thisobj = (CusPopup)obj;

            thisobj.SetTopmostState(thisobj.IsTopmost);
        }
        /// <summary>
        /// 重写open事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnOpened(EventArgs e)
        {
            //设置状态
            SetTopmostState(IsTopmost);
            base.OnOpened(e);
        }
        /// <summary>
        /// 设置置顶状态
        /// </summary>
        /// <param name="isTop"></param>
        private void SetTopmostState(bool isTop)
        {
            // 如果状态与输入状态相同，则不要应用状态
            if (_appliedTopMost.HasValue && _appliedTopMost == isTop)
            {
                return;
            }

            if (Child == null)
                return;

            var hwndSource = (PresentationSource.FromVisual(Child)) as HwndSource;

            if (hwndSource == null)
                return;
            var hwnd = hwndSource.Handle;

            RECT rect;

            if (!GetWindowRect(hwnd, out rect))
                return;

            Debug.WriteLine("setting z-order " + isTop);

            if (isTop)
            {
                SetWindowPos(hwnd, HWND_TOPMOST, rect.Left, rect.Top, (int)Width, (int)Height, TOPMOST_FLAGS);
            }
            else
            {
                /*
                 z顺序只会在点击时得到刷新/反射
                 标题栏（与外部的其他部分相对比窗口）除非我先将弹出窗口设置为hwndbottom
                 然后HWND_TOP HWND_NOTOPMOST之前
                 */
                SetWindowPos(hwnd, HWND_BOTTOM, rect.Left, rect.Top, (int)Width, (int)Height, TOPMOST_FLAGS);
                SetWindowPos(hwnd, HWND_TOP, rect.Left, rect.Top, (int)Width, (int)Height, TOPMOST_FLAGS);
                SetWindowPos(hwnd, HWND_NOTOPMOST, rect.Left, rect.Top, (int)Width, (int)Height, TOPMOST_FLAGS);
            }

            _appliedTopMost = isTop;
        }

        #region P / Invoke 入口和定义
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT

        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X,
        int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        private const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_NOZORDER = 0x0004;
        const UInt32 SWP_NOREDRAW = 0x0008;
        const UInt32 SWP_NOACTIVATE = 0x0010;

        const UInt32 SWP_FRAMECHANGED = 0x0020; /* The frame changed: send WM_NCCALCSIZE */
        const UInt32 SWP_SHOWWINDOW = 0x0040;
        const UInt32 SWP_HIDEWINDOW = 0x0080;
        const UInt32 SWP_NOCOPYBITS = 0x0100;
        const UInt32 SWP_NOOWNERZORDER = 0x0200; /* Don’t do owner Z ordering */
        const UInt32 SWP_NOSENDCHANGING = 0x0400; /* Don’t send WM_WINDOWPOSCHANGING */

        const UInt32 TOPMOST_FLAGS =
            SWP_NOACTIVATE | SWP_NOOWNERZORDER | SWP_NOSIZE | SWP_NOMOVE | SWP_NOREDRAW | SWP_NOSENDCHANGING;
        #endregion
    }
}
