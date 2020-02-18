using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LemonApp
{
    /// <summary>
    /// MDButton.xaml 的交互逻辑
    /// </summary>
    public partial class MDButton : UserControl
    {
        public MDButton()
        {
            InitializeComponent();
            theme = 0;
        }
        private int theme = 0;
        public int Theme
        {
            get => theme; set
            {
                theme = value;
                if (theme == 0)
                {
                    bd.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#19000000"));
                    tb.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                }
                else if (theme == 1)
                {
                    bd.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#19FFFFFF"));
                    tb.Foreground = new SolidColorBrush(Colors.White);
                }
                else if (theme == 2)
                {
                    bd.SetResourceReference(BackgroundProperty, "ColorBrush");
                    tb.Foreground = new SolidColorBrush(Colors.White);
                }
            }
        }
        public string TName { get => tb.Text; set => tb.Text = value; }
        public CornerRadius cr { get => bd.CornerRadius; set => bd.CornerRadius = value; }
        public Geometry pData
        {
            get => p.Data; set
            {
                p.Data = value;
                if (value == null)
                    tb.Margin = new Thickness(10, 0, 10, 0);
                else tb.Margin = new Thickness(30, 0, 0, 0);
            }
        }
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            cg.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            cg.Visibility = Visibility.Collapsed;
        }
    }
}
