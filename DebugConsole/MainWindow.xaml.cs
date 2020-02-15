using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DebugConsole
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
        private void Add(string text) {
            Dispatcher.Invoke(() => {
                tb.Text += text+"\r\n";
                if (NeedTurnToBottom)
                    sv.ScrollToBottom();
            });
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Add("Hello World!");
            Add("LemonApp Debug Console");
            Add("Power by TwilightLemon");
            new Thread(Start).Start();
        }
        NamedPipeServerStream pipe;
        StreamReader sr;
        private async void Start()
        {
            pipe = new NamedPipeServerStream("DebugConsolePipeForLemonApp", PipeDirection.InOut, 1);
            try
            {
                pipe.WaitForConnection();
                Add("Connect Successfully!");
                pipe.ReadMode = PipeTransmissionMode.Byte;
                sr = new StreamReader(pipe);
                while (true)
                {
                    string text = await sr.ReadLineAsync();
                    Add(text);
                }
            }
            catch { }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        bool NeedTurnToBottom = true;
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) {
                NeedTurnToBottom = !NeedTurnToBottom;
                Add("[Console] NeedTurnToBottom: " + NeedTurnToBottom);
            }
        }
    }
}
