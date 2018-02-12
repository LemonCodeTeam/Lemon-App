using LemonLibrary;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lemon_App
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
        public MainWindow()
        {
            InitializeComponent();
        }
        LemonLibrary.MusicLib ml = new MusicLib();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private async void window_Loaded(object sender, RoutedEventArgs e)
        {
            LyricView lv = new LyricView();
            lv.FoucsLrcColor = new SolidColorBrush(Color.FromRgb(48, 195, 124));
            lv.NoramlLrcColor = new SolidColorBrush(Color.FromRgb(199, 199, 199));
            lv.TextAlignment = TextAlignment.Left;
            lv.tbWidth = 470;
            ly.Children.Add(lv);
            ml = new MusicLib(lv);
            t.Interval = 500;
            t.Tick += delegate {
                try
                {
                    jd.Maximum = ml.m.NaturalDuration.TimeSpan.TotalMilliseconds;
                    jd.Value = ml.m.Position.TotalMilliseconds;
                    ml.lv.LrcRoll(ml.m.Position.TotalMilliseconds);
                }
                catch { }
            };
            (Resources["Closing"] as Storyboard).Completed += delegate { Environment.Exit(0); };
            ml.m.MediaEnded += delegate{
                if (xh){
                    
                }
            };
            ////////////
            var dt =await ml.GetTopIndexAsync();
            foreach (var d in dt)
            {
                var top = new TopControl(d.ID, d.Photo,d.Name);
                top.MouseDown += async delegate (object seb, MouseButtonEventArgs ed) {
                    var g = seb as TopControl;
                    (Resources["ClickTop"] as Storyboard).Begin();
                    TX.Background = new ImageBrush(new BitmapImage(new Uri(g.pic)));
                    TB.Text = g.name;
                    var dta =await  ml.GetToplistAsync(int.Parse(g.topID));
                    DataItemsList.Children.Clear();
                    foreach (var j in dta) {
                        var k = new DataItem(j.MusicID, j.MusicName, j.Singer, j.ImageUrl) { Margin = new Thickness(20, 0, 0, 20) };
                        k.MouseDown += PlayMusic;
                        DataItemsList.Children.Add(k);
                    }
                };
                top.Margin = new Thickness(0, 0, 20, 20);
                topIndexList.Children.Add(top);
            }
            //////TopPage Loaded//////
            var sin = await ml.GetSingerAsync("all_all_all");
            foreach (var d in sin) {
                var sinx = new SingerItem(d.Photo, d.Name) { Margin = new Thickness(20, 0, 0, 20) };
                singerItemsList.Children.Add(sinx);
            }
            //////SingerPage Loaded//////
            var wk = await ml.GetFLGDIndexAsync();
            FLGDIndexList.Children.Add(new RadioButton() { Content = wk.Hot[0].name ,Uid=wk.Hot[0].id,Margin=new Thickness(0,0,30,10)});
            FLGDIndexList.Children.Add(new TextBlock() { Text = "语种:" });
            foreach(var d in wk.Lauch)
            {
                FLGDIndexList.Children.Add(new RadioButton() { Content = d.name, Uid = d.id, Margin = new Thickness(0, 0, 10, 10) });
            }
            FLGDIndexList.Children.Add(new TextBlock() { Text = "流派:" });
            foreach (var d in wk.LiuPai)
            {
                FLGDIndexList.Children.Add(new RadioButton() { Content = d.name, Uid = d.id, Margin = new Thickness(0, 0, 10, 10) });
            }
            FLGDIndexList.Children.Add(new TextBlock() { Text = "主题:" });
            foreach (var d in wk.Theme)
            {
                FLGDIndexList.Children.Add(new RadioButton() { Content = d.name, Uid = d.id, Margin = new Thickness(0, 0, 10, 10) });
            }
            FLGDIndexList.Children.Add(new TextBlock() { Text = "心情:" });
            foreach (var d in wk.Heart)
            {
                FLGDIndexList.Children.Add(new RadioButton() { Content = d.name, Uid = d.id, Margin = new Thickness(0, 0, 10, 10) });
            }
            FLGDIndexList.Children.Add(new TextBlock() { Text = "场景:" });
            foreach (var d in wk.Changjing)
            {
                FLGDIndexList.Children.Add(new RadioButton() { Content = d.name, Uid = d.id, Margin = new Thickness(0, 0, 10, 10) });
            }
            foreach (var d in FLGDIndexList.Children) {
                if (d is RadioButton)
                    (d as RadioButton).Checked += FLGDPageChecked;
            }
            var dat = await ml.GetFLGDAsync(int.Parse(wk.Hot[0].id));
            FLGDItemsList.Children.Clear();
            foreach (var d in dat)
            {
                FLGDItemsList.Children.Add(new FLGDIndexItem(d.ID, d.Name, d.Photo));
            }
            //////FLGDPage Loaded//////
        }
        string SingerKey1 = "all_all_";
        string SingerKey2 = "all";
        private async void SingerPageChecked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                SingerKey1 = (sender as RadioButton).Uid;
                string sk = SingerKey1 + SingerKey2;
                if (sk == "all")
                    sk = "all_all_all";
                var sin = await ml.GetSingerAsync(sk);
                singerItemsList.Children.Clear();
                foreach (var d in sin)
                {
                    var sinx = new SingerItem(d.Photo, d.Name) { Margin = new Thickness(20, 0, 0, 20) };
                    sinx.MouseDown += GetSinger;
                    singerItemsList.Children.Add(sinx);
                }
            }
        }
        public async void GetSinger(object sender , MouseEventArgs e) {
            await SearchMusicAsync((sender as SingerItem).singer);
        }
        private async void SIngerPageChecked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                if (SingerKey1 == "")
                    SingerKey1 = "all_all_";
                SingerKey2 =(sender as RadioButton).Content.ToString().Replace("热门", "all").Replace("#", "9");
                var sin = await ml.GetSingerAsync(SingerKey1+SingerKey2);
                singerItemsList.Children.Clear();
                foreach (var d in sin)
                {
                    var sinx = new SingerItem(d.Photo, d.Name) { Margin = new Thickness(20, 0, 0, 20) };
                    sinx.MouseDown += GetSinger;
                    singerItemsList.Children.Add(sinx);
                }
            }
        }
        private async void FLGDPageChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender != null)
                {
                    var dt = sender as RadioButton;
                    var data = await ml.GetFLGDAsync(int.Parse(dt.Uid));
                    FLGDItemsList.Children.Clear();
                    foreach (var d in data)
                    {
                        var k = new FLGDIndexItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(20, 0, 0, 20) };
                        k.MouseDown +=delegate (object s,MouseButtonEventArgs se)
                        {
                            GetGD((s as FLGDIndexItem).id);
                        };
                        FLGDItemsList.Children.Add(k);
                    }
                }
            }
            catch { }
        }
        public async void GetGD(string id) {
            var dt =await ml.GetGDAsync(id);
            TX.Background = new ImageBrush(new BitmapImage(new Uri(dt.pic)));
            TB.Text = dt.name;
            DataItemsList.Children.Clear();
            foreach (var j in dt.Data)
            {
                var k = new DataItem(j.MusicID, j.MusicName, j.Singer, j.ImageUrl) { Margin = new Thickness(20, 0, 0, 20) };
                k.MouseDown += PlayMusic;
                DataItemsList.Children.Add(k);
            }
               (Resources["OpenDataPage"] as Storyboard).Begin();
        }
        private async void RadioPageChecked(object sender, RoutedEventArgs e) {
            try
            {
                if (sender != null)
                {
                    var dt = sender as RadioButton;
                    var data = await ml.GetRadioList();
                    RadioItemsList.Children.Clear();
                    switch (dt.Uid)
                    {
                        case "0":
                            foreach (var d in data.Hot) {
                                RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) {Margin=new Thickness(0,0,20,20)});
                            }
                            break;
                        case "1":

                            foreach (var d in data.Evening)
                            {
                                RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                            }
                            break;
                        case "2":
                            foreach (var d in data.Love)
                            {
                                RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                            }
                            break;
                        case "3":
                            foreach (var d in data.Theme)
                            {
                                RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                            }
                            break;
                        case "4":
                            foreach (var d in data.Changjing)
                            {
                                RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                            }
                            break;
                        case "5":
                            foreach (var d in data.Style)
                            {
                                RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                            }
                            break;
                        case "6":
                            foreach (var d in data.Lauch)
                            {
                                RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                            }
                            break;
                        case "7":
                            foreach (var d in data.People)
                            {
                                RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                            }
                            break;
                        case "8":
                            foreach (var d in data.Diqu)
                            {
                                RadioItemsList.Children.Add(new RadioItem(d.ID, d.Name, d.Photo) { Margin = new Thickness(0, 0, 20, 20) });
                            }
                            break;
                    }
                    foreach (var i in RadioItemsList.Children) {
                        (i as RadioItem).MouseDown += GetRadioAsync;
                    }
                }
            }
            catch { }
        }
        DataItem MusicData;
        public void PlayMusic(object sender, MouseEventArgs e) {
            var dt = sender as DataItem;
            PlayMusic(dt.ID, dt.im.Background, dt.SongName, dt.Singer);
        }
        public void PlayMusic(string id,Brush x,string name,string singer) {
            ml.GetAndPlayMusicUrlAsync(id, true, delegate { }, delegate { });
            MusicImage.Background = x;
            MusicName.Text = name;
            Singer.Text = singer;
            (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Pause);
            t.Start();
        }
        private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
               await SearchMusicAsync(SearchBox.Text);
            }
        }
        public async System.Threading.Tasks.Task SearchMusicAsync(string key) {
            var dt = await ml.SearchMusicAsync(key);
            TX.Background = new ImageBrush(new BitmapImage(new Uri(dt.First().ImageUrl)));
            TB.Text = key;
            DataItemsList.Children.Clear();
            foreach (var j in dt)
            {
                var k = new DataItem(j.MusicID, j.MusicName, j.Singer, j.ImageUrl) { Margin = new Thickness(20, 0, 0, 20) };
                k.MouseDown += PlayMusic;
                DataItemsList.Children.Add(k);
            }
               (Resources["OpenDataPage"] as Storyboard).Begin();
        }

        public async void GetRadioAsync(object sender,MouseEventArgs e) {
            var dt = sender as RadioItem;
            var data =await ml.GetRadioMusicAsync(dt.id);
            ml.mldata.Add(data.MusicID, data.MusicName+" - "+data.Singer);
            PlayMusic(data.MusicID, new ImageBrush(new BitmapImage(new Uri(data.ImageUrl))), data.MusicName, data.Singer);
        }
        bool isplay = false;
        private void PlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isplay){
                isplay = false;
                ml.m.Pause();
                t.Stop();
                (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Play);
            }
            else {
                isplay = true;
                ml.m.Play();
                t.Stop();
                (PlayBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Pause);
            }
        }

        private void jd_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ml.m.Position = TimeSpan.FromMilliseconds(jd.Value);
        }
        int ind = 0;
        private void MusicImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ind == 0){
                ind = 1;
                (Resources["OpenLyricPage"] as Storyboard).Begin();
            }
            else {
                ind = 0;
                (Resources["CloseLyricPage"] as Storyboard).Begin();
            }
        }
        bool xh = false;//false: lb true:dq
        private void XHBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (xh) {
                xh = false;
                (XHBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Lbxh);
            }else{
                xh = true;
                (XHBtn.Child as Path).Data = Geometry.Parse(Properties.Resources.Dqxh);
            }
        }
    }
}