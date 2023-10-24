using LemonLib;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static LemonLib.InfoHelper;

namespace LemonApp.ContentPage
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : UserControl
    {
        private MainWindow mw;
        public HomePage(MainWindow Context)
        {
            InitializeComponent();
            mw = Context;
            SizeChanged += delegate
            {
                mw.WidthUI(HomePage_GFGD, HomePage_GFGD.ActualWidth - 12);
                mw.WidthUI(HomePage_Gdtj, HomePage_Gdtj.ActualWidth - 12);
            };
        }

        public async void LoadHomePage()
        {
            //---------加载主页HomePage----动画加持--
            mw.OpenLoading();
            this.Visibility = Visibility.Hidden;
            var data = await MusicLib.GetHomePageData();
            //--Top Focus--------
            HomePage_IFV.Update(data.focus, mw);
            HomePage_GFGD.Children.Clear();
            //--官方歌单----------
            foreach (var a in data.GFdata)
            {
                var k = new FLGDIndexItem(a) { Width = 175, Height = 175, Margin = new Thickness(10, 0, 10, 20) };
                k.StarEvent += async (sx) =>
                 {
                     await MusicLib.AddGDILikeAsync(sx.data.ID);
                     Toast.Send("收藏成功");
                 };
                k.ImMouseDown += mw.FxGDMouseDown;
                HomePage_GFGD.Children.Add(k);
            }
            mw.WidthUI(HomePage_GFGD, HomePage_GFGD.ActualWidth - 12);
            //--歌单推荐----------
            HomePage_Gdtj.Children.Clear();
            foreach (var a in data.Gdata)
            {
                var k = new FLGDIndexItem(a) { Width = 175, Height = 175, Margin = new Thickness(10, 0, 10, 20) };
                k.StarEvent += async (sx) =>
                {
                    await MusicLib.AddGDILikeAsync(sx.data.ID);
                    Toast.Send("收藏成功");
                };
                k.ImMouseDown += mw.FxGDMouseDown;
                HomePage_Gdtj.Children.Add(k);
            }
            mw.WidthUI(HomePage_Gdtj, HomePage_Gdtj.ActualWidth - 12);
            //--新歌首发----------
            HomePage_Nm.Children.Clear();
            foreach (var a in data.NewMusic)
            {
                var k = new PlayDLItem(a, true) { Margin = new Thickness(10, 0, 10, 20) };
                k.Tag = a;
                k.MouseDown += (object s, MouseButtonEventArgs es) =>
                {
                    var sx = s as PlayDLItem;
                    Music dt = sx.Tag as Music;
                    mw.AddPlayDl_CR(new DataItem(dt));
                    mw.PlayMusic(dt);
                };
                HomePage_Nm.Children.Add(k);
                if (HomePage_Nm.Children.Count == 28)
                    break;
            }
            //------------------
            mw.CloseLoading();
            await Task.Delay(200);
            this.Visibility = Visibility.Visible;
            mw.ContentAnimation(this);
        }

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadHomePage();
        }
    }
}
