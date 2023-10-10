using LemonLib;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
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
        string _url;
        public UpdateBox(string ver, string des,string url)
        {
            InitializeComponent();
            _url = url;
            con.Text = "最新版:" + ver + "\r\n" +
                des.Replace("#", "\r\n#");
        }

        private void CloseBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private async void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            con.Visibility = Visibility.Collapsed;
            btn.Visibility = Visibility.Collapsed;
            pro.Visibility = Visibility.Visible;
            tb.Text = "下载升级包中...";
            Height = 210;
            var xpath = Settings.USettings.DataCachePath + "win-release.zip";
            using var hc = new HttpClient(HttpHelper.GetSta());
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9\"");
           hc.DefaultRequestHeaders.TryAddWithoutValidation("accept-language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
           hc.DefaultRequestHeaders.TryAddWithoutValidation("cache-control", "no-cache");
           hc.DefaultRequestHeaders.TryAddWithoutValidation("pragma", "no-cache");
           hc.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-dest", "document");
           hc.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-mode", "navigate");
           hc.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-site", "none");
           hc.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-user", "?1");
           hc.DefaultRequestHeaders.TryAddWithoutValidation("upgrade-insecure-requests", "1");
           hc.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36 Edg/84.0.522.44");
            tb.Text = "连接至服务器...";
            var response =await hc.GetAsync(_url);
            using Stream st = await response.Content.ReadAsStreamAsync();
            var length = (long)response.Content.Headers.ContentLength;
            int total = 0;
            using (var filestream = new FileStream(xpath, FileMode.Create,FileAccess.ReadWrite))
            {
                byte[] bArry=new byte[409600];
                int size = await st.ReadAsync(bArry);
                total += size;
                while(size>0)
                {
                    await filestream.WriteAsync(bArry.AsMemory(0, size));
                    size = await st.ReadAsync(bArry);
                    total += size;
                    int process = (int)(total*100/ length);
                    pro.Value = process;
                }
                filestream.Close();
            }
            st.Close();
            //Finished
            tb.Text = "下载完成，解压升级包...";
            string file = Settings.USettings.DataCachePath + "win-release.exe";
            await Task.Run(() =>
            {
                ZipFile.ExtractToDirectory(xpath, Settings.USettings.DataCachePath, true);
            });
            tb.Text = "完成";
            Process.Start("explorer.exe", Settings.USettings.DataCachePath + "win-release.exe");
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
