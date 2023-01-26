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
    public partial class TranslationAir : UserControl
    {
        public TranslationAir()
        {
            InitializeComponent();
            this.Loaded += delegate {
                mp.MediaEnded += delegate{ mp.Close(); };
            };
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
    }
}
