using System.Windows.Controls;
using System.Windows.Media;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// MVItem.xaml 的交互逻辑
    /// </summary>
    public partial class MVItem : UserControl
    {
        public MVItem()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
        }
        private MVData mData;
        public MVData Data
        {
            get => mData; set
            {
                mData = value;
                Update();
            }
        }
        private async void Update()
        {
            img.Background = new ImageBrush(await LemonLib.ImageCacheHelp.GetImageByUrl(mData.img, new int[2] { 150, 200 }));
            tit.Text = mData.name;
            bfCount.Text = mData.lstCount;
        }
    }
}
