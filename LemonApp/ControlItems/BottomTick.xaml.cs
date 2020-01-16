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

namespace LemonApp
{
    /// <summary>
    /// BottomTick.xaml 的交互逻辑
    /// </summary>
    public partial class BottomTick : UserControl
    {
        public BottomTick()
        {
            InitializeComponent();
            if(BtD.LastBt==null)
                BtD.LastBt = this;
        }
        private bool isChecked = false;
        public bool IsChecked { get => isChecked;set {
                isChecked = value;
                Check(isChecked);
            } }
        private bool hasBg = false;
        public bool HasBg { get => hasBg; set {
                hasBg = value;
                if (!isChecked){
                    if (hasBg)
                        tb.Foreground = new SolidColorBrush(Colors.White);
                    else tb.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                }
            } }
        public string Text { get => tb.Text; set => tb.Text = value; }
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            tb.SetResourceReference(ForegroundProperty, "ThemeColor");
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsChecked){
                if (hasBg)
                    tb.Foreground = new SolidColorBrush(Colors.White);
                else tb.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (BtD.LastBt != null)
                BtD.LastBt.Check(false);
            Check(true);
            BtD.LastBt = this;
        }

        public void Check(bool h) {
            if (h) {
                tb.SetResourceReference(ForegroundProperty, "ThemeColor");
                bt.Visibility = Visibility.Visible;
            }
            else {
                if (hasBg)
                    tb.Foreground = new SolidColorBrush(Colors.White);
                else tb.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                bt.Visibility = Visibility.Collapsed;
            }
            isChecked = h;
        }
    }
    public class BtD {
        public static BottomTick LastBt;
    }
}
