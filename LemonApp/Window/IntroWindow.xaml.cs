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
    /// IntroWindow.xaml 的交互逻辑
    /// </summary>
    public partial class IntroWindow : Window
    {
        public IntroWindow()
        {
            InitializeComponent();
        }
        public delegate void Finished();
        public event Finished FinishedEvent;
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        bool mod = true;//true : qq false : wy
        private void IntoGDPage_CloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        private void IntoGDPage_qqmod_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!mod)
            {
                mod = true;
                QPath_Bg.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF8C913"));
                QPath_Ic.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF02B053"));
                IntoGDPage_wymod.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB2B2B2"));
            }
        }

        private void IntoGDPage_wymod_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mod)
            {
                mod = false;
                QPath_Bg.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF1F1F1"));
                QPath_Ic.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB2B2B2"));
                IntoGDPage_wymod.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE72D2C"));
            }
        }
        private async void IntoGDPage_DrBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mod)
            {
                await MusicLib.AddGDILikeAsync(IntoGDPage_id.Text);
                TwMessageBox.Show("添加成功");
                FinishedEvent();
                Close();
            }
            else
            {
                IntoGDPage_main.Visibility = Visibility.Collapsed;
                IntoGDPage_loading.Visibility = Visibility.Visible;
                await MusicLib.GetGDbyWYAsync(IntoGDPage_id.Text,
                    (count) => { IntoGDPage_ps_jd.Maximum = count; },
                    (i, title) => { IntoGDPage_ps_jd.Value = i; IntoGDPage_ps_name.Text = title; },
                    () =>
                    {
                        FinishedEvent();
                        Close();
                    }); ;
            }
        }

    }
}
