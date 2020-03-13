using LemonLib;
using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LemonApp
{
    /// <summary>
    /// MyMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class UpdataBox : Window
    {
        public UpdataBox(string ver, string des)
        {
            InitializeComponent();
            con.Text = "最新版:" + ver + "\r\n" +
                des.Replace("#", "\r\n#");
        }

        private void CloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            con.Visibility = Visibility.Collapsed;
            btn.Visibility = Visibility.Collapsed;
            pro.Visibility = Visibility.Visible;
            tb.Text = "下载升级包中...";
            var xpath = Settings.USettings.CachePath + "win-release.zip";
            WebClient wb = new WebClient();
            wb.DownloadProgressChanged += (o, ex) =>
            {
                pro.Value = ex.ProgressPercentage;
            };
            wb.DownloadFileCompleted += async delegate
            {
                tb.Text = "下载完成，解压升级包...";
                string file = Settings.USettings.CachePath + "win-release.exe";
                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory(xpath, Settings.USettings.CachePath, true);
                });
                tb.Text = "完成";
                Process.Start("explorer.exe", Settings.USettings.CachePath + "win-release.exe");
            };
            wb.DownloadFileAsync(new Uri("https://files.cnblogs.com/files/TwilightLemon/win-release.zip"), xpath);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
