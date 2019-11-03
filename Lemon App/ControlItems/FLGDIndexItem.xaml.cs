using LemonLibrary;
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

namespace Lemon_App
{
    /// <summary>
    /// FLGDIndexItem.xaml 的交互逻辑
    /// </summary>
    public partial class FLGDIndexItem : UserControl
    {
        public string id { get; set; }
        public string sname { get; set; }
        public string img { get; set; }

        public delegate void Del(FLGDIndexItem fl);
        public event Del DeleteEvent;
        public event Del StarEvent;
        public delegate void Delv(object sender, MouseButtonEventArgs e);
        public event Delv ImMouseDown;
        public FLGDIndexItem()
        {
            InitializeComponent();
        }
        public FLGDIndexItem(string Id, string nae, string pic,int lstCount, bool hasDeleteBtn = false,string subtitle="")
        {
            InitializeComponent();
            id = Id;
            sname = nae;
            img = pic;
            name.Text = nae;
            if (lstCount == 0&&subtitle=="")
                lstBord.Visibility = Visibility.Collapsed;
            else listenCount.Text = lstCount.IntToWn();
            if (subtitle != "")
                listenCount.Text = subtitle;
            if (!hasDeleteBtn)
            {
                MouseEnter += delegate { StarBtn.Visibility = Visibility.Visible; };
                MouseLeave += delegate { StarBtn.Visibility = Visibility.Collapsed; };
            }
            else
            {
                MouseEnter += delegate { DeleteBtn.Visibility = Visibility.Visible; };
                MouseLeave += delegate { DeleteBtn.Visibility = Visibility.Collapsed; };
            }
            Loaded += async delegate
            {
                Height = Height = ActualWidth + 50;
                im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(img));
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
