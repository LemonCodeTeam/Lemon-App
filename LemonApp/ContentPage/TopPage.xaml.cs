using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LemonApp.ContentPage
{
    /// <summary>
    /// TopPage.xaml 的交互逻辑
    /// </summary>
    public partial class TopPage : UserControl
    {
        private MainWindow mw;
        public TopPage(MainWindow Context,ControlTemplate ct)
        {
            InitializeComponent();
            mw = Context;
            sv.Template = ct;
            SizeChanged += delegate {
                mw.WidthUI(topIndexList);
            };
        }
        private async void LoadTopData()
        {
            mw.OpenLoading();
            var dt = await mw.ml.GetTopIndexAsync();
            topIndexList.Visibility = Visibility.Hidden;
            topIndexList.Children.Clear();
            foreach (var d in dt)
            {
                var top = new TopControl(d);
                top.MouseDown += mw.Top_MouseDown;
                top.Margin = new Thickness(12, 0, 12, 20);
                topIndexList.Children.Add(top);
            }
            mw.WidthUI(topIndexList);
            await Task.Delay(50);
            mw.CloseLoading();
            topIndexList.Visibility = Visibility.Visible;
            mw.RunAnimation(topIndexList);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTopData();
        }
    }
}
