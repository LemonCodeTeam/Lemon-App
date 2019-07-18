using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lemon_App
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
