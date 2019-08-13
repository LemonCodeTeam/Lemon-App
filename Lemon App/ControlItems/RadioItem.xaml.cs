using LemonLibrary;
using System;
using System.IO;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media;
using static LemonLibrary.InfoHelper;

namespace Lemon_App
{
    /// <summary>
    /// RadioItem.xaml 的交互逻辑
    /// </summary>
    public partial class RadioItem : UserControl
    {
        public MusicRadioListItem data = new MusicRadioListItem();
        public RadioItem(MusicRadioListItem da)
        {
            InitializeComponent();
            data = da;
            this.name.Text = data.Name;
            Loaded += async delegate {
                Height = ActualWidth + 40;
                listenCount.Text = data.lstCount.IntToWn();
                im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(data.Photo));
            };
        }
        public RadioItem(string ID) {
            data.ID = ID;
        }

        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            Height = Height = ActualWidth + 40;
        }
    }
}
