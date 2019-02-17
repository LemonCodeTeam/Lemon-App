using LemonLibrary;
using System;
using System.Windows;
using System.Windows.Controls;
using static LemonLibrary.InfoHelper;

namespace Lemon_App
{
    /// <summary>
    /// DownloadItem.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadItem : UserControl
    {
        public delegate void fx();
        public event fx Loadedd;
        public delegate void en(DownloadItem s);
        public event en Delete;
        public int index = 0;
        public bool finished = false;
        public DownloadItem(Music m,string downloadpath,int ind)
        {
            InitializeComponent();
            MData = m;
            path = downloadpath;
            index = ind;
            Console.WriteLine(m.MusicName+"   "+ind);
            Loaded += DownloadItem_Loaded;
        }
        private void DownloadItem_Loaded(object sender, RoutedEventArgs e)
        {
            tb.Text = MData.MusicName + " - " + MData.Singer;
            d = new HttpDownloadHelper(MData.MusicID, path);
            Loadedd();
            d.ProgressChanged += (pro) =>
            {
                Dispatcher.Invoke(() =>
                {
                    finished = true;
                    Pb.Value = pro;
                    zt.Text = pro + "%";
                });
            };
            d.Finished += () =>
            {
                Dispatcher.Invoke(() =>
                {
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
            if (index == 0||index==1||index==2)
                d.Download();
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
            Delete(this);
        }
    }
}
