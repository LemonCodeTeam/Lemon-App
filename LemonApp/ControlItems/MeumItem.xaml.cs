using System.Windows.Controls;
using System.Windows.Media;

namespace LemonApp
{
    /// <summary>
    /// MeumItem.xaml 的交互逻辑
    /// </summary>
    public partial class MeumItem : UserControl
    {
        public MeumItem()
        {
            InitializeComponent();
        }
        public string Text { get => tb.Text; set => tb.Text = value; }
        public Geometry Icon { get => icon.Data; set => icon.Data = value; }
    }
}
