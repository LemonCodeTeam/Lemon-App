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
                IsPathFillBinding = false;
                if (value == null)
                {
                    IsPathFillBinding = true;
                    path.SetResourceReference(Shape.FillProperty, "ResuColorBrush");
                }
            }
        }
        public Geometry PathData { get => path.Data; set => path.Data = value; }
        public Thickness Pathness { get => path.Margin; set => path.Margin = value; }

        private bool IsPathFillBinding = true;
        private Brush LastPathBrush;
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            LastPathBrush = path.Fill;
            path.SetResourceReference(Shape.FillProperty, "ThemeColor");
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsPathFillBinding)
                path.SetResourceReference(Shape.FillProperty, "ResuColorBrush");
            else path.Fill = LastPathBrush;
        }
    }
}
