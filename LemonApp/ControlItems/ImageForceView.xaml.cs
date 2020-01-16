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
        private int lastindex = 0;
        private int lastcheck = -1;//0:Left 1:Right
        private MainWindow mw;
        public ImageForceView()
        {
            InitializeComponent();
            M.MouseDown += PartMouseDown;
            R.MouseDown += PartMouseDown;
            L.MouseDown += PartMouseDown;
        }
        public async void Updata(List<IFVData> iFVData,MainWindow m) {
            iv = iFVData;
            index = 0;
            lastindex = iv.Count - 1;
            mw = m;
            M.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv[0].pic)) { Stretch = Stretch.Uniform };
            R.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv[1].pic)) { Stretch = Stretch.Uniform };
            L.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv.Last().pic)) { Stretch = Stretch.Uniform };
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            while (true) {
                if (lastcheck != -1)
                {
                    lastcheck = -1;
                    await Task.Delay(10000);
                }
                TurnRight();
                (Resources["TurnRight"] as Storyboard).Begin();
                await Task.Delay(5000);
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
            Border leftb = new Border(), mid = new Border(), righb = new Border();
            foreach (Border bd in mj.Children)
            {
                //位于左边的
                if (bd.Margin.Equals(new Thickness(0, 0, 350, 0)))
                    leftb = bd;
                //位于中间的
                else if (bd.Margin.Equals(new Thickness(125, 0, 125, 0)))
                    mid = bd;
                //位于右边的
                else if (bd.Margin.Equals(new Thickness(350, 0, 0, 0)))
                    righb = bd;
            }
            righb.Background = mid.Background;
            mid.Background = leftb.Background;
            if (index - 1 == -1)
                leftb.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv[lastindex].pic)) { Stretch = Stretch.Uniform };
            else leftb.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv[index - 1].pic)) { Stretch = Stretch.Uniform };
        }
        private void Left_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (lastcheck == 1)
                TurnLeft();
            TurnLeft();
            lastcheck = 0;
            (Resources["TurnLeft"] as Storyboard).Begin();
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
                Border leftb = new Border(), mid = new Border(), righb = new Border();
                foreach (Border bd in mj.Children)
                {
                    //位于左边的
                    if (bd.Margin.Equals(new Thickness(0, 0, 350, 0)))
                        leftb = bd;
                    //位于中间的
                    else if (bd.Margin.Equals(new Thickness(125, 0, 125, 0)))
                        mid = bd;
                    //位于右边的
                    else if (bd.Margin.Equals(new Thickness(350, 0, 0, 0)))
                        righb = bd;
                }
                leftb.Background = mid.Background;
                mid.Background = righb.Background;
                if (index + 1 > lastindex)
                    righb.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv[0].pic)) { Stretch = Stretch.Uniform };
                else righb.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv[index + 1].pic)) { Stretch = Stretch.Uniform };
            }
            catch { }
        }
        private void Right_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (lastcheck == 0)
                TurnRight();
            TurnRight();
            lastcheck = 1;
            (Resources["TurnRight"] as Storyboard).Begin();
        }
    }
}
