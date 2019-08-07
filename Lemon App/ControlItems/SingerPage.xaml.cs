using LemonLibrary;
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
using static LemonLibrary.InfoHelper;

namespace Lemon_App
{
    /// <summary>
    /// SingerPage.xaml 的交互逻辑
    /// </summary>
    public partial class SingerPage : UserControl
    {
        public SingerPageData Data;
        private MainWindow mw;
        public SingerPage(SingerPageData spd,MainWindow m)
        {
            InitializeComponent();
            Data = spd;
            mw = m;
            if (!spd.HasBigPic) {
                (Resources["mSingerTX"] as Storyboard).Begin();
                SingerName.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                FansCount.SetResourceReference(ForegroundProperty, "PlayDLPage_Font_Low");
                foreach (BottomTick bt in DHBtns.Children) {
                    bt.HasBg = false;
                }
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LastPage = TuiJianPage;
            SingerName.Text = Data.mSinger.Name;

            if (Data.HasBigPic)
                mSingerBig.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(Data.mSinger.Photo)) { Stretch=Stretch.UniformToFill};
            else TX.Background= new ImageBrush(await ImageCacheHelp.GetImageByUrl(Data.mSinger.Photo));

            FansCount.Text = "粉丝数：" + Data.FansCount;
            if (Data.HasGJ) {
                if (Data.HasBigPic)
                    FanBt.Theme = 1;
                else FanBt.Theme = 0;
                FanBt.TName = "已关注";
                FanBt.p.Stretch = Stretch.Uniform;
                FanBt.pData = Geometry.Parse("M825.742222 376.035556l-349.866666 355.612444a45.169778 45.169778 0 0 1-64.568889 0L213.560889 530.602667a46.478222 46.478222 0 0 1-13.368889-32.768c0-25.6 20.48-46.364444 45.624889-46.364445 12.629333 0 24.064 5.233778 32.312889 13.653334l165.489778 168.106666 317.610666-322.844444a45.283556 45.283556 0 0 1 32.312889-13.539556 46.08 46.08 0 0 1 45.624889 46.364445 46.648889 46.648889 0 0 1-13.425778 32.824889z");
            }

            Lx_Img_1.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(Data.liangxia[0].img));            
            Lx_Tit_1.Text = Data.liangxia[0].name;
            Lx_dat_1.Text = Data.liangxia[0].lstCount;
            if (Data.liangxia.Count >= 2)
            {
                Lx_Img_2.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(Data.liangxia[1].img));
                Lx_Tit_2.Text = Data.liangxia[1].name;
                Lx_dat_2.Text = Data.liangxia[1].lstCount;
            }
            else {
                lx2.Visibility = Visibility.Hidden;
            }

            foreach (var c in Data.HotSongs) {
                mw.np = NowPage.SingerItem;
                var k = new DataItem(c) { Width = ActualWidth };
                if (k.music.MusicID == mw.MusicData.Data.MusicID)
                {
                    k.ShowDx();
                }
                k.GetToSingerPage += mw.K_GetToSingerPage;
                k.Play += mw.PlayMusic;
                k.Download += mw.K_Download;
                HotMusicList.Children.Add(k);
            }

            int i = 0;
            NewAlbumList.Children.Clear();
            foreach (var c in Data.Album) {
                FLGDIndexItem f = new FLGDIndexItem(c.ID, c.Name, c.Photo);
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
            foreach (DataItem ex in HotMusicList.Children)
            {
                var k = new PlayDLItem(ex.music);
                k.MouseDoubleClick += mw.K_MouseDoubleClick;
                mw.PlayDL_List.Items.Add(k);
            }
            PlayDLItem dk = mw.PlayDL_List.Items[0] as PlayDLItem;
            dk.p(true);
            var n = HotMusicList.Children[0] as DataItem;
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
            //------------附加处理
            if(fm!=SongsPage&&mview!=null)
                mw.Cisv.ScrollChanged += Cisv_MusicListScrollChanged;
        }
        private void TuiJianBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(TuiJianPage);
        }

        private async void Cisv_MusicListScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (mw.Cisv.IsVerticalScrollBarAtButtom()) {
                PageIndex++;
                var data = await MusicLib.GetSingerMusicByIdAsync(Data.mSinger.Mid, PageIndex);
                mview.CreatItems(data);
            }
        }

        /// <summary>
        /// 歌曲页的页数索引
        /// </summary>
        private int PageIndex = 1;
        private MusicListView mview=null;
        private async void SongsBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NSPage(SongsPage);
            mw.Cisv.ScrollChanged += Cisv_MusicListScrollChanged;
            if (SongsPage.Children.Count == 0) {
                var data = await MusicLib.GetSingerMusicByIdAsync(Data.mSinger.Mid);
                mview = new MusicListView(data, mw,NowPage.SingerItem);
                SongsPage.Children.Add(mview);
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (FrameworkElement c in HotMusicList.Children)
                c.Width = ActualWidth;
        }
    }
}
