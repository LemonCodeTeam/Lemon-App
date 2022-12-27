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
                {
                    path.SetResourceReference(Shape.FillProperty, "ResuColorBrush");
                }
            }
        }
        public Geometry PathData { get => path.Data; set => path.Data = value; }
        public Thickness Pathness { get => path.Margin; set => path.Margin = value; }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Mask.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Mask.Visibility = Visibility.Collapsed;
        }
    }
}
