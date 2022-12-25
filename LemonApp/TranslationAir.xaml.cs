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
    /// TranslationAir.xaml 的交互逻辑
    /// </summary>
    public partial class TranslationAir : Window
    {
        public TranslationAir()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void CloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private async void MDButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!String.IsNullOrEmpty(text.Text))
            {
                result.Text = "";
                result.Text = await TranslateAirHelper.GetSug(text.Text);
            }
        }
        MediaPlayer mp = new MediaPlayer();
        private async void SpeechButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!String.IsNullOrEmpty(text.Text)) {
                string path = await TranslateAirHelper.Speech(text.Text);
                mp.Open(new Uri(path, UriKind.Absolute));
                mp.Play();
            }
        }
        public void UpdateText(string tx) {
            text.Text = tx;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mp.MediaEnded += delegate {
                mp.Close();
            };

            var osVersion = Environment.OSVersion.Version;
            var windows10_1809 = new Version(10, 0, 17763);
            if (osVersion >= windows10_1809)
            {
                await Task.Delay(100);
                Background = null;
                if (App.BaseApp.ThemeColor == 0)
                {
                    WdBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180));
                    WindowAccentCompositor wac = new WindowAccentCompositor(this, (c) =>
                    {
                        Background = new SolidColorBrush(c);
                    });
                    wac.Color = Color.FromArgb(200, 255, 255, 255);
                    wac.IsEnabled = true;
                }
                else
                {
                    WdBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(32, 32, 32));
                    WindowAccentCompositor wac = new WindowAccentCompositor(this, (c) =>
                    {
                        Background = new SolidColorBrush(c);
                    });
                    wac.Color = Color.FromArgb(220, 0, 0, 0);
                    wac.IsEnabled = true;
                }
            }
        }
    }
}
