using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LemonApp.ControlItems
{
    /// <summary>
    /// MainMeumItem.xaml 的交互逻辑
    /// </summary>
    public partial class MainMeumItem : UserControl
    {
        public MainMeumItem()
        {
            InitializeComponent();
            MouseEnter += delegate {
                FocusMask.Visibility = Visibility.Visible;
            };
            MouseLeave += delegate {
                if (!_hasChecked)
                    FocusMask.Visibility = Visibility.Collapsed;
            };
        }

        bool _hasChecked = false;
        public bool HasChecked {
            get => _hasChecked;
            set {
                _hasChecked = value;
                if (value)
                {
                    FocusMask.Visibility = Visibility.Visible;
                    //titBtn.SetResourceReference(ForegroundProperty, "ThemeColor");
                }
                else
                {
                    FocusMask.Visibility = Visibility.Collapsed;
                    // titBtn.SetResourceReference(ForegroundProperty, "ResuTextColor");
                }
            }
        }
        public Geometry PathData { get => path.Data; set => path.Data = value; }
        public string TitleContent { get => titBtn.Text; set => titBtn.Text = value; }
    }
}
