using System.Windows.Controls;
using System.Windows.Input;

namespace LemonApp
{
    /// <summary>
    /// HotKeyChooser.xaml 的交互逻辑
    /// </summary>
    public partial class HotKeyChooser : UserControl
    {
        public HotKeyChooser()
        {
            InitializeComponent();
        }
        public int KeyId { get; set; }
        public Key key
        {
            get => (Key)tKey.Tag;
            set
            {
                tKey.Tag = value;
                tKey.Text = value.ToString();
            }
        }
        public int MainKey
        {
            get
            {
                return int.Parse((mainKey.SelectedItem as ComboBoxItem).Uid);
            }
        }
        public int index { get => mainKey.SelectedIndex; set => mainKey.SelectedIndex = value; }
        public string desc { get => des.Text; set => des.Text = value; }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Text = e.Key.ToString();
            textBox.Tag = e.Key;
        }
    }
}
