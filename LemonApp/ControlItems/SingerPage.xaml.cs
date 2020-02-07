using LemonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// SingerPage.xaml 的交互逻辑
    /// </summary>
    public partial class SingerPage : UserControl
    {
        public SingerPageData Data;
        private MainWindow mw;
        private Action Finished;
        public SingerPage(SingerPageData spd,MainWindow m,Action ac)
        {
            InitializeComponent();
            Data = spd;
            mw = m;
            Finished = ac;
            if (!spd.HasBigPic) {
                (Resources["mSingerTX"] as Storyboard).Begin();
                DHBtns.Background = new SolidColorBrush(Colors.Transparent);
                SingerName.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                FansCount.SetResourceReference(ForegroundProperty, "PlayDLPage_Font_Low");
                foreach (BottomTick bt in DHBtns.Children) {
                    bt.HasBg = false;
                }
            }
        }

        public async void Load()
        {
            mw.Cisv.ScrollChanged += Cisv_MusicListScrollChanged;
            LastPage = TuiJianPage;
            SingerName.Text = Data.mSinger.Name;

            if (Data.HasBigPic)
                mSingerBig.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(Data.mSinger.Photo,new int[2] {469,1000})) { Stretch=Stretch.UniformToFill};
            else TX.Background= new ImageBrush(await ImageCacheHelp.GetImageByUrl(Data.mSinger.Photo,new int[2] { 225 , 225 }));

            FansCount.Text = "粉丝数：" + Data.FansCount;
            if (Data.HasGJ) {
                if (Data.HasBigPic)
                    FanBt.Theme = 1;
                else FanBt.Theme = 0;
                FanBt.TName = "已关注";
                FanBt.p.Stretch = Stretch.Uniform;
                FanBt.pData = Geometry.Parse("M825.742222 376.035556l-349.866666 355.612444a45.169778 45.169778 0 0 1-64.568889 0L213.560889 530.602667a46.478222 46.478222 0 0 1-13.368889-32.768c0-25.6 20.48-46.364444 45.624889-46.364445 12.629333 0 24.064 5.233778 32.312889 13.653334l165.489778 168.106666 317.610666-322.844444a45.283556 45.283556 0 0 1 32.312889-13.539556 46.08 46.08 0 0 1 45.624889 46.364445 46.648889 46.648889 0 0 1-13.425778 32.824889z");
            }

            if (Data.liangxia.Count >= 1)
            {
                Lx_Img_1.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(Data.liangxia[0].img,new int[2] { 60, 60 }));
                Lx_Tit_1.Text = Data.liangxia[0].name;
                Lx_dat_1.Text = Data.liangxia[0].lstCount;
            }
            else {
                lx1.Visibility = Visibility.Hidden;
            }
            if (Data.liangxia.Count >= 2)
            {
                Lx_Img_2.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(Data.liangxia[1].img, new int[2] { 60, 60 }));
                Lx_Tit_2.Text = Data.liangxia[1].name;
                Lx_dat_2.Text = Data.liangxia[1].lstCount;
            }
            else {
                LiangxiaPi.Visibility = Visibility.Collapsed;
            }

            foreach (var c in Data.HotSongs) {
                mw.np = NowPage.SingerItem;
                var k = new DataItem(c,mw) { Width = ActualWidth };
                if (k.music.MusicID == mw.MusicData.Data.MusicID)
                {
                    k.ShowDx();
                }
                k.GetToSingerPage += mw.K_GetToSingerPage;
                k.Play += K_Play;
                k.Download += mw.K_Download;
                HotMusicList.Items.Add(k);
                HotMusicList.Animation(k);
            }

            int i = 0;
            NewAlbumList.Children.Clear();
            foreach (var c in Data.Album) {
                FLGDIndexItem f = new FLGDIndexItem(c.ID, c.Name, c.Photo,0);
                f.Margin = new Thickness(5, 0, 5, 0);
                f.ImMouseDown += F_MouseDown;
                Grid.SetColumn(f, i);
                i++;
                NewAlbumList.Children.Add(f);
            }

            NewMVList.Children.Clear();
            int ix = 0;
            foreach (var c in Data.mVDatas) {
                MVItem m = new MVItem();
                m.Margin = new Thickness(5,0,5,0);
                Grid.SetColumn(m, ix);
                m.MouseDown += M_MouseDown;
                ix++;
                NewMVList.Children.Add(m);
                m.Data = c;
            }

            SimilarSingerList.Children.Clear();
            foreach (var c in Data.ssMs) {
                SingerItem m = new SingerItem(c) {
                    Height=200,
                    Width=150,
                    Margin=new Thickness(20,0,0,0)
                };
                m.MouseDown += mw.GetSinger;
                SimilarSingerList.Children.Add(m);
            }
            Finished();
        }

        private void M_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mw.PlayMv((sender as MVItem).Data);
        }

        private void K_Play(DataItem sender)
        {
            mw.np = NowPage.SingerItem;
            mw.PushPlayMusic(sender, HotMusicList);
        }

        private void F_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mw.IFVCALLBACK_LoadAlbum((sender as FLGDIndexItem).id);
        }

        private void Lx1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mw.IFVCALLBACK_LoadAlbum(Data.liangxia[0].id);
        }

        private void Lx2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mw.IFVCALLBACK_LoadAlbum(Data.liangxia[1].id);
        }

        private void HotSong_PaBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mw.DLMode = false;
            mw.PlayDL_List.Items.Clear();
            foreach (DataItem ex in HotMusicList.Items)
            {
                var k = new PlayDLItem(ex.music);
                k.MouseDoubleClick += mw.K_MouseDoubleClick;
                mw.PlayDL_List.Items.Add(k);
                mw.PlayDL_List.Animation(k);
            }
            PlayDLItem dk = mw.PlayDL_List.Items[0] as PlayDLItem;
            dk.p(true);
            var n = HotMusicList.Items[0] as DataItem;
            mw.MusicData = dk;
            mw.PlayMusic(n);
        }

        private async void FanBt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Data.HasGJ){
                Data.HasGJ = false;
                FanBt.p.Stretch = Stretch.Fill;
                await MusicLib.DelSingerLikeById(Data.mSinger.Mid);
                FanBt.TName = "关注";
                FanBt.pData = Geometry.Parse("M451.318,451.318L87.282,451.318C53.528,451.318 26.548,478.486 26.548,512 26.548,545.747 53.739,572.682 87.282,572.682L451.318,572.682 451.318,936.718C451.318,970.472 478.486,997.452 512,997.452 545.747,997.452 572.682,970.261 572.682,936.718L572.682,572.682 936.718,572.682C970.472,572.682 997.452,545.514 997.452,512 997.452,478.253 970.261,451.318 936.718,451.318L572.682,451.318 572.682,87.282C572.682,53.528 545.514,26.548 512,26.548 478.253,26.548 451.318,53.739 451.318,87.282L451.318,451.318z");
                FanBt.Theme = 2;
            }
            else {
                Data.HasGJ = true;
                await MusicLib.AddSingerLikeById(Data.mSinger.Mid);
                if (Data.HasBigPic)
                    FanBt.Theme = 1;
                else FanBt.Theme = 0;
                FanBt.TName = "已关注";
                FanBt.p.Stretch = Stretch.Uniform;
                FanBt.pData = Geometry.Parse("M825.742222 376.035556l-349.866666 355.612444a45.169778 45.169778 0 0 1-64.568889 0L213.560889 530.602667a46.478222 46.478222 0 0 1-13.368889-32.768c0-25.6 20.48-46.364444 45.624889-46.364445 12.629333 0 24.064 5.233778 32.312889 13.653334l165.489778 168.106666 317.610666-322.844444a45.283556 45.283556 0 0 1 32.312889-13.539556 46.08 46.08 0 0 1 45.624889 46.364445 46.648889 46.648889 0 0 1-13.425778 32.824889z");
            }
        }

        private FrameworkElement LastPage = null;
        private void NSPage(FrameworkElement fm) {
            if (LastPage != null)
                LastPage.Visibility = Visibility.Collapsed;
            fm.Visibility = Visibility.Visible;
            mw.RunAnimation(fm);
            LastPage = fm;
        }
        private void TuiJianBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(TuiJianPage);
        }

        private async void Cisv_MusicListScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (mw.Cisv.IsVerticalScrollBarAtButtom()) {
                if (LastPage == SongsPage)
                {
                    PageIndex++;
                    var data = await MusicLib.GetSingerMusicByIdAsync(Data.mSinger.Mid, PageIndex);
                    mview.CreatItems(data);
                }
                else if (LastPage == AlbumPage) {
                    AlbumIndex++;
                    LoadAlbum();
                }else if (LastPage == MvPage) {
                    MvIndex++;
                    LoadMv();
                }
            }
        }

        /// <summary>
        /// 歌曲页的页数索引
        /// </summary>
        private int PageIndex = 1;
        private MusicListView mview=null;
        private async void SongsBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SongsPage.Children.Count == 0) {
                var data = await MusicLib.GetSingerMusicByIdAsync(Data.mSinger.Mid);
                mview = new MusicListView(data, mw,NowPage.SingerItem);
                SongsPage.Children.Add(mview);
            }
            await Task.Delay(10);
            NSPage(SongsPage);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (FrameworkElement c in HotMusicList.Items)
                c.Width = ActualWidth;
            WidthUI(AlbumItemsList);
            WidthUI(MvItemsList);
        }

        private int AlbumIndex = 1;
        private async void AlbumBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (AlbumItemsList.Children.Count == 0) {
                //new Thickness(10, 0, 10, 20)
                LoadAlbum();

            }
            await Task.Delay(10);
            NSPage(AlbumPage);
        }
        private async void LoadAlbum() {
            var data = await MusicLib.GetSingerAlbumById(Data.mSinger.Mid,AlbumIndex);
            foreach (var d in data)
            {
                var k = new FLGDIndexItem(d.ID, d.Name, d.Photo,0) { Margin = new Thickness(12, 0, 12, 20) };
                k.StarEvent += (sx) =>
                {
                    MusicLib.AddGDILike(sx.id);
                    Toast.Send("收藏成功");
                };
                k.Width = 200;
                k.ImMouseDown += K_ImMouseDown;
                AlbumItemsList.Children.Add(k);
            }
            WidthUI(AlbumItemsList);
        }
        public void WidthUI(Panel wp)
        {
            if (wp.Visibility == Visibility.Visible && wp.Children.Count > 0)
            {
                int lineCount = 4;
                var uc = wp.Children[0] as UserControl;
                double max = uc.MaxWidth;
                double min = uc.MinWidth;
                if (wp.ActualWidth > (24 + max) * lineCount)
                    lineCount++;
                else if (wp.ActualWidth < (24 + min) * lineCount)
                    lineCount--;
                WidTX(wp, lineCount);
            }
        }

        private void WidTX(Panel wp, int lineCount)
        {
            foreach (UserControl dx in wp.Children)
                dx.Width = (wp.ActualWidth - 24 * lineCount) / lineCount;
        }
        private void K_ImMouseDown(object sender, MouseButtonEventArgs e)
        {
            mw.IFVCALLBACK_LoadAlbum((sender as FLGDIndexItem).id);
        }

        private async void MoreBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MoreText.Inlines.Count == 0) {
                var data = await MusicLib.GetSingerDesc(Data.mSinger.Mid);
                MoreText.Inlines.Add(new Run() { Text=data.Desc});
                MoreText.Inlines.Add(new LineBreak());
                MoreText.Inlines.Add(new LineBreak());
                Run r1 = new Run() { Text = "基本资料" };
                r1.SetResourceReference(ForegroundProperty, "PlayDLPage_Font_Most");
                MoreText.Inlines.Add(r1);
                MoreText.Inlines.Add(new LineBreak());
                foreach (var c in data.basic) {
                    MoreText.Inlines.Add(new Run() { Text = c.Key+"："+c.Value });
                    MoreText.Inlines.Add(new LineBreak());
                }
                MoreText.Inlines.Add(new LineBreak());
                foreach (var c in data.other) {
                    Run r2 = new Run() { Text =c.Key };
                    r2.SetResourceReference(ForegroundProperty, "PlayDLPage_Font_Most");
                    MoreText.Inlines.Add(r2);
                    MoreText.Inlines.Add(new LineBreak());
                    MoreText.Inlines.Add(new Run() { Text = c.Value });
                    MoreText.Inlines.Add(new LineBreak());
                    MoreText.Inlines.Add(new LineBreak());
                }
            }
            await Task.Delay(10);
            NSPage(MorePage);
        }

        int MvIndex = 1;
        private async void MVBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MvItemsList.Children.Count == 0) {
                LoadMv();
            }
            await Task.Delay(10);
            NSPage(MvPage);
        }

        public async void LoadMv() {
            var data = await MusicLib.GetSingerMvList(Data.mSinger.Mid, MvIndex);
            foreach (var c in data)
            {
                MVItem m = new MVItem();
                m.Margin = new Thickness(12, 0, 12, 20);
                m.MouseDown += M_MouseDown;
                MvItemsList.Children.Add(m);
                m.Data = c;
            }
            WidthUI(MvItemsList);
        }
    }
}
