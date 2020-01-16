using System.Windows.Controls;
using System.Windows.Media;

namespace LemonApp
{
    /// <summary>
    /// UserTxControl.xaml 的交互逻辑
    /// </summary>
    public partial class UserTxControl : UserControl
    {
        public UserTxControl()
        {
            InitializeComponent();
        }
        public string qq = "";
        public UserTxControl(ImageBrush img, string name, string qq)
        {
            InitializeComponent();
            UserTX.Background = img;
            UserName.Text = name;
            this.qq = qq;
        }
    }
}
