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
        bool ns = false;
        public bool isChecked = false;
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
        public void NSDownload(bool ns) {
            this.ns = ns;
            if (ns)
            {
                namss.Margin = new System.Windows.Thickness(60, 0, 10, 0);
                CheckView.Visibility = System.Windows.Visibility.Visible;
                MouseDown += CheckView_MouseDown;
            }
            else {
                namss.Margin = new System.Windows.Thickness(10, 0, 10, 0);
                CheckView.Visibility = System.Windows.Visibility.Collapsed;
                MouseDown -= CheckView_MouseDown;
            }
        }

        private void CheckView_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Check();
        }

        public void Check() {
            if (!isChecked)
            {
                GO.Visibility = System.Windows.Visibility.Visible;
                CheckView.SetResourceReference(BorderBrushProperty, "ThemeColor");
                isChecked = true;
            }
            else {
                GO.Visibility = System.Windows.Visibility.Collapsed;
                CheckView.SetResourceReference(BorderBrushProperty, "TextX1ColorBrush");
                isChecked = false;
            }
        }
        private void userControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(!ns)
              ShowDx();
        }
    }


    public class He
    {
        public static DataItem LastItem = null;
    }
}
