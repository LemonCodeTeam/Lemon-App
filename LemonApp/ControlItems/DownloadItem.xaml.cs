using LemonLib;
using System;
using System.Windows;
using System.Windows.Controls;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// DownloadItem.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadItem : UserControl
    {
        public delegate void en(DownloadItem s);
        public event en Delete;
        public event en Finished;
        public bool finished = false;
        private int index = 0;
        public DownloadItem(Music m,string downloadpath,int id)
        {
            InitializeComponent();
            MData = m;
            path = downloadpath;
            MouseEnter += UserControl_MouseEnter;
            MouseLeave += UserControl_MouseLeave;
            index = id;
            Load();
        }
        private async void Load()
        {
            tb.Text = MData.MusicName + " - " + MData.SingerText;
            d = new HttpDownloadHelper(MData.MusicID, path);
            d.ProgressChanged += (pro) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Pb.Value = pro;
                    zt.Text = pro + "%";
                });
            };
            d.Finished += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    Finished(this);
                    finished = true;
                    zt.Text = "已完成";
                });
            };
            d.GetSize += (s) =>
            {
                Dispatcher.Invoke(() =>
                {
                    size.Text = s;
                });
            };
            if (index==0)
                d.Download();
            if (Settings.USettings.DownloadWithLyric)
                await MusicLib.GetLyric(MData.MusicID, path.Replace(".mp3", ".lrc"));
        }
        public HttpDownloadHelper d;
        public Music MData;
        public string path;

        private void Pb_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (zt.Text != "等待下载")
            {
                if (d.Downloading)
                {
                    d.Pause();
                    zt.Text = "已暂停";
                }
                else
                {
                    d.Start();
                    zt.Text = "已开始";
                }
            }
        }

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DeleteBtn.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DeleteBtn.Visibility = Visibility.Collapsed;
        }

        private void DeleteBtn_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Finished(this);
            Delete(this);
        }
    }
}
