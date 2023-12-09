using System.Windows;

namespace LemonApp
{
    /// <summary>
    /// LoadingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public LoadingWindow(int maxValue)
        {
            InitializeComponent();
            jd.Maximum = maxValue;
        }
        public void Update(string text,int value)
        {
            SignText.Text = text;
            jd.Value = value;
        }
    }
}
