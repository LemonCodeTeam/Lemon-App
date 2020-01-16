using System;
using System.Collections.Generic;
using System.Text;
using LemonLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static LemonLib.InfoHelper;

namespace LemonApp.ContentPage
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : UserControl
    {
        private MainWindow mw;
        public HomePage(MainWindow Context, ControlTemplate ct)
        {
            InitializeComponent();
            mw = Context;
            sv.Template=ct;
            SizeChanged += delegate {
                mw.WidthUI(HomePage_Gdtj);
                mw.WidthUI(HomePage_Nm);
            };
        }

        private async void LoadHomePage()
        {
            //---------加载主页HomePage
            var data = await MusicLib.GetHomePageData();
            HomePage_IFV.Updata(data.focus, mw);
            foreach (var a in data.Gdata)
            {
                var k = new FLGDIndexItem(a.ID, a.Name, a.Photo, a.ListenCount) { Width = 175, Height = 175, Margin = new Thickness(12, 0, 12, 20) };
                k.StarEvent += (sx) =>
                {
                    MusicLib.AddGDILike(sx.id);
                    Toast.Send("收藏成功");
                };
                k.ImMouseDown += mw.FxGDMouseDown;
                HomePage_Gdtj.Children.Add(k);
            }
            mw.WidthUI(HomePage_Gdtj);
            foreach (var a in data.NewMusic)
            {
                var k = new FLGDIndexItem(a.MusicID, a.MusicName + " - " + a.SingerText, a.ImageUrl, 0) { Width = 175, Height = 175, Margin = new Thickness(10, 0, 10, 20) };
                k.Tag = a;
                k.ImMouseDown += (object s, MouseButtonEventArgs es) =>
                {
                    var sx = s as FLGDIndexItem;
                    Music dt = sx.Tag as Music;
                    mw.AddPlayDl_CR(new DataItem(dt));
                    mw.PlayMusic(dt.MusicID, dt.ImageUrl, dt.MusicName, dt.SingerText);
                };
                HomePage_Nm.Children.Add(k);
            }
            mw.WidthUI(HomePage_Nm);
        }

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadHomePage();
        }
    }
}
