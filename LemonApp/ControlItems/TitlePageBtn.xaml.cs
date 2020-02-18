using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LemonApp
{
    /// <summary>
    /// TitlePageBtn.xaml 的交互逻辑
    /// </summary>
    public partial class TitlePageBtn : UserControl
    {
        public TitlePageBtn()
        {
            InitializeComponent();
        }
        public Brush ColorDx
        {
            get => path.Fill; set
            {
                path.Fill = value;
                if (value == null)
                    path.SetResourceReference(Shape.FillProperty, "ButtonColorBrush");
            }
        }
        public Geometry PathData { get => path.Data; set => path.Data = value; }
        public double MinOp { get => min.Opacity; set => min.Opacity = value; }
        public Thickness Pathness { get => path.Margin; set => path.Margin = value; }
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            path.Visibility = Visibility.Collapsed;
            spath.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            path.Visibility = Visibility.Visible;
            spath.Visibility = Visibility.Collapsed;
        }
    }
}
