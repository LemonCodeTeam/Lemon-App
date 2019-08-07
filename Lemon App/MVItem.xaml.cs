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
using static LemonLibrary.InfoHelper;

namespace Lemon_App
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
                img.Background = new ImageBrush(new BitmapImage(new Uri(mData.img)));
                tit.Text = mData.name;
                bfCount.Text = mData.lstCount;
            } }
    }
}
