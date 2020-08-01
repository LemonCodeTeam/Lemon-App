using LemonLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// TopControl.xaml 的交互逻辑
    /// </summary>
    public partial class TopControl : UserControl
    {
        public MusicTop Data;
        public TopControl(MusicTop mp)
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
            Loaded += TopControl_Loaded;
            Data = mp;
        }

        private async void TopControl_Loaded(object sender, RoutedEventArgs e)
        {
            im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(Data.Photo, new int[2] { 127, 127 }));
            title.Text = Data.Name;
            c1.Text = Data.content[0];
            c2.Text = Data.content[1];
            c3.Text = Data.content[2];
            MouseEnter += delegate {
                AddToQGT.Visibility = Visibility.Visible;
            };
            MouseLeave += delegate {
                AddToQGT.Visibility = Visibility.Collapsed;
            };
            AddToQGT.MouseDown += delegate {
                UIHelper.GetAncestor<MainWindow>(this).AddToQGT(new InfoHelper.QuickGoToData()
                {
                    type = "TopList",
                    id = Data.ID,
                    name=Data.Name,
                    imgurl = Data.Photo,
                    data=Data
                });
            };
        }
    }
}