using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LemonApp
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
        public delegate void Cked(RbBox sender);
        public event Cked Checked;
        public string ContentText
        {
            get => tb.Text;
            set => tb.Text = value;
        }
        public bool IsChecked = false;
        public string Data { get; set; } = "";
        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsChecked)
                tb.SetResourceReference(ForegroundProperty, "ThemeColor");
        }

        private void Bg_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsChecked)
                tb.SetResourceReference(ForegroundProperty, "ResuColorBrush");
        }
        public void Check(bool set)
        {
            if (set)
            {
                IsChecked = true;
                bg.SetResourceReference(BackgroundProperty, "ThemeColor");
                tb.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                IsChecked = false;
                bg.Background = new SolidColorBrush(Colors.Transparent);
                tb.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            }
        }

        private void Bg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsChecked)
            {
                IsChecked = false;
                bg.Background = new SolidColorBrush(Colors.Transparent);
                tb.SetResourceReference(ForegroundProperty, "ResuColorBrush");
            }
            else
            {
                IsChecked = true;
                bg.SetResourceReference(BackgroundProperty, "ThemeColor");
                tb.Foreground = new SolidColorBrush(Colors.White);
            }
            Checked(this);
        }
    }
}
