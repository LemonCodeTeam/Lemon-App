using LemonLib;
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

namespace LemonApp
{
    /// <summary>
    /// MyFollowSingerList.xaml 的交互逻辑
    /// </summary>
    public partial class MyFollowSingerList : UserControl
    {
        private MainWindow mw;
        public MyFollowSingerList(MainWindow m, ControlTemplate ct)
        {
            InitializeComponent();
            mw = m;
            sv.Template = ct;
            SizeChanged += delegate {
                mw.WidthUI(ItemsList);
            };
            Loaded += delegate {
                GetSingerList();
            };
        }
        private async void GetSingerList(int cur_page = 1)
        {
            if (cur_page == 1)
                ItemsList.Opacity = 0;
            mw.OpenLoading();
            ixSingerList = cur_page;
            var data = await MusicLib.GetSingerIFollowListAsync(cur_page);
            if (cur_page == 1)
            {
                ItemsList.Children.Clear();
            }
            foreach (var d in data)
            {
                var sinx = new SingerItem(d) { Margin = new Thickness(12, 0, 12, 20) };
                sinx.MouseDown += mw.GetSinger;
                ItemsList.Children.Add(sinx);
            }
            mw.WidthUI(ItemsList);
            mw.CloseLoading();
            if (cur_page == 1)
            {
                await Task.Delay(10);
                mw.RunAnimation(ItemsList,new Thickness(0,50,0,0));
            }
        }
        private int ixSingerList = 1;
        private void SingerPage_sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sv.IsVerticalScrollBarAtButtom())
            {
                ixSingerList++;
                GetSingerList(ixSingerList);
            }
        }
    }
}
