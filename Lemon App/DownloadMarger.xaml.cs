using LemonLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lemon_App
{
    /// <summary>
    /// DownloadMarger.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadMarger : Window
    {
        public DownloadMarger(UIElementCollection data)
        {
            InitializeComponent();
            filepath.Text = AppDomain.CurrentDomain.BaseDirectory+"Download";
            foreach (DataItem x in data)
                DownloadList.Children.Add(new CheckBox() {Width=370,Content= x.SongName + " - " + x.Singer, Uid=x.ID,FocusVisualStyle=null});
        }

        private async void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var data = new List<CheckBox>();
            foreach (var x in DownloadList.Children) {
                var f = x as CheckBox;
                if (f.IsChecked == true)
                    data.Add(f);
            }
            int index = 0;
            Msg msg = new Msg("正在下载全部歌曲(" + data.Count + ")");
            msg.Show();
            for (index = 0; index < data.Count; index++)
            {
                if (msg.IsClose)
                    break;
                var cl = new WebClient();
                string mid = data[index].Uid;
                string url = await new LemonLibrary.MusicLib().GetUrlAsync(mid);
                string name = data[index].Content.ToString();
                msg.tb.Text = "正在下载全部歌曲(" + data.Count + ")\n已完成:" + (index + 1) + "  " + name;
                string file = filepath.Text+$"\\{name}.mp3";
                cl.DownloadFileAsync(new Uri(url), file);
                cl.DownloadFileCompleted += delegate { cl.Dispose(); };
            }
            if (!msg.IsClose)
            {
                msg.tb.Text = "已完成.";
                await Task.Delay(5000);
                msg.tbclose();
            }
            else
            {
                await Task.Delay(2000);
                Msg msxg = new Msg("已取消下载");
                msxg.Show();
                await Task.Delay(5000);
                msxg.tbclose();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void CLOSE_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var d = sender as CheckBox;
            if (d.IsChecked == true)
            {
                d.Content = "全不选";
                foreach (CheckBox x in DownloadList.Children)
                    x.IsChecked = true;
            }
            else
            {
                d.Content = "全选";
                foreach (CheckBox x in DownloadList.Children)
                    x.IsChecked = false;
            }
        }

        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            var g = new System.Windows.Forms.FolderBrowserDialog();
            if (g.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                filepath.Text = g.SelectedPath;
        }
    }
}
