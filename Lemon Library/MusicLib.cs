using LemonLibrary.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        public MusicLib(LyricView LV, string id)
        {
            if (!Directory.Exists(Settings.USettings.DownloadPath))
                Directory.CreateDirectory(Settings.USettings.DownloadPath);
            if (!Directory.Exists(Settings.USettings.CachePath))
                Directory.CreateDirectory(Settings.USettings.CachePath);
            if (!Directory.Exists(Settings.USettings.CachePath+"Music\\"))
                Directory.CreateDirectory(Settings.USettings.CachePath+"Music\\");
            if (!Directory.Exists(Settings.USettings.CachePath + "Lyric\\"))
                Directory.CreateDirectory(Settings.USettings.CachePath + "Lyric\\");
            if (!Directory.Exists(Settings.USettings.CachePath + "Image\\"))
                Directory.CreateDirectory(Settings.USettings.CachePath + "Image\\");
            lv = LV;
            qq = id;
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
        }
        public Dictionary<string, string> mldata = new Dictionary<string, string>();// mid,name
        public static PlayerControl pc = new PlayerControl();
        public LyricView lv;
        public string qq = "";
        public async Task<string> GetImageUrlByIDAsync(string mid) {
            var op = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg?songmid={mid}&platform=yqq&format=json"))["data"][0];
            var op_abid = op["album"]["mid"].ToString();
            var url = $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{op_abid}.jpg?max_age=2592000";
            if (op_abid == "001ZaCQY2OxVMg") {
                op_abid = op["singer"][0]["mid"].ToString();
                url = $"https://y.gtimg.cn/music/photo_new/T001R300x300M000{op_abid}.jpg?max_age=2592000";
            }
            return url;
        }
        public async Task<List<Music>> SearchMusicAsync(string Content, int osx = 1)
        {
            if (HttpHelper.IsNetworkTrue())
            {
                JObject o = JObject.Parse(await HttpHelper.GetWebAsync($"http://59.37.96.220/soso/fcgi-bin/client_search_cp?format=json&t=0&inCharset=GB2312&outCharset=utf-8&qqmusic_ver=1302&catZhida=0&p={osx}&n=20&w={HttpUtility.UrlDecode(Content)}&flag_qc=0&remoteplace=sizer.newclient.song&new_json=1&lossless=0&aggr=1&cr=1&sem=0&force_zonghe=0"));
                List<Music> dt = new List<Music>();
                int i = 0;
                while (i < o["data"]["song"]["list"].Count())
                {
                    Music m = new Music();
                    m.MusicName = o["data"]["song"]["list"][i]["title"].ToString().Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
                    m.MusicName_Lyric = o["data"]["song"]["list"][i]["lyric"].ToString().Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
                    string Singer = "";
                    for (int osxc = 0; osxc != o["data"]["song"]["list"][i]["singer"].Count(); osxc++)
                    { Singer += o["data"]["song"]["list"][i]["singer"][osxc]["name"] + "&"; }
                    m.Singer = Singer.Substring(0, Singer.LastIndexOf("&"));
                    m.MusicID = o["data"]["song"]["list"][i]["mid"].ToString();
                    m.ImageUrl =await GetImageUrlByIDAsync(m.MusicID);
                    m.GC = o["data"]["song"]["list"][i]["id"].ToString();
                    dt.Add(m);
                    if (!mldata.ContainsKey(m.MusicID))
                        mldata.Add(m.MusicID, (m.MusicName + " - " + m.Singer).Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""));
                    i++;
                }
                return dt;
            }
            else return null;
        }
        public async Task<List<string>> Search_SmartBoxAsync(string key) {
            var data = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/splcloud/fcgi-bin/smartbox_new.fcg?key={HttpUtility.UrlDecode(key)}&utf8=1&is_xml=0&loginUin=2728578956&qqmusic_ver=1592&searchid=3DA3E73D151F48308932D9680A3A5A1722872&pcachetime=1535710304"))["data"];
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
        public async Task<MusicGData> GetGDAsync(string id = "2591355982")
        {
            var s = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/qzone/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&json=1&utf8=1&onlysong=0&disstid={id}&format=json&g_tk=1157737156&loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0", Encoding.UTF8);
            JObject o = JObject.Parse(s);
            var dt = new MusicGData();
            dt.name = o["cdlist"][0]["dissname"].ToString();
            dt.pic = o["cdlist"][0]["logo"].ToString();
            dt.id = id;
            int i = 0;
            while (i != o["cdlist"][0]["songlist"].Count())
            {
                string singer = "";
                for (int ix = 0; ix != o["cdlist"][0]["songlist"][i]["singer"].Count(); ix++)
                { singer += o["cdlist"][0]["songlist"][i]["singer"][ix]["name"].ToString() + "&"; }
                Music m = new Music();
                try
                {
                    m.MusicName = o["cdlist"][0]["songlist"][i]["songname"].ToString().Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
                    m.MusicName_Lyric = o["cdlist"][0]["songlist"][i]["albumdesc"].ToString();
                    m.Singer = singer.Substring(0, singer.Length - 1);
                    m.GC = o["cdlist"][0]["songlist"][i]["songid"].ToString();
                    m.MusicID = o["cdlist"][0]["songlist"][i]["songmid"].ToString();
                    m.ImageUrl = await GetImageUrlByIDAsync(m.MusicID);
                }//莫名其妙的System.NullReferenceException:“未将对象引用设置到对象的实例。”
                catch { }
                dt.Data.Add(m);
                if (!mldata.ContainsKey(m.MusicID))
                    mldata.Add(m.MusicID, (m.MusicName + " - " + m.Singer).Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""));
                i++;
            }
            return dt;
        }
        public async Task UpdateGdAsync()
        {
            var dt = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={qq}&reqfrom=1&reqtype=0", Encoding.UTF8, "pgv_pvi=9798155264; RK=JKKMei2V0M; ptcz=f60f58ab93a9b59848deb2d67b6a7a4302dd1208664e448f939ed122c015d8d1; pgv_pvid=4173718307; ts_uid=5327745136; ts_uid=5327745136; pt2gguin=o2728578956; ts_refer=xui.ptlogin2.qq.com/cgi-bin/xlogin; yq_index=0; o_cookie=2728578956; pac_uid=1_2728578956; pgv_info=ssid=s8910034002; pgv_si=s3134809088; _qpsvr_localtk=0.8145813010716534; uin=o2728578956; skey=@ZF3GfLQsE; ptisp=ctc; luin=o2728578956; lskey=00010000c504a12a536ab915ce52f0ba2a3d24042adcea8e3b78ef55972477fd6d67417e4fc27cdaa8a0bd86; p_uin=o2728578956; pt4_token=YoecK598VtlFoQ7Teus8nC51UayhpD9rfitjZ6BMUkc_; p_skey=SFU7-V*Vwn3XsXtF3MF4T2OAOBbSp96ol-zzMbhcCzM_; p_luin=o2728578956; p_lskey=00040000768e027ce038844edbd57908c83024d365b4a86c9c12cf8b979d473a573567e70c30bd779d5f20cd; yqq_stat=0");
            var o = JObject.Parse(dt);
            var dx = o["data"]["mydiss"]["list"];
            foreach (var ex in dx)
            {
                if (Settings.USettings.MusicGD.ContainsKey(ex["dissid"].ToString()))
                    Settings.USettings.MusicGD.Remove(ex["dissid"].ToString());
                var df = new MusicGData();
                df.id = ex["dissid"].ToString();
                df.Data = (await GetGDAsync(df.id)).Data;
                df.name = ex["title"].ToString();
                if (ex["picurl"].ToString() != "")
                    df.pic = ex["picurl"].ToString();
                else df.pic = "https://y.gtimg.cn/mediastyle/global/img/cover_playlist.png?max_age=31536000";
                Settings.USettings.MusicGD.Add(df.id, df);
            }
        }
        public async Task<string> GetUrlAsync(string Musicid)
        {
            List<String[]> MData = new List<String[]>();
            MData.Add(new String[] { "M800", "mp3" });
            MData.Add(new String[] { "C600", "m4a" });
            MData.Add(new String[] { "M500", "mp3" });
            MData.Add(new String[] { "C400", "m4a" });
            MData.Add(new String[] { "M200", "mp3" });
            MData.Add(new String[] { "M100", "mp3" });

            var guid = "365305415";
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
        }
        public async void GetAndPlayMusicUrlAsync(string mid, Boolean openlyric, Run x, Window s, bool doesplay = true)
        {
            string name = mldata[mid] + ".mp3";
            string downloadpath = Settings.USettings.CachePath+"Music\\" + mid+".mp3";
            if (!File.Exists(downloadpath))
            {
                string musicurl = "";
                musicurl = await GetUrlAsync(mid);
                WebClient dc = new WebClient();
                dc.DownloadFileCompleted += delegate
                {
                    var fm = File.Open(downloadpath, FileMode.Open);
                    if (fm.Length != 0)
                    {
                        fm.Close();
                        fm.Dispose();
                        pc.Open(downloadpath);
                        if (doesplay)
                            pc.Play();
                        s.Dispatcher.Invoke(DispatcherPriority.Normal, new System.Windows.Forms.MethodInvoker(delegate ()
                        {
                            x.Text = TextHelper.XtoYGetTo("[" + name, "[", " -", 0).Replace("Wy", "");
                        }));
                    }
                    else {
                        fm.Close();
                        fm.Dispose();
                        File.Delete(downloadpath);
                        GetAndPlayMusicUrlAsync(mid, openlyric, x, s, doesplay);
                    }
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
                var fm = File.Open(downloadpath, FileMode.Open);
                if (fm.Length != 0)
                {
                    fm.Close();
                    fm.Dispose();
                    pc.Open(downloadpath);
                    if (doesplay)
                        pc.Play();
                    s.Dispatcher.Invoke(DispatcherPriority.Normal, new System.Windows.Forms.MethodInvoker(delegate ()
                    {
                        x.Text = TextHelper.XtoYGetTo("[" + name, "[", " -", 0).Replace("Wy", "");
                    }));
                }
                else
                {
                    fm.Close();
                    fm.Dispose();
                    File.Delete(downloadpath);
                    GetAndPlayMusicUrlAsync(mid, openlyric, x, s, doesplay);
                }
            }
            if (openlyric)
            {
                name = TextHelper.XtoYGetTo("[" + name, "[", ".mp3", 0);
                string dt =await GetLyric(mid);
                lv.LoadLrc(dt);
            }
        }
        public string PushLyric(string t,string x,string file) {
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
        public async Task<string> GetLyric(string McMind)
        {
            string name = mldata[McMind];
            string file = Settings.USettings.CachePath+"Lyric\\" + name + ".lrc";
            if (!File.Exists(file))
            {
                WebClient c = new WebClient();
                c.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.110 Safari/537.36");
                c.Headers.Add("Accept", "*/*");
                c.Headers.Add("Referer", "https://y.qq.com/portal/player.html");
                c.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8");
                c.Headers.Add("Cookie", $"tvfe_boss_uuid=c3db0dcc4d677c60; pac_uid=1_{qq}; qq_slist_autoplay=on; ts_refer=ADTAGh5_playsong; RK=pKOOKi2f1O; pgv_pvi=8927113216; o_cookie={qq}; pgv_pvid=5107924810; ptui_loginuin={qq}; ptcz=897c17d7e17ae9009e018ebf3f818355147a3a26c6c67a63ae949e24758baa2d; pt2gguin=o{qq}; pgv_si=s5715204096; qqmusic_fromtag=66; yplayer_open=1; ts_last=y.qq.com/portal/player.html; ts_uid=996779984; yq_index=0");
                c.Headers.Add("Host", "c.y.qq.com");
                string s = TextHelper.XtoYGetTo(c.DownloadString($"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?callback=MusicJsonCallback_lrc&pcachetime=1494070301711&songmid={McMind}&g_tk=5381&jsonpCallback=MusicJsonCallback_lrc&loginUin=0&hostUin=0&format=jsonp&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"), "MusicJsonCallback_lrc(", ")", 0);
                Console.WriteLine(s);
                JObject o = JObject.Parse(s);
                string t = Encoding.UTF8.GetString(Convert.FromBase64String(o["lyric"].ToString())).Replace("&apos;", "\'");
                if (o["trans"].ToString() == "") { await Task.Run(()=> {File.WriteAllText(file, t); }); return t; }
                else
                {
                    string x = Encoding.UTF8.GetString(Convert.FromBase64String(o["trans"].ToString())).Replace("&apos;", "\'");
                    return PushLyric(t, x, file);
                }
            }
            else
                return File.ReadAllText(file);
        }
        public async Task<List<MusicTop>> GetTopIndexAsync()
        {
            var dt = await HttpHelper.GetWebAsync("https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_opt.fcg?page=index&format=html&tpl=macv4&v8debug=1");
            var sh = "{\"data\":" + dt.Replace("jsonCallback(", "").Replace("}]\n)", "") + "}]" + "}";
            var o = JObject.Parse(sh);
            var data = new List<MusicTop>();
            int i = 0;
            while (i < o["data"][0]["List"].Count())
            {
                data.Add(new MusicTop
                {
                    Name = o["data"][0]["List"][i]["ListName"].ToString(),
                    Photo = o["data"][0]["List"][i]["pic_v12"].ToString(),
                    ID = o["data"][0]["List"][i]["topID"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"][1]["List"].Count())
            {
                data.Add(new MusicTop
                {
                    Name = o["data"][1]["List"][i]["ListName"].ToString(),
                    Photo = o["data"][1]["List"][i]["pic_v12"].ToString(),
                    ID = o["data"][1]["List"][i]["topID"].ToString()
                });
                i++;
            }
            return data;
        }
        public async Task<List<Music>> GetToplistAsync(int TopID)
        {
            JObject o = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_cp.fcg?tpl=3&page=detail&topid={TopID}&type=top&song_begin=0&song_num=30&g_tk=1206122277&loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"));
            List<Music> dt = new List<Music>();
            int i = 0;
            while (i < o["songlist"].Count())
            {
                Music m = new Music();
                m.MusicName = o["songlist"][i]["data"]["songname"].ToString().Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
                m.MusicName_Lyric = o["songlist"][i]["data"]["albumdesc"].ToString();
                string Singer = "";
                for (int osxc = 0; osxc != o["songlist"][i]["data"]["singer"].Count(); osxc++)
                { Singer += o["songlist"][i]["data"]["singer"][osxc]["name"] + "&"; }
                m.Singer = Singer.Substring(0, Singer.LastIndexOf("&"));
                m.MusicID = o["songlist"][i]["data"]["songmid"].ToString();
                m.ImageUrl = await GetImageUrlByIDAsync(m.MusicID);
                m.GC = o["songlist"][i]["data"]["songmid"].ToString();
                dt.Add(m);
                if (!mldata.ContainsKey(m.MusicID))
                    mldata.Add(m.MusicID, (m.MusicName + " - " + m.Singer).Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""));
                i++;
            }
            return dt;
        }
        public async Task<List<MusicSinger>> GetSingerAsync(string key, int page = 1)
        {
            var o = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/v8.fcg?channel=singer&page=list&key={key}&pagesize=100&pagenum={page}&g_tk=5381&loginUin=0&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"));
            var data = new List<MusicSinger>();
            int i = 0;
            while (i < o["data"]["list"].Count())
            {
                data.Add(new MusicSinger
                {
                    Name = o["data"]["list"][i]["Fsinger_name"].ToString(),
                    Photo = $"https://y.gtimg.cn/music/photo_new/T001R150x150M000{o["data"]["list"][i]["Fsinger_mid"]}.jpg?max_age=2592000"
                });
                i++;
            }
            return data;
        }
        public async Task<MusicFLGDIndexItemsList> GetFLGDIndexAsync()
        {
            var o = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/splcloud/fcgi-bin/fcg_get_diss_tag_conf.fcg?g_tk=1206122277&loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0", Encoding.UTF8));
            var data = new MusicFLGDIndexItemsList();
            data.Hot.Add(new MusicFLGDIndexItems { id = "10000000", name = "全部" });
            int i = 0;
            while (i < o["data"]["categories"][1]["items"].Count())
            {
                data.Lauch.Add(new MusicFLGDIndexItems
                {
                    id = o["data"]["categories"][1]["items"][i]["categoryId"].ToString(),
                    name = o["data"]["categories"][1]["items"][i]["categoryName"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["categories"][2]["items"].Count())
            {
                data.LiuPai.Add(new MusicFLGDIndexItems
                {
                    id = o["data"]["categories"][2]["items"][i]["categoryId"].ToString(),
                    name = o["data"]["categories"][2]["items"][i]["categoryName"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["categories"][3]["items"].Count())
            {
                data.Theme.Add(new MusicFLGDIndexItems
                {
                    id = o["data"]["categories"][3]["items"][i]["categoryId"].ToString(),
                    name = o["data"]["categories"][3]["items"][i]["categoryName"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["categories"][4]["items"].Count())
            {
                data.Heart.Add(new MusicFLGDIndexItems
                {
                    id = o["data"]["categories"][4]["items"][i]["categoryId"].ToString(),
                    name = o["data"]["categories"][4]["items"][i]["categoryName"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["categories"][5]["items"].Count())
            {
                data.Changjing.Add(new MusicFLGDIndexItems
                {
                    id = o["data"]["categories"][5]["items"][i]["categoryId"].ToString(),
                    name = o["data"]["categories"][5]["items"][i]["categoryName"].ToString()
                });
                i++;
            }
            return data;
        }
        public async Task<List<MusicGD>> GetFLGDAsync(int id)
        {
            var o = JObject.Parse(await HttpHelper.GetWebDatadAsync($"https://c.y.qq.com/splcloud/fcgi-bin/fcg_get_diss_by_tag.fcg?picmid=1&rnd=0.38615680484561965&g_tk=5381&loginUin=0&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&categoryId={id}&sortId=5&sin=0&ein=29", Encoding.UTF8));
            var data = new List<MusicGD>();
            int i = 0;
            while (i < o["data"]["list"].Count())
            {
                data.Add(new MusicGD
                {
                    Name = o["data"]["list"][i]["dissname"].ToString(),
                    Photo = o["data"]["list"][i]["imgurl"].ToString(),
                    ID = o["data"]["list"][i]["dissid"].ToString()
                });
                i++;
            }
            return data;
        }
        public async Task<MusicRadioList> GetRadioList()
        {
            var o = JObject.Parse(await HttpHelper.GetWebAsync("https://c.y.qq.com/v8/fcg-bin/fcg_v8_radiolist.fcg?channel=radio&format=json&page=index&tpl=wk&new=1&p=0.8663229811059507&g_tk=5381&loginUin=0&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"));
            var data = new MusicRadioList();
            try
            {
                int i = 0;
                while (i < o["data"]["data"]["groupList"][0]["radioList"].Count())
                {
                    data.Hot.Add(new MusicRadioListItem
                    {
                        Name = o["data"]["data"]["groupList"][0]["radioList"][i]["radioName"].ToString(),
                        Photo = o["data"]["data"]["groupList"][0]["radioList"][i]["radioImg"].ToString(),
                        ID = o["data"]["data"]["groupList"][0]["radioList"][i]["radioId"].ToString()
                    });
                    i++;
                }
                i = 0;
                while (i < o["data"]["data"]["groupList"][1]["radioList"].Count())
                {
                    data.Evening.Add(new MusicRadioListItem
                    {
                        Name = o["data"]["data"]["groupList"][1]["radioList"][i]["radioName"].ToString(),
                        Photo = o["data"]["data"]["groupList"][1]["radioList"][i]["radioImg"].ToString(),
                        ID = o["data"]["data"]["groupList"][1]["radioList"][i]["radioId"].ToString()
                    });
                    i++;
                }
                i = 0;
                while (i < o["data"]["data"]["groupList"][2]["radioList"].Count())
                {
                    data.Love.Add(new MusicRadioListItem
                    {
                        Name = o["data"]["data"]["groupList"][2]["radioList"][i]["radioName"].ToString(),
                        Photo = o["data"]["data"]["groupList"][2]["radioList"][i]["radioImg"].ToString(),
                        ID = o["data"]["data"]["groupList"][2]["radioList"][i]["radioId"].ToString()
                    });
                    i++;
                }
                i = 0;
                while (i < o["data"]["data"]["groupList"][3]["radioList"].Count())
                {
                    data.Theme.Add(new MusicRadioListItem
                    {
                        Name = o["data"]["data"]["groupList"][3]["radioList"][i]["radioName"].ToString(),
                        Photo = o["data"]["data"]["groupList"][3]["radioList"][i]["radioImg"].ToString(),
                        ID = o["data"]["data"]["groupList"][3]["radioList"][i]["radioId"].ToString()
                    });
                    i++;
                }
                i = 0;
                while (i < o["data"]["data"]["groupList"][4]["radioList"].Count())
                {
                    data.Changjing.Add(new MusicRadioListItem
                    {
                        Name = o["data"]["data"]["groupList"][4]["radioList"][i]["radioName"].ToString(),
                        Photo = o["data"]["data"]["groupList"][4]["radioList"][i]["radioImg"].ToString(),
                        ID = o["data"]["data"]["groupList"][4]["radioList"][i]["radioId"].ToString()
                    });
                    i++;
                }
                i = 0;
                while (i < o["data"]["data"]["groupList"][5]["radioList"].Count())
                {
                    data.Style.Add(new MusicRadioListItem
                    {
                        Name = o["data"]["data"]["groupList"][5]["radioList"][i]["radioName"].ToString(),
                        Photo = o["data"]["data"]["groupList"][5]["radioList"][i]["radioImg"].ToString(),
                        ID = o["data"]["data"]["groupList"][5]["radioList"][i]["radioId"].ToString()
                    });
                    i++;
                }
                i = 0;
                while (i < o["data"]["data"]["groupList"][6]["radioList"].Count())
                {
                    data.Lauch.Add(new MusicRadioListItem
                    {
                        Name = o["data"]["data"]["groupList"][6]["radioList"][i]["radioName"].ToString(),
                        Photo = o["data"]["data"]["groupList"][6]["radioList"][i]["radioImg"].ToString(),
                        ID = o["data"]["data"]["groupList"][6]["radioList"][i]["radioId"].ToString()
                    });
                    i++;
                }
                i = 0;
                while (i < o["data"]["data"]["groupList"][7]["radioList"].Count())
                {
                    data.People.Add(new MusicRadioListItem
                    {
                        Name = o["data"]["data"]["groupList"][7]["radioList"][i]["radioName"].ToString(),
                        Photo = o["data"]["data"]["groupList"][7]["radioList"][i]["radioImg"].ToString(),
                        ID = o["data"]["data"]["groupList"][7]["radioList"][i]["radioId"].ToString()
                    });
                    i++;
                }
                i = 0;
                while (i < o["data"]["data"]["groupList"][8]["radioList"].Count())
                {
                    data.MusicTools.Add(new MusicRadioListItem
                    {
                        Name = o["data"]["data"]["groupList"][8]["radioList"][i]["radioName"].ToString(),
                        Photo = o["data"]["data"]["groupList"][8]["radioList"][i]["radioImg"].ToString(),
                        ID = o["data"]["data"]["groupList"][8]["radioList"][i]["radioId"].ToString()
                    });
                    i++;
                }
                i = 0;
                while (i < o["data"]["data"]["groupList"][9]["radioList"].Count())
                {
                    data.Diqu.Add(new MusicRadioListItem
                    {
                        Name = o["data"]["data"]["groupList"][9]["radioList"][i]["radioName"].ToString(),
                        Photo = o["data"]["data"]["groupList"][9]["radioList"][i]["radioImg"].ToString(),
                        ID = o["data"]["data"]["groupList"][9]["radioList"][i]["radioId"].ToString()
                    });
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
                var o = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/rcmusic2/fcgi-bin/fcg_guess_youlike_pc.fcg?g_tk=1206122277&loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=703&uin={qq}"));
                string Singer = "";
                for (int osxc = 0; osxc != o["songlist"][0]["singer"].Count(); osxc++)
                { Singer += o["songlist"][0]["singer"][osxc]["name"] + "&"; }
                var data = new Music
                {
                    MusicName = o["songlist"][0]["name"].ToString().Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""),
                    Singer = Singer.Substring(0, Singer.LastIndexOf("&")),
                    GC = o["songlist"][0]["mid"].ToString(),
                    MusicID = o["songlist"][0]["mid"].ToString(),
                    ImageUrl = $"http://y.gtimg.cn/music/photo_new/T002R300x300M000{o["songlist"][0]["album"]["mid"]}.jpg"
                };
                return data;
            }
            else
            {
                var o = JObject.Parse(await HttpHelper.GetWebAsync($"https://u.y.qq.com/cgi-bin/musicu.fcg?g_tk=1206122277&loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&data=%7B\"songlist\"%3A%7B\"module\"%3A\"pf.radiosvr\"%2C\"method\"%3A\"GetRadiosonglist\"%2C\"param\"%3A%7B\"id\"%3A{id}%2C\"firstplay\"%3A1%2C\"num\"%3A10%7D%7D%2C\"radiolist\"%3A%7B\"module\"%3A\"pf.radiosvr\"%2C\"method\"%3A\"GetRadiolist\"%2C\"param\"%3A%7B\"ct\"%3A\"24\"%7D%7D%2C\"comm\"%3A%7B\"ct\"%3A\"24\"%7D%7D"));
                string Singer = "";
                for (int osxc = 0; osxc != o["songlist"]["data"]["track_list"][0]["singer"].Count(); osxc++)
                { Singer += o["songlist"]["data"]["track_list"][0]["singer"][osxc]["name"] + "&"; }
                var data = new Music
                {
                    MusicName = o["songlist"]["data"]["track_list"][0]["name"].ToString().Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""),
                    Singer = Singer.Substring(0, Singer.LastIndexOf("&")),
                    GC = o["songlist"]["data"]["track_list"][0]["mid"].ToString(),
                    MusicID = o["songlist"]["data"]["track_list"][0]["mid"].ToString(),
                    ImageUrl = $"http://y.gtimg.cn/music/photo_new/T002R300x300M000{o["songlist"]["data"]["track_list"][0]["album"]["mid"]}.jpg"
                };
                return data;
            }
        }
        public async Task<MusicGData> GetGDbyWYAsync(string id, Window x, TextBlock tb, ProgressBar pb)
        {
            string data = HttpHelper.PostWeb("http://lab.mkblog.cn/music/api.php", "types=playlist&id=" + id);
            JObject o = JObject.Parse(data);
            var dt = new MusicGData();
            dt.name = o["playlist"]["name"].ToString();
            dt.id = o["playlist"]["id"].ToString();
            dt.pic = o["playlist"]["coverImgUrl"].ToString();
            x.Dispatcher.Invoke(() => { pb.Maximum = o["playlist"]["tracks"].Count(); });
            for (int i = 0; i != o["playlist"]["tracks"].Count(); i++)
            {
                var dtname = o["playlist"]["tracks"][i]["name"].ToString();
                var dtsinger = "";
                for (int dx = 0; dx != o["playlist"]["tracks"][i]["ar"].Count(); dx++)
                    dtsinger += o["playlist"]["tracks"][i]["ar"][dx]["name"] + "&";
                dtsinger = dtsinger.Substring(0, dtsinger.LastIndexOf("&"));
                var dtf = await SearchMusicAsync(dtname + "-" + dtsinger);
                if (dtf.Count > 0)
                {
                    dt.Data.Add(dtf[0]);
                    x.Dispatcher.Invoke(() => { pb.Value = i; tb.Text = dtf[0].MusicName + " - " + dtf[0].Singer; });
                }
                else x.Dispatcher.Invoke(() => { pb.Value--; });
            }
            return dt;
        }
        public async Task<List<MusicPL>> GetPLAsync(string name, int page = 1)
        {
            string Page = ((page - 1) * 20).ToString();
            string id = GetWYIdByName(name);
            var data = await HttpHelper.GetWebAsync($"https://music.163.com/api/v1/resource/comments/R_SO_4_{id}?offset={Page}");
            JObject o = JObject.Parse(data);
            var d = new List<MusicPL>();
            for (int i = 0; i != o["hotComments"].Count(); i++)
            {
                d.Add(new MusicPL()
                {
                    text = o["hotComments"][i]["content"].ToString(),
                    name = o["hotComments"][i]["user"]["nickname"].ToString(),
                    img = o["hotComments"][i]["user"]["avatarUrl"].ToString(),
                    like = o["hotComments"][i]["likedCount"].ToString()
                });
            }
            return d;
        }
        public async Task<List<MusicPL>> GetPLByQQAsync(string mid)
        {
            var id = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg?songmid={mid}&tpl=yqq_song_detail&format=json&g_tk=268405378&loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"))["data"][0]["id"].ToString();
            var dt = await HttpHelper.GetWebAsync($"https://c.y.qq.com/base/fcgi-bin/fcg_global_comment_h5.fcg?g_tk=642290724&loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=GB2312&notice=0&platform=yqq&needNewCode=0&cid=205360772&reqtype=2&biztype=1&topid={id}&cmd=8&needmusiccrit=0&pagenum=0&pagesize=25&lasthotcommentid=&domain=qq.com&ct=24&cv=101010");
            var ds = JObject.Parse(dt.Replace("\n", ""));
            var data = new List<MusicPL>();
            for (int i = 0; i != ds["hot_comment"]["commentlist"].Count(); i++)
            {
                data.Add(new MusicPL()
                {
                    img = ds["hot_comment"]["commentlist"][i]["avatarurl"].ToString(),
                    like = ds["hot_comment"]["commentlist"][i]["praisenum"].ToString(),
                    name = ds["hot_comment"]["commentlist"][i]["nick"].ToString(),
                    text = ds["hot_comment"]["commentlist"][i]["rootcommentcontent"].ToString().Replace(@"\n","\n")
                });
            }
            return data;
        }
        public string GetWYIdByName(string name)
        {
            var ds = "{\"data\":" + HttpHelper.PostWeb("http://lab.mkblog.cn/music/api.php", "types=search&count=20&source=netease&pages=1&name=" + Uri.EscapeDataString(name)) + "}";
            var s = JObject.Parse(ds);
            return s["data"][0]["id"].ToString();
        }
    }
}