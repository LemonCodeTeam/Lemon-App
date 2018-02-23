using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace LemonLibrary
{
    /// <summary>
    /// LyricView.xaml 的交互逻辑
    /// </summary>
    public partial class LyricView : UserControl
    {
        #region 
        public class LrcModel
        {
            public TextBlock c_LrcTb { get; set; }
            public string LrcText { get; set; }
            public double Time { get; set; }
        }
        #endregion
        #region 
        public Dictionary<double, LrcModel> Lrcs = new Dictionary<double, LrcModel>();
        public LrcModel foucslrc { get; set; }
        public double tbWidth { get; set; }

        public SolidColorBrush NoramlLrcColor = new SolidColorBrush(Colors.Black);
        public SolidColorBrush FoucsLrcColor = new SolidColorBrush(Colors.OrangeRed);
        public TextAlignment TextAlignment = TextAlignment.Center;
        #endregion
        public LyricView()
        {
            InitializeComponent();
        }

        #region 
        public void RestWidth(double width) {
            foreach (TextBlock tb in c_lrc_items.Children)
                tb.Width = width;
        }
        public void LoadLrc(string lrcstr)
        {
            Lrcs.Clear();
            c_lrc_items.Children.Clear();
            foucslrc = null;
            foreach (string str in lrcstr.Split("\r\n".ToCharArray()))
            {
                if (str.Length > 0 && str.IndexOf(":") != -1&& !str.StartsWith("[ti:") && !str.StartsWith("[ar:") && !str.StartsWith("[al:") && !str.StartsWith("[by:") && !str.StartsWith("[offset:"))
                {
                    TimeSpan time = GetTime(str);
                    string lrc = str.Split(']')[1];
                    TextBlock c_lrcbk = new TextBlock();
                    c_lrcbk.Foreground = NoramlLrcColor;
                    c_lrcbk.Width = tbWidth;
                    c_lrcbk.TextWrapping = TextWrapping.Wrap;
                    c_lrcbk.TextAlignment = TextAlignment;
                    c_lrcbk.Text = lrc.Replace("^","\n").Replace("//","").Replace("null","");
                    if (c_lrc_items.Children.Count > 0)
                        c_lrcbk.Margin = new Thickness(0, 20, 0, 0);
                    if(!Lrcs.ContainsKey(time.TotalMilliseconds))
                    Lrcs.Add(time.TotalMilliseconds, new LrcModel()
                    {
                        c_LrcTb = c_lrcbk,
                        LrcText = lrc,
                        Time = time.TotalMilliseconds

                    });
                    c_lrc_items.Children.Add(c_lrcbk);
                }
            }
        }
        public TimeSpan GetTime(string str)
        {
            Regex reg = new Regex(@"\[(?<time>.*)\]", RegexOptions.IgnoreCase);
            string timestr = reg.Match(str).Groups["time"].Value;
            int m = Convert.ToInt32(timestr.Split(':')[0]);
            int s = 0, f = 0;
            if (timestr.Split(':')[1].IndexOf(".") != -1)
            {
                s = Convert.ToInt32(timestr.Split(':')[1].Split('.')[0]);
                f = Convert.ToInt32(timestr.Split(':')[1].Split('.')[1]);
            }
            else
                s = Convert.ToInt32(timestr.Split(':')[1]);
            Debug.WriteLine(m + "-" + s + "-" + f + "->" + new TimeSpan(0, 0, m, s, f).TotalMilliseconds);
            return new TimeSpan(0, 0, m, s, f);
        }
        #endregion
        #region
        public void LrcRoll(double nowtime)
        {
            if (foucslrc == null)
            {
                foucslrc = Lrcs.Values.First();
                foucslrc.c_LrcTb.Foreground = FoucsLrcColor;
            }
            else
            {
                IEnumerable<KeyValuePair<double, LrcModel>> s = Lrcs.Where(m => nowtime >= m.Key);
                if (s.Count() > 0)
                {
                    LrcModel lm = s.Last().Value;
                    foucslrc.c_LrcTb.Foreground = NoramlLrcColor;

                    foucslrc = lm;
                    foucslrc.c_LrcTb.Foreground = FoucsLrcColor;
                    ResetLrcviewScroll();
                }
            }
        }
        #endregion
        #region 
        public void ResetLrcviewScroll()
        {
            GeneralTransform gf = foucslrc.c_LrcTb.TransformToVisual(c_lrc_items);
            Point p = gf.Transform(new Point(0, 0));
            double os = p.Y - (c_scrollviewer.ActualHeight / 2) + 10;
            c_scrollviewer.ScrollToVerticalOffset(os);
        }
        public static String parserLine(String str, List<String> times, List<String> texs, Dictionary<String, String> data)
        {
            if (!str.StartsWith("[ti:") && !str.StartsWith("[ar:") && !str.StartsWith("[al:") && !str.StartsWith("[by:") && !str.StartsWith("[offset:"))
            {
                String TimeData =TextHelper.XtoYGetTo(str, "[", "]", 0) + "0";
                times.Add(TimeData);
                String INFO = TextHelper.XtoYGetTo(str, "[", "]", 0);
                String io = "[" + INFO + "]";
                String TexsData = str.Replace(io, "");
                texs.Add(TexsData);
                data.Add(TimeData, TexsData);
                return TimeData + "     " + TexsData;
            }
            else return "";
        }
        #endregion
    }
}
