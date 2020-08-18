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
using System.Xml.Linq;
using static LemonLib.InfoHelper;
/*
   作者:Twilight./Lemon        QQ:2728578956
   请保留版权信息，侵权必究。
     
     Author:Twilight./Lemon QQ:2728578956
Please retain the copyright information, rights reserved.
     */

namespace LemonLib
{
    public class MusicLib
    {
        #region  构造函数
        public MusicLib(string id)
        {
            if (!Directory.Exists(Settings.USettings.DownloadPath))
                Directory.CreateDirectory(Settings.USettings.DownloadPath);
            if (!Directory.Exists(Settings.USettings.MusicCachePath))
                Directory.CreateDirectory(Settings.USettings.MusicCachePath);
            if (!Directory.Exists(Settings.USettings.MusicCachePath + "Music\\"))
                Directory.CreateDirectory(Settings.USettings.MusicCachePath + "Music\\");
            if (!Directory.Exists(Settings.USettings.MusicCachePath + "Lyric\\"))
                Directory.CreateDirectory(Settings.USettings.MusicCachePath + "Lyric\\");
            if (!Directory.Exists(Settings.USettings.MusicCachePath + "Image\\"))
                Directory.CreateDirectory(Settings.USettings.MusicCachePath + "Image\\");
            qq = id;
        }
        public MusicLib()
        {
            if (!Directory.Exists(Settings.USettings.DownloadPath))
                Directory.CreateDirectory(Settings.USettings.DownloadPath);
            if (!Directory.Exists(Settings.USettings.MusicCachePath))
                Directory.CreateDirectory(Settings.USettings.MusicCachePath);
            if (!Directory.Exists(Settings.USettings.MusicCachePath + "Music\\"))
                Directory.CreateDirectory(Settings.USettings.MusicCachePath + "Music\\");
            if (!Directory.Exists(Settings.USettings.MusicCachePath + "Lyric\\"))
                Directory.CreateDirectory(Settings.USettings.MusicCachePath + "Lyric\\");
            if (!Directory.Exists(Settings.USettings.MusicCachePath + "Image\\"))
                Directory.CreateDirectory(Settings.USettings.MusicCachePath + "Image\\");
        }
        #endregion
        #region 一些字段
        public static string qq = "";
        public static string MusicLikeGDid = null;
        public static string MusicLikeGDdirid = "";
        #endregion
        #region 播放时 
        /// <summary>
        /// 获取歌曲相关的歌单
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public static async Task<List<MusicGD>> GetSongListAboutSong(string mid) {
            string songid = await GetMusicIdByMidAsync(mid);
            JObject o = JObject.Parse(await HttpHelper.GetWebDataqAsync("https://u.y.qq.com/cgi-bin/musicu.fcg?-=getUCGI5445484706088479&g_tk=1162521571&loginUin=2728578956&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0&data=%7B%22comm%22%3A%7B%22ct%22%3A24%2C%22cv%22%3A0%7D%2C%22song_gedan%22%3A%7B%22module%22%3A%22music.mb_gedan_recommend_svr%22%2C%22method%22%3A%22get_related_gedan%22%2C%22param%22%3A%7B%22song_id%22%3A"+songid+"%2C%22song_type%22%3A1%2C%22sin%22%3A0%2C%22last_id%22%3A0%7D%7D%7D"));
            List<MusicGD> list = new List<MusicGD>();
            var data = o["song_gedan"]["data"]["vec_gedan"];
            foreach (var item in data)
            {
                MusicGD d = new MusicGD()
                {
                    ID = item["tid"].ToString(),
                    Name = item["dissname"].ToString(),
                    Photo = item["imgurl"].ToString().Replace("http://", "https://"),
                    ListenCount = int.Parse(item["listen_num"].ToString()),
                };
                list.Add(d);
            }
            return list;
        }
        #endregion
        #region 搜索歌曲&搜索智能提示 (似乎不太智能)
        public async Task<List<Music>> SearchMusicAsync(string Content, int osx = 1)
        {
            JObject o = JObject.Parse(await HttpHelper.GetWebAsync($"http://59.37.96.220/soso/fcgi-bin/client_search_cp?format=json&t=0&inCharset=GB2312&outCharset=utf-8&qqmusic_ver=1302&catZhida=0&p={osx}&n=20&w={HttpUtility.UrlDecode(Content)}&flag_qc=0&remoteplace=sizer.newclient.song&new_json=1&lossless=0&aggr=1&cr=1&sem=0&force_zonghe=0"));
            List<Music> dt = new List<Music>();
            int i = 0;
            var dsl = o["data"]["song"]["list"];
            while (i < dsl.Count())
            {
                try
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
                        m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R500x500M000{dsli["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                    else if (amid == "") m.ImageUrl = $"https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000";
                    else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000";
                    if (amid != "")
                        m.Album = new MusicGD()
                        {
                            ID = amid,
                            Photo = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000",
                            Name = dsli["album"]["name"].ToString()
                        };
                    var file = dsli["file"];
                    if (file["size_320"].ToString() != "0")
                        m.Pz = "HQ";
                    if (file["size_flac"].ToString() != "0")
                        m.Pz = "SQ";
                    m.Mvmid = dsli["mv"]["vid"].ToString();
                    dt.Add(m);
                }
                catch { }
                i++;
            }
            return dt;
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

        public static async Task<List<string>> SearchHotKey()
        {
            var data = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/splcloud/fcgi-bin/gethotkey.fcg?g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0"));
            List<string> list = new List<string>();
            var dt = data["data"]["hotkey"];
            foreach (var a in dt)
                list.Add(a["k"].ToString());
            return list;
        }
        /// <summary>
        /// 尝试用歌曲名称在网易云音乐中搜索此歌曲并返回歌曲ID
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<string> GetWYIdByName(string name)
        {
            string data = await HttpHelper.PostWeb("https://music.yeie.net/api.php", "types=search&count=20&source=netease&pages=1&name=" + Uri.EscapeDataString(name), HttpHelper.GetWebHeader_Yeie());
            var ds = "{\"data\":" + data + "}";
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
        public static async Task<string[]> AddMusicToGDAsync(string id, string dirid)
        {
            MainClass.DebugCallBack("User Cookies",Settings.USettings.Cookie + "   " + Settings.USettings.g_tk);
            string result = await HttpHelper.PostWeb("https://c.y.qq.com/splcloud/fcgi-bin/fcg_music_add2songdir.fcg?g_tk=" + Settings.USettings.g_tk,
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
        public static async Task<string[]> AddMusicToGDPLAsync(string ids, string dirid, string typelist)
        {
            MainClass.DebugCallBack("User Cookies",Settings.USettings.Cookie + "   " + Settings.USettings.g_tk);
            string result = await HttpHelper.PostWeb("https://c.y.qq.com/splcloud/fcgi-bin/fcg_music_add2songdir.fcg?g_tk=" + Settings.USettings.g_tk,
                $"loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.post&needNewCode=0&uin={Settings.USettings.LemonAreeunIts}&midlist={ids}&typelist={typelist}&dirid={dirid}&addtype=&formsender=4&source=153&r2=0&r3=1&utf8=1&g_tk=" + Settings.USettings.g_tk, HttpHelper.GetWebHeader_YQQCOM());
            //添加本地缓存
            JObject o = JObject.Parse(result);
            string msg = o["msg"].ToString();
            string title = o["title"].ToString();
            return new string[2] { msg, title };
        }
        /// <summary>
        /// 从我的歌单里删除歌曲(可批量)
        /// </summary>
        /// <param name="Musicid">Array 歌曲id(s)</param>
        /// <param name="dirid">歌单id</param>
        /// <returns></returns>
        public static async Task<string> DeleteMusicFromGDAsync(string[] Musicids, string dirid)
        {
            string Musicid = "";
            string types = "";
            foreach (string id in Musicids)
            {
                types += "3,";
            }
            Musicid = string.Join(",", Musicids);
            types = types[0..^1];
            string result = await HttpHelper.PostWeb("https://c.y.qq.com/qzone/fcg-bin/fcg_music_delbatchsong.fcg?g_tk=" + Settings.USettings.g_tk,
                $"loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.post&needNewCode=0&uin={Settings.USettings.LemonAreeunIts}&dirid={dirid}&ids={Musicid}&source=103&types={types}&formsender=4&flag=2&from=3&utf8=1&g_tk=" + Settings.USettings.g_tk, HttpHelper.GetWebHeader_YQQCOM());
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
            MainClass.DebugCallBack("User ID", Settings.USettings.LemonAreeunIts);
            JObject o = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/splcloud/fcgi-bin/songlist_list.fcg?utf8=1&-=MusicJsonCallBack&uin={Settings.USettings.LemonAreeunIts}&rnd=0.693477705380313&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0"));
            foreach (var a in o["list"])
            {
                string st = HttpUtility.HtmlDecode(a["dirname"].ToString());
                MainClass.DebugCallBack("GetGDdirid Data", st);
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
        public static async Task<string> AddGDILikeAsync(string dissid)
        {
            string result = await HttpHelper.PostWeb("https://c.y.qq.com/folder/fcgi-bin/fcg_qm_order_diss.fcg?g_tk=" + Settings.USettings.g_tk,
                $"loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=fs&inCharset=GB2312&outCharset=utf8&notice=0&platform=yqq&needNewCode=0&g_tk={Settings.USettings.g_tk}&uin={Settings.USettings.LemonAreeunIts}&dissid={dissid}&from=1&optype=1&utf8=1&qzreferrer=https%3A%2F%2Fy.qq.com%2Fn%2Fyqq%2Fplaysquare%2F{dissid}.html%23stat%3Dy_new.playlist.pic_click", HttpHelper.GetWebHeader_YQQCOM());
            return result;
        }
        /// <summary>
        /// 取消收藏一份歌单
        /// </summary>
        /// <param name="dissid"></param>
        /// <returns></returns>
        public static async Task<string> DelGDILikeAsync(string dissid)
        {
            string result = await HttpHelper.PostWeb("https://c.y.qq.com/folder/fcgi-bin/fcg_qm_order_diss.fcg?g_tk=" + Settings.USettings.g_tk,
                $"loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=fs&inCharset=GB2312&outCharset=gb2312&notice=0&platform=yqq&needNewCode=0&g_tk={Settings.USettings.g_tk}&uin={Settings.USettings.LemonAreeunIts}&ordertype=0&optype=2&dissid={dissid}&from=1", HttpHelper.GetWebHeader_YQQCOM());
            return result;
        }
        /// <summary>
        /// 新建一个歌单
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<string> AddNewGdAsync(string name, string imgurl = "")
        {
            string result = await HttpHelper.PostWeb("https://c.y.qq.com/splcloud/fcgi-bin/create_playlist.fcg?g_tk=" + Settings.USettings.g_tk,
                $"loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=fs&inCharset=GB2312&outCharset=utf8&notice=0&platform=yqq&needNewCode=0&g_tk={Settings.USettings.g_tk}&uin={Settings.USettings.LemonAreeunIts}&name={HttpUtility.UrlEncode(name)}&description=&show=1&pic_url={imgurl}&tags=&tagids=&formsender=1&utf8=1&qzreferrer=https%3A%2F%2Fy.qq.com%2Fportal%2Fprofile.html%23sub%3Dother%26tab%3Dcreate%26stat%3Dy_new.top.user_pic", HttpHelper.GetWebHeader_YQQCOM());
            return result;
        }
        /// <summary>
        /// 删除一个歌单
        /// </summary>
        /// <param name="dirid"></param>
        /// <returns></returns>
        public static async Task<string> DeleteGdByIdAsync(string dirid)
        {
            string result = await HttpHelper.PostWeb("https://c.y.qq.com/splcloud/fcgi-bin/fcg_fav_modsongdir.fcg?g_tk=" + Settings.USettings.g_tk,
                $"loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=fs&inCharset=GB2312&outCharset=gb2312&notice=0&platform=yqq&needNewCode=0&g_tk={Settings.USettings.g_tk}&uin={Settings.USettings.LemonAreeunIts}&delnum=1&deldirids={dirid}&forcedel=1&formsender=1&source=103", HttpHelper.GetWebHeader_YQQCOM());
            return result;
        }

        /// <summary>
        /// 上传一个文件
        /// </summary>
        /// <param name="path">文件地址（本机）</param>
        /// <returns></returns>
        public static async Task<string> UploadAFile(string path)
        {
            FileInfo e = new FileInfo(path);
            string ex = "0";
            string exTen = "";
            if (e.Extension.Equals(".jpg"))
            {
                ex = "0";
                exTen = "jpeg";
            }
            else
            {
                ex = "1";
                exTen = "png";
            }
            string q = $@"------WebKitFormBoundarye8oXp9zt6XFYGpye
Content-Disposition: form-data; name=""data""; filename=""{e.Name}""
Content-Type: image/{exTen}

";
            string h = $@"------WebKitFormBoundarye8oXp9zt6XFYGpye
Content-Disposition: form-data; name=""auth_appid""

music_cover
------WebKitFormBoundarye8oXp9zt6XFYGpye
Content-Disposition: form-data; name=""parentid""

/
------WebKitFormBoundarye8oXp9zt6XFYGpye
Content-Disposition: form-data; name=""fileid""

{qq}_{new Random().Next(100000000, 999999999)}{new Random().Next(1000, 9999)}
------WebKitFormBoundarye8oXp9zt6XFYGpye
Content-Disposition: form-data; name=""uin""

{qq}
------WebKitFormBoundarye8oXp9zt6XFYGpye
Content-Disposition: form-data; name=""crop""

0
------WebKitFormBoundarye8oXp9zt6XFYGpye
Content-Disposition: form-data; name=""x""

0
------WebKitFormBoundarye8oXp9zt6XFYGpye
Content-Disposition: form-data; name=""y""

0
------WebKitFormBoundarye8oXp9zt6XFYGpye
Content-Disposition: form-data; name=""width""

0
------WebKitFormBoundarye8oXp9zt6XFYGpye
Content-Disposition: form-data; name=""height""

0
------WebKitFormBoundarye8oXp9zt6XFYGpye
Content-Disposition: form-data; name=""origin_size""

1
------WebKitFormBoundarye8oXp9zt6XFYGpye
Content-Disposition: form-data; name=""png""

{ex}
------WebKitFormBoundarye8oXp9zt6XFYGpye
Content-Disposition: form-data; name=""picformat""

jpg
------WebKitFormBoundarye8oXp9zt6XFYGpye--";
            string url = "https://c.y.qq.com/splcloud/fcgi-bin/fcg_upload_image.fcg";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = 20000;
            request.Referer = "https://y.qq.com/portal/mymusic_edit.html?dirid=9";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";
            request.Host = "c.y.qq.com";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            request.ContentType = "multipart/form-data; boundary=----WebKitFormBoundarye8oXp9zt6XFYGpye";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9");
            request.Headers.Add(HttpRequestHeader.Cookie, Settings.USettings.Cookie);
            request.Headers.Add(HttpRequestHeader.Pragma, "no-cache");
            request.Headers.Add(HttpRequestHeader.CacheControl, "no-cache");
            request.Headers.Add("Origin", "https://y.qq.com");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");

            byte[] qByte = Encoding.UTF8.GetBytes(q);
            byte[] UpdateFile = File.ReadAllBytes(path);
            byte[] hByte = Encoding.UTF8.GetBytes(h);

            Stream myRequestStream = request.GetRequestStream();
            await myRequestStream.WriteAsync(qByte, 0, qByte.Length);
            await myRequestStream.WriteAsync(UpdateFile, 0, UpdateFile.Length);
            await myRequestStream.WriteAsync(hByte, 0, h.Length);

            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);

            string retString = await myStreamReader.ReadToEndAsync();
            myRequestStream.Close();
            myStreamReader.Close();
            var json = TextHelper.XtoYGetTo(retString, "frameElement.callback)(", ");</script></body></html>", 0);
            return JObject.Parse(json)["imageurl"].ToString().Replace("http://", "https://");
        }
        #endregion
        #region  歌单数据的获取
        /// <summary>
        /// 获取“我喜欢”的歌单ID
        /// </summary>
        public async Task<string> GetMusicLikeGDid()
        {
            try
            {
                string dta = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={Settings.USettings.LemonAreeunIts}&reqfrom=1&reqtype=0");
                JObject o = JObject.Parse(dta);
                string id = "";
                foreach (var a in o["data"]["mymusic"])
                {
                    if (a["title"].ToString() == "我喜欢")
                    {
                        id = a["id"].ToString();
                        break;
                    }
                }
                MusicLikeGDid = id;
                MusicLikeGDdirid = await GetGDdiridByNameAsync("我喜欢");
                return id;
            }
            catch { return null; }
        }
        /// <summary>
        /// 通过歌单ID 获取其中的歌曲和歌单信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        /// <param name="wx"></param>
        /// <param name="getAll"></param>
        /// <returns></returns>
        public static async Task<MusicGData> GetGDAsync(string id = "2591355982", Action<MusicGData> GetInfo = null, Action<int, Music, bool> callback = null, Window wx = null, Action<int> getAll = null)
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
            dt.desc = c0["desc"].ToString();
            dt.Creater = new MusicSinger()
            {
                Name = c0["nick"].ToString(),
                Photo = c0["headurl"].ToString()
            };
            GetInfo?.Invoke(dt);
            var c0s = c0["songlist"];
            await wx?.Dispatcher.BeginInvoke(new Action(() => getAll?.Invoke(c0s.Count())));
            Parallel.For(0, c0s.Count(), async (index) =>
                {
                    string singer = "";
                    var c0si = c0s[index];
                    string songtype = c0si["songtype"].ToString();
                    if (songtype == "0")
                    {
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
                            m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R500x500M000{c0si["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                        else if (amid == "") m.ImageUrl = $"https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000";
                        else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000";
                        if (amid != "")
                            m.Album = new MusicGD()
                            {
                                ID = amid,
                                Photo = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000",
                                Name = c0si["albumname"].ToString()
                            };
                        if (c0si["size320"].ToString() != "0")
                            m.Pz = "HQ";
                        if (c0si["sizeflac"].ToString() != "0")
                            m.Pz = "SQ";
                        m.Mvmid = c0si["vid"].ToString();
                        m.Littleid = dt.ids[index];
                        dt.Data.Add(m);
                        await wx?.Dispatcher.BeginInvoke(new Action(() => { callback?.Invoke(index, m, dt.IsOwn); }));
                    }
                    else {
                        var c0sis = c0si["singer"];
                        List<MusicSinger> lm = new List<MusicSinger>();
                        foreach (var cc in c0sis)
                        {
                            singer += cc["name"].ToString() + "&";
                            lm.Add(new MusicSinger()
                            {
                                Name = cc["name"].ToString()
                            });
                        }
                        Music m = new Music();
                        m.MusicID = null;
                        m.MusicName = c0si["songname"].ToString();
                        m.Singer = lm;
                        m.SingerText = singer.Substring(0, singer.Length - 1);
                        await wx?.Dispatcher.BeginInvoke(new Action(() => { callback?.Invoke(index, m, dt.IsOwn); }));
                    }
                });
            return dt;
        }
        /// <summary>
        /// 通过歌单ID 获取歌单Mid 与 id Simple
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        /// <param name="wx"></param>
        /// <param name="getAll"></param>
        /// <returns></returns>
        public static async void GetGDAsync(string id = "2591355982",Action<string,string> callback = null)
        {
            var s = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/qzone/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&json=1&utf8=1&onlysong=0&disstid={id}&format=json&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0", Encoding.UTF8);
            JObject o = JObject.Parse(s);
            var c0 = o["cdlist"][0];
            List<string> ids = c0["songids"].ToString().Split(',').ToList();
            var c0s = c0["songlist"];
            Parallel.For(0, c0s.Count(), (index) =>
            {
                try
                {
                    var c0si = c0s[index];
                    var MusicID = c0si["songmid"].ToString();
                    var Littleid = ids[index];
                    callback(MusicID, Littleid);
                }
                catch { }
            });
        }
        /// <summary>
        /// 获取 我创建的歌单 列表
        /// </summary>
        /// <returns></returns>
        public async Task<SortedDictionary<string, MusicGData>> GetGdListAsync()
        {
            if (Settings.USettings.LemonAreeunIts == "")
                return new SortedDictionary<string, MusicGData>();
            var dt = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={Settings.USettings.LemonAreeunIts}&reqfrom=1&reqtype=0");
            var o = JObject.Parse(dt);
            MainClass.DebugCallBack("Get Gd Data",o.ToString());
            var data = new SortedDictionary<string, MusicGData>();
            var dx = o["data"]["mydiss"]["list"];
            foreach (var ex in dx)
            {
                var df = new MusicGData();
                df.id = ex["dissid"].ToString();
                df.name = ex["title"].ToString();
                df.subtitle = ex["subtitle"].ToString();
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
            MainClass.DebugCallBack("Result",o.ToString());
            var data = new SortedDictionary<string, MusicGData>();
            var dx = o["data"]["cdlist"];
            foreach (var ex in dx)
            {
                var df = new MusicGData();
                df.id = ex["dissid"].ToString();
                df.name = ex["dissname"].ToString();
                df.listenCount = int.Parse(ex["listennum"].ToString());
                if (ex["logo"].ToString() != "")
                    df.pic = ex["logo"].ToString().Replace("http://", "https://");
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
        public async Task<List<MusicGD>> GetFLGDAsync(string id, string sortId = "5", int osx = 1)
        {
            int start = (osx - 1) * 30;
            int end = start + 29;
            var o = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/splcloud/fcgi-bin/fcg_get_diss_by_tag.fcg?picmid=1&rnd=0.38615680484561965&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&categoryId={id}&sortId={sortId}&sin={start}&ein={end}", Encoding.UTF8));
            MainClass.DebugCallBack("FLGD Data",o.ToString());
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
                    ID = dli["dissid"].ToString(),
                    ListenCount = int.Parse(dli["listennum"].ToString())
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
            await AddNewGdAsync(dt.name);
            await Task.Delay(500);
            string dir = await GetGDdiridByNameAsync(dt.name);
            var amt = await AddMusicToGDPLAsync(ids, dir, typelist);
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
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create("https://i.y.qq.com/v8/playsong.html?songmid=000edOaL1WZOWq");
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
            /*
             两种地址类型:
            aqqmusic.tc.qq.com/amobile.music.tc.qq.com/
            apd-vlive.apdcdn.tc.qq.com/amobile.music.tc.qq.com/
             */
            string vk = TextHelper.XtoYGetTo(st, "amobile.music.tc.qq.com/C400000By9MX0yKL2c.m4a", "&fromtag=38", 0);
            if (string.IsNullOrEmpty(vk)) {
                MainClass.DebugCallBack("MusicUrlGet","Vkey被吃掉了!!");
                await Task.Delay(500);
                MainClass.DebugCallBack("MusicUrlGet", "重连...");
                return await GetUrlAsync(Musicid);
            }
            var mid = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg?songmid={Musicid}&platform=yqq&format=json"))["data"][0]["file"]["media_mid"].ToString();
            return $"http://musichy.tc.qq.com/amobile.music.tc.qq.com/C400{mid}.m4a" + vk + "&fromtag=98";

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

        #endregion
        #region 歌词 获取|处理
        /// <summary>
        /// 获取歌词
        /// </summary>
        /// <param name="McMind"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task<LyricData> GetLyric(string McMind, string file = "")
        {
            string split = "\n<LemonApp TransLyric/>\n";//分隔符
            if (file == "") 
                file = Settings.USettings.MusicCachePath + "Lyric\\" + McMind + ".lmrc";
            if (!File.Exists(file))
            {
                WebClient c = new WebClient();
                c.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.110 Safari/537.36");
                c.Headers.Add("Accept", "*/*");
                c.Headers.Add("Referer", "https://y.qq.com/portal/player.html");
                c.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
                c.Headers.Add("Cookie", Settings.USettings.Cookie);
                c.Headers.Add("Host", "c.y.qq.com");
                string url = $"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?-=MusicJsonCallback_lrc&pcachetime=1563410858607&songmid={McMind}&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0";
                string td = c.DownloadString(url);
                JObject o = JObject.Parse(td);

                LyricData data = new LyricData();

                string lyric = WebUtility.HtmlDecode( Encoding.UTF8.GetString(Convert.FromBase64String(o["lyric"].ToString())));
                if (o["trans"].ToString() == "")
                {
                    //没有翻译  直接返回歌词
                    await Task.Run(() => { File.WriteAllText(file, lyric); });//保存歌词
                    data.lyric = lyric;
                    data.HasTrans = false;
                    return data;
                }
                else
                {
                    string trans = WebUtility.HtmlDecode( Encoding.UTF8.GetString(Convert.FromBase64String(o["trans"].ToString())));
                    await Task.Run(() => { File.WriteAllText(file, lyric+split+trans); });
                    data.lyric = lyric;
                    data.trans = trans;
                    data.HasTrans = true;
                    return data;
                }
            }
            else
            {
                string filedata = await File.ReadAllTextAsync(file);
                LyricData data = new LyricData();
                if (filedata.Contains(split))
                {
                    //有翻译歌词
                    var dta = filedata.Split(split);
                    data.lyric = dta[0];
                    data.trans = dta[1];
                    data.HasTrans = true;
                    return data;
                }
                else {
                    //没有翻译歌词
                    data.lyric = filedata;
                    data.HasTrans = false;
                    return data;
                }
            }
        }
        #endregion
        #region 排行榜
        /// <summary>
        /// 排行榜列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<MusicTop>> GetTopIndexAsync()
        {
            var dt = await HttpHelper.GetWebAsync("https://u.y.qq.com/cgi-bin/musicu.fcg?_=1580276407716&data={%22comm%22:{%22g_tk%22:5381,%22uin%22:%22%22,%22format%22:%22json%22,%22inCharset%22:%22utf-8%22,%22outCharset%22:%22utf-8%22,%22notice%22:0,%22platform%22:%22h5%22,%22needNewCode%22:1,%22ct%22:23,%22cv%22:0},%22topList%22:{%22module%22:%22musicToplist.ToplistInfoServer%22,%22method%22:%22GetAll%22,%22param%22:{}}}");
            var o = JObject.Parse(dt);
            var data = new List<MusicTop>();
            var d0l = o["topList"]["data"]["group"];
            foreach (var c in d0l)
            {
                var toplist = c["toplist"];
                foreach (var d in toplist)
                {
                    List<string> content = new List<string>();
                    foreach (var a in d["song"])
                        content.Add(a["title"] + " - " + a["singerName"]);
                    data.Add(new MusicTop
                    {
                        Name = d["title"].ToString(),
                        Photo = d["frontPicUrl"].ToString().Replace("http://", "https://"),
                        ID = d["topId"].ToString(),
                        desc = "[" + d["titleShare"] + "] " + d["intro"].ToString().Replace("<br>", ""),
                        content = content
                    });
                }
            }
            return data;
        }
        /// <summary>
        /// 排行榜里的音乐
        /// </summary>
        /// <param name="TopID"></param>
        /// <param name="osx"></param>
        /// <returns></returns>
        public async Task<List<Music>> GetToplistAsync(string TopID, Action<Music, bool> callback, Window wx, Action finished, int osx = 1)
        {
            int index = (osx - 1) * 30;
            string json = await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_cp.fcg?tpl=3&page=detail&topid={TopID}&type=top&song_begin={index}&song_num=30&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0");
            JObject o = JObject.Parse(json);
            MainClass.DebugCallBack("TopList",json);
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
                    lm.Add(new MusicSinger()
                    {
                        Name = sid["singer"][osxc]["name"].ToString(),
                        Mid = sid["singer"][osxc]["mid"].ToString()
                    });
                }
                m.Singer = lm;
                m.SingerText = Singer.Substring(0, Singer.LastIndexOf("&"));
                m.MusicID = sid["songmid"].ToString();
                var amid = sid["albummid"].ToString();
                if (amid == "001ZaCQY2OxVMg")
                    m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R500x500M000{sid["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                else if (amid == "") m.ImageUrl = $"https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000";
                else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000";
                if (amid != "")
                    m.Album = new MusicGD()
                    {
                        ID = amid,
                        Photo = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000",
                        Name = sid["albumname"].ToString()
                    };
                if (sid["size320"].ToString() != "0")
                    m.Pz = "HQ";
                if (sid["sizeflac"].ToString() != "0")
                    m.Pz = "SQ";
                m.Mvmid = sid["vid"].ToString();
                wx.Dispatcher.Invoke(() => { callback(m, false); });
                dt.Add(m);
                i++;
            }
            wx.Dispatcher.Invoke(() => { finished(); });
            return dt;
        }
        #endregion
        #region 歌手
        /// <summary>
        /// 用于歌手页的数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<SingerPageData> GetSingerPageAsync(string id)
        {
            JObject o = JObject.Parse(await HttpHelper.PostInycAsync("https://u.y.qq.com/cgi-bin/musicu.fcg",
                "{\"req_0\":{\"module\":\"musichall.singer_info_server\",\"method\":\"GetSingerDetail\",\"param\":{\"singer_mids\":[\"" + id + "\"],\"pic\":1,\"group_singer\":1,\"wiki_singer\":1,\"ex_singer\":1}},\"req_1\":{\"module\":\"musichall.song_list_server\",\"method\":\"GetSingerSongList\",\"param\":{\"singerMid\":\"" + id + "\",\"begin\":0,\"num\":10,\"order\":1}},\"req_2\":{\"module\":\"Concern.ConcernSystemServer\",\"method\":\"cgi_qry_concern_status\",\"param\":{\"vec_userinfo\":[{\"usertype\":1,\"userid\":\"" + id + "\"}],\"opertype\":5,\"encrypt_singerid\":1}},\"req_3\":{\"module\":\"music.musichallAlbum.SelectedAlbumServer\",\"method\":\"SelectedAlbumList\",\"param\":{\"singerMid\":\"" + id + "\"}},\"comm\":{\"g_tk\":" + Settings.USettings.g_tk + ",\"uin\":\"" + Settings.USettings.LemonAreeunIts + "\",\"format\":\"json\",\"ct\":20,\"cv\":1710}}"));
            //Part 1 歌手信息
            var req0 = o["req_0"]["data"]["singer_list"][0];
            MusicSinger mSinger = new MusicSinger();
            mSinger.Mid = id;
            mSinger.Name = req0["basic_info"]["name"].ToString();
            string pic = req0["pic"]["big_black"].ToString();
            bool hasBigPic = true;
            if (pic == "")
            {
                hasBigPic = false;
                pic = req0["pic"]["pic"].ToString();
            }
            mSinger.Photo = pic.Replace("http://", "https://");
            //Part 2 热门歌曲
            var req1 = o["req_1"]["data"]["songList"];
            List<Music> HotSongs = new List<Music>();
            foreach (var c in req1)
            {
                Debug.Print(c.ToString());
                var data = c["songInfo"];
                Music m = new Music();
                m.MusicName = data["name"].ToString();
                m.MusicName_Lyric = data["subtitle"].ToString();
                m.MusicID = data["mid"].ToString();
                string Singer = "";
                List<MusicSinger> lm = new List<MusicSinger>();
                foreach (var s in data["singer"])
                {
                    Singer += s["name"] + "&";
                    lm.Add(new MusicSinger() { Name = s["name"].ToString(), Mid = s["mid"].ToString() });
                }
                m.Singer = lm;
                m.SingerText = Singer.Substring(0, Singer.LastIndexOf("&"));
                string amid = data["album"]["mid"].ToString();
                if (amid == "001ZaCQY2OxVMg")
                    m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R500x500M000{data["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                else if (amid == "") m.ImageUrl = $"https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000";
                else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000";
                if (amid != "")
                    m.Album = new MusicGD() { Name = data["album"]["name"].ToString(), ID = amid, Photo = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000" };
                var file = data["file"];
                if (file["size_320mp3"].ToString() != "0")
                    m.Pz = "HQ";
                if (file["size_flac"].ToString() != "0")
                    m.Pz = "SQ";
                m.Mvmid = data["mv"]["vid"].ToString();
                HotSongs.Add(m);
            }
            //Part 3 是否关注此歌手
            bool HasGJ;
            if (o["req_2"]["data"]["map_singer_status"][id].ToString() == "0")
                HasGJ = false;
            else HasGJ = true;
            //Part 4 顶部的凉虾
            var lx = o["req_3"]["data"]["albumList"];
            List<MVData> lix = new List<MVData>();
            foreach (var c in lx)
            {
                MVData m = new MVData();
                m.id = c["albumMid"].ToString();
                m.lstCount = c["publishDate"].ToString();
                m.name = c["albumName"].ToString();
                m.img = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{m.id}.jpg?max_age=2592000";
                lix.Add(m);
            }
            //不知为何 这个API给的专辑数据错乱 所以我们用另一个API
            JObject album = JObject.Parse(await HttpHelper.PostInycAsync("https://u.y.qq.com/cgi-bin/musicu.fcg", "{\"comm\":{\"g_tk\":\"" + Settings.USettings.g_tk + "\",\"uin\":\"" + Settings.USettings.LemonAreeunIts + "\",\"format\":\"json\",\"ct\":20,\"cv\":1710},\"singerAlbum\":{\"method\":\"get_singer_album\",\"param\":{\"singermid\":\"" + id + "\",\"order\":\"time\",\"begin\":0,\"num\":5,\"exstatus\":1},\"module\":\"music.web_singer_info_svr\"}}"));
            List<MusicGD> mg = new List<MusicGD>();
            var datac = album["singerAlbum"]["data"]["list"];
            foreach (var c in datac)
            {
                MusicGD m = new MusicGD();
                m.ID = c["album_mid"].ToString();
                m.Name = c["album_name"].ToString();
                m.Photo = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{m.ID}.jpg?max_age=2592000";
                mg.Add(m);
            }
            // MV
            JObject mv = JObject.Parse(await HttpHelper.GetWebDatacAsync("https://c.y.qq.com/mv/fcgi-bin/fcg_singer_mv.fcg?cid=205360581&singermid=" + id + "&order=listen&begin=0&num=5&g_tk=" + Settings.USettings.g_tk + "&loginUin=" + Settings.USettings.LemonAreeunIts + "&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0"));
            List<MVData> mVDatas = new List<MVData>();
            foreach (var c in mv["data"]["list"])
            {
                MVData m = new MVData();
                m.id = c["vid"].ToString();
                m.img = c["pic"].ToString();
                m.name = c["title"].ToString();
                m.lstCount = int.Parse(c["listenCount"].ToString()).IntToWn();
                mVDatas.Add(m);
            }
            //相似歌手
            var ss = JObject.Parse(await HttpHelper.GetWebDatacAsync("https://c.y.qq.com/v8/fcg-bin/fcg_v8_simsinger.fcg?utf8=1&singer_mid=" + id + "&start=0&num=5&g_tk=" + Settings.USettings.g_tk + "&loginUin=" + Settings.USettings.LemonAreeunIts + "&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0"))["singers"]["items"];
            List<MusicSinger> ssMs = new List<MusicSinger>();
            foreach (var c in ss)
            {
                MusicSinger m = new MusicSinger();
                m.Mid = c["mid"].ToString();
                m.Name = c["name"].ToString();
                m.Photo = c["pic"].ToString();
                ssMs.Add(m);
            }
            //关注量
            var gj = JObject.Parse(await HttpHelper.GetWebDatacAsync("https://c.y.qq.com/rsc/fcgi-bin/fcg_order_singer_getnum.fcg?g_tk=" + Settings.USettings.g_tk + "&loginUin=" + Settings.USettings.LemonAreeunIts + "&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0&singermid=" + id + "&utf8=1&rnd=1565074512297"));
            string num = int.Parse(gj["num"].ToString()).IntToWn();
            return new SingerPageData() { HasBigPic = hasBigPic, liangxia = lix, mSinger = mSinger, HotSongs = HotSongs, HasGJ = HasGJ, Album = mg, mVDatas = mVDatas, ssMs = ssMs, FansCount = num };
        }
        /// <summary>
        /// 该歌手的专辑
        /// </summary>
        /// <param name="id"></param>
        /// <param name="osx"></param>
        /// <returns></returns>
        public static async Task<List<MusicGD>> GetSingerAlbumById(string id, int osx = 1)
        {
            int num = 20;
            int begin = (osx - 1) * num;
            JObject album = JObject.Parse(await HttpHelper.PostInycAsync("https://u.y.qq.com/cgi-bin/musicu.fcg",
                "{\"comm\":{\"g_tk\":\"" + Settings.USettings.g_tk + "\",\"uin\":\"" + Settings.USettings.LemonAreeunIts + "\",\"format\":\"json\",\"ct\":20,\"cv\":1710},\"singerAlbum\":{\"method\":\"get_singer_album\",\"param\":{\"singermid\":\"" + id + "\",\"order\":\"time\",\"begin\":" + begin + ",\"num\":" + num + ",\"exstatus\":1},\"module\":\"music.web_singer_info_svr\"}}"));
            List<MusicGD> mg = new List<MusicGD>();
            var datac = album["singerAlbum"]["data"]["list"];
            foreach (var c in datac)
            {
                MusicGD m = new MusicGD();
                m.ID = c["album_mid"].ToString();
                m.Name = c["album_name"].ToString();
                m.Photo = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{m.ID}.jpg?max_age=2592000";
                mg.Add(m);
            }
            return mg;
        }

        public static async Task<List<MVData>> GetSingerMvList(string id, int osx = 1)
        {
            int num = 20;
            int begin = (osx - 1) * num;
            JObject mv = JObject.Parse(await HttpHelper.GetWebDatacAsync("https://c.y.qq.com/mv/fcgi-bin/fcg_singer_mv.fcg?cid=205360581&singermid=" + id + "&order=listen&begin=" + begin + "&num=" + num + "&g_tk=" + Settings.USettings.g_tk + "&loginUin=" + Settings.USettings.LemonAreeunIts + "&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0"));
            List<MVData> mVDatas = new List<MVData>();
            foreach (var c in mv["data"]["list"])
            {
                MVData m = new MVData();
                m.id = c["vid"].ToString();
                m.img = c["pic"].ToString();
                m.name = c["title"].ToString();
                m.lstCount = int.Parse(c["listenCount"].ToString()).IntToWn();
                mVDatas.Add(m);
            }
            return mVDatas;
        }

        public static async Task<SingerDesc> GetSingerDesc(string id)
        {
            string data = await HttpHelper.GetWebDatacAsync("https://c.y.qq.com/splcloud/fcgi-bin/fcg_get_singer_desc.fcg?singermid=" + id + "&utf8=1&outCharset=utf-8&format=xml&r=1565243621590");
            SingerDesc sd = new SingerDesc();
            XElement x = XDocument.Parse(data).Element("result").Element("data").Element("info");
            sd.Desc = x.Element("desc").Value.Replace("<![CDATA[", "").Replace("]]>", "");

            var a = from b in x.Element("basic").Descendants("item")
                    select new { key = b.Element("key").Value.Replace("<![CDATA[", "").Replace("]]>", ""), value = b.Element("value").Value.Replace("<![CDATA[", "").Replace("]]>", "") };
            sd.basic = new Dictionary<string, string>();
            foreach (var c in a)
                sd.basic.Add(c.key, c.value);

            var d = from b in x.Element("other").Descendants("item")
                    select new { key = b.Element("key").Value.Replace("<![CDATA[", "").Replace("]]>", ""), value = b.Element("value").Value.Replace("<![CDATA[", "").Replace("]]>", "") };
            sd.other = new Dictionary<string, string>();
            foreach (var c in d)
                sd.other.Add(c.key, c.value);
            return sd;
        }

        public static async Task<bool> AddSingerLikeById(string id)
        {
            var o = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_order_singer_add.fcg?g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=gb2312&notice=0&platform=yqq.json&needNewCode=0&singermid={id}&rnd=1565150765773"));
            return o["code"].ToString() == "0";
        }
        public static async Task<bool> DelSingerLikeById(string id)
        {
            var o = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_order_singer_del.fcg?g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=gb2312&notice=0&platform=yqq.json&needNewCode=0&singermid={id}&rnd=1565150765773"));
            return o["code"].ToString() == "0";
        }
        public static async Task<List<MusicSinger>> GetSingerIFollowListAsync(int index)
        {
            int size = 30;
            int from = 30 * (index - 1);
            string str = await HttpHelper.GetWebDataqAsync("https://u.y.qq.com/cgi-bin/musicu.fcg?g_tk=5381&loginUin=" + Settings.USettings.LemonAreeunIts + "&hostUin=0&format=json&inCharset=utf8&outCharset=GB2312&notice=0&platform=yqq.json&needNewCode=0&data=%7B%22comm%22%3A%7B%22ct%22%3A24%2C%22cv%22%3A0%7D%2C%22singerList%22%3A%7B%22module%22%3A%22music.concern.RelationList%22%2C%22method%22%3A%22GetFollowSingerList%22%2C%22param%22%3A%7B%22From%22%3A" + from + "%2C%22Size%22%3A" + size + "%7D%7D%7D");
            JToken obj = JObject.Parse(str)
                ["singerList"]["data"]["List"];

            List<MusicSinger> data = new List<MusicSinger>();
            foreach (var a in obj)
            {
                string mid = a["MID"].ToString();
                data.Add(new MusicSinger()
                {
                    Mid = mid,
                    Name = a["Name"].ToString(),
                    Photo = "https://y.gtimg.cn/music/photo_new/T001R500x500M000" + mid + ".jpg?max_age=2592000"
                });
            }
            return data;
        }
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
        public static async Task<List<MusicSinger>> GetSingerListAsync(string index, string area, string sex, string genre, string sin, int cur_page)
        {
            var o = JObject.Parse(await HttpHelper.GetWebAsync($"https://u.y.qq.com/cgi-bin/musicu.fcg?-=getUCGI6639758764435573&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0&data=" +
                $"%7B%22comm%22%3A%7B%22ct%22%3A24%2C%22cv%22%3A0%7D%2C%22singerList%22%3A%7B%22module%22%3A%22Music.SingerListServer%22%2C%22method%22%3A%22get_singer_list%22%2C%22param%22%3A%7B%22area%22%3A{area}%2C%22sex%22%3A{sex}%2C%22genre%22%3A{genre}%2C%22index%22%3A{index}%2C%22sin%22%3A{sin}%2C%22cur_page%22%3A{cur_page}%7D%7D%7D"));
            var data = new List<MusicSinger>();
            var dl = o["singerList"]["data"]["singerlist"];
            foreach (var dli in dl)
            {
                data.Add(new MusicSinger
                {
                    Name = dli["singer_name"].ToString(),
                    Mid = dli["singer_mid"].ToString(),
                    Photo = $"https://y.gtimg.cn/music/photo_new/T001R150x150M000{dli["singer_mid"]}.jpg?max_age=2592000"
                });
            }
            return data;
        }
        /// <summary>
        /// 通过歌手ID获取此歌手的音乐
        /// </summary>
        /// <param name="mid"></param>
        /// <param name="osx"></param>
        /// <returns></returns>
        public static async Task<List<Music>> GetSingerMusicByIdAsync(string mid, int osx = 1)
        {
            int begin = (osx - 1) * 30;
            JObject o = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_v8_singer_track_cp.fcg?hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0&ct=24&singermid={mid}&order=listen&begin={begin}&num=30&songstatus=1"));
            List<Music> dt = new List<Music>();
            JToken dtl = o["data"]["list"];
            foreach (JToken dtli in dtl)
            {
                var dsli = dtli["musicData"];
                Music m = new Music();
                m.MusicName = dsli["songname"].ToString();
                m.MusicName_Lyric = dsli["albumdesc"].ToString();
                string Singer = "";
                List<MusicSinger> lm = new List<MusicSinger>();
                for (int osxc = 0; osxc != dsli["singer"].Count(); osxc++)
                {
                    Singer += dsli["singer"][osxc]["name"] + "&";
                    lm.Add(new MusicSinger() { Name = dsli["singer"][osxc]["name"].ToString(), Mid = dsli["singer"][osxc]["mid"].ToString() });
                }
                m.Singer = lm;
                m.SingerText = Singer.Substring(0, Singer.LastIndexOf("&"));
                m.MusicID = dsli["songmid"].ToString();
                var amid = dsli["albummid"].ToString();
                if (amid == "001ZaCQY2OxVMg")
                    m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R500x500M000{dsli["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                else if (amid == "") m.ImageUrl = $"https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000";
                else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000";
                if (amid != "")
                    m.Album = new MusicGD()
                    {
                        ID = amid,
                        Photo = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000",
                        Name = dsli["albumname"].ToString()
                    };
                if (dsli["size320"].ToString() != "0")
                    m.Pz = "HQ";
                if (dsli["sizeflac"].ToString() != "0")
                    m.Pz = "SQ";
                m.Mvmid = dsli["vid"].ToString();
                dt.Add(m);
            }
            return dt;
        }
        #endregion
        #region 电台
        public static async Task<Dictionary<string, MusicRadioList>> GetRadioList()
        {
            var o = JObject.Parse(await HttpHelper.GetWebDataqAsync($"https://u.y.qq.com/cgi-bin/musicu.fcg?g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0&data="+HttpUtility.UrlEncode("{\"radiolist\":{\"module\":\"pf.radiosvr\",\"method\":\"GetRadiolist\",\"param\":{\"ct\":\"24\"}},\"comm\":{\"ct\":24,\"cv\":0}}")));
            var data = new Dictionary<string, MusicRadioList>();
            var ddg = o["radiolist"]["data"]["radio_list"];
            try
            {
                foreach (var list in ddg)
                {
                    var dt = new MusicRadioList();
                    string name = list["title"].ToString();
                    var ax = list["list"];
                    foreach (var ms in ax)
                    {
                        dt.Items.Add(new MusicRadioListItem
                        {
                            Name = ms["title"].ToString(),
                            Photo = ms["pic_url"].ToString(),
                            ID = ms["tjreport"].ToString().Split('_').Last(),
                            lstCount = int.Parse(ms["listenNum"].ToString())
                        });
                    }
                    data.Add(name, dt);
                }
            }
            catch { }
            return data;
        }
        public static async Task<List<Music>> GetRadioMusicAsync(string id)
        {
            var o = JObject.Parse(await HttpHelper.PostInycAsync("https://u.y.qq.com/cgi-bin/musicu.fcg",
    "{\"songlist\":{\"module\":\"mb_track_radio_svr\",\"method\":\"get_radio_track\",\"param\":{\"id\":" + id + ",\"firstplay\":1,\"num\":10}},\"comm\":{\"g_tk\":\"" + Settings.USettings.g_tk + "\",\"uin\":\"" + Settings.USettings.LemonAreeunIts + "\",\"format\":\"json\",\"ct\":20,\"cv\":0}}"))
                ["songlist"]["data"]["tracks"];
            List<Music> Data = new List<Music>();
            foreach (var e in o)
            {
                string Singer = "";
                List<MusicSinger> lm = new List<MusicSinger>();
                foreach (var a in e["singer"])
                {
                    Singer += a["name"] + "&";
                    lm.Add(new MusicSinger()
                    {
                        Name = a["name"].ToString(),
                        Mid = a["mid"].ToString()
                    });
                }
                string amid = e["album"]["mid"].ToString();
                Music m = new Music
                {
                    MusicName = e["name"].ToString(),
                    SingerText = Singer.Substring(0, Singer.LastIndexOf("&")),
                    Singer = lm,
                    MusicID = e["mid"].ToString(),
                };
                if (amid == "001ZaCQY2OxVMg")
                    m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R500x500M000{e["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                else if (amid == "") m.ImageUrl = $"https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000";
                else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000";
                if (amid != "")
                    m.Album = new MusicGD()
                    {
                        ID = amid,
                        Photo = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000",
                        Name = e["album"]["name"].ToString()
                    };
                Data.Add(m);
            }
            return Data;
        }
        #endregion
        #region 专辑
        /// <summary>
        /// 获取专辑音乐
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<MusicGData> GetAlbumSongListByIDAsync(string id, Action<Music, bool> callback, Window wx, Action<MusicGData> getImformation, int aniCount)
        {
            string json = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_v8_album_info_cp.fcg?ct=24&albummid={id}&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0&song_begin=0&song_num=50");
            JObject o = JObject.Parse(json);
            MusicGData md = new MusicGData();
            md.pic = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{id}.jpg?max_age=2592000";
            md.name = o["data"]["name"].ToString();
            var data = new List<Music>();
            var list = o["data"]["list"];
            md.desc = o["data"]["desc"].ToString().Replace("\r", "").Replace("\n", "");
            md.Creater = new MusicSinger()
            {
                Name = o["data"]["singername"].ToString(),
                Photo = $"https://y.gtimg.cn/music/photo_new/T001R500x500M000{o["data"]["singermid"].ToString()}.jpg?max_age=2592000"
            };
            wx.Dispatcher.Invoke(() => { getImformation(md); });
            int i = 0;
            foreach (var a in list)
            {
                Music m = new Music();
                m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{id}.jpg?max_age=2592000";
                m.Album = new MusicGD() { ID = id, Name = o["data"]["name"].ToString(), Photo = md.pic };
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
                if (a["size320"].ToString() != "0")
                    m.Pz = "HQ";
                if (a["sizeflac"].ToString() != "0")
                    m.Pz = "SQ";
                m.Mvmid = a["vid"].ToString();
                wx.Dispatcher.Invoke(() => { callback(m, false); });
                i++;
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
            //---------官方歌单--------(QQ音乐的歌单和电台都是鸡肋)
            JObject obj = JObject.Parse(await HttpHelper.PostInycAsync("https://u.y.qq.com/cgi-bin/musicu.fcg",
           "{\"req_0\":{\"module\":\"playlist.HotRecommendServer\",\"method\":\"get_new_hot_recommend\",\"param\":{\"cmd\":0,\"page\":0,\"daily_page\":0,\"size\":1}},\"comm\":{\"g_tk\":" + Settings.USettings.g_tk + ",\"uin\":\"" + Settings.USettings.LemonAreeunIts + "\",\"format\":\"json\",\"ct\":20,\"cv\":1751}}"));
            var data = obj["req_0"]["data"]["modules"];
            var gf = data[0]["grids"];
            List<MusicGD> gdList = new List<MusicGD>();
            foreach (var ab in gf)
            {
                MusicGD d = new MusicGD()
                {
                    ID = ab["id"].ToString(),
                    Name = ab["title"].ToString(),
                    Photo = ab["picurl"].ToString()
                };
                int lc = int.Parse(ab["listeners"].ToString());
                d.ListenCount = lc == 0 ? -1 : lc;
                gdList.Add(d);
            }
            //-------------------i.y.qq.com 达人歌单  客户端 歌单推荐-----
            var Gdata = new List<MusicGD>();
            var dr = data[1]["grids"];
            foreach (var ab in dr)
            {
                MusicGD d = new MusicGD()
                {
                    ID = ab["id"].ToString(),
                    Name = ab["title"].ToString(),
                    Photo = ab["picurl"].ToString(),
                    ListenCount = int.Parse(ab["listeners"].ToString()),
                };
                Gdata.Add(d);
            }
            //---------------------------------
            string json = await HttpHelper.GetWebDataqAsync($"https://u.y.qq.com/cgi-bin/musicu.fcg?-=recom9439610432420651&g_tk={Settings.USettings.g_tk}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq.json&needNewCode=0&data=%7B%22comm%22%3A%7B%22ct%22%3A24%7D%2C%22category%22%3A%7B%22method%22%3A%22get_hot_category%22%2C%22param%22%3A%7B%22qq%22%3A%22%22%7D%2C%22module%22%3A%22music.web_category_svr%22%7D%2C%22recomPlaylist%22%3A%7B%22method%22%3A%22get_hot_recommend%22%2C%22param%22%3A%7B%22async%22%3A1%2C%22cmd%22%3A2%7D%2C%22module%22%3A%22playlist.HotRecommendServer%22%7D%2C%22playlist%22%3A%7B%22method%22%3A%22get_playlist_by_category%22%2C%22param%22%3A%7B%22id%22%3A8%2C%22curPage%22%3A1%2C%22size%22%3A40%2C%22order%22%3A5%2C%22titleid%22%3A8%7D%2C%22module%22%3A%22playlist.PlayListPlazaServer%22%7D%2C%22new_song%22%3A%7B%22module%22%3A%22newsong.NewSongServer%22%2C%22method%22%3A%22get_new_song_info%22%2C%22param%22%3A%7B%22type%22%3A5%7D%7D%2C%22new_album%22%3A%7B%22module%22%3A%22newalbum.NewAlbumServer%22%2C%22method%22%3A%22get_new_album_info%22%2C%22param%22%3A%7B%22area%22%3A1%2C%22sin%22%3A0%2C%22num%22%3A10%7D%7D%2C%22new_album_tag%22%3A%7B%22module%22%3A%22newalbum.NewAlbumServer%22%2C%22method%22%3A%22get_new_album_area%22%2C%22param%22%3A%7B%7D%7D%2C%22toplist%22%3A%7B%22module%22%3A%22musicToplist.ToplistInfoServer%22%2C%22method%22%3A%22GetAll%22%2C%22param%22%3A%7B%7D%7D%2C%22focus%22%3A%7B%22module%22%3A%22QQMusic.MusichallServer%22%2C%22method%22%3A%22GetFocus%22%2C%22param%22%3A%7B%7D%7D%7D");
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
            var recomPlaylist_obj = o["recomPlaylist"]["data"]["v_hot"];
            foreach (var rp in recomPlaylist_obj)
            {
                MusicGD md = new MusicGD();
                md.ID = rp["content_id"].ToString();
                md.Name = rp["title"].ToString();
                md.Photo = rp["cover"].ToString().Replace("http://", "https://");
                md.ListenCount = int.Parse(rp["listen_num"].ToString());
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
                    m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R500x500M000{ns["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                else if (amid == "") m.ImageUrl = $"https://y.gtimg.cn/mediastyle/global/img/album_300.png?max_age=31536000";
                else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000";
                if (amid != "")
                    m.Album = new MusicGD()
                    {
                        ID = amid,
                        Photo = $"https://y.gtimg.cn/music/photo_new/T002R500x500M000{amid}.jpg?max_age=2592000",
                        Name = ns["album"]["name"].ToString()
                    };
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
                GFdata = gdList,
                Gdata = Gdata,
                NewMusic = NewMusic
            };
        }
        #endregion
        #region MV
        public static async Task<List<MusicPL>> GetMVPL(string id)
        {
            JObject ds = JObject.Parse(await HttpHelper.GetWebDatacAsync("https://c.y.qq.com/base/fcgi-bin/fcg_global_comment_h5.fcg?g_tk=" + Settings.USettings.g_tk + "&loginUin=" + Settings.USettings.LemonAreeunIts + "&hostUin=0&format=json&inCharset=utf8&outCharset=GB2312&notice=0&platform=yqq.json&needNewCode=0&cid=205360772&reqtype=2&biztype=5&topid=" + id + "&cmd=8&needmusiccrit=0&pagenum=0&pagesize=25&lasthotcommentid=&domain=qq.com&ct=24&cv=10101010"));
            List<MusicPL> data = new List<MusicPL>();
            JToken hcc = ds["hot_comment"]["commentlist"];
            for (int i = 0; i != hcc.Count(); i++)
            {
                JToken hcc_i = ds["hot_comment"]["commentlist"][i];
                MusicPL mpl = new MusicPL()
                {
                    img = hcc_i["avatarurl"].ToString(),
                    like = hcc_i["praisenum"].ToString(),
                    name = hcc_i["nick"].ToString(),
                    text = TextHelper.Exem(hcc_i["rootcommentcontent"].ToString().Replace(@"\n", "\n")),
                    commentid = hcc_i["commentid"].ToString()
                };
                DateTime dtStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
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
        public static async Task<string> GetMVDesc(string id)
        {
            JObject o = JObject.Parse(await HttpHelper.PostInycAsync("https://u.y.qq.com/cgi-bin/musicu.fcg",
                "{\"comm\":{\"g_tk\":\"" + Settings.USettings.g_tk + "\",\"uin\":\"" + Settings.USettings.LemonAreeunIts + "\",\"format\":\"json\",\"ct\":20,\"cv\":1710},\"mvinfo\":{\"module\":\"video.VideoDataServer\",\"method\":\"get_video_info_batch\",\"param\":{\"vidlist\":[\"" + id + "\"],\"required\":[\"vid\",\"type\",\"sid\",\"cover_pic\",\"duration\",\"singers\",\"video_switch\",\"msg\",\"name\",\"desc\",\"playcnt\",\"pubdate\",\"isfav\",\"gmid\"]}}}"));
            return o["mvinfo"]["data"][id]["desc"].ToString();
        }
        public static async Task<string> GetMVUrl(string id)
        {
            JObject o = JObject.Parse(await HttpHelper.PostInycAsync("https://u.y.qq.com/cgi-bin/musicu.fcg",
                "{\"getMvUrl\":{\"module\":\"gosrf.Stream.MvUrlProxy\",\"method\":\"GetMvUrls\",\"param\":{\"vids\":[\"" + id + "\"],\"request_typet\":10001}},\"comm\":{\"g_tk\":\"" + Settings.USettings.g_tk + "\",\"uin\":\"" + Settings.USettings.LemonAreeunIts + "\",\"format\":\"json\",\"ct\":20,\"cv\":1710}}"));
            var list = o["getMvUrl"]["data"][id]["mp4"];
            List<string> sList = new List<string>();
            foreach (var c in list)
            {
                if (c["freeflow_url"].Count() > 0)
                    sList.Add(c["freeflow_url"][0].ToString());
            }
            return sList.Last();
        }
        #endregion
        #region 评论 网易云|QQ音乐
        /// <summary>
        /// 网易云音乐评论
        /// </summary>
        /// <param name="name"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static async Task<List<MusicPL>> GetPLByWyyAsync(string name, int page = 1)
        {
            string Page = ((page - 1) * 20).ToString();
            string id = await GetWYIdByName(name);
            MainClass.DebugCallBack("WyyGDid",id);
            var data = await HttpHelper.GetWebAsync($"https://music.163.com/api/v1/resource/comments/R_SO_4_{id}?offset={Page}");
            MainClass.DebugCallBack("result",data);
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
        public static async Task<List<List<MusicPL>>> GetPLByQQAsync(string mid)
        {
            string id = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg?songmid={mid}&tpl=yqq_song_detail&format=json&g_tk=268405378&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"))["data"][0]["id"].ToString();
            var da = await HttpHelper.PostInycAsync("https://u.y.qq.com/cgi-bin/musicu.fcg",
"{\"req_0\":{\"method\":\"GetNewCommentList\",\"module\":\"music.globalComment.CommentReadServer\",\"param\":{\"BizType\":1,\"BizId\":\"" + id + "\",\"PageSize\":20,\"PageNum\":0,\"WithHot\":1}},\"req_1\":{\"method\":\"GetHotCommentList\",\"module\":\"music.globalComment.CommentReadServer\",\"param\":{\"BizType\":1,\"BizId\":\"" + id + "\",\"LastCommentSeqNo\":null,\"" +
"PageSize\":10,\"PageNum\":0,\"HotType\":2,\"WithAirborne\":1}},\"comm\":{\"g_tk\":" + Settings.USettings.g_tk + ",\"uin\":\"" + Settings.USettings.LemonAreeunIts + "\",\"format\":\"json\",\"ct\":20,\"cv\":1773,\"platform\":\"wk_v17\"}}");
            Console.WriteLine(da);
            JObject ds = JObject.Parse(da);
            List<List<MusicPL>> data = new List<List<MusicPL>>();
            var main = ds["req_0"]["data"];
            //---------最近热评-----
            List<MusicPL> Present = new List<MusicPL>();
            foreach (var a in main["CommentList3"]["Comments"])
            {
                Present.Add(BuildMusicPl(a));
            }
            data.Add(Present);
            //---------精彩评论-----
            List<MusicPL> Hot = new List<MusicPL>();
            foreach (var a in main["CommentList2"]["Comments"])
            {
                Hot.Add(BuildMusicPl(a));
            }
            data.Add(Hot);
            //---------最新评论-----
            List<MusicPL> Now = new List<MusicPL>();
            foreach (var a in main["CommentList"]["Comments"])
            {
                Now.Add(BuildMusicPl(a));
            }
            data.Add(Now);
            return data;
        }

        private static MusicPL BuildMusicPl(JToken a) {
            MusicPL mpl = new MusicPL()
            {
                img = a["Avatar"].ToString(),
                like = a["PraiseNum"].ToString(),
                name = a["Nick"].ToString(),
                text = TextHelper.Exem(a["Content"].ToString().Replace(@"\n", "\n")),
                commentid = a["CmId"].ToString()
            };
            DateTime dtStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            long lTime = long.Parse(a["PubTime"].ToString() + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime daTime = dtStart.Add(toNow);
            mpl.time = daTime.ToString("yyyy-MM-dd  HH:mm");
            if (a["IsPraised"].ToString() == "1")
                mpl.ispraise = true;
            else mpl.ispraise = false;
            return mpl;
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
            MainClass.DebugCallBack("GetMusicIdData",json);

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
            MainClass.DebugCallBack("Praise",id + " - " + mp.commentid);
            string get;
            if (mp.ispraise)
                get = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/base/fcgi-bin/fcg_global_comment_praise_h5.fcg?g_tk={Settings.USettings.LemonAreeunIts}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=GB2312&notice=0&platform=yqq.json&needNewCode=0&cid=205360774&cmd=2&reqtype=2&biztype=1&topid={id}&commentid={mp.commentid}&qq={Settings.USettings.LemonAreeunIts}&domain=qq.com&ct=24&cv=101010");
            else get = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/base/fcgi-bin/fcg_global_comment_praise_h5.fcg?g_tk={Settings.USettings.LemonAreeunIts}&loginUin={Settings.USettings.LemonAreeunIts}&hostUin=0&format=json&inCharset=utf8&outCharset=GB2312&notice=0&platform=yqq.json&needNewCode=0&cid=205360774&cmd=1&reqtype=2&biztype=1&topid={id}&commentid={mp.commentid}&qq={Settings.USettings.LemonAreeunIts}&domain=qq.com&ct=24&cv=101010");
            return get;
        }
        #endregion
    }
}