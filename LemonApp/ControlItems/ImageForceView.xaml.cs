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
            this.IsVisibleChanged += ImageForceView_IsVisibleChanged;
            SizeChanged += ImageForceView_SizeChanged;
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.LowQuality);
        }

        private void ImageForceView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Height = ActualWidth / 2;
        }

        private void ImageForceView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                t.Start();
            else t.Stop();
        }

        public async void Update(List<IFVData> iFVData, MainWindow m)
        {
            iv = iFVData;
            index = 0;
            lastindex = iv.Count - 1;
            mw = m;
            await SetImageAsync(0);
        }
        private async Task SetImageAsync(int index)
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
        private System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CheckAniLeft = Resources["CheckAniLeft"] as Storyboard;
            CheckAniRight = Resources["CheckAniRight"] as Storyboard;
            t.Interval = 5000;
            t.Tick += delegate
            {
                t.Interval = 5000;
                if (HasCheck)
                {
                    HasCheck = false;
                    t.Interval = 10000;
                }
                TurnRight();
            };
            t.Start();
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
                    string mid = TextHelper.FindTextByAB(url, "&mid=", "&g_f=yqqjiaodian", 0);
                    mw.IFVCALLBACK_LoadAlbum(mid);
                }
                else if (url.Contains("y.qq.com/topic/piaowu"))
                {
                    Process.Start("explorer", url);
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
        private async void TurnLeft()
        {
            if (index.Equals(0))
            {
                await SetImageAsync(lastindex);
                index = lastindex;
            }
            else
            {
                index--;
                await SetImageAsync(index);
            }
            CheckAniLeft.Begin();
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
                if (index.Equals(lastindex))
                {
                    await SetImageAsync(0);
                    index = 0;
                }
                else
                {
                    index++;
                    await SetImageAsync(index);
                }

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
