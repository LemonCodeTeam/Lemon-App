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
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        }
        private MVData mData;
        public MVData Data { get => mData; set {
                mData = value;
                Updata();
            } }
        private async void Updata() {
            img.Background = new ImageBrush(await LemonLib.ImageCacheHelp.GetImageByUrl(mData.img, new int[2] { 100, 150 }));
            tit.Text = mData.name;
            bfCount.Text = mData.lstCount;
        }
    }
}
