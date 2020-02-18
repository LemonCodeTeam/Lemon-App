using LemonLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LemonApp
{
    /// <summary>
    /// LyricView.xaml 的交互逻辑
    /// </summary>
    public partial class LyricView : UserControl
    {
        public delegate void NextData(string text);
        public event NextData NextLyric;
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

        public SolidColorBrush NoramlLrcColor;
        public TextAlignment TextAlignment = TextAlignment.Center;
        #endregion
        public LyricView()
        {
            InitializeComponent();
        }

        #region 
        public void RestWidth(double width)
        {
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
                if (str.Length > 0 && str.IndexOf(":") != -1 && !str.StartsWith("[ti:") && !str.StartsWith("[ar:") && !str.StartsWith("[al:") && !str.StartsWith("[by:") && !str.StartsWith("[offset:"))
                {
                    TimeSpan time = GetTime(str);
                    string lrc = str.Split(']')[1];
                    TextBlock c_lrcbk = new TextBlock();
                    c_lrcbk.FontSize = 18;
                    c_lrcbk.Foreground = NoramlLrcColor;
                    c_lrcbk.TextWrapping = TextWrapping.Wrap;
                    c_lrcbk.TextAlignment = TextAlignment;
                    c_lrcbk.Text = lrc.Replace("^", "\n").Replace("//", "").Replace("null", "");
                    if (c_lrc_items.Children.Count > 0)
                        c_lrcbk.Margin = new Thickness(0, 15, 0, 15);
                    if (!Lrcs.ContainsKey(time.TotalMilliseconds))
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
        public void LrcRoll(double nowtime, bool needScrol)
        {
            if (foucslrc == null)
            {
                foucslrc = Lrcs.Values.First();
                foucslrc.c_LrcTb.SetResourceReference(ForegroundProperty, "ThemeColor");
            }
            else
            {
                IEnumerable<KeyValuePair<double, LrcModel>> s = Lrcs.Where(m => nowtime >= m.Key);
                if (s.Count() > 0)
                {
                    LrcModel lm = s.Last().Value;
                    if (needScrol)
                        foucslrc.c_LrcTb.Foreground = NoramlLrcColor;

                    foucslrc = lm;
                    if (needScrol)
                    {
                        foucslrc.c_LrcTb.SetResourceReference(ForegroundProperty, "ThemeColor");
                        ResetLrcviewScroll();
                    }
                    string tx = foucslrc.LrcText.Replace("//", "");
                    if (tx.Substring(tx.Length - 1, 1) == "^")
                        tx = tx.Substring(0, tx.Length - 1);
                    tx = tx.Replace("^", "\r\n");
                    NextLyric(tx);
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
            var da = new DoubleAnimation(os, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut };
            c_scrollviewer.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, da);
        }
        #endregion
    }
}
