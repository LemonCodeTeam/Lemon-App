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
        public delegate void Delv(object sender, MouseButtonEventArgs e);
        public event Delv ImMouseDown;
        public FLGDIndexItem(string Id,string nae,string pic,bool hasDeleteBtn=false)
        {
            InitializeComponent();
            id = Id;
            sname = nae;
            img = pic;
            name.Text = nae;
            if (!hasDeleteBtn) DeleteBtn.Visibility = Visibility.Collapsed;
            else {
                MouseEnter += delegate { DeleteBtn.Visibility = Visibility.Visible; };
                MouseLeave += delegate { DeleteBtn.Visibility = Visibility.Collapsed; };
            }
             Loaded += async delegate {
                Height = Height = ActualWidth + 45;
                im.Background = new ImageBrush(await ImageCacheHelp.GetImageByUrl(img));
            };
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Height = Height = ActualWidth + 45;
        }

        private void DeleteBtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DeleteEvent(this);
        }

        private void Im_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ImMouseDown(this,null);
        }
    }
}
