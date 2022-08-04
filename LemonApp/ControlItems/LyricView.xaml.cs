using LemonLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        public Dictionary<double, LrcModel> Lrcs = new();
        private List<Run> TransRunReference = new();
        private List<Run> RomajiRunReference = new();
        public LrcModel foucslrc { get; set; }

        public SolidColorBrush NormalLrcColor;
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
        public void SetTransLyric(bool open=true) {
            if (!open)
            {
                foreach (Run r in TransRunReference)
                {
                    r.Foreground = null;
                    r.FontSize = 0.01;
                }
            }
            else {
                int size = Settings.USettings.LyricFontSize - 4;
                foreach (Run r in TransRunReference)
                {
                    r.Foreground = NormalLrcColor;
                    r.FontSize = size;
                }
            }
        }

        public void SetRomajiLyric(bool open = true)
        {
            if (!open)
            {
                foreach (Run r in RomajiRunReference)
                {
                    r.Foreground = null;
                    r.FontSize = 0.01;
                }
            }
            else
            {
                int size = Settings.USettings.LyricFontSize - 5;
                foreach (Run r in RomajiRunReference)
                {
                    r.Foreground = NormalLrcColor;
                    r.FontSize = size;
                }
            }
        }
        public void RestWidth(double width)
        {
            foreach (TextBlock tb in c_lrc_items.Children)
                tb.Width = width;
        }
        public async void LoadLrc(LyricData data,bool LoadRomaji)
        {
            Lrcs.Clear();
            c_lrc_items.Children.Clear();
            TransRunReference.Clear();
            RomajiRunReference.Clear();
            foucslrc = null;

            string[] lrcdata = data.lyric.Split('\n');
            string[] transdata = null;
            Dictionary<double, string> transDic = null;
            List<string> sb = new();
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
                try
                {
                    if (CanSolve(str))
                    {
                        //以歌词Lyric内的时间为准....
                        TimeSpan time = GetTime(str);

                        //歌词翻译的  解析和适配
                        //1.正常对应
                        //2.翻译与歌词之间有+-2ms的误差
                        string lrc = str.Split(']')[1].Replace("\r", "").Replace("\n", "");
                        sb.Add(lrc.Contains('：')?" ":lrc);
                        string trans = null;
                        if (data.HasTrans)
                        {
                            IEnumerable<KeyValuePair<double, string>> s = transDic.Where(m => m.Key >= (time.TotalMilliseconds - 2));
                            string a = s.First().Value;
                            trans = lrc != string.Empty && !a.Contains("//") ? a : null;
                        }

                        TextBlock c_lrcbk = new TextBlock();
                        c_lrcbk.FontSize = Settings.USettings.LyricFontSize;
                        c_lrcbk.Foreground = NormalLrcColor;
                        c_lrcbk.TextWrapping = TextWrapping.Wrap;
                        c_lrcbk.TextAlignment = TextAlignment;

                        var rm = new Run()
                        {
                            FontWeight = FontWeights.Regular,
                            FontSize = c_lrcbk.FontSize - 5,
                            Foreground = Settings.USettings.RomajiLyric ? NormalLrcColor : null
                        };
                        c_lrcbk.Inlines.Add(rm);
                        RomajiRunReference.Add(rm);
                        c_lrcbk.Inlines.Add(new LineBreak());

                        c_lrcbk.Inlines.Add(new Run() { Text = lrc });
                        if (trans != null)
                        {
                            var bl = new LineBreak();
                            c_lrcbk.Inlines.Add(bl);
                            var ts = new Run()
                            {
                                Text = trans,
                                FontWeight = FontWeights.Regular,
                                FontSize = c_lrcbk.FontSize - 4,
                                Foreground = Settings.USettings.TransLyric? NormalLrcColor:null
                            };
                            c_lrcbk.Inlines.Add(ts);
                            TransRunReference.Add(ts);
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
                catch { }
            }
            if (LoadRomaji)
            {
                string file = Settings.USettings.MusicCachePath + "Lyric\\" + data.id + ".rm.lmrc";
                if (File.Exists(file))
                {
                    var listu = await LoadRomajiLrcFromFile(data.id);
                    int ind = 0;
                    foreach (var tx in listu)
                    {
                        RomajiRunReference[ind].Text = tx;
                        ind++;
                    }
                }
                else
                {
                    sb[0] = " ";
                    StringBuilder s = new();
                    foreach (var i in sb) s.AppendLine(i);
                    string lc = s.ToString();
                    Console.WriteLine(lc);
                    var list = await RomajiHelper.GetRomaji(lc);
                    int index = 0;
                    s = new();
                    foreach (var tx in list)
                    {
                        s.AppendLine(tx);
                        RomajiRunReference[index].Text = tx;
                        index++;
                    }
                    SaveRomajiLrc(data.id, s.ToString());
                }
            }
        }
        private async void SaveRomajiLrc(string id,string data) {
            string file = Settings.USettings.MusicCachePath + "Lyric\\" + id + ".rm.lmrc";
            await File.WriteAllTextAsync(file, data);
        }
        private async Task<List<string>> LoadRomajiLrcFromFile(string id)
        {
            string file = Settings.USettings.MusicCachePath + "Lyric\\" + id + ".rm.lmrc";
            return (await File.ReadAllLinesAsync(file)).ToList();
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
                    s = int.Parse(sp[1].Split('.')[0]);
                    f = int.Parse(sp[1].Split('.')[1]);
            }
            else
                s = Convert.ToInt32(sp[1]);
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
                        foucslrc.c_LrcTb.Foreground = NormalLrcColor;
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
