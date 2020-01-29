using LemonLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// TopControl.xaml 的交互逻辑
    /// </summary>
    public partial class TopControl : UserControl
    {
        public MusicTop Data;
        public TopControl(MusicTop mp)
        {
            InitializeComponent();
            Loaded += TopControl_Loaded;
            Data = mp;
        }

        private async void TopControl_Loaded(object sender, RoutedEventArgs e)
        {
            im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(Data.Photo, new int[2] { 127,127 }));
            title.Text = Data.Name;
            c1.Text = Data.content[0];
            c2.Text = Data.content[1];
            c3.Text = Data.content[2];
        }
    }
}