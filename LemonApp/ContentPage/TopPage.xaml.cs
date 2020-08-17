using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LemonApp.ContentPage
{
    /// <summary>
    /// TopPage.xaml 的交互逻辑
    /// </summary>
    public partial class TopPage : UserControl
    {
        private MainWindow mw;
        public TopPage(MainWindow Context, ControlTemplate ct)
        {
            InitializeComponent();
            mw = Context;
            sv.Template = ct;
            SizeChanged += delegate
            {
                mw.WidthUI(topIndexList);
            };
        }
        public async void LoadTopData()
        {
            mw.OpenLoading();
            var dt = await mw.ml.GetTopIndexAsync();
            topIndexList.Visibility = Visibility.Hidden;
            topIndexList.Children.Clear();
            foreach (var d in dt)
            {
                var top = new TopControl(d);
                top.t.MouseDown += delegate {
                    mw.GetTopItems(top);
                };
                top.Margin = new Thickness(10, 0, 10, 20);
                topIndexList.Children.Add(top);
            }
            await Task.Delay(50);
            mw.CloseLoading();
            topIndexList.Visibility = Visibility.Visible;
            mw.WidthUI(topIndexList);
            mw.RunAnimation(topIndexList);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTopData();
        }
    }
}
