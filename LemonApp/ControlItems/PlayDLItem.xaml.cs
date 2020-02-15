using LemonLib;
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
    /// PlayDLItem.xaml 的交互逻辑
    /// </summary>
    public partial class PlayDLItem : UserControl
    {
        public Music Data;
        public PlayDLItem(Music m)
        {
            InitializeComponent();
            Data = m;
            SingerName.Text = m.SingerText;
            MusicName.Text = m.MusicName;
        }
        public PlayDLItem(Music m,bool NeedImg,string imgUrl)
        {
            InitializeComponent();
            Loaded += async delegate {
                if (NeedImg)
                {
                    img.Visibility = Visibility.Visible;
                    SingerName.Margin = new Thickness(66, 32, 10, 0);
                    MusicName.Margin = new Thickness(66, 12, 10, 0);
                    img.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(imgUrl, new int[2] { 55,55 }));
                }
            };
            Data = m;
            SingerName.Text = m.SingerText;
            MusicName.Text = m.MusicName;
        }
        public void p(bool isp) {
            PD.LastPD.v(false);
            v(isp);
        }
        public bool pv;
        private void v(bool isp) {
            pv = isp;
            if (isp)
            {
                bk.Visibility = Visibility.Visible;
                MusicName.SetResourceReference(ForegroundProperty, "ThemeColor");
                SingerName.SetResourceReference(ForegroundProperty, "ThemeColor");

            }
            else
            {
                bk.Visibility = Visibility.Collapsed;
                MusicName.SetResourceReference(ForegroundProperty, "PlayDLPage_Font_Most");
                SingerName.SetResourceReference(ForegroundProperty, "PlayDLPage_Font_Low");
            }
            PD.LastPD = this;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Borderbg.Background = Resources["Touched"] as SolidColorBrush;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Borderbg.Background = null;
        }
    }

    public class PD {
        public static PlayDLItem LastPD = new PlayDLItem(new Music());
    }
}
