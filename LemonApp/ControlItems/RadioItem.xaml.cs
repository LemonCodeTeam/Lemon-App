using LemonLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static LemonLib.InfoHelper;

namespace LemonApp
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
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
            data = da;
            this.name.Text = data.Name;
            Loaded += async delegate
            {
                Height = ActualWidth + 40;
                listenCount.Text = data.lstCount.IntToWn();
                im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(data.Photo, new int[2] { 200, 200 }));
            };
            MouseEnter += delegate {
                AddToQGT.Visibility = Visibility.Visible;
            };
            MouseLeave += delegate {
                AddToQGT.Visibility = Visibility.Collapsed;
            };
            AddToQGT.MouseDown += delegate {
                UIHelper.GetAncestor<MainWindow>(this).AddToQGT(new InfoHelper.QuickGoToData()
                {
                    type = "Radio",
                    id = data.ID,
                    name=data.Name,
                    imgurl = data.Photo
                });
            };
        }
        public RadioItem(string ID)
        {
            data.ID = ID;
        }

        private void UserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            Height = Height = ActualWidth + 40;
        }
    }
}
