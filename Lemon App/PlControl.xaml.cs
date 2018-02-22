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
    /// PlControl.xaml 的交互逻辑
    /// </summary>
    public partial class PlControl : UserControl
    {
        public PlControl(string tx,string name,string text)
        {
            InitializeComponent();
            Image.Background = new ImageBrush(new BitmapImage(new Uri(tx)));
            this.name.Text = name;
            Te.Text = text;
        }
    }
}
