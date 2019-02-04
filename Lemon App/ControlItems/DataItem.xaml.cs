using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static LemonLibrary.InfoHelper;

namespace Lemon_App
{
    /// <summary>
    /// DataItem.xaml 的交互逻辑
    /// </summary>
    public partial class DataItem : UserControl
    {
        public delegate void MouseDownHandle(object sender, MouseButtonEventArgs e);
        public event MouseDownHandle Play;
        public event MouseDownHandle Add;
        public event MouseDownHandle Download;


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
                He.LastItem.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                He.LastItem.namss.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                He.LastItem.ser.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                He.LastItem.color.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
            Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0));
            ser.SetResourceReference(ForegroundProperty, "ThemeColor");
            namss.SetResourceReference(ForegroundProperty, "ThemeColor");
            color.SetResourceReference(BackgroundProperty, "ThemeColor");
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

        private void PlayBtn_MouseDown(object sender, MouseButtonEventArgs e) => Play(this, e);

        private void AddBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Add(this, e);
            if (!ns)
                ShowDx();
        }

        private void DownloadBtn_MouseDown(object sender, MouseButtonEventArgs e) => Download(this, e);
    }


    public class He
    {
        public static DataItem LastItem = null;
    }
}
