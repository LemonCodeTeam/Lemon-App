using LemonLibrary;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lemon_App
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
        public ImageForceView(List<IFVData> iFVData)
        {
            InitializeComponent();
            iv = iFVData;
        }
        public ImageForceView()
        {
            InitializeComponent();
            iv.Add(new IFVData("http://y.gtimg.cn/music/common/upload/MUSIC_FOCUS/1593418.jpg", ""));//哇
            iv.Add(new IFVData("http://y.gtimg.cn/music/common/upload/MUSIC_FOCUS/1585546.jpg", ""));//直播
            iv.Add(new IFVData("http://y.gtimg.cn/music/common/upload/t_mv_focus/1587025.jpg", ""));//GEM
            iv.Add(new IFVData("http://y.gtimg.cn/music/common/upload/MUSIC_FOCUS/1589996.jpg", ""));//EXO
            iv.Add(new IFVData("http://y.gtimg.cn/music/common/upload/MUSIC_FOCUS/1596362.jpg", ""));//胡夏
            iv.Add(new IFVData("http://y.gtimg.cn/music/common/upload/MUSIC_FOCUS/1594052.jpg", ""));//明月
            lastindex = iv.Count - 1;
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            M.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv[0].pic)) { Stretch = Stretch.Uniform };
            R.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv[1].pic)) { Stretch = Stretch.Uniform };
            L.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(iv.Last().pic)) { Stretch = Stretch.Uniform };
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

        public async void TurnRight() {
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
        private void Right_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (lastcheck == 0)
                TurnRight();
            TurnRight();
            lastcheck = 1;
            (Resources["TurnRight"] as Storyboard).Begin();
        }
    }

    public class IFVData {
        public IFVData(string image,string uri) {
            pic = image;
            url = uri;
        }
        public string pic;
        public string url;
    }
}
