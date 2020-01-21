using LemonLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// DataItem.xaml 的交互逻辑
    /// </summary>
    public partial class DataItem : UserControl
    {
        public delegate void MouseDownHandle(DataItem sender);
        public event MouseDownHandle Play;
        public event MouseDownHandle Download;
        public delegate void MouseDownHandle_sm(MusicSinger ms);
        public event MouseDownHandle_sm GetToSingerPage;

        public Music music;
        private MainWindow Mainwindow = null;
        private bool needb = false;
        private bool needDeleteBtn;
        private int BtnWidth = 0;
        public DataItem(Music dat, MainWindow mw,bool NeedDeleteBtn=false)
        {
            try
            {
                InitializeComponent();
                Mainwindow = mw;
                music = dat;
                needDeleteBtn = NeedDeleteBtn;
                Loaded += DataItem_Loaded;
            }
            catch { }
        }

        private bool HasLoaded = false;
        private void DataItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (!HasLoaded)
            {
                HasLoaded = true;
                ser.Inlines.Clear();
                needb = needDeleteBtn;
                name.Text = music.MusicName;
                if (music.Album.Name != null)
                    ab.Text = music.Album.Name.Replace("空", "");
                foreach (MusicSinger a in music.Singer)
                {
                    Ran r = new Ran() { Text = a.Name, data = a };
                    r.MouseDown += R_MouseDown;
                    ser.Inlines.Add(r);
                    ser.Inlines.Add(new Run(" / "));
                }
                ser.Inlines.Remove(ser.Inlines.LastInline);
                mss.Text = music.MusicName_Lyric;

                int BtnCount = 0;
                if (music.Mvmid != "")
                {
                    var MV = (Border)XamlReader.Parse(@"<Border xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Height=""15"" Width=""30"" Background=""#00000000"">
                    <Path Data=""M878.028 244.364H145.842c-54.044 0-97.486 43.83-97.486 97.874v341.075c0 53.915 43.7 97.875 97.486 97.875h732.316c54.044 0 97.486-43.83 97.486-97.875V342.238c-0.129-53.915-43.7-97.874-97.616-97.874z m48.744 438.95c0 27.021-21.85 49.001-48.614 49.001H145.842c-26.893 0-48.614-21.98-48.614-49.002V342.238c0-27.022 21.85-49.002 48.614-49.002h732.316c26.893 0 48.614 21.98 48.614 49.002v341.075zM437.657 376.888h47.191v265.697h-47.191V463.903L358.4 568.372l-79.257-104.469v178.683h-47.062V376.889h47.192l79.256 106.796 79.128-106.796z m220.573 199.37l84.3-198.594h52.622L677.107 643.362h-38.141L520.92 377.665h52.881l84.428 198.594z"" Stretch=""Uniform"" Fill=""{DynamicResource PlayDLPage_Font_Low}""/>
                </Border>");
                    MV.MouseDown += MV_MouseDown;
                    wpl.Children.Add(MV);
                    BtnCount++;
                }
                if (music.Pz == "HQ")
                {
                    wpl.Children.Add((Border)XamlReader.Parse(@"<Border xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Height=""15"" Width=""30""  Background=""#00000000"">
                    <Path Data=""M865.968777 252.511038 158.029176 252.511038c-52.202942 0-94.237291 42.34748-94.237291 94.585215l0 329.807495c0 52.113915 42.190915 94.584192 94.237291 94.584192l707.940624 0c52.204989 0 94.238314-42.346457 94.238314-94.584192L960.208115 347.096253C960.208115 294.981315 918.015154 252.511038 865.968777 252.511038zM913.028583 676.904771c0 26.13831-21.110807 47.40466-47.058782 47.40466L158.029176 724.30943c-25.947975 0-47.057759-21.266349-47.057759-47.40466L110.971417 347.096253c0-26.139334 21.10876-47.405683 47.057759-47.405683l707.940624 0c25.947975 0 47.058782 21.266349 47.058782 47.405683L913.028583 676.904771z M462.1128 370.876865c-12.522198 0-22.645766 9.813506-22.645766 21.904892l0 92.985788L289.436018 485.767546l0-93.402273c0-12.091386-10.123568-21.904892-22.646789-21.904892-12.523221 0-22.646789 9.813506-22.646789 21.904892l0 229.739405c0 12.090363 10.123568 21.903869 22.646789 21.903869 12.523221 0 22.646789-9.813506 22.646789-21.903869l0-92.526323 150.031015 0 0 92.964298c0 12.091386 10.123568 21.904892 22.645766 21.904892 12.523221 0 22.646789-9.813506 22.646789-21.904892L484.759589 392.781758C484.759589 380.690372 474.637044 370.876865 462.1128 370.876865z M773.813914 615.112426c7.397481-10.317996 11.782348-22.796192 11.782348-36.291554L785.596262 436.779831c0-35.30611-29.616528-63.908542-66.172095-63.908542L588.419484 372.871289c-36.534078 0-66.172095 28.603455-66.172095 63.908542l0 142.040017c0 35.282574 29.639041 63.907519 66.172095 63.907519l131.004683 0c6.144954 0 11.976776-1.069355 17.620309-2.582825l13.56904 13.301957c4.259 4.206812 9.858532 6.310729 15.456017 6.310729 5.64251 0 11.30651-2.146896 15.587-6.4192 8.540514-8.500605 8.497535-22.228257-0.130983-30.66337L773.813914 615.112426zM588.419484 599.35658c-12.272511 0-22.262026-9.215896-22.262026-20.535709L566.157457 436.779831c0-11.320836 9.989515-20.537755 22.262026-20.537755l131.004683 0c12.272511 0 22.262026 9.215896 22.262026 20.537755l0 142.040017c0 1.394766-0.574075 2.617617-0.86367 3.936659l-24.019043-23.555485c-8.563026-8.414647-22.50455-8.370645-31.045063 0.12996-8.53949 8.501628-8.496511 22.229281 0.132006 30.66337l9.54847 9.361205L588.419484 599.355557z"" Stretch=""Fill"" Fill=""#FF19BE75"" Width=""26""/>
                </Border>"));
                    BtnCount++;
                }
                else if (music.Pz == "SQ")
                {
                    wpl.Children.Add((Border)XamlReader.Parse(@"<Border xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Height=""15"" Width=""30"" Margin=""5,0,0,0"" Background=""#00000000"">
                    <Path Data=""M411.82958 487.166405c-0.548492-0.042979-1.032516-0.303922-1.581008-0.303922l-90.782609 0c-19.49603 0-35.325553-15.678074-35.325553-34.914184s15.829524-34.892695 35.325553-34.892695c0.724501 0 1.339508-0.346901 2.020006-0.411369 0.703011 0.064468 1.315972 0.411369 2.041496 0.411369l135.110187 0c12.140505 0 21.954011-9.714246 21.954011-21.686928 0-11.969613-9.813506-21.684882-21.954011-21.684882L323.526441 373.683794c-0.725524 0-1.339508 0.325411-2.041496 0.38988-0.681522-0.064468-1.295506-0.38988-2.020006-0.38988-43.691081 0-79.235622 35.109636-79.235622 78.263481 0 40.030715 30.692023 72.776514 69.992097 77.352739 1.559519 0.346901 2.986007 0.933255 4.63353 0.933255l93.834108 0c19.47454 0 35.325553 15.656585 35.325553 34.892695 0 19.234063-15.851013 34.892695-35.325553 34.892695L262.185374 600.018659c-12.141528 0-21.956058 9.714246-21.956058 21.684882 0 11.992125 9.813506 21.686928 21.956058 21.686928l150.851707 0c1.557472 0 2.919493-0.564865 4.368494-0.867764 39.563064-4.358261 70.5191-37.190017 70.5191-77.395718C487.924675 523.033287 454.025611 488.834394 411.82958 487.166405z M773.813914 615.112426c7.397481-10.317996 11.782348-22.796192 11.782348-36.291554L785.596262 436.777785c0-35.304064-29.616528-63.907519-66.172095-63.907519L588.419484 372.870266c-36.534078 0-66.172095 28.603455-66.172095 63.907519l0 142.042063c0 35.282574 29.639041 63.907519 66.172095 63.907519l131.004683 0c6.144954 0 11.976776-1.069355 17.620309-2.582825l13.56904 13.301957c4.259 4.206812 9.858532 6.310729 15.456017 6.310729 5.64251 0 11.30651-2.146896 15.587-6.4192 8.540514-8.500605 8.497535-22.228257-0.130983-30.66337L773.813914 615.112426zM741.686193 578.819848c0 1.394766-0.574075 2.617617-0.86367 3.936659l-24.019043-23.555485c-8.563026-8.414647-22.50455-8.370645-31.045063 0.12996-8.53949 8.500605-8.496511 22.228257 0.132006 30.66337l9.54847 9.361205L588.419484 599.355557c-12.272511 0-22.262026-9.215896-22.262026-20.535709L566.157457 436.777785c0-11.319813 9.989515-20.535709 22.262026-20.535709l131.004683 0c12.272511 0 22.262026 9.215896 22.262026 20.535709L741.686193 578.819848z M865.968777 252.511038 158.029176 252.511038c-52.202942 0-94.237291 42.34748-94.237291 94.585215l0 329.806471c0 52.115961 42.190915 94.585215 94.237291 94.585215l707.940624 0c52.204989 0 94.238314-42.34748 94.238314-94.585215L960.208115 347.096253C960.208115 294.981315 918.015154 252.511038 865.968777 252.511038zM913.028583 676.902724c0 26.139334-21.110807 47.405683-47.058782 47.405683L158.029176 724.308407c-25.947975 0-47.057759-21.266349-47.057759-47.405683L110.971417 347.096253c0-26.139334 21.10876-47.405683 47.057759-47.405683l707.940624 0c25.947975 0 47.058782 21.266349 47.058782 47.405683L913.028583 676.902724z"" Stretch=""Fill"" Fill=""#FFFD893C"" Width=""26""/>
                </Border>"));
                    BtnCount++;
                }
                BtnWidth = 32 * BtnCount + 5;
                if (namss.ActualWidth > wpl.ActualWidth - BtnWidth)
                    namss.Width = wpl.ActualWidth - 101;
                LoadUI();
            }
        }
        private Border CheckView;
        private Border GO;

        private Grid Buttons;
        private Popup Gdpop;
        private ListBox Add_Gdlist;
        private TitlePageBtn DeleteBtn;
        private void LoadUI() {
            string CheckViewxaml = @"<Border xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" x:Name=""CheckView"" HorizontalAlignment=""Left"" Width=""14"" Height=""14"" Margin=""25,0,0,0"" BorderThickness=""1"" BorderBrush=""{DynamicResource TextX1ColorBrush}"" Visibility=""Collapsed"">
                <Border x:Name=""GO"" Margin=""1"" Visibility=""Collapsed"">
                    <Border.Background>
                        <VisualBrush Stretch=""Uniform"">
                            <VisualBrush.Visual>
                                <Grid>
                                    <Path Data=""M377.483636,837.818182A63.301818,63.301818,0,0,1,333.730909,820.130909L64.698182,554.821818A59.578182,59.578182,0,0,1,64.698182,469.178182A61.905455,61.905455,0,0,1,151.738182,469.178182L418.909091,734.021818A60.043636,60.043636,0,0,1,418.909091,820.130909A61.44,61.44,0,0,1,377.483636,837.818182z"" Fill=""{DynamicResource ThemeColor}""/>
                                    <Path Data=""M377.483636,837.818182A63.301818,63.301818,0,0,1,333.730909,820.130909A60.043636,60.043636,0,0,1,333.730909,734.021818L872.261818,203.869091A61.905455,61.905455,0,0,1,959.301818,203.869091A60.043636,60.043636,0,0,1,959.301818,289.978182L418.909091,820.130909A61.44,61.44,0,0,1,377.483636,837.818182z"" Fill=""{DynamicResource ThemeColor}""/>
                                </Grid>
                            </VisualBrush.Visual>
                        </VisualBrush>
                    </Border.Background>
                </Border>
            </Border>";
            CheckView = (Border)XamlReader.Parse(CheckViewxaml);
            GO = (Border)CheckView.Child;
            grid.Children.Add(CheckView);

            string Buttonsxaml = @"<Grid xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" x:Name=""Buttons"" Margin=""0,15,10,15"" HorizontalAlignment=""Right"" Width=""65"" Visibility=""Collapsed""/>";
            Buttons = (Grid)XamlReader.Parse(Buttonsxaml);
            TitlePageBtn DownloadBtn = new TitlePageBtn() { Pathness=new Thickness(0),PathData=Geometry.Parse("M168.064,498.7008L493.9008,824.5376C496.2944,826.9312 499.0848,828.7232 502.0416,829.9648 502.0544,829.9648 502.0544,829.9776 502.0672,829.9776 503.3216,830.5024 504.6144,830.8608 505.92,831.1808 506.2656,831.2704 506.5856,831.4112 506.944,831.488 510.2848,832.1536 513.728,832.1536 517.056,831.488 517.4144,831.4112 517.7344,831.2704 518.08,831.1808 519.3856,830.8608 520.6784,830.5024 521.9328,829.9776 521.9456,829.9648 521.9584,829.9648 521.984,829.952 524.9408,828.7104 527.7056,826.9184 530.0992,824.5248L855.936,498.7008C865.8944,488.7424 865.8944,472.448 855.936,462.5024 845.9776,452.544 829.6832,452.544 819.7376,462.5024L537.6,744.64 537.6,89.6128C537.6,75.5328 526.08,64.0128 512,64.0128 497.92,64.0128 486.4,75.5328 486.4,89.6128L486.4,744.64 204.2624,462.5024C194.304,452.544 178.0096,452.544 168.064,462.5024 158.1056,472.4608 158.1056,488.7424 168.064,498.7008z M972.8,729.6L972.8,857.6C972.8,885.8752,949.8752,908.8,921.6,908.8L102.4,908.8C74.1248,908.8,51.2,885.8752,51.2,857.6L51.2,729.6C51.2,715.456,39.744,704,25.6,704L25.6,704C11.456,704,0,715.456,0,729.6L0,857.6C0,913.92,46.08,960,102.4,960L921.6,960C977.92,960,1024,913.92,1024,857.6L1024,729.6C1024,715.456,1012.544,704,998.4,704L998.4,704C984.256,704,972.8,715.456,972.8,729.6z"),Height=15,Width=15,HorizontalAlignment=HorizontalAlignment.Right };
            DownloadBtn.MouseDown += DownloadBtn_MouseDown;
            Buttons.Children.Add(DownloadBtn);
            TitlePageBtn AddBtn = new TitlePageBtn() {Pathness=new Thickness(0),Width=15,Height=15,PathData=Geometry.Parse("M1024,972.8C1024,1001.075,1001.075,1024,972.8,1024L51.2,1024C22.922,1024,0,1001.075,0,972.8L0,51.2C0,22.922,22.922,0,51.2,0L972.8,0C1001.075,0,1024,22.922,1024,51.2L1024,972.8z M972.8,102.4C972.8,74.123,949.875,51.2,921.6,51.2L102.4,51.2C74.125,51.2,51.2,74.123,51.2,102.4L51.2,921.6C51.2,949.875,74.125,972.8,102.4,972.8L921.6,972.8C949.875,972.8,972.8,949.875,972.8,921.6L972.8,102.4z M768,512C768,526.14,756.54,537.6,742.4,537.6L537.6,537.6 537.6,746.776C537.6,758.496 526.14,768.001 512,768.001 497.86,768.001 486.4,758.496 486.4,746.776L486.4,537.6 281.6,537.6C267.46,537.6 256,526.14 256,512 256,497.86 267.46,486.4 281.6,486.4L486.4,486.4 486.4,277.225C486.4,265.5 497.86,256 512,256 526.14,256 537.6,265.5 537.6,277.225L537.6,486.4 742.4,486.4C756.54,486.4,768,497.86,768,512z") };
            AddBtn.MouseDown += AddBtn_MouseDown;
            Buttons.Children.Add(AddBtn);
            TitlePageBtn PlayBtn = new TitlePageBtn() {Pathness=new Thickness(0),PathData=Geometry.Parse("M914.24,512C914.24,511.808 914.24,511.68 914.24,511.488 914.304,498.304 907.712,485.504 895.872,478.336L164.736,41.664C147.136,31.168 124.736,37.376 114.624,55.488 110.08,63.616 108.8,72.768 110.336,81.28L110.336,942.656C108.8,951.232 110.08,960.384 114.624,968.512 124.736,986.624 147.136,992.832 164.672,982.336L895.872,545.664C907.776,538.56 914.304,525.696 914.24,512.512 914.24,512.32 914.24,512.192 914.24,512z M183.36,140.288L805.76,512 183.36,883.712 183.36,140.288z"),Width=15,Height=15,HorizontalAlignment=HorizontalAlignment.Left };
            PlayBtn.MouseDown += PlayBtn_MouseDown;
            Buttons.Children.Add(PlayBtn);
            grid.Children.Add(Buttons);

            string Gdpopxaml = @"<Popup xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" x:Name=""Gdpop"" AllowsTransparency=""True"" Placement=""Mouse"">
                <Border Background=""{DynamicResource PlayDLPage_Bg}"" CornerRadius=""5"" Margin=""10"" BorderBrush=""{DynamicResource PlayDLPage_Border}"" BorderThickness=""1"">
                    <Grid>
                        <ListBox x:Name=""Add_Gdlist""  VirtualizingPanel.VirtualizationMode=""Recycling""
                            VirtualizingPanel.IsVirtualizing=""True""  Background=""{x:Null}"" Style=""{DynamicResource ListBoxStyle1}"" ScrollViewer.HorizontalScrollBarVisibility=""Disabled"" ItemContainerStyle=""{DynamicResource ListBoxItemStyle1}"" Margin=""5"" Foreground=""{DynamicResource PlayDLPage_Font_Most}"" >
                            <ListBoxItem Content=""我喜欢的歌单""/>
                        </ListBox>
                    </Grid>
                </Border>
            </Popup>";
            Gdpop = (Popup)XamlReader.Parse(Gdpopxaml);
            Add_Gdlist = (ListBox)((Grid)((Border)Gdpop.Child).Child).Children[0];
            grid.Children.Add(Gdpop);
            DeleteBtn = new TitlePageBtn() {Visibility=Visibility.Collapsed,Pathness=new Thickness(0),Height=15,Width=15,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0, 0, 25, 0),PathData=Geometry.Parse("M880,240L704,240 704,176C704,123.2,660.8,80,608,80L416,80C363.2,80,320,123.2,320,176L320,240 144,240C126.4,240 112,254.4 112,272 112,289.6 126.4,304 144,304L192,304 192,816C192,886.4,249.6,944,320,944L704,944C774.4,944,832,886.4,832,816L832,304 880,304C897.6,304 912,289.6 912,272 912,254.4 897.6,240 880,240z M384,176C384,158.4,398.4,144,416,144L608,144C625.6,144,640,158.4,640,176L640,240 384,240 384,176z M768,816C768,851.2,739.2,880,704,880L320,880C284.8,880,256,851.2,256,816L256,304 768,304 768,816z M416 432c-17.6 0-32 14.4-32 32v256c0 17.6 14.4 32 32 32s32-14.4 32-32V464c0-17.6-14.4-32-32-32zM608 432c-17.6 0-32 14.4-32 32v256c0 17.6 14.4 32 32 32s32-14.4 32-32V464c0-17.6-14.4-32-32-32z") };
            Grid.SetColumn(DeleteBtn, 2);
            grid.Children.Add(DeleteBtn);
        }
        public DataItem(Music dat) {
            InitializeComponent();
            music = dat;
        }
        private void R_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ran r = sender as Ran;
            MusicSinger ms = r.data as MusicSinger;
            GetToSingerPage(ms);
        }

        bool ns = false;
        public bool isChecked = false;
        public bool pv;
        public void ShowDx() {
            if (He.LastItem != null)
            {
                mss.Opacity = 0.6;
                ser.Opacity = 0.8;
                He.LastItem.bg.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                He.LastItem.namss.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                He.LastItem.ser.SetResourceReference(ForegroundProperty, "ResuColorBrush");
                He.LastItem.color.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                He.LastItem.pv = false;
            }
            pv = true;
            bg.Background = new SolidColorBrush(Color.FromArgb(10, 0, 0, 0));
            ser.SetResourceReference(ForegroundProperty, "ThemeColor");
            namss.SetResourceReference(ForegroundProperty, "ThemeColor");
            color.SetResourceReference(BackgroundProperty, "ThemeColor");
            mss.Opacity = 1;
            ser.Opacity = 1;

            He.LastItem = this;
        }
        public void NSDownload(bool ns) {
            this.ns = ns;
            if (ns)
            {
                wpl.Margin = new Thickness(60, 0, 10, 0);
                CheckView.Visibility = Visibility.Visible;
                MouseDown += CheckView_MouseDown;
            }
            else {
                wpl.Margin = new Thickness(10, 0, 10, 0);
                CheckView.Visibility = Visibility.Collapsed;
                MouseDown -= CheckView_MouseDown;
            }
        }

        private void CheckView_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Check();
        }

        public void Check() {
            if (!isChecked)
            {
                GO.Visibility = Visibility.Visible;
                CheckView.SetResourceReference(BorderBrushProperty, "ThemeColor");
                isChecked = true;
            }
            else {
                GO.Visibility = Visibility.Collapsed;
                CheckView.SetResourceReference(BorderBrushProperty, "TextX1ColorBrush");
                isChecked = false;
            }
        }

        private void PlayBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Play(this);
        }
        private Dictionary<string, string> ListData = new Dictionary<string, string>();//name,id
        private async void AddBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Add_Gdlist.Items.Clear();
            ListData.Clear();
            JObject o = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/splcloud/fcgi-bin/songlist_list.fcg?utf8=1&-=MusicJsonCallBack&uin={Settings.USettings.LemonAreeunIts}&rnd=0.693477705380313&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0"));
            foreach (var a in o["list"]) {
                string name = a["dirname"].ToString();
                ListData.Add(name, a["dirid"].ToString());
                var mdb = new ListBoxItem { Background = new SolidColorBrush(Colors.Transparent), Height =30, Content = name,Margin=new Thickness(10, 10, 10, 0) };
                mdb.PreviewMouseDown += Mdb_MouseDown;
                Add_Gdlist.Items.Add(mdb);
            }
            var md = new ListBoxItem { Background = new SolidColorBrush(Colors.Transparent), Height = 30, Content = "取消", Margin = new Thickness(10, 10,10, 0) };
            md.PreviewMouseDown += delegate { Gdpop.IsOpen = false; };
            Add_Gdlist.Items.Add(md);
            Gdpop.IsOpen = true;
        }

        private void Mdb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Gdpop.IsOpen = false;
            string name = (sender as ListBoxItem).Content.ToString();
            string id = ListData[name];
            string[] a = MusicLib.AddMusicToGD(music.MusicID,id);
            Toast.Send(a[1]+": "+a[0]);
        }

        private void DownloadBtn_MouseDown(object sender, MouseButtonEventArgs e) => Download(this);

        private async void DeleteBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (TwMessageBox.Show("确定要删除此歌曲吗?"))
            {
                int index = He.MGData_Now.Data.IndexOf(music);
                string dirid = await MusicLib.GetGDdiridByNameAsync(He.MGData_Now.name);
                string Musicid = He.MGData_Now.ids[index];
                Mainwindow.DataItemsList.Items.RemoveAt(index);
                Toast.Send(MusicLib.DeleteMusicFromGD(Musicid, He.MGData_Now.id, dirid));
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Buttons.Visibility = Visibility.Visible;
            if (isChecked)
                wpl.Margin = new Thickness(60, 10, 80, 10);
            else wpl.Margin = new Thickness(10, 10, 80, 10);
            if (needb)DeleteBtn.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Buttons.Visibility = Visibility.Collapsed;
            DeleteBtn.Visibility = Visibility.Collapsed;
            if (isChecked)
                wpl.Margin = new Thickness(60, 10, 10, 10);
            else wpl.Margin = new Thickness(10, 10, 10, 10);
        }
        /// <summary>
        /// 没啥用，留着懒得改模板了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Datasv_ScrollChanged(object sender, ScrollChangedEventArgs e) { }

        private void Ab_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine(music.Album.ID);
            Mainwindow.IFVCALLBACK_LoadAlbum(music.Album.ID);
        }

        private void MV_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mainwindow .PlayMv(new MVData() {
                id=music.Mvmid,
                name=music.MusicName+" - "+music.SingerText
            });
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (namss.ActualWidth > wpl.ActualWidth - BtnWidth)
                namss.Width = wpl.ActualWidth - BtnWidth;
            else namss.Width = double.NaN;
        }
    }


    public class He
    {
        public static DataItem LastItem = null;
        public static MusicGData MGData_Now = null;
    }
}
