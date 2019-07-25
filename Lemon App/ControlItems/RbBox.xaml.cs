using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lemon_App
{
    /// <summary>
    /// RbBox.xaml 的交互逻辑
    /// </summary>
    public partial class RbBox : UserControl
    {
        public RbBox()
        {
            InitializeComponent();
        }
        public delegate EventArgs Cked();
        public event Cked Checked;
        public string ContentText{
            get => tb.Text;
            set => tb.Text = value;
        }
        private bool isChecked = false;
        public bool IsChecked { get => isChecked; set { isChecked = value; Checked(); } }
        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            tb.SetResourceReference(ForegroundProperty, "ThemeColor");
        }

        private void Bg_MouseLeave(object sender, MouseEventArgs e)
        {
            tb.SetResourceReference(ForegroundProperty, "ResuColorBrush");
        }

        private void Bg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsChecked)
            {
                IsChecked = false;
                bg.Background = new SolidColorBrush(Colors.Transparent);
                tb.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            }
            else {
                IsChecked = true;
                bg.SetResourceReference(BackgroundProperty, "ThemeColor");
                tb.Foreground = new SolidColorBrush(Colors.White);
            }
        }
    }
}
