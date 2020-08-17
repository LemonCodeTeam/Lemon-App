using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LemonLib
{
    public class InfoHelper
    {
        public class QuickGoToData {
            /// <summary>
            /// 主要支持四类: Singer  GD  TopList  Radio
            /// </summary>
            public string type;
            public string id;
            public string name;
            public string imgurl;
        }
        public class HotKeyInfo {
            public string desc { get; set; }
            public int KeyID { get; set; }
            public int MainKeyIndex { get; set; }
            public int MainKey { get; set; }
            public Key tKey { get; set; }
        }
        public enum NowPage
        {
            Search, Top, SingerItem, GDItem
        }
        public class MeumInfo
        {
            public MeumInfo(TextBlock t, UIElement p, Border c)
            {
                tb = t;
                Page = p;
                Com = c;
            }
            public TextBlock tb;
            public UIElement Page;
            public Border Com;
            public Thickness value = default;
            public string cmd = "";
            public object data = "";
        }
        ////////Music Helper////////
        public class IFVData
        {
            public IFVData(string image, string uri, string ty)
            {
                pic = image;
                url = uri;
                type = ty;
            }
            public string pic;
            public string url;
            public string type;
        }
        public class HomePageData
        {
            public List<IFVData> focus = new List<IFVData>();
            public List<MusicGD> GFdata = new List<MusicGD>();
            public List<MusicGD> Gdata = new List<MusicGD>();
            public List<Music> NewMusic = new List<Music>();
        }

        public class SingerDesc
        {
            public string Desc;
            public Dictionary<string, string> basic;
            public Dictionary<string, string> other;
        }

        public class SingerPageData
        {
            /// <summary>
            /// 歌手信息
            /// </summary>
            public MusicSinger mSinger = new MusicSinger();
            /// <summary>
            /// 热门歌曲
            /// </summary>
            public List<Music> HotSongs = new List<Music>();
            /// <summary>
            /// 专辑
            /// </summary>
            public List<MVData> liangxia = new List<MVData>();
            /// <summary>
            /// 是否关注
            /// </summary>
            public bool HasGJ;
            /// <summary>
            /// 是否有大图
            /// </summary>
            public bool HasBigPic;
            /// <summary>
            /// 专辑
            /// </summary>
            public List<MusicGD> Album = new List<MusicGD>();
            /// <summary>
            /// MV
            /// </summary>
            public List<MVData> mVDatas = new List<MVData>();
            /// <summary>
            /// 相似歌手
            /// </summary>
            public List<MusicSinger> ssMs = new List<MusicSinger>();

            /// <summary>
            /// 粉丝数
            /// </summary>
            public string FansCount = "";
        }
        public class MVData
        {
            public string id { get; set; } = "";
            public string name { set; get; } = "";
            public string img { set; get; } = "";
            public string lstCount { get; set; } = "";
        }
        public class LoginData {
            public string qq;
            public string cookie;
            public string g_tk = null;
        }
        public class Music
        {
            public string MusicName { set; get; } = "";
            public string MusicName_Lyric { get; set; } = "";
            public List<MusicSinger> Singer { set; get; } = new List<MusicSinger>();
            public string SingerText { get; set; } = "";
            public string MusicID { set; get; } = "";
            public string ImageUrl { set; get; } = "";
            public MusicGD Album { set; get; } = new MusicGD();
            public string Mvmid { set; get; } = "";
            public string Pz { set; get; } = "";

            /// <summary>
            /// 相对于个人歌单里的id   用于“我喜欢”歌单
            /// </summary>
            public string Littleid { set; get; } = "";
        }
        public class MusicSinger
        {
            public string Name { set; get; }
            public string Photo { set; get; }
            public string Mid { set; get; }
        }
        public class MusicGD
        {
            public string Name { set; get; }
            public string Photo { set; get; }
            public string ID { set; get; }
            public int ListenCount { set; get; }
        }
        public class MusicTop
        {
            public string Name { set; get; }
            public string Photo { set; get; }
            public string ID { set; get; }
            public string desc;
            public List<string> content { set; get; }
        }
        public class MusicPL
        {
            public string name { get; set; }
            public string img { get; set; }
            public string like { get; set; }
            public string text { get; set; }
            public string commentid { get; set; }
            public string time { get; set; }
            public bool ispraise { get; set; }
        }
        public class MusicRadioList
        {
            public List<MusicRadioListItem> Items { set; get; } = new List<MusicRadioListItem>();
        }
        public class MusicRadioListItem
        {
            public string Name { set; get; }
            public string Photo { set; get; }
            public string ID { set; get; }
            public int lstCount { get; set; }
        }
        public class MusicFLGDIndexItemsList
        {
            public List<MusicFLGDIndexItems> Hot { get; set; } = new List<MusicFLGDIndexItems>();
            public List<MusicFLGDIndexItems> Lauch { get; set; } = new List<MusicFLGDIndexItems>();
            public List<MusicFLGDIndexItems> LiuPai { get; set; } = new List<MusicFLGDIndexItems>();
            public List<MusicFLGDIndexItems> Theme { get; set; } = new List<MusicFLGDIndexItems>();
            public List<MusicFLGDIndexItems> Heart { get; set; } = new List<MusicFLGDIndexItems>();
            public List<MusicFLGDIndexItems> Changjing { get; set; } = new List<MusicFLGDIndexItems>();
        }
        public class MusicFLGDIndexItems
        {
            public string name { get; set; }
            public string id { get; set; }
        }
        public class MusicGData
        {
            public List<Music> Data { get; set; } = new List<Music>();
            public string name { get; set; }
            public string pic { get; set; }
            public string subtitle { get; set; }

            public string desc;
            public MusicSinger Creater;
            public string id { get; set; }
            public bool IsOwn = false;
            public int listenCount { get; set; }
            public List<string> ids = new List<string>();
        }
        public class MusicGLikeData
        {
            /// <summary>
            /// MID(000A23nK23dF),ID(10382021)
            /// </summary>
            public Dictionary<string, string> ids = new Dictionary<string, string>();
        }
        ////////Weather Helper/////
        public class Weather
        {
            public string Qiwen { get; set; }
            public string KongQiZhiLiang { get; set; }
            public string MiaoShu { get; set; }
            public string TiGan { get; set; }
            public string FenSu { get; set; }
            public string NenJianDu { get; set; }
            public List<WeatherByDay> Data { get; set; }

        }
        public class WeatherByDay
        {
            public string Date { set; get; }
            public string Icon { set; get; }
            public string QiWen { set; get; }
        }
    }
}
