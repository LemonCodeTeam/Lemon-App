using LemonLib;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// SingerItem.xaml 的交互逻辑
    /// </summary>
    public partial class SingerItem : UserControl
    {
        public MusicSinger data { get; set; }
        public SingerItem(MusicSinger dt)
        {
            InitializeComponent();
            data = dt;
            name.Text = dt.Name;
            Loaded += async delegate
            {
                Height = Height = ActualWidth + 45;
                try
                {
                    im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(dt.Photo, new int[2] { 200, 200 }));
                }
                catch
                {
                    im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl("https://y.gtimg.cn/mediastyle/global/img/singer_300.png?max_age=31536000", new int[2] { 200, 200 }));
                }
            };
        }
        public MusicSinger mData
        {
            get => data; set
            {
                data = value;
                new Action(async () =>
                {
                    try
                    {
                        im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(data.Photo, new int[2] { 200, 200 }));
                    }
                    catch
                    {
                        im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl("https://y.gtimg.cn/mediastyle/global/img/singer_300.png?max_age=31536000", new int[2] { 200, 200 }));
                    }
                })();
            }
        }
        public SingerItem()
        {
            InitializeComponent();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Height = Height = ActualWidth + 45;
        }
    }
}
