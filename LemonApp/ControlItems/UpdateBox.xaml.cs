using LemonLib;
using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LemonApp
{
    /// <summary>
    /// MyMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateBox : Window
    {
        public UpdateBox(string ver, string des)
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
            Height = 210;
            var xpath = Settings.USettings.DataCachePath + "win-release.zip";
            WebClient wb = new WebClient();
            wb.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            wb.Headers.Add("accept-language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            wb.Headers.Add("cache-control", "no-cache");
            wb.Headers.Add("pragma", "no-cache");
            wb.Headers.Add("sec-fetch-dest", "document");
            wb.Headers.Add("sec-fetch-mode", "navigate");
            wb.Headers.Add("sec-fetch-site", "none");
            wb.Headers.Add("sec-fetch-user", "?1");
            wb.Headers.Add("upgrade-insecure-requests", "1");
            wb.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36 Edg/84.0.522.44");
            wb.DownloadProgressChanged += (o, ex) =>
            {
                pro.Value = ex.ProgressPercentage;
            };
            wb.DownloadFileCompleted += async delegate
            {
                tb.Text = "下载完成，解压升级包...";
                string file = Settings.USettings.DataCachePath + "win-release.exe";
                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory(xpath, Settings.USettings.DataCachePath, true);
                });
                tb.Text = "完成";
                Process.Start("explorer.exe", Settings.USettings.DataCachePath + "win-release.exe");
            };
            wb.DownloadFileAsync(new Uri("https://files-cdn.cnblogs.com/files/TwilightLemon/win-release.zip"), xpath);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
