﻿using LemonLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lemon_App
{
    /// <summary>
    /// SingerItem.xaml 的交互逻辑
    /// </summary>
    public partial class SingerItem : UserControl
    {
        public string img { get; set; }
        public string singer { get; set; }
        public SingerItem(string ig, string sing)
        {
            try
            {
                InitializeComponent();
                img = ig;
                singer = sing;
                name.Text = singer;
                if (!File.Exists(InfoHelper.GetPath() + "Cache/Singer" + sing + ".jpg"))
                {
                    WebClient v = new WebClient();
                    v.DownloadFileAsync(new Uri(img), InfoHelper.GetPath() + "Cache/Singer" + sing + ".jpg");
                    v.DownloadFileCompleted += delegate
                    {
                        v.Dispose();
                        im.Background = new ImageBrush(new BitmapImage(new Uri(InfoHelper.GetPath() + "Cache/Singer" + sing + ".jpg", UriKind.Relative)));
                        var dt = new System.Drawing.Bitmap(InfoHelper.GetPath() + "Cache/Singer" + sing + ".jpg").GetMajorColor();
                        var color = Color.FromArgb(dt.A, dt.R, dt.G, dt.B);
                        back.Background = new SolidColorBrush(color);
                    };
                }
                else
                {
                    im.Background = new ImageBrush(new BitmapImage(new Uri(InfoHelper.GetPath() + "Cache/Singer" + sing + ".jpg", UriKind.Relative)));
                    var dt = new System.Drawing.Bitmap(InfoHelper.GetPath() + "Cache/Singer" + sing + ".jpg").GetMajorColor();
                    var color = Color.FromArgb(dt.A, dt.R, dt.G, dt.B);
                    back.Background = new SolidColorBrush(color);
                }
            }
            catch
            {
                if (!File.Exists(InfoHelper.GetPath() + "Cache/SingerNo.jpg"))
                {
                    WebClient v = new WebClient();
                    v.DownloadFileAsync(new Uri("https://y.gtimg.cn/mediastyle/global/img/singer_300.png?max_age=31536000"), InfoHelper.GetPath() + "Cache/SingerNo.jpg");
                    v.DownloadFileCompleted += delegate
                    {
                        im.Background = new ImageBrush(new BitmapImage(new Uri(InfoHelper.GetPath() + "Cache/SingerNo.jpg", UriKind.Relative)));
                        var dt = new System.Drawing.Bitmap(InfoHelper.GetPath() + "Cache/SingerNo.jpg").GetMajorColor();
                        var color = Color.FromArgb(dt.A, dt.R, dt.G, dt.B);
                        back.Background = new SolidColorBrush(color);
                    };
                }
                else
                {
                    im.Background = new ImageBrush(new BitmapImage(new Uri(InfoHelper.GetPath() + "Cache/SingerNo.jpg", UriKind.Relative)));
                    var dt = new System.Drawing.Bitmap(InfoHelper.GetPath() + "Cache/SingerNo.jpg").GetMajorColor();
                    var color = Color.FromArgb(dt.A, dt.R, dt.G, dt.B);
                    back.Background = new SolidColorBrush(color);
                }
            }
        }
    }
}