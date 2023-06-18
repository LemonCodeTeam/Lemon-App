using LemonLib;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static LemonLib.InfoHelper;

namespace LemonApp.ContentPage
{
    /// <summary>
    /// RadioIndexPage.xaml 的交互逻辑
    /// </summary>
    public partial class RadioIndexPage : UserControl
    {
        private MainWindow mw;
        public RadioIndexPage(MainWindow m)
        {
            InitializeComponent();
            mw = m;
            SizeChanged += delegate
            {
                mw.WidthUI(RadioItemsList);
            };
        }
        Dictionary<string, MusicRadioList> Radiodata;
        private async void Load()
        {
            Radiodata = await MusicLib.GetRadioList();
            foreach (var list in Radiodata)
            {
                RbBox r = new RbBox();
                r.ContentText = list.Key;
                r.Margin = new Thickness(20, 0, 0, 0);
                r.Checked += RadioPageChecked;
                RadioIndexList.Children.Add(r);
            }
            RbBox first = RadioIndexList.Children[0] as RbBox;
            first.Check(true);
            RadioPageChecked(first);
        }
        private RbBox RadioPage_RbLast = null;
        private async void RadioPageChecked(RbBox sender)
        {
            RadioItemsList.Opacity = 0;
            if (sender != null)
            {
                mw.OpenLoading();
                if (RadioPage_RbLast != null)
                    RadioPage_RbLast.Check(false);
                RadioPage_RbLast = sender;
                RadioItemsList.Children.Clear();
                List<MusicRadioListItem> dat = Radiodata[sender.ContentText].Items;
                foreach (var d in dat)
                {
                    RadioItem a = new RadioItem(d) { Margin = new Thickness(12, 0, 12, 20) };
                    a.im.MouseDown += delegate { mw.GetRadio(a, null); };
                    a.Width = RadioItemsList.ActualWidth / 5;
                    RadioItemsList.Children.Add(a);
                }
                mw.WidthUI(RadioItemsList);
                mw.CloseLoading();
                await Task.Yield();
                RadioItemsList.Opacity = 1;
                mw.ContentAnimation(RadioItemsList);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
        }
    }
}
