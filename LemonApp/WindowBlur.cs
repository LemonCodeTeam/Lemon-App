using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using System.Windows.Media;
using LemonLib;

namespace LemonApp
{
    /// <summary>
    /// 为窗口提供模糊特效。
    /// </summary>
    public class WindowAccentCompositor
    {
        public bool DarkMode = true;

        private readonly Window _window;
        private bool _isEnabled;
        private int _blurColor;
        private Version osVersion;
        private Action<Color> NoneCallback;

        /// <summary>
        /// 创建 <see cref="WindowAccentCompositor"/> 的一个新实例。
        /// </summary>
        /// <param name="window"></param>
        /// <param name="enableBlurin">对ToolWindow启用模糊特效</param>
        /// <param name="noneCallback">没有可以的模糊特效</param>
        public WindowAccentCompositor(Window window, bool enableBlurin = false, Action<Color> noneCallback = null)
        {
            _window = window;
            _enableBlurin = enableBlurin;
            osVersion = Environment.OSVersion.Version;
            NoneCallback = noneCallback;
            DarkMode = Settings.USettings.Skin_FontColor == "White";
        }

        /// <summary>
        /// 获取或设置此窗口模糊特效是否生效的一个状态。
        /// 默认为 false，即不生效。
        /// </summary>
        [DefaultValue(false)]
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnIsEnabledChanged(value);
            }
        }
        private Color _color;
        /// <summary>
        /// 获取或设置此窗口模糊特效叠加的颜色。
        /// </summary>
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                _blurColor =value.R << 0 | value.G << 8 | value.B << 16 | value.A << 24;
            }
        }

        private void OnIsEnabledChanged(bool isEnabled)
        {
            Window window = _window;
            var handle = new WindowInteropHelper(window).EnsureHandle();
            Composite(handle, isEnabled);
        }

        /// <summary>
        /// 在win11下对ToolWindow窗口启用模糊特效
        /// </summary>
        private bool _enableBlurin = false;
        private void Composite(IntPtr handle, bool isEnabled)
        {
            // 操作系统版本判定。
            var windows10_1809 = new Version(10, 0, 17763);
            var windows10 = new Version(10, 0);
            var windows11 = new Version(10, 0, 22621);
            if (osVersion >= windows11 && !_enableBlurin)
            {
                if (!isEnabled)
                {
                    SetWindowBlur(handle, 0, BlurMode.None);
                    return;
                }
                //对于win11需要  其它默认1的边框
                WindowChrome.SetWindowChrome(_window, new WindowChrome()
                {
                    GlassFrameThickness = new Thickness(-1),
                    CaptionHeight = 1,
                    ResizeBorderThickness = new Thickness(5)
                });
                _window.Background = new SolidColorBrush(_color);
                SetWindowBlur(handle, DarkMode ? 1 : 0, BlurMode.Acrylic);
            }
            else
            {
                var accent = new AccentPolicy();
                if (!isEnabled)
                    accent.AccentState = AccentState.ACCENT_DISABLED;
                else if (osVersion >= windows10_1809)
                {
                    //1803能用但是兼容性不好哇----  1903完美支持 
                    accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
                    accent.GradientColor = _blurColor;
                }
                else if (osVersion >= windows10)
                {
                    accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
                }
                else
                {
                    NoneCallback?.Invoke(Color);
                    return;
                }

                var accentPolicySize = Marshal.SizeOf(accent);
                var accentPtr = Marshal.AllocHGlobal(accentPolicySize);
                Marshal.StructureToPtr(accent, accentPtr, false);

                try
                {
                    var data = new WindowCompositionAttributeData
                    {
                        Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                        SizeOfData = accentPolicySize,
                        Data = accentPtr,
                    };
                    SetWindowCompositionAttribute(handle, ref data);
                }
                finally
                {
                    Marshal.FreeHGlobal(accentPtr);
                }
            }
        }

        #region

        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private enum AccentState
        {
            /// <summary>
            /// 完全禁用 DWM 的叠加特效。
            /// </summary>
            ACCENT_DISABLED = 0,

            /// <summary>
            /// 
            /// </summary>
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_INVALID_STATE = 5,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        private enum WindowCompositionAttribute
        {
            // 省略其他未使用的字段
            WCA_ACCENT_POLICY = 19,
            // 省略其他未使用的字段
        }

        /// <summary>
        /// 当前操作系统支持的透明模糊特效级别。
        /// </summary>
        public enum BlurSupportedLevel
        {
            NotSupported,
            Aero,
            Blur,
            Acrylic,
        }


        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, int cbAttribute);

        public static int SetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, int parameter)
            => DwmSetWindowAttribute(hwnd, attribute, ref parameter, Marshal.SizeOf<int>());

        [Flags]
        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_SYSTEMBACKDROP_TYPE = 38
        }
        public enum BlurMode
        {
            None = 1,
            Acrylic = 3,
            Mica = 2,
            Tabbed = 4
        }
        /// <summary>
        /// 应用模糊特效 for Win11
        /// </summary>
        /// <param name="win"></param>
        /// <param name="color">1:Dark 0:Light</param>
        public static void SetWindowBlur(IntPtr handle, int color, BlurMode mode)
        {
            SetWindowAttribute(
                handle,
                DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                color);
            SetWindowAttribute(
                handle,
                DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE,
                (int)mode);
        }
        #endregion
    }
}