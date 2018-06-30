using LemonLibrary;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Updata
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog =
                new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "MSBuild|*.exe";
            dialog.FileName = @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\amd64\MSBuild.exe";
            if (dialog.ShowDialog() == true)
                mkFilePath.Text = dialog.FileName;
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://www.visualstudio.com/zh-hans/downloads/");
        }

        private async void Storyboard_Completed(object sender, EventArgs e)
        {
            tb.Text = "正在连接至服务器...";
            float percent = 0;
                HttpWebRequest Myrq = (HttpWebRequest)WebRequest.Create("https://codeload.github.com/TwilightLemon/Lemon-App/zip/master");
                Myrq.Host = "codeload.github.com";
                Myrq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36";
                Myrq.Headers.Add("Upgrade-Insecure-Requests: 1");
                Myrq.Referer = "https://github.com/TwilightLemon/Lemon-App";
                Myrq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                Myrq.Headers.Add("Accept-Language: zh-CN,zh;q=0.9");
                Myrq.Headers.Add("Cookie: _octo=GH1.1.1248683057.1517749075; _ga=GA1.2.90314030.1517749075; logged_in=no; _gat=1");
                HttpWebResponse myrp = (HttpWebResponse)await Myrq.GetResponseAsync();
                long totalBytes = myrp.ContentLength;
                Stream st = myrp.GetResponseStream();
                Stream so = new FileStream("./api.zip", FileMode.Create);
                long totalDownloadedByte = 0;
                byte[] by = new byte[1024];
                int osize = await st.ReadAsync(by, 0, by.Length);
                while (osize > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte;
                    await so.WriteAsync(by, 0, osize);
                    osize = await st.ReadAsync(by, 0, by.Length);
                    percent = (float)totalDownloadedByte / totalBytes * 100;
                    tb.Text = "正在接收数据..." + percent.ToString() + "%";
                }
                so.Close();
                st.Close();
                await RunAsync();
        }
        public async Task RunAsync() {
            tb.Text = "正在处理数据...";
            try { System.IO.Compression.ZipFile.ExtractToDirectory("./api.zip", "./api/"); } catch { }
            await Task.Delay(2000);
            tb.Text = "正在编译数据...";
            Process p = new Process();
            p.StartInfo.FileName = mkFilePath.Text;
            p.StartInfo.Arguments = InfoHelper.GetPath() + @"api\Lemon-App-master";
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit(); 
            p.Close();
            tb.Text = "处理完成，请稍等...";
            DirectoryInfo dir = new DirectoryInfo(InfoHelper.GetPath()+@"api\Lemon-App-master\Lemon App\bin\Debug\");
            FileInfo[] fil = dir.GetFiles();
            DirectoryInfo[] dii = dir.GetDirectories();
            foreach (FileInfo f in fil)
                File.Copy(f.FullName, InfoHelper.GetPath()+ System.IO.Path.GetFileName(f.FullName),true);
            tb.Text = "完成！";
            await Task.Delay(2000);
            Process.Start(InfoHelper.GetPath()+"Lemon App.exe");
            (Resources["Close"] as Storyboard).Begin();
        }
        private void Storyboard_Completed_1(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void TextBlock_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            (Resources["Close"] as Storyboard).Begin();
        }
    }
}
