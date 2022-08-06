using LemonLib;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace LemonApp
{
    /// <summary>
    /// SearchWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SearchWindow : Window
    {
        public SearchWindow()
        {
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        double Tp = 0;
        private void Search()
        {
            string url = "https://www.baidu.com/s?wd=" + HttpUtility.UrlEncode(textBox1.Text);
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
            p.Start();//启动程序

            //向cmd窗口发送输入信息
            p.StandardInput.WriteLine("start " + url + "&exit");
            p.StandardInput.AutoFlush = true;
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
        }
        private async void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox1.Text != "")
            {
                if (textBox1.Text != "搜索")
                {
                    var data = "{\"data\":" + await HttpHelper.GetWebAsync("http://suggestion.baidu.com/su?wd=" + Uri.EscapeDataString(textBox1.Text).Replace("#", "%23") + "&action=opensearch", Encoding.GetEncoding("GBK")) + "}";
                    Console.WriteLine(data);
                    JObject obj = JObject.Parse(data);
                    var aa = obj["data"][1];
                    listBox.Items.Clear();
                    foreach (var item in aa)
                        listBox.Items.Add(new ListBoxItem() { Content = item, Height = 35 });
                    if (aa.Count() != 0)
                    {
                        var d = new DoubleAnimation(70 + listBox.Items.Count * 35, TimeSpan.FromSeconds(0.3));
                        d.Completed += delegate { BeginAnimation(TopProperty, new DoubleAnimation(Tp - listBox.Items.Count * 25, TimeSpan.FromSeconds(0.3))); };
                        BeginAnimation(HeightProperty, d);
                    }
                    else { var d = new DoubleAnimation(70, TimeSpan.FromSeconds(0.3)); d.Completed += delegate { BeginAnimation(TopProperty, new DoubleAnimation(Tp, TimeSpan.FromSeconds(0.3))); }; BeginAnimation(HeightProperty, d); listBox.Items.Clear(); }
                }
            }
            else { var d = new DoubleAnimation(70, TimeSpan.FromSeconds(0.3)); d.Completed += delegate { BeginAnimation(TopProperty, new DoubleAnimation(Tp, TimeSpan.FromSeconds(0.3))); }; BeginAnimation(HeightProperty, d); listBox.Items.Clear(); }
        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (textBox1.Text == "搜索" && e.Key != Key.Enter)
                textBox1.Text = "";
            if (e.Key == Key.Enter)
                if (textBox1.Text != "")
                {
                    Search();
                    Close();
                }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (textBox1.Text != "")
            {
                Search();
                Close();
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Yield();
            Focus();
            textBox1.Focus();
            Keyboard.Focus(textBox1);
            Tp = Top;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            { DragMove(); Tp = Top; }
        }

        private void CLOSE_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void textBox1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (textBox1.Text == "搜索")
                textBox1.Text = "";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void textBox1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
                listBox.Focus();
        }

        private void listBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if (listBox.SelectedIndex != -1)
                {
                    textBox1.Text = (listBox.SelectedItem as ListBoxItem).Content.ToString();
                    Search();
                    Close();
                }
        }

        private void Bd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (listBox.SelectedIndex != -1)
            {
                textBox1.Text = (listBox.SelectedItem as ListBoxItem).Content.ToString();
                Search();
                Close();
            }
        }
    }
}
