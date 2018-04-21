using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            tb.Text = "正在连接至服务器...";
            var c = new WebClient();
            c.DownloadFileAsync(new Uri("https://github.com/TwilightLemon/Lemon-App/archive/master.zip"), "./api.zip");
            c.DownloadProgressChanged += ProgressChangedAsync;
        }
        public async Task RunAsync() {
            tb.Text = "正在处理数据...";
            try { System.IO.Compression.ZipFile.ExtractToDirectory("./api.zip", "./api/"); } catch { }
            await Task.Delay(2000);
            tb.Text = "正在编译数据...";
            string strInput = "MSBuild.exe " + AppDomain.CurrentDomain.BaseDirectory + @"api\Lemon-App-master";
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.StandardInput.WriteLine("cd "+ System.IO.Path.GetDirectoryName(mkFilePath.Text));
            p.StandardInput.WriteLine(strInput + "&exit");
            p.StandardInput.AutoFlush = true;
            p.Close();
            await Task.Delay(5000);
            tb.Text = "处理完成，请稍等...";
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory+@"api\Lemon-App-master\Lemon App\bin\Debug\");
            FileInfo[] fil = dir.GetFiles();
            DirectoryInfo[] dii = dir.GetDirectories();
            foreach (FileInfo f in fil)
                File.Copy(f.FullName, AppDomain.CurrentDomain.BaseDirectory+ System.IO.Path.GetFileName(f.FullName),true);
            tb.Text = "完成！";
            await Task.Delay(2000);
            Process.Start(AppDomain.CurrentDomain.BaseDirectory+"Lemon App.exe");
            (Resources["Close"] as Storyboard).Begin();
        }
        private async void ProgressChangedAsync(object sender, DownloadProgressChangedEventArgs e)
        {
                tb.Text = "正在接收数据..." + e.ProgressPercentage + "%";
            if (e.ProgressPercentage == 100)
            { await RunAsync(); (sender as WebClient).Dispose(); }
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
