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

namespace LemonApp.ContentPage
{
    /// <summary>
    /// SingerIndexPage.xaml 的交互逻辑
    /// </summary>
    public partial class SingerIndexPage : UserControl
    {
        private MainWindow mw;
        public SingerIndexPage(MainWindow m, ControlTemplate ct)
        {
            InitializeComponent();
            mw = m;
            SingerPage_sv.Template = ct;
            SizeChanged += delegate {
                mw.WidthUI(SingerItemsList);
            };
        }
        private async void GetSingerList(string index = "-100", string area = "-100", string sex = "-100", string genre = "-100", int cur_page = 1)
        {
            if (cur_page == 1)
                SingerItemsList.Opacity = 0;
            string sin = (80 * (cur_page - 1)).ToString();
            mw.OpenLoading();
            ixSingerList = cur_page;
            var data = await MusicLib.GetSingerListAsync(index, area, sex, genre, sin, cur_page);
            if (cur_page == 1)
            {
                SingerItemsList.Children.Clear();
            }
            foreach (var d in data)
            {
                var sinx = new SingerItem(d) { Margin = new Thickness(12, 0, 12, 20) };
                sinx.MouseDown += mw.GetSinger;
                SingerItemsList.Children.Add(sinx);
            }
            mw.WidthUI(SingerItemsList);
            if (cur_page == 1) SingerPage_sv.ScrollToTop();
            mw.CloseLoading();
            if (cur_page == 1)
            {
                await Task.Delay(10);
                mw.RunAnimation(SingerItemsList);
            }
        }
        private int ixSingerList = 1;
        private void SingerPage_sv_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (SingerPage_sv.IsVerticalScrollBarAtButtom())
            {
                ixSingerList++;
                GetSingerList(SingerTab_ABC.Uid, SingerTab_Area.Uid, SingerTab_Sex.Uid, SingerTab_Genre.Uid, ixSingerList);
            }
        }

        RbBox SingerTab_ABC = new RbBox() { Uid = "-100" };
        RbBox SingerTab_Area = new RbBox() { Uid = "-100" };
        RbBox SingerTab_Sex = new RbBox() { Uid = "-100" };
        RbBox SingerTab_Genre = new RbBox() { Uid = "-100" };
        private void SingerTabChecked_ABC(RbBox sender)
        {
            if (sender != null)
            {
                SingerTab_ABC.Check(false);
                SingerTab_ABC = sender;
                GetSingerList(SingerTab_ABC.Uid, SingerTab_Area.Uid, SingerTab_Sex.Uid, SingerTab_Genre.Uid, 1);
            }
        }

        private void SingerTabChecked_Genre(RbBox sender)
        {
            if (sender != null)
            {
                SingerTab_Genre.Check(false);
                SingerTab_Genre = sender;
                GetSingerList(SingerTab_ABC.Uid, SingerTab_Area.Uid, SingerTab_Sex.Uid, SingerTab_Genre.Uid, 1);
            }
        }

        private void SingerTabChecked_Sex(RbBox sender)
        {
            if (sender != null)
            {
                SingerTab_Sex.Check(false);
                SingerTab_Sex = sender;
                GetSingerList(SingerTab_ABC.Uid, SingerTab_Area.Uid, SingerTab_Sex.Uid, SingerTab_Genre.Uid, 1);
            }
        }

        private void SingerTabChecked_Area(RbBox sender)
        {
            if (sender != null)
            {
                SingerTab_Area.Check(false);
                SingerTab_Area = sender;
                GetSingerList(SingerTab_ABC.Uid, SingerTab_Area.Uid, SingerTab_Sex.Uid, SingerTab_Genre.Uid, 1);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SingerTab_ABC = SingerABC.Children[0] as RbBox;
            SingerTab_ABC.Check(true);
            SingerTab_Area = SingerArea.Children[0] as RbBox;
            SingerTab_Area.Check(true);
            SingerTab_Sex = SingerSex.Children[0] as RbBox;
            SingerTab_Sex.Check(true);
            SingerTab_Genre = SingerGenre.Children[0] as RbBox;
            SingerTab_Genre.Check(true);
            foreach (var c in SingerABC.Children)
                (c as RbBox).Checked += SingerTabChecked_ABC;
            foreach (var c in SingerArea.Children)
                (c as RbBox).Checked += SingerTabChecked_Area;
            foreach (var c in SingerSex.Children)
                (c as RbBox).Checked += SingerTabChecked_Sex;
            foreach (var c in SingerGenre.Children)
                (c as RbBox).Checked += SingerTabChecked_Genre;
            GetSingerList();
        }
    }
}
