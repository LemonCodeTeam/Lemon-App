using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace LemonApp
{
    /// <summary>
    /// CannotPlay.xaml 的交互逻辑
    /// </summary>
    public partial class CannotPlay : Window
    {
        public CannotPlay()
        {
            InitializeComponent();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Run_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/TwilightLemon/Lemon-App/issues");
        }
    }
}
