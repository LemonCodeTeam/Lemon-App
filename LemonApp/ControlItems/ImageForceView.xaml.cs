using LemonLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// ImageForceView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageForceView : UserControl
    {
        private List<IFVData> iv = new List<IFVData>();
        private int index = 0;
        private int lastindex = 0;//最后一个索引
        private bool HasCheck=false;
        private MainWindow mw;
        public ImageForceView()
        {
            InitializeComponent();
            Image.MouseDown += PartMouseDown;
        }
        public async void Updata(List<IFVData> iFVData,MainWindow m) {
            iv = iFVData;
            index = 0;
            lastindex = iv.Count - 1;
            mw = m;
            Image.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv[0].pic)) { Stretch = Stretch.Fill };
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            while (true) {
                await Task.Delay(5000);
                if (HasCheck)
                {
                    HasCheck = false;
                    await Task.Delay(10000);
                }
                TurnRight();
            }
        }

        private void PartMouseDown(object sender, MouseButtonEventArgs e)
        {
            string url= iv[index].url;
            string type = iv[index].type;
            // type:3002 活动 ？？？？
            if (type == "3002")
            {
                if (url.Contains("y.qq.com/m/digitalbum/gold/index.html"))
                {
                    //专辑售卖
                    string mid = TextHelper.XtoYGetTo(url, "&mid=", "&g_f=yqqjiaodian", 0);
                    mw.IFVCALLBACK_LoadAlbum(mid);
                }
                else if (url.Contains("y.qq.com/topic/piaowu")) {
                    Process.Start(url);
                }
            }
            else if (type == "10002")
            {
                // type:10002 专辑  我也表示很无奈╮(╯▽╰)╭
                mw.IFVCALLBACK_LoadAlbum(url);
            }
            else if (type == "10012")
            {
                mw.PlayMv(new MVData() { id = url,name="" });
            }
            else Process.Start(url);
        }

        private async void TurnLeft() {
            if (index == 0)
                index = lastindex;
            else
                index--;
            Console.WriteLine("LEFT" + index);
            if (index - 1 == -1)
                Image.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv[lastindex].pic)) { Stretch = Stretch.Fill};
            else Image.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv[index].pic)) { Stretch = Stretch.Fill };
            (Resources["CheckAniLeft"] as Storyboard).Begin();
        }
        private void Left_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TurnLeft();
            HasCheck = true;
        }

        public async void TurnRight()
        {
            try
            {
                if (index == lastindex)
                    index = 0;
                else
                    index++;
                Console.WriteLine("RIGHT:" + index);
                if (index + 1 > lastindex)
                    Image.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv[0].pic)) { Stretch = Stretch.Fill };
                else Image.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv[index].pic)) { Stretch = Stretch.Fill };
                (Resources["CheckAniRight"] as Storyboard).Begin();
            }
            catch { }
        }
        private void Right_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TurnRight();
            HasCheck = true;
        }
    }
}
