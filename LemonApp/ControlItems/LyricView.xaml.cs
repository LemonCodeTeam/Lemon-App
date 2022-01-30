using LemonLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// LyricView.xaml 的交互逻辑
    /// </summary>
    public partial class LyricView : UserControl
    {
        public delegate void NextData(string lyric,string trans);
        public event NextData NextLyric;
        #region 
        public class LrcModel
        {
            public TextBlock c_LrcTb { get; set; }
            public string LrcText { get; set; }
            public string LrcTransText { get; set; }
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
        public void SetFontSize(int size) {
            foreach (TextBlock tb in c_lrc_items.Children)
            {
                tb.FontSize = size;
                if (tb.Inlines.Count > 1) {
                    ((Run)tb.Inlines.Last()).FontSize = tb.FontSize - 4;
                }
            }
        }
        public void RestWidth(double width)
        {
            foreach (TextBlock tb in c_lrc_items.Children)
                tb.Width = width;
        }
        public void LoadLrc(LyricData data)
        {
            Lrcs.Clear();
            c_lrc_items.Children.Clear();
            foucslrc = null;

            string[] lrcdata = data.lyric.Split('\n');
            string[] transdata = null;
            Dictionary<double, string> transDic = null;
            if (data.HasTrans)
            {
                transdata = data.trans.Split('\n');
                transDic = new Dictionary<double, string>();
                foreach (string str in transdata)
                {
                    if (CanSolve(str))
                    {
                        TimeSpan time = GetTime(str);
                        string tran = str.Split(']')[1];
                        if (!transDic.ContainsKey(time.TotalMilliseconds))
                            transDic.Add(time.TotalMilliseconds, tran);
                    }
                }
            }
            foreach (string str in lrcdata)
            {
                if (CanSolve(str))
                {
                    //以歌词Lyric内的时间为准....
                    TimeSpan time = GetTime(str);

                    //歌词翻译的  解析和适配
                    //1.正常对应
                    //2.翻译与歌词之间有+-2ms的误差
                    string lrc = str.Split(']')[1];
                    string trans = null;
                    if (data.HasTrans){
                        IEnumerable<KeyValuePair<double, string>> s = transDic.Where(m => m.Key >=( time.TotalMilliseconds-1));
                        string a = s.First().Value;
                        trans = lrc != string.Empty && a != "//" ? a : null;
                    }

                    TextBlock c_lrcbk = new TextBlock();
                    c_lrcbk.FontSize = Settings.USettings.LyricFontSize;
                    c_lrcbk.Foreground = NoramlLrcColor;
                    c_lrcbk.TextWrapping = TextWrapping.Wrap;
                    c_lrcbk.TextAlignment = TextAlignment;
                    c_lrcbk.Inlines.Add(new Run() { Text=lrc});
                    if (trans != null) {
                        c_lrcbk.Inlines.Add(new LineBreak());
                        c_lrcbk.Inlines.Add(new Run()
                        {
                            Text = trans,
                            FontWeight=FontWeights.Regular,
                            FontSize = c_lrcbk.FontSize-4,
                            Foreground = NoramlLrcColor
                        });
                    }

                    if (c_lrc_items.Children.Count > 0)
                        c_lrcbk.Margin = new Thickness(0, 15, 0, 15);
                    if (!Lrcs.ContainsKey(time.TotalMilliseconds))
                        Lrcs.Add(time.TotalMilliseconds, new LrcModel()
                        {
                            c_LrcTb = c_lrcbk,
                            LrcText = lrc,
                            Time = time.TotalMilliseconds,
                            LrcTransText = trans
                        });
                    c_lrc_items.Children.Add(c_lrcbk);
                }
            }
        }
        public bool CanSolve(string str)
        {
            if (str.Length > 0)
            {
                //直接判断是否为数字...
                var key = TextHelper.FindTextByAB(str, "[", ":", 0);
                return int.TryParse(key, out _);
            }
            else return false;
        }

        public TimeSpan GetTime(string str)
        {
            Regex reg = new Regex(@"\[(?<time>.*)\]", RegexOptions.IgnoreCase);
            string timestr = reg.Match(str).Groups["time"].Value;
            string[] sp = timestr.Split(':');
            int m = Convert.ToInt32(sp[0]);
            int s = 0, f = 0;
            if (sp[1].IndexOf(".") != -1)
            {
                s = Convert.ToInt32(sp[1].Split('.')[0]);
                f = Convert.ToInt32(sp[1].Split('.')[1]);
            }
            else
                s = Convert.ToInt32(sp[1]);
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
                foucslrc.c_LrcTb.FontWeight = FontWeights.Bold;
            }
            else
            {
                IEnumerable<KeyValuePair<double, LrcModel>> s = Lrcs.Where(m => nowtime >= m.Key);
                if (s.Count() > 0)
                {
                    LrcModel lm = s.Last().Value;
                    if (needScrol)
                    {
                        foucslrc.c_LrcTb.Foreground = NoramlLrcColor;
                        foucslrc.c_LrcTb.FontWeight = FontWeights.Regular;
                    }

                    foucslrc = lm;
                    if (needScrol)
                    {
                        foucslrc.c_LrcTb.SetResourceReference(ForegroundProperty, "ThemeColor");
                        foucslrc.c_LrcTb.FontWeight = FontWeights.Bold;
                        ResetLrcviewScroll();
                    }
                    NextLyric(foucslrc.LrcText,foucslrc.LrcTransText);
                }
            }
        }
        #endregion
        #region 
        public void ResetLrcviewScroll()
        {
            GeneralTransform gf = foucslrc.c_LrcTb.TransformToVisual(c_lrc_items);
            Point p = gf.Transform(new Point(0, 0));
            double os = p.Y - (c_scrollviewer.ActualHeight / 2) + 60;
            var da = new DoubleAnimation(os, TimeSpan.FromMilliseconds(300));
            da.EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut };
            c_scrollviewer.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, da);
        }
        #endregion
    }
}
