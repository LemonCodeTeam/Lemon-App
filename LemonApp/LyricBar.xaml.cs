using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using LemonLib;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using static LemonLib.WindowStyles;

namespace LemonApp
{
    /// <summary>
    /// LyricBar.xaml 的交互逻辑
    /// </summary>
    public partial class LyricBar : Window
    {
        public LyricBar()
        {
            InitializeComponent();
        }
        public int LyricFontSize { 
            get => (int)text.FontSize;
            set { int v=value; text.FontSize = v; Height = value+6; }
        }
        public void Update(string txt) {
            text.Text = txt.Replace("\r\n","   ");
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

            var osVersion = Environment.OSVersion.Version;
            var windows10_1809 = new Version(10, 0, 17763);
            if (osVersion >= windows10_1809)
            {
                if (App.BaseApp.ThemeColor == 0)
                {
                    WindowAccentCompositor wac = new WindowAccentCompositor(this, (c) =>
                    {
                        Background = new SolidColorBrush(c);
                    });
                    wac.enableBlurin = true;
                    wac.Color = Color.FromArgb(200, 255, 255, 255);
                    wac.IsEnabled = true;
                }
                else
                {
                    WindowAccentCompositor wac = new WindowAccentCompositor(this, (c) =>
                    {
                        Background = new SolidColorBrush(c);
                    });
                    wac.enableBlurin = true;
                    wac.Color = Color.FromArgb(220, 0, 0, 0);
                    wac.IsEnabled = true;
                }
            }
            else bg.Visibility = Visibility.Visible;
        }
    }
}
