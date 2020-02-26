using LemonLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
        private bool HasCheck = false;
        private MainWindow mw;
        public ImageForceView()
        {
            InitializeComponent();
            Image.MouseDown += PartMouseDown;
        }
        public void Updata(List<IFVData> iFVData, MainWindow m)
        {
            iv = iFVData;
            index = 0;
            lastindex = iv.Count - 1;
            mw = m;
            SetImageAsync(0);
        }
        private async void SetImageAsync(int index)
        {
            try
            {

                Image.Background = null;
                var ib = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv[index].pic, new int[2] { 363, 907 })) { Stretch = Stretch.Fill };
                Image.Background = ib;
                GC.Collect();
            }
            catch { }
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CheckAniLeft = Resources["CheckAniLeft"] as Storyboard;
            CheckAniRight = Resources["CheckAniRight"] as Storyboard;
            while (true)
            {
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
            string url = iv[index].url;
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
                else if (url.Contains("y.qq.com/topic/piaowu"))
                {
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
                mw.PlayMv(new MVData() { id = url, name = "" });
            }
            else Process.Start(url);
        }

        private Storyboard CheckAniLeft;
        private Storyboard CheckAniRight;
        private void TurnLeft()
        {
            if (index.Equals(0))
            {
                SetImageAsync(lastindex);
                index = lastindex;
            }
            else
            {
                index--;
                SetImageAsync(index);
            }
            Console.WriteLine("LEFT" + index);
            CheckAniLeft.Begin();
        }
        private void Left_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TurnLeft();
            HasCheck = true;
        }

        public void TurnRight()
        {
            try
            {
                if (index.Equals(lastindex))
                {
                    SetImageAsync(0);
                    index = 0;
                }
                else
                {
                    index++;
                    SetImageAsync(index);
                }
                Console.WriteLine("RIGHT:" + index);
                CheckAniRight.Begin();
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
