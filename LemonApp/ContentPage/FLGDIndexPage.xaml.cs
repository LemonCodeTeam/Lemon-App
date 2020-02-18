using LemonLib;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LemonApp.ContentPage
{
    /// <summary>
    /// FLGDIndexPage.xaml 的交互逻辑
    /// </summary>
    public partial class FLGDIndexPage : UserControl
    {
        private MainWindow mw;
        public FLGDIndexPage(MainWindow m, ControlTemplate ct)
        {
            InitializeComponent();
            mw = m;
            FLGDPage_sv.Template = ct;
            SizeChanged += delegate
            {
                mw.WidthUI(FLGDItemsList);
            };
        }
        bool FLGDPage_Tag_IsOpen = false;
        private void FLGDPage_Tag_Turn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (FLGDPage_Tag_IsOpen)
            {
                FLGDPage_Tag_IsOpen = false;
                FLGDIndexList.Height = 130;
                FLGDPage_Tag_Open.Text = "展开";
            }
            else
            {
                FLGDPage_Tag_IsOpen = true;
                //相当于xaml中的 Height="Auto"
                FLGDIndexList.Height = double.NaN;
                FLGDPage_Tag_Open.Text = "收缩";
            }
        }
        RbBox FLGDPage_Tag = new RbBox();
        string sortId;
        private void FLGDPageChecked(RbBox sender)
        {
            if (sender != null)
            {
                FLGDPage_Tag.Check(false);
                FLGDPage_Tag = sender;
                mw.OpenLoading();
                GetGDList(sender.Uid);
            }
        }
        private void FLGDPage_Tag_All_Checked(RbBox sender)
        {
            FLGDPage_Tag.Check(false);
            FLGDPage_Tag = FLGDPage_Tag_All;
            mw.OpenLoading();
            GetGDList(FLGDPage_Tag_All.Uid);
        }
        private void FLGDPage_SortId_Tj_Checked(RbBox sender)
        {
            sortId = "5";
            FLGDPage_SortId_Newest.Check(false);
            GetGDList(FLGDPage_Tag.Uid, ixFLGD);
        }

        private void FLGDPage_SortId_Newest_Checked(RbBox sender)
        {
            sortId = "2";
            FLGDPage_SortId_Tj.Check(false);
            GetGDList(FLGDPage_Tag.Uid, ixFLGD);
        }
        private int ixFLGD = 0;
        private void FLGDPage_sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (FLGDPage_sv.IsVerticalScrollBarAtButtom())
            {
                ixFLGD++;
                GetGDList(FLGDPage_Tag.Uid, ixFLGD);
            }
        }
        private async void GetGDList(string id, int osx = 1)
        {
            if (osx == 1)
                FLGDItemsList.Opacity = 0;
            FLGDPage_Tag.Uid = id;
            ixFLGD = osx;
            mw.OpenLoading();
            var data = await mw.ml.GetFLGDAsync(id, sortId, osx);
            if (osx == 1) FLGDItemsList.Children.Clear();
            foreach (var d in data)
            {
                var k = new FLGDIndexItem(d.ID, d.Name, d.Photo, d.ListenCount) { Margin = new Thickness(12, 0, 12, 20) };
                k.StarEvent += async (sx) =>
                 {
                     await MusicLib.AddGDILikeAsync(sx.id);
                     Toast.Send("收藏成功");
                 };
                k.ImMouseDown += mw.FxGDMouseDown;
                FLGDItemsList.Children.Add(k);
            }
            mw.WidthUI(FLGDItemsList);
            if (osx == 1) FLGDPage_sv.ScrollToTop();
            mw.CloseLoading();
            if (osx == 1)
            {
                await Task.Delay(10);
                mw.RunAnimation(FLGDItemsList);
            }
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            mw.OpenLoading();
            //加载Tag标签
            var wk = await mw.ml.GetFLGDIndexAsync();
            //--------语种------------
            foreach (var d in wk.Lauch)
            {
                var tb = new RbBox()
                {
                    Uid = d.id,
                    ContentText = d.name,
                    Margin = new Thickness(0, 0, 5, 5)
                };
                tb.Checked += FLGDPageChecked;
                FLGDPage_Tag_Lau.Children.Add(tb);
            }
            //--------流派-------------
            foreach (var d in wk.LiuPai)
            {
                var tb = new RbBox()
                {
                    Uid = d.id,
                    ContentText = d.name.Replace("&#38;", "&"),
                    Margin = new Thickness(0, 0, 5, 5)
                };
                tb.Checked += FLGDPageChecked;
                FLGDPage_Tag_LiuPai.Children.Add(tb);
            }
            //--------主题------------
            foreach (var d in wk.Theme)
            {
                var tb = new RbBox()
                {
                    Uid = d.id,
                    ContentText = d.name,
                    Margin = new Thickness(0, 0, 5, 5)
                };
                tb.Checked += FLGDPageChecked;
                FLGDPage_Tag_Theme.Children.Add(tb);
            }
            //---------心情-----------
            foreach (var d in wk.Heart)
            {
                var tb = new RbBox()
                {
                    Uid = d.id,
                    ContentText = d.name,
                    Margin = new Thickness(0, 0, 5, 5)
                };
                tb.Checked += FLGDPageChecked;
                FLGDPage_Tag_Heart.Children.Add(tb);
            }
            //--------场景-------
            foreach (var d in wk.Changjing)
            {
                var tb = new RbBox()
                {
                    Uid = d.id,
                    ContentText = d.name,
                    Margin = new Thickness(0, 0, 5, 5)
                };
                tb.Checked += FLGDPageChecked;
                FLGDPage_Tag_Changjing.Children.Add(tb);
            }
            GetGDList("10000000");
            FLGDPage_Tag = FLGDPage_Tag_All;
            FLGDPage_Tag_All.Check(true);
            FLGDPage_Tag_All.Checked += FLGDPage_Tag_All_Checked;
            FLGDPage_SortId_Tj.Checked += FLGDPage_SortId_Tj_Checked;
            FLGDPage_SortId_Newest.Checked += FLGDPage_SortId_Newest_Checked;
        }
    }
}
