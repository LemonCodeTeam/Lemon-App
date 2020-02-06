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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// MusicListView.xaml 的交互逻辑
    /// </summary>
    public partial class MusicListView : UserControl
    {
        public MainWindow mw;
        private NowPage np;
        public MusicListView(List<Music> data,MainWindow m,NowPage n)
        {
            InitializeComponent();
            mw = m;
            np = n;
            Loaded += delegate {
                CreatItems(data);
            };
        }

        public void CreatItems(List<Music> Data) {
            foreach (var c in Data)
            {
                var k = new DataItem(c,mw) { Width = ActualWidth };
                if (k.music.MusicID == mw.MusicData.Data.MusicID)
                {
                    k.ShowDx();
                }
                k.GetToSingerPage += mw.K_GetToSingerPage;
                k.Play += K_Play;
                k.Download += mw.K_Download;
                if (DownloadMode) {
                    k.Play -= K_Play;
                    k.NSDownload(true);
                    k.Check(true);
                }
                MusicItemsList.Items.Add(k);
            }
        }

        private void K_Play(DataItem sender)
        {
            mw.np = np;
            mw.PushPlayMusic(sender, MusicItemsList);
        }

        private void ckFile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var g = new System.Windows.Forms.FolderBrowserDialog();
            if (g.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Download_Path.Text = g.SelectedPath;
                Settings.USettings.DownloadPath = g.SelectedPath;
            }
        }

        private void cb_color_Click(object sender, RoutedEventArgs e)
        {
            var d = sender as CheckBox;
            if (d.IsChecked == true)
            {
                d.Content = "全不选";
                foreach (DataItem x in MusicItemsList.Items)
                { x.isChecked = false; x.Check(true); }
            }
            else
            {
                d.Content = "全选";
                foreach (DataItem x in MusicItemsList.Items)
                { x.isChecked = true; x.Check(false); }
            }
        }

        private void DataPlayAllBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mw.DLMode = false;
            mw.PlayDL_List.Items.Clear();
            foreach (DataItem ex in MusicItemsList.Items)
            {
                var k = new PlayDLItem(ex.music);
                k.MouseDoubleClick += mw.K_MouseDoubleClick;
                mw.PlayDL_List.Items.Add(k);
            }
            PlayDLItem dk = mw.PlayDL_List.Items[0] as PlayDLItem;
            dk.p(true);
            var n = MusicItemsList.Items[0] as DataItem;
            mw.MusicData = dk;
            mw.PlayMusic(n);
        }
        bool DownloadMode = false;
        private void DataDownloadBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DownloadMode = true;
            Download_Path.Text = Settings.USettings.DownloadPath;
            sp.Visibility = Visibility.Collapsed;
            dp.Visibility = Visibility.Visible;
            DownloadQx.IsChecked = true;
            DownloadQx.Content = "全不选";
            foreach (DataItem x in MusicItemsList.Items)
            {
                x.Play -= K_Play;
                x.NSDownload(true);
                x.Check(true);
            }
        }

        private void DataDownloadBtn_Back_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DownloadMode = false;
            dp.Visibility = Visibility.Collapsed;
            sp.Visibility = Visibility.Visible;
            foreach (DataItem x in MusicItemsList.Items)
            {
                x.Play += K_Play;
                x.NSDownload(false);
                x.Check(false);
            }
        }

        private void DownloadBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mw.PushDownload(MusicItemsList);
            DataDownloadBtn_Back_MouseDown(null, null);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (FrameworkElement c in MusicItemsList.Items)
                c.Width = ActualWidth;
        }
    }
}
