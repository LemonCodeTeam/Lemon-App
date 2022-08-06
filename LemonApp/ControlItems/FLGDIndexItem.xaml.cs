using LemonLib;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// FLGDIndexItem.xaml 的交互逻辑
    /// </summary>
    public partial class FLGDIndexItem : UserControl
    {
        private Border StarBtn;
        private Border DeleteBtn;

        public delegate void Del(FLGDIndexItem fl);
        public event Del DeleteEvent;
        public event Del StarEvent;
        public delegate void Delv(object sender, MouseButtonEventArgs e);
        public event Delv ImMouseDown;
        public FLGDIndexItem()
        {
            InitializeComponent();
        }
        public MusicGD data = new MusicGD();
        public FLGDIndexItem(MusicGD dat, bool hasDeleteBtn = false, string subtitle = "")
        {
            InitializeComponent();
            data = dat;
            name.Text = dat.Name;
            if (dat.ListenCount == 0 && subtitle == "")
                lstBord.Visibility = Visibility.Collapsed;
            else if (dat.ListenCount == -1)
                listenCount.Text = "刚刚更新";
            else listenCount.Text = dat.ListenCount.IntToWn();
            if (subtitle != "")
                listenCount.Text = subtitle;
            if (!hasDeleteBtn)
            {
                string Buttonsxaml = "<Border " + He.XAMLUSINGS + @" x:Name=""StarBtn"" Height=""25"" VerticalAlignment=""Top"" HorizontalAlignment=""Left"" Width=""25"" Background=""#CC000000"" Visibility=""Collapsed"" CornerRadius=""25"" Margin=""35,5,0,0"">
            <Path Data=""M1003.2,400.32A49.408,49.408,0,0,0,985.728,386.496A42.88,42.88,0,0,0,968.128,379.456L669.632,337.152 550.4,64.256A44.928,44.928,0,0,0,506.624,38.336A44.992,44.992,0,0,0,462.784,64.256L343.744,336.768 49.408,378.496A44.928,44.928,0,0,0,12.672,413.696A44.992,44.992,0,0,0,25.984,462.848L251.84,646.784 165.888,924.544A44.864,44.864,0,0,0,182.016,972.672A44.8,44.8,0,0,0,232.256,981.632L513.28,846.976 785.856,977.6A44.8,44.8,0,0,0,836.096,968.704A44.8,44.8,0,0,0,852.224,920.512L766.72,644.096 997.056,465.792C1016.64,450.56,1019.328,421.248,1003.2,400.32z"" Fill=""White"" Stretch=""Uniform"" Margin=""6""/>
        </Border>";
                StarBtn = (Border)XamlReader.Parse(Buttonsxaml);
                StarBtn.MouseDown += StarBtn_MouseDown;
                grid.Children.Add(StarBtn);
                MouseEnter += delegate { StarBtn.Visibility = Visibility.Visible; };
                MouseLeave += delegate { StarBtn.Visibility = Visibility.Collapsed; };
            }
            else
            {
                string Buttonsxaml = "<Border " + He.XAMLUSINGS + @" x:Name=""DeleteBtn"" Height=""25"" VerticalAlignment=""Top"" HorizontalAlignment=""Left"" Width=""25"" Background=""#CC000000"" CornerRadius=""25"" Visibility=""Collapsed"" Margin=""35,5,0,0"">
            <Path Data=""M590.514383 488.040949 952.888054 851.125263C981.253371 879.546168 981.11016 925.804176 952.546629 954.423734 923.783934 983.242895 877.825294 983.196523 849.450322 954.76589L487.076651 591.681576 124.70298 954.76589C96.337663 983.186795 50.170204 983.043365 21.606672 954.423734-7.156023 925.604646-7.109798 879.555896 21.265175 851.125263L383.638845 488.040949 21.265175 124.956635C-7.100143 96.53573-6.956932 50.277722 21.606672 21.658164 50.369368-7.160997 96.328008-7.114625 124.70298 21.316008L487.076651 384.400322 849.450322 21.316008C877.815566-7.104897 923.983025-6.961467 952.546629 21.658164 981.309325 50.477252 981.263026 96.526002 952.888054 124.956635L590.514383 488.040949 590.514383 488.040949Z"" Fill=""White"" Stretch=""Uniform"" Margin=""7""/>
        </Border>";
                DeleteBtn = (Border)XamlReader.Parse(Buttonsxaml);
                DeleteBtn.MouseDown += DeleteBtn_MouseDown;
                grid.Children.Add(DeleteBtn);
                MouseEnter += delegate { DeleteBtn.Visibility = Visibility.Visible; };
                MouseLeave += delegate { DeleteBtn.Visibility = Visibility.Collapsed; };
            }
            Loaded += async delegate
            {
                Height = Height = ActualWidth + 50;
                var ims = await ImageCacheHelp.GetImageByUrl(dat.Photo, new int[2] { 300, 300 });
                im.Background = new ImageBrush(ims);
            };
            MouseEnter += delegate
            {
                AddToQGT.Visibility = Visibility.Visible;
            };
            MouseLeave += delegate
            {
                AddToQGT.Visibility = Visibility.Collapsed;
            };
            AddToQGT.MouseDown += delegate
            {
                UIHelper.GetAncestor<MainWindow>(this).AddToQGT(new InfoHelper.QuickGoToData()
                {
                    type = "GD",
                    id = data.ID,
                    name = data.Name,
                    imgurl = data.Photo
                });
            };
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Height = Height = ActualWidth + 50;
        }

        private void DeleteBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DeleteEvent(this);
        }

        private void Im_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ImMouseDown(this, null);
        }

        private void StarBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StarEvent(this);
        }
    }
}
