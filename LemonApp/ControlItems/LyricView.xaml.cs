using LemonLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using static LemonLib.InfoHelper;

namespace LemonApp
{
    /// <summary>
    /// LyricView.xaml 的交互逻辑
    /// </summary>
    public partial class LyricView : UserControl
    {
        public delegate void NextData(string lyric, string trans);
        public event NextData NextLyric;
        #region LrcModel
        public class LrcModel
        {
            public TextBlock LrcTb { get; set; }

            public Run RomajiTb = null;
            public string LrcRomajiText { get; set; }
            public TextBlock LrcMainTb = null;
            public string LrcText { get; set; }
            public Run LrcTransTb = null;
            public string LrcTransText { get; set; }

            public double Time { get; set; }
        }
        #endregion
        #region Properties
        /// <summary>
        /// 非高亮歌词的透明度
        /// </summary>
        private const double LyricOpacity = 0.8;
        /// <summary>
        /// 高亮歌词效果
        /// </summary>
        public Effect Hightlighter = new DropShadowEffect() { BlurRadius = 20,Color=Colors.White, Opacity = 0.5, ShadowDepth = 0,Direction=0 };
        /// <summary>
        /// 非高亮歌词的颜色
        /// </summary>
        public SolidColorBrush NormalLrcColor;
        /// <summary>
        /// 歌词的文本对齐方式
        /// </summary>
        public TextAlignment TextAlignment = TextAlignment.Left;
        #endregion
        public Dictionary<double, LrcModel> Lrcs = new();
        private double _FontSize;
        /// <summary>
        /// 高亮歌词
        /// </summary>
        public LrcModel foucslrc { get; set; }
        public LyricView()
        {
            InitializeComponent();
        }

        #region 
        public void SetFontSize(int size)
        {
            _FontSize = size;
            foreach (var item in Lrcs.Values)
            {
                item.LrcTb.FontSize = size;
                if (item.LrcTransTb != null) item.LrcTransTb.FontSize = size - 4;
                if(item.RomajiTb!=null)item.RomajiTb.FontSize = size - 6;
            }
        }
        public void SetTransLyric(bool open = true)
        {
            if (!open)
            {
                foreach (var item in Lrcs.Values)
                {
                    var r= item.LrcTransTb;
                    if (r is null) continue;
                    r.Foreground = null;
                    r.FontSize = 0.01;
                }
            }
            else
            {
                int size = Settings.USettings.LyricFontSize - 4;
                foreach (var item in Lrcs.Values)
                {
                    var r = item.LrcTransTb;
                    if (r is null) continue;
                    r.Foreground = NormalLrcColor;
                    r.FontSize = size;
                }
            }
        }

        public void SetRomajiLyric(bool open = true)
        {
            if (!open)
            {
                foreach (var item in Lrcs.Values)
                {
                    var r = item.RomajiTb;
                    if (r is null) continue;
                    r.Foreground = null;
                    r.FontSize = 0.01;
                }
            }
            else
            {
                int size = Settings.USettings.LyricFontSize - 5;
                foreach (var item in Lrcs.Values)
                {
                    var r = item.RomajiTb;
                    if (r is null) continue;
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
        public event MouseButtonEventHandler ClickLyric;
        public async void LoadLrc(LyricData data, bool LoadRomaji)
        {
            Lrcs.Clear();
            c_lrc_items.Children.Clear();
            foucslrc = null;

            c_lrc_items.Children.Add(new TextBlock() { Text = "lemonapp", Height = 200 });

            string[] lrcdata = data.lyric.Split('\n');
            string[] transdata = null;
            Dictionary<double, string> transDic = null;
            List<Run> RomajiRunReference = new();
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
                        sb.Add(lrc.Contains('：') ? " " : lrc);
                        string trans = null;
                        if (data.HasTrans)
                        {
                            IEnumerable<KeyValuePair<double, string>> s = transDic.Where(m => m.Key >= (time.TotalMilliseconds - 2));
                            string a = s.First().Value;
                            trans = lrc != string.Empty && !a.Contains("//") ? a : null;
                        }

                        var model = new LrcModel()
                        {
                            LrcText = lrc,
                            Time = time.TotalMilliseconds,
                            LrcTransText = trans
                        };

                        _FontSize = Settings.USettings.LyricFontSize;
                        TextBlock c_lrcbk = new()
                        {
                            FontSize = Settings.USettings.LyricFontSize,
                            Foreground = NormalLrcColor,
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment,
                            Opacity=LyricOpacity,
                            TextTrimming= TextTrimming.None
                        };
                        model.LrcTb = c_lrcbk;
                        c_lrcbk.MouseDown += ClickLyric;

                        if (LoadRomaji)
                        {
                            var rm = new Run()
                            {
                                FontWeight = FontWeights.Regular,
                                FontSize = c_lrcbk.FontSize - 5,
                                Foreground = Settings.USettings.RomajiLyric ? NormalLrcColor : null
                            };
                            c_lrcbk.Inlines.Add(rm);
                            RomajiRunReference.Add(rm);
                            c_lrcbk.Inlines.Add(new LineBreak());
                            model.RomajiTb = rm;
                        }
                        var mainLine = new TextBlock()
                        {Text = lrc, TextWrapping = TextWrapping.Wrap, TextTrimming = TextTrimming.None };
                        model.LrcMainTb = mainLine;
                        c_lrcbk.Inlines.Add(mainLine);

                        if (trans != null)
                        {
                            var bl = new LineBreak();
                            c_lrcbk.Inlines.Add(bl);
                            var ts = new Run()
                            {
                                Text = trans,
                                FontWeight = FontWeights.Regular,
                                FontSize = c_lrcbk.FontSize - 6,
                                Foreground = Settings.USettings.TransLyric ? NormalLrcColor : null
                            };
                            model.LrcTransTb = ts;
                            c_lrcbk.Inlines.Add(ts);
                        }

                        if (c_lrc_items.Children.Count > 0)
                            c_lrcbk.Margin = new Thickness(0, 15, 0, 15);
                        if (!Lrcs.ContainsKey(time.TotalMilliseconds))
                            Lrcs.Add(time.TotalMilliseconds, model);
                        c_lrcbk.Padding = new Thickness(10, 0, 0, 0);
                        c_lrc_items.Children.Add(c_lrcbk);
                    }
                }
                catch { }
            }
            if (LoadRomaji)
            {
                string file =Path.Combine(Settings.USettings.MusicCachePath , "Lyric",  data.id + ".rm.lmrc");
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
            c_lrc_items.Children.Add(new TextBlock() {Text="lemonapp", Height = 200 });
        }
        private async void SaveRomajiLrc(string id, string data)
        {
            string file = Path.Combine(Settings.USettings.MusicCachePath, "Lyric", id + ".rm.lmrc");
            await File.WriteAllTextAsync(file, data);
        }
        private async Task<List<string>> LoadRomajiLrcFromFile(string id)
        {
            string file = Path.Combine(Settings.USettings.MusicCachePath, "Lyric", id + ".rm.lmrc");
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

        private static double pixelsPerDip = VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip;
        public string InsertLineBreaks(TextBlock tb, double fontSize, double maxWidth)
        {
            string result = string.Empty;
            string line = string.Empty;
            string text = tb.Text;
            bool hasBlank = text.Contains(' ');
            var list =hasBlank?text.Split(' '):text.Split();
            foreach (var word in list)
            {
                var typeface = new Typeface(tb.FontFamily,tb.FontStyle,FontWeights.Bold,tb.FontStretch);
                var formattedLine = new FormattedText(line + " " + word,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    fontSize,
                    Brushes.Black,
                    pixelsPerDip);

                if (formattedLine.WidthIncludingTrailingWhitespace > maxWidth)
                {
                    result += line + "\n";
                    line = hasBlank ? word + " " : word;
                }
                else
                {
                    line += hasBlank ? word+ " " : word;
                }
            }

            result += line;
            //去除result的第一个换行符
            if (result.StartsWith("\n"))
            {
                result = result.Substring(1);
            }
            return result;
        }

        #endregion
        #region
        Brush hl= new SolidColorBrush(Colors.White);
        public void LrcRoll(double nowtime, bool needScrol)
        {
            if (foucslrc == null)
            {
                foucslrc = Lrcs.Values.First();
                foucslrc.LrcTb.SetResourceReference(ForegroundProperty, "ThemeColor");
                foucslrc.LrcTb.FontWeight = FontWeights.Bold;
                foucslrc.LrcTb.Opacity = 1;
            }
            else
            {
                IEnumerable<KeyValuePair<double, LrcModel>> s = Lrcs.Where(m => nowtime >= m.Key);
                if (s.Count() > 0)
                {
                    LrcModel lm = s.Last().Value;
                    if (foucslrc == lm) return;
                    if (needScrol)
                    {

                        foucslrc.LrcTb.Foreground = NormalLrcColor;
                        foucslrc.LrcTb.FontWeight = FontWeights.Regular;
                        foucslrc.LrcTb.BeginAnimation(FontSizeProperty, null);
                        foucslrc.LrcTb.Opacity = LyricOpacity;
                        var ml = foucslrc.LrcMainTb;
                        ml.Text = foucslrc.LrcText;
                        ml.TextWrapping = TextWrapping.Wrap;
                        foucslrc.LrcTb.Effect = null;

                        foucslrc = lm;
                        foucslrc.LrcTb.Opacity = 1;
                        foucslrc.LrcTb.Foreground = hl;
                        //foucslrc.c_LrcTb.SetResourceReference(ForegroundProperty, "ThemeColor");
                        foucslrc.LrcTb.FontWeight = FontWeights.Bold;
                        foucslrc.LrcTb.Effect = Hightlighter;
                        double targetFontsize = _FontSize + 8;
                        var mainLine = foucslrc.LrcMainTb;
                        mainLine.TextWrapping = TextWrapping.NoWrap;
                        mainLine.Text = InsertLineBreaks(mainLine, targetFontsize, mainLine.ActualWidth);
                        var da = new DoubleAnimation(targetFontsize, TimeSpan.FromSeconds(0.3))
                        {
                            EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
                        };
                        Timeline.SetDesiredFrameRate(da, 60);
                        ResetLrcviewScroll();
                        foucslrc.LrcTb.BeginAnimation(FontSizeProperty,da);
                    }
                    NextLyric(lm.LrcText, lm.LrcTransText);
                    
                }
            }
        }
        #endregion
        #region 
        private void ResetLrcviewScroll()
        {
            GeneralTransform gf = foucslrc.LrcTb.TransformToVisual(c_lrc_items);
            Point p = gf.Transform(new Point(0, 0));
            double os = p.Y - (c_scrollviewer.ActualHeight / 2) + 120;
            var da = new DoubleAnimation(os, TimeSpan.FromMilliseconds(500));
            da.EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut };
            Timeline.SetDesiredFrameRate(da, 60);
            c_scrollviewer.BeginAnimation(UIHelper.ScrollViewerBehavior.VerticalOffsetProperty, da);
        }
        #endregion
    }
}
