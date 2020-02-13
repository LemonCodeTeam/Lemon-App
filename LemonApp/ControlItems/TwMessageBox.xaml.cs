using LemonLib;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LemonApp
{
    /// <summary>
    /// TwMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class TwMessageBox : Window
    {
        public static bool Show(string tb) {
            return (bool)new TwMessageBox(tb).ShowDialog();
        }


        public TwMessageBox(string tb)
        {
            InitializeComponent();
            title.Text = tb;
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(100);
            if (App.BaseApp.ThemeColor==0)
            {
                WdBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180));
                title.Foreground = new SolidColorBrush(Color.FromRgb(75, 75, 75));
                candle.Theme = 0;
                WindowAccentCompositor wac = new WindowAccentCompositor(this,(c)=> {
                    Background = new SolidColorBrush(c);
                });
                wac.Color = Color.FromArgb(200, 255, 255, 255);
                wac.IsEnabled = true;
            }
            else {
                WdBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(32, 32, 32));
                title.Foreground = new SolidColorBrush(Color.FromRgb(234,234,234));
                candle.Theme = 1;
                WindowAccentCompositor wac = new WindowAccentCompositor(this, (c) => {
                    Background = new SolidColorBrush(c);
                }); 
                wac.Color = Color.FromArgb(220, 0, 0, 0);
                wac.IsEnabled = true;
            }
        }
    }
}
