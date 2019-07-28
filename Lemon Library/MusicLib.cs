using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using static LemonLibrary.InfoHelper;
/*
   作者:Twilight./Lemon        QQ:2728578956
   请保留版权信息，侵权必究。
     
     Author:Twilight./Lemon QQ:2728578956
Please retain the copyright information, rights reserved.
     */

namespace LemonLibrary
{
    public class MusicLib
    {
        #region  构造函数
        public MusicLib(LyricView LV, string id)
        {
            if (!Directory.Exists(Settings.USettings.DownloadPath))
                Directory.CreateDirectory(Settings.USettings.DownloadPath);
            if (!Directory.Exists(Settings.USettings.CachePath))
                Directory.CreateDirectory(Settings.USettings.CachePath);
            if (!Directory.Exists(Settings.USettings.CachePath + "Music\\"))
                Directory.CreateDirectory(Settings.USettings.CachePath + "Music\\");
            if (!Directory.Exists(Settings.USettings.CachePath + "Lyric\\"))
                Directory.CreateDirectory(Settings.USettings.CachePath + "Lyric\\");
            if (!Directory.Exists(Settings.USettings.CachePath + "Image\\"))
                Directory.CreateDirectory(Settings.USettings.CachePath + "Image\\");
            lv = LV;
            qq = id;
            GetMusicLikeGDid();
        }
        public MusicLib()
        {
            if (!Directory.Exists(Settings.USettings.DownloadPath))
                Directory.CreateDirectory(Settings.USettings.DownloadPath);
            if (!Directory.Exists(Settings.USettings.CachePath))
                Directory.CreateDirectory(Settings.USettings.CachePath);
            if (!Directory.Exists(Settings.USettings.CachePath + "Music\\"))
                Directory.CreateDirectory(Settings.USettings.CachePath + "Music\\");
            if (!Directory.Exists(Settings.USettings.CachePath + "Lyric\\"))
                Directory.CreateDirectory(Settings.USettings.CachePath + "Lyric\\");
            if (!Directory.Exists(Settings.USettings.CachePath + "Image\\"))
                Directory.CreateDirectory(Settings.USettings.CachePath + "Image\\");
        }
        #endregion
        #region 一些字段
        public static MediaPlayer mp = new MediaPlayer();
        public LyricView lv;
        public static string qq = "";
        public static string MusicLikeGDid = "";
        public static string MusicLikeGDdirid = "";
        #endregion
        #region 搜索歌曲&搜索智能提示 (似乎不太智能)
        public async Task<List<Music>> SearchMusicAsync(string Content, int osx = 1)
        {
            if (HttpHelper.IsNetworkTrue)
            {
                JObject o = JObject.Parse(await HttpHelper.GetWebAsync($"http://59.37.96.220/soso/fcgi-bin/client_search_cp?format=json&t=0&inCharset=GB2312&outCharset=utf-8&qqmusic_ver=1302&catZhida=0&p={osx}&n=20&w={HttpUtility.UrlDecode(Content)}&flag_qc=0&remoteplace=sizer.newclient.song&new_json=1&lossless=0&aggr=1&cr=1&sem=0&force_zonghe=0"));
                List<Music> dt = new List<Music>();
                int i = 0;
                var dsl = o["data"]["song"]["list"];
                while (i < dsl.Count())
                {
                    var dsli = dsl[i];
                    Music m = new Music();
                    m.MusicName = dsli["title"].ToString();
                    m.MusicName_Lyric = dsli["lyric"].ToString();
                    string Singer = "";
                    List<MusicSinger> lm = new List<MusicSinger>();
                    for (int osxc = 0; osxc != dsli["singer"].Count(); osxc++)
                    {
                        Singer += dsli["singer"][osxc]["name"] + "&";
                        lm.Add(new MusicSinger() { Name = dsli["singer"][osxc]["name"].ToString(), Mid = dsli["singer"][osxc]["mid"].ToString() });
                    }
                    m.Singer = lm;
                    m.SingerText = Singer.Substring(0, Singer.LastIndexOf("&"));
                    m.MusicID = dsli["mid"].ToString();
                    var amid = dsli["album"]["mid"].ToString();
                    if (amid == "001ZaCQY2OxVMg")
                        m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R300x300M000{dsli["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                    else if (amid == "") m.ImageUrl = $"https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000";
                    else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{amid}.jpg?max_age=2592000";
                    dt.Add(m);
                    i++;
                }
                return dt;
            }
            else return null;
        }
        public async Task<List<string>> Search_SmartBoxAsync(string key)
        {
            var data = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/splcloud/fcgi-bin/smartbox_new.fcg?key={HttpUtility.UrlDecode(key)}&utf8=1&is_xml=0&loginUin={Settings.USettings.LemonAreeunIts}&qqmusic_ver=1592&searchid=3DA3E73D151F48308932D9680A3A5A1722872&pcachetime=1535710304"))["data"];
            List<String> list = new List<String>();
            var song = data["song"]["itemlist"];
            for (int i = 0; i < song.Count(); i++)
            {
                var o = song[i];
                list.Add("歌曲:" + o["name"] + " - " + o["singer"]);
            }
            var album = data["album"]["itemlist"];
            for (int i = 0; i < album.Count(); i++)
            {
                var o = album[i];
                list.Add("专辑:" + o["singer"] + " - 《" + o["name"] + "》");
            }
            var singer = data["singer"]["itemlist"];
            for (int i = 0; i < singer.Count(); i++)
            {
                var o = singer[i];
                list.Add("歌手:" + o["singer"]);
            }
            return list;
        }

        /// <summary>
        /// 尝试用歌曲名称在网易云音乐中搜索此歌曲并返回歌曲ID
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetWYIdByName(string name)
        {
            var ds = "{\"data\":" + HttpHelper.PostWeb("http://lab.mkblog.cn/music/api.php", "types=search&count=20&source=netease&pages=1&name=" + Uri.EscapeDataString(name), HttpHelper.GetWebHeader_MKBlog()) + "}";
            var s = JObject.Parse(ds);
            return s["data"][0]["id"].ToString();
        }
        #endregion
        #region 歌单相关 我的歌单(歌单操作 删除|添加)|分类歌单
        #region 我的歌单
        #region 歌单操作 删除|添加
        /// <summary>
        /// 向歌单里添加一首歌
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dirid"></param>
        /// <returns></returns>
        public static string[] AddMusicToGD(string id, string dirid)
        {
            Console.WriteLine(Settings.USettings.Cookie + "   " + Settings.USettings.g_tk);
            string result = HttpHelper.PostWeb("https://c.y.qq.com/splcloud/fcgi-bin/fcg_music_add2songdir.fcg?g_tk=" + Settings.USettings.g_tk,
                $"loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.post&needNewCode=0&uin={Settings.USettings.LemonAreeunIts}&midlist={id}&typelist=13&dirid={dirid}&addtype=&formsender=4&source=153&r2=0&r3=1&utf8=1&g_tk=" + Settings.USettings.g_tk, HttpHelper.GetWebHeader_YQQCOM());
            //添加本地缓存
            JObject o = JObject.Parse(result);
            string msg = o["msg"].ToString();
            string title = o["title"].ToString();
            return new string[2] { msg, title };
        }
        /// <summary>
        /// 向歌单里批量添加歌曲
        /// </summary>
        /// <param name="ids"> 每一个id用","隔开</param>
        /// <param name="dirid"></param>
        /// <param name="typelist"></param>
        /// <returns></returns>
        public static string[] AddMusicToGDPL(string ids, string dirid, string typelist)
        {
            Console.WriteLine(Settings.USettings.Cookie + "   " + Settings.USettings.g_tk);
            string result = HttpHelper.PostWeb("https://c.y.qq.com/splcloud/fcgi-bin/fcg_music_add2songdir.fcg?g_tk=" + Settings.USettings.g_tk,
                $"loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.post&needNewCode=0&uin={Settings.USettings.LemonAreeunIts}&midlist={ids}&typelist={typelist}&dirid={dirid}&addtype=&formsender=4&source=153&r2=0&r3=1&utf8=1&g_tk=" + Settings.USettings.g_tk, HttpHelper.GetWebHeader_YQQCOM());
            //添加本地缓存
            JObject o = JObject.Parse(result);
            string msg = o["msg"].ToString();
            string title = o["title"].ToString();
            return new string[2] { msg, title };
        }
        /// <summary>
        /// 从我的歌单里删除一首歌
        /// </summary>
        /// <param name="Musicid"></param>
        /// <param name="Dissid"></param>
        /// <param name="dirid"></param>
        /// <returns></returns>
        public static string DeleteMusicFromGD(string Musicid, string Dissid, string dirid)
        {
            string result = HttpHelper.PostWeb("https://c.y.qq.com/qzone/fcg-bin/fcg_music_delbatchsong.fcg?g_tk=" + Settings.USettings.g_tk,
                $"loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.post&needNewCode=0&uin={Settings.USettings.LemonAreeunIts}&dirid={dirid}&ids={Musicid}&source=103&types=3&formsender=4&flag=2&from=3&utf8=1&g_tk=" + Settings.USettings.g_tk, HttpHelper.GetWebHeader_YQQCOM());
            string ok = JObject.Parse(result)["msg"].ToString();
            return ok;
        }

        /// <summary>
        /// 在我的歌单中 通过名称获取dirID
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<string> GetGDdiridByNameAsync(string name)
        {
            Console.WriteLine(Settings.USettings.LemonAreeunIts);
            JObject o = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/splcloud/fcgi-bin/songlist_list.fcg?utf8=1&-=MusicJsonCallBack&uin={Settings.USettings.LemonAreeunIts}&rnd=0.693477705380313&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0"));
            foreach (var a in o["list"])
            {
                string st = HttpUtility.HtmlDecode(a["dirname"].ToString());
                Console.WriteLine(st);
                if (name == st)
                    return a["dirid"].ToString();
            }
            return "null";
        }

        /// <summary>
        /// 收藏一份歌单
        /// </summary>
        /// <param name="dissid"></param>
        /// <returns></returns>
        public static string AddGDILike(string dissid)
        {
            string result = HttpHelper.PostWeb("https://c.y.qq.com/folder/fcgi-bin/fcg_qm_order_diss.fcg?g_tk=" + Settings.USettings.g_tk,
                $"loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=fs&inCharset=GB2312&outCharset=utf8&notice=0&platform=yqq&needNewCode=0&g_tk={Settings.USettings.g_tk}&uin={Settings.USettings.LemonAreeunIts}&dissid={dissid}&from=1&optype=1&utf8=1&qzreferrer=https%3A%2F%2Fy.qq.com%2Fn%2Fyqq%2Fplaysquare%2F{dissid}.html%23stat%3Dy_new.playlist.pic_click", HttpHelper.GetWebHeader_YQQCOM());
            return result;
        }
        /// <summary>
        /// 取消收藏一份歌单
        /// </summary>
        /// <param name="dissid"></param>
        /// <returns></returns>
        public static string DelGDILike(string dissid)
        {
            string result = HttpHelper.PostWeb("https://c.y.qq.com/folder/fcgi-bin/fcg_qm_order_diss.fcg?g_tk=" + Settings.USettings.g_tk,
                $"loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=fs&inCharset=GB2312&outCharset=gb2312&notice=0&platform=yqq&needNewCode=0&g_tk={Settings.USettings.g_tk}&uin={Settings.USettings.LemonAreeunIts}&ordertype=0&optype=2&dissid={dissid}&from=1", HttpHelper.GetWebHeader_YQQCOM());
            return result;
        }
        /// <summary>
        /// 新建一个歌单
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string AddNewGd(string name)
        {
            string result = HttpHelper.PostWeb("https://c.y.qq.com/splcloud/fcgi-bin/create_playlist.fcg?g_tk=" + Settings.USettings.g_tk,
                $"loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=fs&inCharset=GB2312&outCharset=utf8&notice=0&platform=yqq&needNewCode=0&g_tk={Settings.USettings.g_tk}&uin={Settings.USettings.LemonAreeunIts}&name={HttpUtility.UrlEncode(name)}&description=&show=1&pic_url=&tags=&tagids=&formsender=1&utf8=1&qzreferrer=https%3A%2F%2Fy.qq.com%2Fportal%2Fprofile.html%23sub%3Dother%26tab%3Dcreate%26stat%3Dy_new.top.user_pic", HttpHelper.GetWebHeader_YQQCOM());
            return result;
        }
        /// <summary>
        /// 删除一个歌单
        /// </summary>
        /// <param name="dirid"></param>
        /// <returns></returns>
        public static string DeleteGdById(string dirid)
        {
            string result = HttpHelper.PostWeb("https://c.y.qq.com/splcloud/fcgi-bin/fcg_fav_modsongdir.fcg?g_tk=" + Settings.USettings.g_tk,
                $"loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=fs&inCharset=GB2312&outCharset=gb2312&notice=0&platform=yqq&needNewCode=0&g_tk={Settings.USettings.g_tk}&uin={Settings.USettings.LemonAreeunIts}&delnum=1&deldirids={dirid}&forcedel=1&formsender=1&source=103", HttpHelper.GetWebHeader_YQQCOM());
            return result;
        }
        #endregion
        #region  歌单数据的获取
        /// <summary>
        /// 获取“我喜欢”的歌单ID
        /// </summary>
        public async void GetMusicLikeGDid()
        {
            string dta = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={Settings.USettings.LemonAreeunIts}&reqfrom=1&reqtype=0");
            JObject o = JObject.Parse(dta);
            string id = "";
            foreach (var a in o["data"]["mymusic"]) {
                if (a["title"].ToString() == "我喜欢") {
                    id = a["id"].ToString();
                    break;
                }
            }
            MusicLikeGDid = id;
            Console.WriteLine("kkkkkkkkkkkkkkk" + MusicLikeGDid);
            MusicLikeGDdirid = await GetGDdiridByNameAsync("我喜欢");
        }
        /// <summary>
        /// 通过歌单ID 获取其中的歌曲和歌单信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        /// <param name="wx"></param>
        /// <param name="getAll"></param>
        /// <returns></returns>
        public static async Task<MusicGData> GetGDAsync(string id = "2591355982", Action<Music, bool> callback = null, Window wx = null,Action<int> getAll=null)
        {
            var s = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/qzone/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&json=1&utf8=1&onlysong=0&disstid={id}&format=json&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0", Encoding.UTF8);
            JObject o = JObject.Parse(s);
            var dt = new MusicGData();
            var c0 = o["cdlist"][0];
            dt.name = c0["dissname"].ToString();
            dt.pic = c0["logo"].ToString().Replace("http://", "https://");
            dt.id = id;
            dt.ids = c0["songids"].ToString().Split(',').ToList();
            dt.IsOwn = c0["login"].ToString() == c0["uin"].ToString();
            var c0s = c0["songlist"];
            await wx.Dispatcher.BeginInvoke(new Action(() => getAll(c0s.Count())));
            try
            {
                foreach (var c0si in c0s)
                {
                    string singer = "";
                    var c0sis = c0si["singer"];
                    List<MusicSinger> lm = new List<MusicSinger>();
                    foreach (var cc in c0sis)
                    {
                        singer += cc["name"].ToString() + "&";
                        lm.Add(new MusicSinger()
                        {
                            Name = cc["name"].ToString(),
                            Mid = cc["mid"].ToString()
                        });
                    }
                    Music m = new Music();
                    m.MusicName = c0si["songname"].ToString();
                    m.MusicName_Lyric = c0si["albumdesc"].ToString();
                    m.Singer = lm;
                    m.SingerText = singer.Substring(0, singer.Length - 1);
                    m.MusicID = c0si["songmid"].ToString();
                    var amid = c0si["albummid"].ToString();
                    if (amid == "001ZaCQY2OxVMg")
                        m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R300x300M000{c0si["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                    else if (amid == "") m.ImageUrl = $"https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000";
                    else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{amid}.jpg?max_age=2592000";
                    dt.Data.Add(m);
                    await wx.Dispatcher.BeginInvoke(new Action(() => { callback(m, dt.IsOwn); }));
                    await Task.Delay(1);
                }
            }
            catch { }
            return dt;
        }
        /// <summary>
        /// 获取 我创建的歌单 列表
        /// </summary>
        /// <returns></returns>
        public async Task<SortedDictionary<string, MusicGData>> GetGdListAsync()
        {
            if(Settings.USettings.LemonAreeunIts=="")
                return new SortedDictionary<string, MusicGData>();
            var dt = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={Settings.USettings.LemonAreeunIts}&reqfrom=1&reqtype=0");
            var o = JObject.Parse(dt);
            var data = new SortedDictionary<string, MusicGData>();
            var dx = o["data"]["mydiss"]["list"];
            foreach (var ex in dx)
            {
                var df = new MusicGData();
                df.id = ex["dissid"].ToString();
                df.name = ex["title"].ToString();
                if (ex["picurl"].ToString() != "")
                    df.pic = ex["picurl"].ToString().Replace("http://", "https://");
                else df.pic = "https://y.gtimg.cn/mediastyle/global/img/cover_playlist.png?max_age=31536000";
                data.Add(df.id, df);
            }
            return data;
        }
        /// <summary>
        /// 获取 我收藏的歌单 列表
        /// </summary>
        /// <returns></returns>
        public async Task<SortedDictionary<string, MusicGData>> GetGdILikeListAsync()
        {
            var dt = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/fav/fcgi-bin/fcg_get_profile_order_asset.fcg?g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&ct=20&cid=205360956&userid={Settings.USettings.LemonAreeunIts}&reqtype=3&sin=0&ein=25");
            var o = JObject.Parse(dt);
            var data = new SortedDictionary<string, MusicGData>();
            var dx = o["data"]["cdlist"];
            foreach (var ex in dx)
            {
                var df = new MusicGData();
                df.id = ex["dissid"].ToString();
                df.name = ex["dissname"].ToString();
                if (ex["logo"].ToString() != "")
                    df.pic = ex["logo"].ToString().Replace("http://","https://");
                else df.pic = "https://y.gtimg.cn/mediastyle/global/img/cover_playlist.png?max_age=31536000";
                data.Add(df.id, df);
            }
            return data;
        }
        #endregion
        #endregion
        #region 分类歌单
        /// <summary>
        /// 通过分类Tag 获取歌单列表
        /// </summary>
        /// <param name="id">分类Tag</param>
        /// <param name="sortId">最新:2  推荐:5 </param>
        /// <param name="osx"></param>
        /// <returns></returns>
        public async Task<List<MusicGD>> GetFLGDAsync(string id,string sortId="5",int osx=1)
        {
            int start = (osx - 1) * 30;
            int end = start + 29;
            var o = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/splcloud/fcgi-bin/fcg_get_diss_by_tag.fcg?picmid=1&rnd=0.38615680484561965&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&categoryId={id}&sortId={sortId}&sin={start}&ein={end}", Encoding.UTF8));
            var data = new List<MusicGD>();
            int i = 0;
            var dl = o["data"]["list"];
            while (i < dl.Count())
            {
                var dli = dl[i];
                data.Add(new MusicGD
                {
                    Name = dli["dissname"].ToString(),
                    Photo = dli["imgurl"].ToString().Replace("http://", "https://"),//不知为何，不用https就会报404错误,
                    ID = dli["dissid"].ToString()
                });
                i++;
            }
            return data;
        }
        /// <summary>
        /// 获取分类歌单的分类Tag
        /// </summary>
        /// <returns></returns>
        public async Task<MusicFLGDIndexItemsList> GetFLGDIndexAsync()
        {
            var o = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/splcloud/fcgi-bin/fcg_get_diss_tag_conf.fcg?g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0", Encoding.UTF8));
            var data = new MusicFLGDIndexItemsList();
            data.Hot.Add(new MusicFLGDIndexItems { id = "10000000", name = "全部" });
            int i = 0;
            var dc = o["data"]["categories"];
            while (i < dc[1]["items"].Count())
            {
                var dci = dc[1]["items"][i];
                data.Lauch.Add(new MusicFLGDIndexItems
                {
                    id = dci["categoryId"].ToString(),
                    name = dci["categoryName"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < dc[2]["items"].Count())
            {
                var dci = dc[2]["items"][i];
                data.LiuPai.Add(new MusicFLGDIndexItems
                {
                    id = dci["categoryId"].ToString(),
                    name = dci["categoryName"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < dc[3]["items"].Count())
            {
                var dci = dc[3]["items"][i];
                data.Theme.Add(new MusicFLGDIndexItems
                {
                    id = dci["categoryId"].ToString(),
                    name = dci["categoryName"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < dc[4]["items"].Count())
            {
                var dci = dc[4]["items"][i];
                data.Heart.Add(new MusicFLGDIndexItems
                {
                    id = dci["categoryId"].ToString(),
                    name = dci["categoryName"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < dc[5]["items"].Count())
            {
                var dci = dc[5]["items"][i];
                data.Changjing.Add(new MusicFLGDIndexItems
                {
                    id = dci["categoryId"].ToString(),
                    name = dci["categoryName"].ToString()
                });
                i++;
            }
            return data;
        }
        #endregion
        #region 网易云音乐
        /// <summary>
        /// 从网易云音乐中导入歌单
        /// </summary>
        /// <param name="id"></param>
        /// <param name="x"></param>
        /// <param name="tb"></param>
        /// <param name="pb"></param>
        /// <param name="Finished"></param>
        public async void GetGDbyWYAsync(string id, Window x, TextBlock tb, ProgressBar pb, Action Finished)
        {
            string data = await HttpHelper.GetWebAsync($"http://music.163.com/api/playlist/detail?id={id}&updateTime=-1");
            JObject o = JObject.Parse(data);
            var dt = new MusicGData();
            string ids = "";
            string typelist = "";
            var pl = o["result"];
            dt.name = pl["name"].ToString();
            dt.id = pl["id"].ToString();
            dt.pic = pl["coverImgUrl"].ToString();
            var pl_t = pl["tracks"];
            x.Dispatcher.Invoke(() => { pb.Maximum = pl_t.Count(); });
            int i = 1;
            foreach (var pl_t_i in pl_t)
            {
                var dtname = pl_t_i["name"].ToString();
                var dtsinger = "";
                var pl_t_i_ar = pl_t_i["artists"];
                for (int dx = 0; dx != pl_t_i_ar.Count(); dx++)
                    dtsinger += pl_t_i_ar[0]["name"] + "&";
                dtsinger = dtsinger.Substring(0, dtsinger.LastIndexOf("&"));
                var dtf = await SearchMusicAsync(dtname + "-" + dtsinger);
                if (dtf.Count > 0)
                {
                    var dtv = dtf[0];
                    dt.Data.Add(dtv);
                    ids += dtv.MusicID + ",";
                    typelist += "13,";
                    x.Dispatcher.Invoke(() => { pb.Value = i; tb.Text = dtv.MusicName + " - " + dtv.Singer; });
                }
                else x.Dispatcher.Invoke(() => { pb.Value--; });
                i++;
            }
            ids = ids.Substring(0, ids.LastIndexOf(","));
            typelist = typelist.Substring(0, typelist.LastIndexOf(","));
            Console.WriteLine("ids:" + ids);
            AddNewGd(dt.name);
            await Task.Delay(500);
            string dir = await GetGDdiridByNameAsync(dt.name);
            Console.WriteLine("dirId" + dir);
            var amt = AddMusicToGDPL(ids, dir, typelist);
            Console.WriteLine(amt[0] + amt[1]);
            x.Dispatcher.Invoke(() =>
            {
                Finished();
            });
        }

        #endregion
        #endregion
        #region 播放相关 获取链接|播放音乐
        /// <summary>
        /// 获取歌曲播放链接
        /// </summary>
        /// <param name="Musicid"></param>
        /// <returns></returns>
        public static async Task<string> GetUrlAsync(string Musicid)
        {
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create("https://i.y.qq.com/v8/playsong.html?songmid=0013KcQ72u8FY7,0011jIhY1wP6wB");
            hwr.Timeout = 20000;
            hwr.KeepAlive = true;
            hwr.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
            hwr.Headers.Add(HttpRequestHeader.Upgrade, "1");
            hwr.UserAgent = "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3854.3 Mobile Safari/537.36";
            hwr.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            hwr.Referer = "https://i.y.qq.com/n2/m/share/details/album.html?albummid=003bBofB3UzHxS&ADTAG=myqq&from=myqq&channel=10007100";
            hwr.Host = "i.y.qq.com";
            hwr.Headers.Add("sec-fetch-mode", "navigate");
            hwr.Headers.Add("sec-fetch-site", "same - origin");
            hwr.Headers.Add("sec-fetch-user", "?1");
            hwr.Headers.Add("upgrade-insecure-requests", "1");
            hwr.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9");
            hwr.Headers.Add(HttpRequestHeader.Cookie, Settings.USettings.Cookie);
            var o = await hwr.GetResponseAsync();
            StreamReader sr = new StreamReader(o.GetResponseStream(), Encoding.UTF8);
            var st = await sr.ReadToEndAsync();
            sr.Dispose();
            Console.WriteLine(st);
            string vk = TextHelper.XtoYGetTo(st, "http://aqqmusic.tc.qq.com/amobile.music.tc.qq.com/C4000013KcQ72u8FY7.m4a", "&fromtag=38", 0);
            Console.WriteLine(vk);
            return $"http://aqqmusic.tc.qq.com/amobile.music.tc.qq.com/C400{Musicid}.m4a" + vk + "&fromtag=38";

            /*  能用 但无法播放受限的歌曲
            var guid = "82800236CAC506A09113040688E3F47F";
            var vkey = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/base/fcgi-bin/fcg_music_express_mobile3.fcg?g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205361747&uin={Settings.USettings.LemonAreeunIts}&songmid={Musicid}&filename=M500{Musicid}.mp3&guid={guid}"))["data"]["items"][0]["vkey"].ToString();
            if (vkey != "")
                return $"https://dl.stream.qqmusic.qq.com/M500{Musicid}.mp3?vkey={vkey}&guid={guid}&uin={Settings.USettings.LemonAreeunIts}&fromtag=66";
            else return null;
            */

            /*   已失效 CODE403
            List<String[]> MData = new List<String[]>();
            MData.Add(new String[] { "M800", "mp3" });
            MData.Add(new String[] { "C600", "m4a" });
            MData.Add(new String[] { "M500", "mp3" });
            MData.Add(new String[] { "C400", "m4a" });
            MData.Add(new String[] { "M200", "mp3" });
            MData.Add(new String[] { "M100", "mp3" });

            var guid = "82800236CAC506A09113040688E3F47F";
            var mid = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg?songmid={Musicid}&platform=yqq&format=json"))["data"][0]["file"]["media_mid"].ToString();
            for (int i = 0; i < MData.Count; i++)
            {
                String[] datakey = MData[i];
                var key = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/base/fcgi-bin/fcg_musicexpress.fcg?json=3&guid={guid}&format=json"))["key"].ToString();
                string uri = $"https://dl.stream.qqmusic.qq.com/{datakey[0]}{mid}.{datakey[1]}?vkey={key}&guid={guid}&uid=0&fromtag=30";
                if (await HttpHelper.GetWebCode(uri) == 200)
                    return uri;
            }
            return "http://ws.stream.qqmusic.qq.com/C100" + mid + ".m4a?fromtag=0&guid=" + guid;
        */
        }
        /// <summary>
        /// 播放音乐
        /// </summary>
        /// <param name="mid"></param>
        /// <param name="openlyric"></param>
        /// <param name="x"></param>
        /// <param name="s"></param>
        /// <param name="songname"></param>
        /// <param name="doesplay"></param>
        public async void GetAndPlayMusicUrlAsync(string mid, Boolean openlyric, Run x, Window s, string songname, bool doesplay = true)
        {
            string downloadpath = Settings.USettings.CachePath + "Music\\" + mid + ".mp3";
            if (!File.Exists(downloadpath))
            {
                string musicurl = "";
                musicurl = await GetUrlAsync(mid);
                WebClient dc = new WebClient();
                dc.DownloadFileCompleted += delegate
                {
                    dc.Dispose();
                    mp.Open(new Uri(downloadpath));
                    if (doesplay)
                        mp.Play();
                    s.Dispatcher.Invoke(DispatcherPriority.Normal, new System.Windows.Forms.MethodInvoker(delegate ()
                    {
                        x.Text = TextHelper.XtoYGetTo("[" + songname, "[", " -", 0).Replace("Wy", "");
                    }));
                };
                dc.DownloadFileAsync(new Uri(musicurl), downloadpath);
                dc.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e)
                {
                    s.Dispatcher.Invoke(DispatcherPriority.Normal, new System.Windows.Forms.MethodInvoker(delegate ()
                    {
                        x.Text = "加载中..." + e.ProgressPercentage + "%";
                    }));
                };
            }
            else
            {
                mp.Open(new Uri(downloadpath));
                if (doesplay)
                    mp.Play();
                s.Dispatcher.Invoke(DispatcherPriority.Normal, new System.Windows.Forms.MethodInvoker(delegate ()
                {
                    x.Text = TextHelper.XtoYGetTo("[" + songname, "[", " -", 0).Replace("Wy", "");
                }));
            }
            if (openlyric)
            {
                string dt = await GetLyric(mid);
                lv.LoadLrc(dt);
            }
        }
        #endregion
        #region 歌词 获取|处理
        /// <summary>
        /// 处理歌词 将歌词写入文件里
        /// </summary>
        /// <param name="t"></param>
        /// <param name="x"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private static string PushLyric(string t, string x, string file)
        {
            List<string> datatime = new List<string>();
            List<string> datatext = new List<string>();
            Dictionary<string, string> gcdata = new Dictionary<string, string>();
            string[] dta = t.Split('\n');
            foreach (var dt in dta)
                LyricView.parserLine(dt, datatime, datatext, gcdata);
            List<String> dataatimes = new List<String>();
            List<String> dataatexs = new List<String>();
            Dictionary<String, String> fydata = new Dictionary<String, String>();
            String[] dtaa = x.Split('\n');
            foreach (var dt in dtaa)
                LyricView.parserLine(dt, dataatimes, dataatexs, fydata);
            List<String> KEY = new List<String>();
            Dictionary<String, String> gcfydata = new Dictionary<String, String>();
            Dictionary<String, String> list = new Dictionary<String, String>();
            foreach (var dt in datatime)
            {
                KEY.Add(dt);
                gcfydata.Add(dt, "");
            }
            for (int i = 0; i != gcfydata.Count; i++)
            {
                if (fydata.ContainsKey(KEY[i]))
                    gcfydata[KEY[i]] = (gcdata[KEY[i]] + "^" + fydata[KEY[i]]).Replace("\n", "").Replace("\r", "");
                else
                {
                    string dt = LyricView.YwY(KEY[i], 1);
                    if (fydata.ContainsKey(dt))
                        gcfydata[KEY[i]] = (gcdata[KEY[i]] + "^" + fydata[dt]).Replace("\n", "").Replace("\r", "");
                    else gcfydata[KEY[i]] = (gcdata[KEY[i]] + "^").Replace("\n", "").Replace("\r", "");
                }
            }
            string LyricData = "";
            for (int i = 0; i != KEY.Count; i++)
            {
                String value = gcfydata[KEY[i]].Replace("[", "").Replace("]", "");
                String key = KEY[i];
                LyricData += $"[{key}]{value}\r\n";
            }
            File.WriteAllText(file, LyricData);
            return LyricData;
        }
        /// <summary>
        /// 获取歌词
        /// </summary>
        /// <param name="McMind"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task<string> GetLyric(string McMind,string file="")
        {
            if(file=="") file=Settings.USettings.CachePath + "Lyric\\" + McMind + ".lrc";
            if (!File.Exists(file))
            {
                WebClient c = new WebClient();
                c.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.110 Safari/537.36");
                c.Headers.Add("Accept", "*/*");
                c.Headers.Add("Referer", "https://y.qq.com/portal/player.html");
                c.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
                c.Headers.Add("Cookie",Settings.USettings.Cookie);
                c.Headers.Add("Host", "c.y.qq.com");
                string url = $"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?-=MusicJsonCallback_lrc&pcachetime=1563410858607&songmid={McMind}&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0";
                Console.WriteLine(url);
                string td = c.DownloadString(url);
                Console.WriteLine(td);
                JObject o = JObject.Parse(td);
                string t = Encoding.UTF8.GetString(Convert.FromBase64String(o["lyric"].ToString())).Replace("&apos;", "\'");
                if (o["trans"].ToString() == "") { await Task.Run(() => { File.WriteAllText(file, t); }); return t; }
                else
                {
                    string x = Encoding.UTF8.GetString(Convert.FromBase64String(o["trans"].ToString())).Replace("&apos;", "\'");
                    return PushLyric(t, x, file);
                }
            }
            else
                return File.ReadAllText(file);
        }
        #endregion
        #region 排行榜
        /// <summary>
        /// 排行榜列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<MusicTop>> GetTopIndexAsync()
        {
            var dt = await HttpHelper.GetWebAsync("https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_opt.fcg?page=index&format=html&tpl=macv4&v8debug=1");
            var sh = "{\"data\":" + dt.Replace("jsonCallback(", "").Replace("}]\n)", "") + "}]" + "}";
            var o = JObject.Parse(sh);
            var data = new List<MusicTop>();
            int i = 0;
            var d0l = o["data"][0]["List"];
            foreach(var d in d0l)
            {
                List<string> content = new List<string>();
                foreach (var a in d["songlist"])
                    content.Add(a["songname"] + " - " + a["singername"]);
                data.Add(new MusicTop
                {
                    Name = d["ListName"].ToString(),
                    Photo = d["pic_v12"].ToString(),
                    ID = d["topID"].ToString(),
                    content=content
                });
                i++;
            }
            i = 0;
            var d1li = o["data"][1]["List"];
            foreach(var d in d1li)
            {
                List<string> content = new List<string>();
                foreach (var a in d["songlist"])
                    content.Add(a["songname"] + " - " + a["singername"]);
                data.Add(new MusicTop
                {
                    Name = d["ListName"].ToString(),
                    Photo = d["pic_v12"].ToString(),
                    ID = d["topID"].ToString(),
                    content=content
                });
                i++;
            }
            return data;
        }
        /// <summary>
        /// 排行榜里的音乐
        /// </summary>
        /// <param name="TopID"></param>
        /// <param name="osx"></param>
        /// <returns></returns>
        public async Task<List<Music>> GetToplistAsync(string TopID, int osx = 1)
        {
            int index = (osx - 1) * 30;
            JObject o = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_cp.fcg?tpl=3&page=detail&topid={TopID}&type=top&song_begin={index}&song_num=30&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"));
            List<Music> dt = new List<Music>();
            int i = 0;
            var s = o["songlist"];
            while (i < s.Count())
            {
                var sid = s[i]["data"];
                Music m = new Music();
                m.MusicName = sid["songname"].ToString();
                m.MusicName_Lyric = sid["albumdesc"].ToString();
                string Singer = "";
                List<MusicSinger> lm = new List<MusicSinger>();
                for (int osxc = 0; osxc != sid["singer"].Count(); osxc++)
                {
                    Singer += sid["singer"][osxc]["name"] + "&";
                    lm.Add(new MusicSinger() { Name = sid["singer"][osxc]["name"].ToString(),
                        Mid = sid["singer"][osxc]["mid"].ToString()
                    });
                }
                m.Singer = lm;
                m.SingerText = Singer.Substring(0, Singer.LastIndexOf("&"));
                m.MusicID = sid["songmid"].ToString();
                var amid = sid["albummid"].ToString();
                if (amid == "001ZaCQY2OxVMg")
                    m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R300x300M000{sid["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                else if (amid == "") m.ImageUrl = $"https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000";
                else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{amid}.jpg?max_age=2592000";
                dt.Add(m);
                i++;
            }
            return dt;
        }
        #endregion
        #region 歌手
        /// <summary>
        /// 歌手列表
        /// </summary>
        /// <param name="index">ABCD 按照此值在26字母表+#中的第几位 默认值-100</param>
        /// <param name="area">地区 全部:-100 内地:200 港台:2 欧美:5 日本:4 韩国:3 其他:6</param>
        /// <param name="sex">男 0 女 1 组合 2 全部 -100</param>
        /// <param name="genre">全部-100 流行1 嘻哈6 摇滚2 电子4  民谣3 R&B 8 民歌10 轻音乐 9 爵士5 古典14 乡村 25 蓝调20</param>
        /// <param name="sin">80*(页数-1)</param>
        /// <param name="cur_page">页数</param>
        /// <returns></returns>
        public static async Task<List<MusicSinger>> GetSingerListAsync(string index,string area,string sex,string genre,string sin,int cur_page)
        {
            var o = JObject.Parse(await HttpHelper.GetWebAsync($"https://u.y.qq.com/cgi-bin/musicu.fcg?-=getUCGI6639758764435573&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0&data="+
                $"%7B%22comm%22%3A%7B%22ct%22%3A24%2C%22cv%22%3A0%7D%2C%22singerList%22%3A%7B%22module%22%3A%22Music.SingerListServer%22%2C%22method%22%3A%22get_singer_list%22%2C%22param%22%3A%7B%22area%22%3A{area}%2C%22sex%22%3A{sex}%2C%22genre%22%3A{genre}%2C%22index%22%3A{index}%2C%22sin%22%3A{sin}%2C%22cur_page%22%3A{cur_page}%7D%7D%7D"));
            var data = new List<MusicSinger>();
            int i = 0;
            var dl = o["singerList"]["data"]["singerlist"];
            foreach(var dli in dl)
            {
                data.Add(new MusicSinger
                {
                    Name = dli["singer_name"].ToString(),
                    Mid=dli["singer_mid"].ToString(),
                    Photo = $"https://y.gtimg.cn/music/photo_new/T001R150x150M000{dli["singer_mid"]}.jpg?max_age=2592000"
                });
                i++;
            }
            return data;
        }
        /// <summary>
        /// 通过歌手ID获取此歌手的音乐
        /// </summary>
        /// <param name="mid"></param>
        /// <param name="osx"></param>
        /// <returns></returns>
        public async Task<List<Music>> GetSingerMusicByIdAsync(string mid,int osx=1) {
            int begin = (osx - 1) * 30;
            JObject o = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_v8_singer_track_cp.fcg?hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0&ct=24&singermid={mid}&order=listen&begin={begin}&num=30&songstatus=1"));
            List<Music> dt = new List<Music>();
            JToken dtl = o["data"]["list"];
            foreach (JToken dtli in dtl) {
                var dsli = dtli["musicData"];
                Music m = new Music();
                m.MusicName = dsli["songname"].ToString();
                m.MusicName_Lyric = dsli["albumdesc"].ToString();
                string Singer = "";
                List<MusicSinger> lm = new List<MusicSinger>();
                for (int osxc = 0; osxc != dsli["singer"].Count(); osxc++) {
                    Singer += dsli["singer"][osxc]["name"] + "&";
                    lm.Add(new MusicSinger() { Name = dsli["singer"][osxc]["name"].ToString(), Mid= dsli["singer"][osxc]["mid"].ToString() });
                }
                m.Singer = lm;
                m.SingerText = Singer.Substring(0, Singer.LastIndexOf("&"));
                m.MusicID = dsli["songmid"].ToString();
                var amid = dsli["albummid"].ToString();
                if (amid == "001ZaCQY2OxVMg")
                    m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R300x300M000{dsli["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                else if (amid == "") m.ImageUrl = $"https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000";
                else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{amid}.jpg?max_age=2592000";
                dt.Add(m);
            }
            return dt;
        }
        #endregion
        #region 电台
        public async Task<MusicRadioList> GetRadioList()
        {
            var o = JObject.Parse(await HttpHelper.GetWebAsync("https://c.y.qq.com/v8/fcg-bin/fcg_v8_radiolist.fcg?channel=radio&format=json&page=index&tpl=wk&new=1&p=0.8663229811059507&g_tk=5381&loginUin=0&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"));
            var data = new MusicRadioList();
            var ddg = o["data"]["data"]["groupList"];
            try
            {
                int i = 0;
                while (i < ddg[0]["radioList"].Count())
                {
                    var ddgri = ddg[0]["radioList"][i];
                    data.Hot.Add(new MusicRadioListItem
                    {
                        Name = ddgri["radioName"].ToString(),
                        Photo = ddgri["radioImg"].ToString(),
                        ID = ddgri["radioId"].ToString()
                    });
                    i++;
                }
                i = 0;
                while (i < ddg[1]["radioList"].Count())
                {
                    var ddgri = ddg[1]["radioList"][i];
                    data.Evening.Add(new MusicRadioListItem
                    {
                        Name = ddgri["radioName"].ToString(),
                        Photo = ddgri["radioImg"].ToString(),
                        ID = ddgri["radioId"].ToString()
                    });
                    i++;
                }
                i = 0;
                while (i < ddg[2]["radioList"].Count())
                {
                    var ddgri = ddg[2]["radioList"][i];
                    data.Love.Add(new MusicRadioListItem
                    {
                        Name = ddgri["radioName"].ToString(),
                        Photo = ddgri["radioImg"].ToString(),
                        ID = ddgri["radioId"].ToString()
                    });
                    i++;
                }
                i = 0;
                while (i < ddg[3]["radioList"].Count())
                {
                    var ddgri = ddg[3]["radioList"][i];
                    data.Theme.Add(new MusicRadioListItem
                    {
                        Name = ddgri["radioName"].ToString(),
                        Photo = ddgri["radioImg"].ToString(),
                        ID = ddgri["radioId"].ToString()
                    });
                    i++;
                }
                i = 0;
                while (i < ddg[4]["radioList"].Count())
                {
                    var ddgri = ddg[4]["radioList"][i];
                    data.Changjing.Add(new MusicRadioListItem
                    {
                        Name = ddgri["radioName"].ToString(),
                        Photo = ddgri["radioImg"].ToString(),
                        ID = ddgri["radioId"].ToString()
                    });
                    i++;
                    i++;
                }
                i = 0;
                while (i < ddg[5]["radioList"].Count())
                {
                    var ddgri = ddg[4]["radioList"][i];
                    data.Style.Add(new MusicRadioListItem
                    {
                        Name = ddgri["radioName"].ToString(),
                        Photo = ddgri["radioImg"].ToString(),
                        ID = ddgri["radioId"].ToString()
                    });
                    i++;
                    i++;
                }
                i = 0;
                while (i < ddg[6]["radioList"].Count())
                {
                    var ddgri = ddg[6]["radioList"][i];
                    data.Lauch.Add(new MusicRadioListItem
                    {
                        Name = ddgri["radioName"].ToString(),
                        Photo = ddgri["radioImg"].ToString(),
                        ID = ddgri["radioId"].ToString()
                    });
                    i++;
                    i++;
                }
                i = 0;
                while (i < ddg[7]["radioList"].Count())
                {
                    var ddgri = ddg[7]["radioList"][i];
                    data.People.Add(new MusicRadioListItem
                    {
                        Name = ddgri["radioName"].ToString(),
                        Photo = ddgri["radioImg"].ToString(),
                        ID = ddgri["radioId"].ToString()
                    });
                    i++;
                    i++;
                }
                i = 0;
                while (i < ddg[8]["radioList"].Count())
                {
                    var ddgri = ddg[8]["radioList"][i];
                    data.MusicTools.Add(new MusicRadioListItem
                    {
                        Name = ddgri["radioName"].ToString(),
                        Photo = ddgri["radioImg"].ToString(),
                        ID = ddgri["radioId"].ToString()
                    });
                    i++;
                    i++;
                }
                i = 0;
                while (i < ddg[9]["radioList"].Count())
                {
                    var ddgri = ddg[9]["radioList"][i];
                    data.Diqu.Add(new MusicRadioListItem
                    {
                        Name = ddgri["radioName"].ToString(),
                        Photo = ddgri["radioImg"].ToString(),
                        ID = ddgri["radioId"].ToString()
                    });
                    i++;
                    i++;
                }
            }
            catch { }
            return data;
        }
        public async Task<Music> GetRadioMusicAsync(string id)
        {
            if (id == "99")
            {
                var o = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/rcmusic2/fcgi-bin/fcg_guess_youlike_pc.fcg?g_tk=1206122277&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=703&uin={Settings.USettings.LemonAreeunIts}"));
                string Singer = "";
                var s_0 = o["songlist"][0];
                var s_0_s = o["songlist"][0]["singer"];
                List<MusicSinger> lm = new List<MusicSinger>();
                for (int osxc = 0; osxc != s_0_s.Count(); osxc++){
                    Singer += s_0_s[osxc]["name"] + "&";
                    lm.Add(new MusicSinger(){
                        Name= s_0_s[osxc]["name"].ToString(),
                        Mid= s_0_s[osxc]["mid"].ToString()
                    });
                }
                var data = new Music
                {
                    MusicName = s_0["name"].ToString(),
                    SingerText = Singer.Substring(0, Singer.LastIndexOf("&")),
                    Singer = lm,
                    MusicID = s_0["mid"].ToString(),
                    ImageUrl = $"http://y.gtimg.cn/music/photo_new/T002R300x300M000{s_0["album"]["mid"]}.jpg"
                };
                return data;
            }
            else
            {
                var o = JObject.Parse(await HttpHelper.GetWebAsync($"https://u.y.qq.com/cgi-bin/musicu.fcg?g_tk=1206122277&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&data=%7B\"songlist\"%3A%7B\"module\"%3A\"pf.radiosvr\"%2C\"method\"%3A\"GetRadiosonglist\"%2C\"param\"%3A%7B\"id\"%3A{id}%2C\"firstplay\"%3A1%2C\"num\"%3A10%7D%7D%2C\"radiolist\"%3A%7B\"module\"%3A\"pf.radiosvr\"%2C\"method\"%3A\"GetRadiolist\"%2C\"param\"%3A%7B\"ct\"%3A\"24\"%7D%7D%2C\"comm\"%3A%7B\"ct\"%3A\"24\"%7D%7D"));
                string Singer = "";
                var sdt0 = o["songlist"]["data"]["track_list"][0];
                var sdt0s = sdt0["singer"];
                List<MusicSinger> lm = new List<MusicSinger>();
                for (int osxc = 0; osxc != sdt0s.Count(); osxc++){
                    Singer += sdt0s[osxc]["name"] + "&";
                    lm.Add(new MusicSinger() {
                        Name= sdt0s[osxc]["name"].ToString(),
                        Mid= sdt0s[osxc]["mid"].ToString()
                    });
                }
                var data = new Music
                {
                    MusicName = sdt0["name"].ToString(),
                    SingerText = Singer.Substring(0, Singer.LastIndexOf("&")),
                    Singer=lm,
                    MusicID = sdt0["mid"].ToString(),
                    ImageUrl = $"http://y.gtimg.cn/music/photo_new/T002R300x300M000{sdt0["album"]["mid"]}.jpg"
                };
                return data;
            }
        }
        #endregion
        #region 专辑
        /// <summary>
        /// 获取专辑音乐
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<MusicGData> GetAlbumSongListByIDAsync(string id)
        {
            string json = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_v8_album_info_cp.fcg?ct=24&albummid={id}&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0&song_begin=0&song_num=50");
            JObject o = JObject.Parse(json);
            MusicGData md = new MusicGData();
            md.pic = $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{id}.jpg?max_age=2592000";
            md.name = "专辑: <" + o["data"]["name"] + "> • " + o["data"]["singername"];
            var data = new List<Music>();
            var list = o["data"]["list"];
            foreach (var a in list)
            {
                Music m = new Music();
                m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{id}.jpg?max_age=2592000";
                m.MusicID = a["songmid"].ToString();
                m.MusicName = a["songname"].ToString();
                m.Singer = new List<MusicSinger>();
                m.SingerText = "";
                foreach (var s in a["singer"])
                {
                    m.Singer.Add(new MusicSinger() { Mid = s["mid"].ToString(), Name = s["name"].ToString() });
                    m.SingerText += s["name"].ToString() + "&";
                }
                m.SingerText = m.SingerText.Substring(0, m.SingerText.Length - 1);
                data.Add(m);
            }
            md.Data = data;
            return md;
        }

        #endregion
        #region 主页
        /// <summary>
        /// 获取主页数据
        /// </summary>
        /// <returns></returns>
        public static async Task<HomePageData> GetHomePageData()
        {
            string json = await HttpHelper.GetWebAsync("https://u.y.qq.com/cgi-bin/musicu.fcg?-=recom9439610432420651&g_tk=924717510&loginUin=2728578956&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0&data=%7B%22comm%22%3A%7B%22ct%22%3A24%7D%2C%22category%22%3A%7B%22method%22%3A%22get_hot_category%22%2C%22param%22%3A%7B%22qq%22%3A%22%22%7D%2C%22module%22%3A%22music.web_category_svr%22%7D%2C%22recomPlaylist%22%3A%7B%22method%22%3A%22get_hot_recommend%22%2C%22param%22%3A%7B%22async%22%3A1%2C%22cmd%22%3A2%7D%2C%22module%22%3A%22playlist.HotRecommendServer%22%7D%2C%22playlist%22%3A%7B%22method%22%3A%22get_playlist_by_category%22%2C%22param%22%3A%7B%22id%22%3A8%2C%22curPage%22%3A1%2C%22size%22%3A40%2C%22order%22%3A5%2C%22titleid%22%3A8%7D%2C%22module%22%3A%22playlist.PlayListPlazaServer%22%7D%2C%22new_song%22%3A%7B%22module%22%3A%22newsong.NewSongServer%22%2C%22method%22%3A%22get_new_song_info%22%2C%22param%22%3A%7B%22type%22%3A5%7D%7D%2C%22new_album%22%3A%7B%22module%22%3A%22newalbum.NewAlbumServer%22%2C%22method%22%3A%22get_new_album_info%22%2C%22param%22%3A%7B%22area%22%3A1%2C%22sin%22%3A0%2C%22num%22%3A10%7D%7D%2C%22new_album_tag%22%3A%7B%22module%22%3A%22newalbum.NewAlbumServer%22%2C%22method%22%3A%22get_new_album_area%22%2C%22param%22%3A%7B%7D%7D%2C%22toplist%22%3A%7B%22module%22%3A%22musicToplist.ToplistInfoServer%22%2C%22method%22%3A%22GetAll%22%2C%22param%22%3A%7B%7D%7D%2C%22focus%22%3A%7B%22module%22%3A%22QQMusic.MusichallServer%22%2C%22method%22%3A%22GetFocus%22%2C%22param%22%3A%7B%7D%7D%7D");
            JObject o = JObject.Parse(json);
            //-----FOCUS---
            List<IFVData> focus = new List<IFVData>();
            var focus_obj = o["focus"]["data"]["content"];
            foreach (var fc in focus_obj)
            {
                IFVData iv = new IFVData(fc["pic_info"]["url"].ToString(), fc["jump_info"]["url"].ToString(), fc["type"].ToString());
                focus.Add(iv);
            }
            //----歌单推荐---
            var Gdata = new List<MusicGD>();
            var recomPlaylist_obj = o["recomPlaylist"]["data"]["v_hot"];
            foreach (var rp in recomPlaylist_obj)
            {
                MusicGD md = new MusicGD();
                md.ID = rp["content_id"].ToString();
                md.Name = rp["title"].ToString();
                md.Photo = rp["cover"].ToString().Replace("http://","https://");
                Gdata.Add(md);
            }
            //----新歌首发---
            var NewMusic = new List<Music>();
            var new_song_obj = o["new_song"]["data"]["songlist"];
            foreach (var ns in new_song_obj)
            {
                Music m = new Music();
                var amid = ns["album"]["mid"].ToString();
                if (amid == "001ZaCQY2OxVMg")
                    m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R300x300M000{ns["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                else if (amid == "") m.ImageUrl = $"https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000";
                else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{amid}.jpg?max_age=2592000";
                m.MusicID = ns["mid"].ToString();
                m.MusicName = ns["name"].ToString();
                m.MusicName_Lyric = ns["subtitle"].ToString();
                m.Singer = new List<MusicSinger>();
                m.SingerText = "";
                foreach (var s in ns["singer"])
                {
                    m.Singer.Add(new MusicSinger() { Mid = s["mid"].ToString(), Name = s["name"].ToString() });
                    m.SingerText += s["name"].ToString() + "&";
                }
                m.SingerText = m.SingerText.Substring(0, m.SingerText.Length - 1);
                NewMusic.Add(m);
            }
            return new HomePageData()
            {
                focus = focus,
                Gdata = Gdata,
                NewMusic = NewMusic
            };
        }
        #endregion
        #region 评论 网易云|QQ音乐
        /// <summary>
        /// 网易云音乐评论
        /// </summary>
        /// <param name="name"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<List<MusicPL>> GetPLByWyyAsync(string name, int page = 1)
        {
            string Page = ((page - 1) * 20).ToString();
            string id = GetWYIdByName(name);
            var data = await HttpHelper.GetWebAsync($"https://music.163.com/api/v1/resource/comments/R_SO_4_{id}?offset={Page}");
            JObject o = JObject.Parse(data);
            var d = new List<MusicPL>();
            var hc = o["hotComments"];
            for (int i = 0; i != hc.Count(); i++)
            {
                var hc_i = o["hotComments"][i];
                var hc_i_u = hc_i["user"];
                d.Add(new MusicPL()
                {
                    text = hc_i["content"].ToString(),
                    name = hc_i_u["nickname"].ToString(),
                    img = hc_i_u["avatarUrl"].ToString(),
                    like = hc_i["likedCount"].ToString()
                });
            }
            return d;
        }
        /// <summary>
        /// QQ音乐评论
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public async Task<List<MusicPL>> GetPLByQQAsync(string mid)
        {
            string id = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg?songmid={mid}&tpl=yqq_song_detail&format=json&g_tk=268405378&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"))["data"][0]["id"].ToString();
            string dt = await HttpHelper.GetWebAsync($"https://c.y.qq.com/base/fcgi-bin/fcg_global_comment_h5.fcg?g_tk=642290724&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=GB2312&notice=0&platform=yqq&needNewCode=0&cid=205360772&reqtype=2&biztype=1&topid={id}&cmd=8&needmusiccrit=0&pagenum=0&pagesize=25&lasthotcommentid=&domain=qq.com&ct=24&cv=101010");
            JObject ds = JObject.Parse(dt.Replace("\n", ""));
            List<MusicPL> data = new List<MusicPL>();
            JToken hcc = ds["hot_comment"]["commentlist"];
            for (int i = 0; i != hcc.Count(); i++)
            {
                JToken hcc_i = ds["hot_comment"]["commentlist"][i];
                MusicPL mpl = new MusicPL(){
                    img =hcc_i["avatarurl"].ToString(),
                    like = hcc_i["praisenum"].ToString(),
                    name = hcc_i["nick"].ToString(),
                    text =TextHelper.Exem(hcc_i["rootcommentcontent"].ToString().Replace(@"\n", "\n")),
                    commentid = hcc_i["commentid"].ToString()
                };
                DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                long lTime = long.Parse(hcc_i["time"].ToString() + "0000000");
                TimeSpan toNow = new TimeSpan(lTime);
                DateTime daTime = dtStart.Add(toNow);
                mpl.time = daTime.ToString("yyyy-MM-dd  HH:mm");
                if (hcc_i["ispraise"].ToString() == "1")
                    mpl.ispraise = true;
                else mpl.ispraise = false;
                data.Add(mpl);
            }
            return data;
        }

        /// <summary>
        /// 通过MusicID获取Mid  例如 002GI6873Q6N6=>283749823
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public static async Task<string> GetMusicIdByMidAsync(string mid)
        {
            string st = (await HttpHelper.GetWebAsync($"https://y.qq.com/n/yqq/song/{mid}.html")).Replace(" ", "").Replace("\r\n", "");
            string json = TextHelper.XtoYGetTo(st, "<script>varg_SongData=", ";</script>", 0);
            Console.WriteLine(json);

            JObject o = JObject.Parse(json);
            return o["songid"].ToString(); ;
        }
        /// <summary>
        /// 给评论点赞(或取消)
        /// </summary>
        /// <param name="mid"></param>
        /// <param name="mp"></param>
        /// <returns></returns>
        public static async Task<string> PraiseMusicPLAsync(string mid, MusicPL mp)
        {
            string id = await GetMusicIdByMidAsync(mid);
            string get = "";
            if (mp.ispraise)
                get = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/base/fcgi-bin/fcg_global_comment_praise_h5.fcg?g_tk={Settings.USettings.LemonAreeunIts}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=GB2312&notice=0&platform=yqq.json&needNewCode=0&cid=205360774&cmd=2&reqtype=2&biztype=1&topid={id}&commentid={mp.commentid}&qq={Settings.USettings.LemonAreeunIts}&domain=qq.com&ct=24&cv=101010");
            else get = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/base/fcgi-bin/fcg_global_comment_praise_h5.fcg?g_tk={Settings.USettings.LemonAreeunIts}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=GB2312&notice=0&platform=yqq.json&needNewCode=0&cid=205360774&cmd=1&reqtype=2&biztype=1&topid={id}&commentid={mp.commentid}&qq={Settings.USettings.LemonAreeunIts}&domain=qq.com&ct=24&cv=101010");
            return get;
        }
        #endregion
    }
}