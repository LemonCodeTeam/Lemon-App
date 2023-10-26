using LemonLib;
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
        public TopPage(MainWindow Context)
        {
            InitializeComponent();
            mw = Context;
            SizeChanged += delegate
            {
                mw.WidthUI(topIndexList);
            };
        }
        public async void LoadTopData()
        {
            mw.OpenLoading();
            var dt = await MusicLib.GetTopIndexAsync();
            topIndexList.Opacity = 0;
            topIndexList.Children.Clear();
            foreach (var d in dt)
            {
                var top = new TopControl(d);
                top.t.MouseDown += delegate
                {
                    mw.GetTopItems(top);
                };
                top.Margin = new Thickness(10, 0, 10, 20);
                topIndexList.Children.Add(top);
            }
            await Task.Yield();
            mw.CloseLoading();
            mw.WidthUI(topIndexList);
            topIndexList.Opacity = 1;
            mw.ContentAnimation(topIndexList);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTopData();
        }
    }
}
