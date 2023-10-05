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
            Closing += delegate {
                AppBarFunctions.SetAppBar(this, ABEdge.None);
            };
        }
        public int LyricFontSize { 
            get => (int)text.FontSize;
            set { int v=value; text.FontSize = v; Height = value+8; }
        }
        public void Update(string txt) {
            text.Text = txt.Replace("\r\n","   ");
        }
        private WindowAccentCompositor wac = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppBarFunctions.SetAppBar(this, ABEdge.Top);

            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);


            wac = new WindowAccentCompositor(this,true, (c) =>
            {
                c.A = 255;
                Background = new SolidColorBrush(c);
            });
            wac.Color = App.BaseApp.ThemeColor == 0 ?
            Color.FromArgb(200, 255, 255, 255) :
            Color.FromArgb(200, 0, 0, 0);
            wac.IsEnabled = true;
        }
        public void UpdataWindowBlurMode(bool darkmode) {
            wac.Color = App.BaseApp.ThemeColor == 0 ?
            Color.FromArgb(200, 255, 255, 255) :
            Color.FromArgb(200, 0, 0, 0);
            wac.DarkMode = darkmode;
            wac.IsEnabled= true;
        }
        public Action PlayLast,Play,PlayNext,PopOutEvent;
        private void PlayControl_PlayLast(object sender, TouchEventArgs e)
        {
            PlayLast?.Invoke();
        }

        private void PopOut_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PopOutEvent?.Invoke();
        }

        private void PlayBtn_TouchDown(object sender, TouchEventArgs e)
        {
            Play?.Invoke();
        }

        private void PlayControl_PlayNext(object sender, TouchEventArgs e)
        {
            PlayNext?.Invoke();
        }
    }
}
