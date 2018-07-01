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
using System.Windows.Shapes;

namespace Lemon_App
{
    /// <summary>
    /// SearchWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SearchWindow : Window
    {
        public SearchWindow()
        {
            InitializeComponent();
        }
        double tp = 0;
        string uri = "https://www.baidu.com/s?wd=%2a";
        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (textBox1.Text != "")
                {
                    if (textBox1.Text != "搜索")
                    {
                        var d = new DoubleAnimation(380, TimeSpan.FromSeconds(0.3));
                        d.Completed += delegate { BeginAnimation(TopProperty, new DoubleAnimation(tp - 160, TimeSpan.FromSeconds(0.3))); };
                        BeginAnimation(HeightProperty, d);
                        HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create("http://suggestion.baidu.com/su?wd=" + Uri.EscapeDataString(textBox1.Text) + "&action=opensearch");
                        string html6 = "";
                        Stream sr = hwr.GetResponse().GetResponseStream();
                        byte[] b = new byte[1024];
                        sr.Read(b, 0, 1024);
                        html6 = Encoding.Default.GetString(b);
                        html6 = html6.Replace("\0", "");
                        string Htp = html6.Substring(html6.LastIndexOf(",[")).Replace("]]", "").Replace(",[", "").Replace(",", "");
                        string[] aa = Htp.Split(new char[] { '\"' }, StringSplitOptions.RemoveEmptyEntries);
                        listBox.Items.Clear();
                        foreach (var item in aa)
                        {
                            listBox.Items.Add(new ListBoxItem() { Content = item, Height = 35 });
                        }
                    }
                }
                else { var d = new DoubleAnimation(50, TimeSpan.FromSeconds(0.3)); d.Completed += delegate { BeginAnimation(TopProperty, new DoubleAnimation(tp, TimeSpan.FromSeconds(0.3))); }; BeginAnimation(HeightProperty, d); listBox.Items.Clear(); }
            }
            catch { }
        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (textBox1.Text == "搜索" && e.Key != Key.Enter)
                textBox1.Text = "";
            if (e.Key == Key.Enter)
                if (textBox1.Text != "")
                {
                    Process.Start(Uri.EscapeUriString(uri.Replace("%2a", textBox1.Text)));
                    this.Close();
                }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (textBox1.Text != "")
            {
                Process.Start(Uri.EscapeUriString(uri.Replace("%2a", textBox1.Text)));
                this.Close();
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            textBox1.Text = (listBox.SelectedItem as ListBoxItem).Content.ToString();
            Process.Start(Uri.EscapeUriString(uri.Replace("%2a", textBox1.Text)));
            this.Close();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //  Page.Clip = new RectangleGeometry() { RadiusX = 5, RadiusY = 5, Rect = new Rect() { Width = Page.ActualWidth, Height = Page.ActualHeight } };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBox1.Focus();
            tp = Top;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void CLOSE_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
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
    }
}
