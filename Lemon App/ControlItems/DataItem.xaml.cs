using System.Windows.Controls;
using System.Windows.Media;
using static LemonLibrary.InfoHelper;

namespace Lemon_App
{
    /// <summary>
    /// DataItem.xaml 的交互逻辑
    /// </summary>
    public partial class DataItem : UserControl
    {
        public string ID { set; get; }
        public string SongName { set; get; }
        public string Singer { set; get; }
        public string Image { set; get; }
        public DataItem(Music dat)
        {
            try
            {
                InitializeComponent();
                ID = dat.MusicID;
                SongName = dat.MusicName;
                Singer = dat.Singer;
                Image = dat.ImageUrl;
                name.Text = SongName;
                ser.Text = Singer;
                mss.Text = dat.MusicName_Lyric;
            }
            catch { }
        }
        public void ShowDx() {
            if (He.LastItem != null)
            {
                mss.Opacity = 0.6;
                ser.Opacity = 0.8;
                He.LastItem.namss.Foreground = App.BaseApp.GetResuColorBrush();
                He.LastItem.ser.Foreground = App.BaseApp.GetResuColorBrush();
                He.LastItem.color.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
            var col = App.BaseApp.GetThemeColorBrush();
            ser.Foreground = col;
            namss.Foreground = col;
            color.Background = col;
            mss.Opacity = 1;
            ser.Opacity = 1;

            He.LastItem = this;
        }
        public bool isPlay(string name) {
            if (this.name.Text == name)
                return true;
            else return false;
        }
        private void userControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowDx();
        }
    }


    public class He
    {
        public static DataItem LastItem = null;
    }
}
