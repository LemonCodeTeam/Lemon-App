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
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LemonApp
{
    /// <summary>
    /// TransAirWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TransAirWindow : Window
    {
        public TransAirWindow()
        {
            InitializeComponent();Width = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (Resources["Open"] as Storyboard).Begin();
            WindowAccentCompositor wac = new WindowAccentCompositor(this, true, (c) =>
            {
                c.A = 255;
                Background = new SolidColorBrush(c);
            });
            wac.Color = App.BaseApp.ThemeColor == 0 ?
            Color.FromArgb(200, 255, 255, 255) :
            Color.FromArgb(200, 0, 0, 0);
            wac.IsEnabled = true;
        }

        private void TranslationPage_CloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            (Resources["Close"] as Storyboard).Begin();
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            Close();
        }
    }
}
