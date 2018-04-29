﻿using LemonLibrary;
using System.Windows;
using System.Windows.Media.Animation;

namespace Lemon_App
{
    /// <summary>
    /// Msg.xaml 的交互逻辑
    /// </summary>
    public partial class Msg : Window
    {
        public Msg(string tx)
        {
            InitializeComponent();
            tb.Text = tx;
            Left = SystemParameters.WorkArea.Width - Width;
            Top = SystemParameters.WorkArea.Height - Height + 10;
            if (Settings.USettings.skin == 0)
                (Resources["Skin"] as Storyboard).Begin();
            else
                Settings.USettings.skin = 0;
            (Resources["unSkin"] as Storyboard).Begin();
        }
        public void tbclose()
        {
            var t = Resources["Closed"] as Storyboard;
            t.Completed += delegate { Close(); };
            t.Begin();
        }
        public bool IsClose = false;
        private void border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsClose = true;
            tbclose();
        }
    }
}