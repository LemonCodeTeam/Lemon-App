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
        }
        bool _tbhasChecked = false;
        public bool TBHasChecked
        {
            get => _tbhasChecked;
            set
            {
                _tbhasChecked = value;
                if (value)
                    ComBlock.Visibility = Visibility.Visible;
                else ComBlock.Visibility = Visibility.Hidden;
            }
        }
        bool _hasChecked = false;
        public bool HasChecked {
            get => _hasChecked;
            set {
                _hasChecked = value;
                if (value)
                    titBtn.SetResourceReference(ForegroundProperty, "ThemeColor");
                else titBtn.SetResourceReference(ForegroundProperty, "ResuTextColor");
            }
        }
        public Geometry PathData { get => path.Data; set => path.Data = value; }
        public string TitleContent { get => titBtn.Text; set => titBtn.Text = value; }
    }
}
