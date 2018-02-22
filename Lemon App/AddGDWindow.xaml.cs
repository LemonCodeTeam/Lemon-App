using LemonLibrary;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lemon_App
{
    /// <summary>
    /// AddGDWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddGDWindow : Window
    {
        public AddGDWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += delegate (object sender, MouseButtonEventArgs e) {
                if (e.LeftButton == MouseButtonState.Pressed)
                    this.DragMove();
            };
        }
        bool mod = true;//true : qq false : wy
        private void qq_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!mod)
            {
                qq.Effect = new DropShadowEffect() { BlurRadius = 10, Opacity = 0.4, ShadowDepth = 0 };
                wy.Effect = null;
                mod = true;
            }
        }

        private void wy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mod) {
                qq.Effect = null;
                wy.Effect = new DropShadowEffect() { BlurRadius = 10, Opacity = 0.4, ShadowDepth = 0 };
                mod = false;
            }
        }
        MusicLib ml = new MusicLib();
        private async void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mod)
            {
                if (!Settings.USettings.MusicGD.ContainsKey(id.Text))
                    Settings.USettings.MusicGD.Add(id.Text, await ml.GetGDAsync(id.Text));
            }
            else
            {
                if (!Settings.USettings.MusicGD.ContainsKey(id.Text))
                    Settings.USettings.MusicGD.Add(id.Text, await ml.GetGDbyWYAsync(id.Text));
            }
            Close();
        }
    }
}
