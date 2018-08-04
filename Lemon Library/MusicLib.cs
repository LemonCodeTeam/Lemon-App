using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static LemonLibrary.InfoHelper;
/*
   作者:Twilight./Lemon        QQ:2728578956
   请保留版权信息，侵权必究。
     
     Author:Twilight./Lemon QQ:2728578956
Please retain the copyright information, rights reserved.
     */

/*
 TODO:music kb/s list:

$type = array(
        'size_320mp3' => array(320, 'M800', 'mp3'),
        'size_192aac' => array(192, 'C600', 'm4a'),
        'size_128mp3' => array(128, 'M500', 'mp3'),
        'size_96aac'  => array(96, 'C400', 'm4a'),
        'size_48aac'  => array(48, 'C200', 'm4a'),
        'size_24aac'  => array(24, 'C100', 'm4a'),
    );
 */
namespace LemonLibrary
{
    public class MusicLib {
        public MusicLib(LyricView LV,string id) {
            if (Directory.Exists(GetPath() + "Download") == false)
                Directory.CreateDirectory(GetPath() + "Download");
            if (Directory.Exists(GetPath() + "Cache") == false)
                Directory.CreateDirectory(GetPath() + "Cache");
            lv = LV;
            qq = id;
        }
        public MusicLib() {
            if (Directory.Exists(GetPath() + $@"Download") == false)
                Directory.CreateDirectory(GetPath() + $@"Download");
        }
        public Dictionary<string, string> mldata = new Dictionary<string, string>();// mid,name
        public MediaPlayer m = new MediaPlayer();
        public LyricView lv;
        public string qq = "";
        public async Task<List<Music>> SearchMusicAsync(string Content, int osx = 1)
        {
            if (HttpHelper.IsNetworkTrue())
            {
                JObject o = JObject.Parse(await HttpHelper.GetWebAsync($"http://59.37.96.220/soso/fcgi-bin/client_search_cp?format=json&t=0&inCharset=GB2312&outCharset=utf-8&qqmusic_ver=1302&catZhida=0&p={osx}&n=20&w={Content}&flag_qc=0&remoteplace=sizer.newclient.song&new_json=1&lossless=0&aggr=1&cr=1&sem=0&force_zonghe=0"));
                List<Music> dt = new List<Music>();
                int i = 0;
                while (i < o["data"]["song"]["list"].Count())
                {
                    Music m = new Music();
                    m.MusicName = o["data"]["song"]["list"][i]["name"].ToString().Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
                    string Singer = "";
                    for (int osxc = 0; osxc != o["data"]["song"]["list"][i]["singer"].Count(); osxc++)
                    { Singer += o["data"]["song"]["list"][i]["singer"][osxc]["name"] + "&"; }
                    m.Singer = Singer.Substring(0, Singer.LastIndexOf("&"));
                    m.MusicID = o["data"]["song"]["list"][i]["mid"].ToString();
                    m.ImageUrl = $"http://y.gtimg.cn/music/photo_new/T002R300x300M000{o["data"]["song"]["list"][i]["album"]["mid"]}.jpg";
                    m.GC = o["data"]["song"]["list"][i]["id"].ToString();
                    dt.Add(m);
                    try
                    {
                        if (!mldata.ContainsKey(m.MusicID))
                            mldata.Add(m.MusicID, (m.MusicName+" - "+m.Singer).Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""));
                    }
                    catch { }
                    i++;
                }
                return dt;
            }
            else return null;
        }
        public async Task<MusicGData> GetGDAsync(string id = "2591355982") {
            var s = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/qzone/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&json=1&utf8=1&onlysong=0&disstid={id}&format=json&g_tk=1157737156&loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0", Encoding.UTF8);
            JObject o = JObject.Parse(s);
            var dt = new MusicGData();
            dt.name = o["cdlist"][0]["dissname"].ToString();
            dt.pic = o["cdlist"][0]["logo"].ToString();
            dt.id = id;
            int i = 0;
            while (i != o["cdlist"][0]["songlist"].Count())
            {
                try
                {
                    Music m = new Music()
                    {
                        MusicName = o["cdlist"][0]["songlist"][i]["songname"].ToString().Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""),
                        Singer = o["cdlist"][0]["songlist"][i]["singer"][0]["name"].ToString(),
                        GC = o["cdlist"][0]["songlist"][i]["songid"].ToString(),
                        MusicID = o["cdlist"][0]["songlist"][i]["songmid"].ToString(),
                        ImageUrl = $"http://y.gtimg.cn/music/photo_new/T002R300x300M000{o["cdlist"][0]["songlist"][i]["albummid"]}.jpg"
                    };
                    dt.Data.Add(m);
                    if (!mldata.ContainsKey(m.MusicID))
                        mldata.Add(m.MusicID, (m.MusicName + " - " + m.Singer).Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""));
                    i++;
                }
                catch { i++; }
            }
            return dt;
        }
        public async Task UpdateGdAsync() {
            var dt = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=205360838&ct=20&userid={qq}&reqfrom=1&reqtype=0",Encoding.UTF8, "pgv_pvi=9798155264; RK=JKKMei2V0M; ptcz=f60f58ab93a9b59848deb2d67b6a7a4302dd1208664e448f939ed122c015d8d1; pgv_pvid=4173718307; ts_uid=5327745136; ts_uid=5327745136; pt2gguin=o2728578956; ts_refer=xui.ptlogin2.qq.com/cgi-bin/xlogin; yq_index=0; o_cookie=2728578956; pac_uid=1_2728578956; pgv_info=ssid=s8910034002; pgv_si=s3134809088; _qpsvr_localtk=0.8145813010716534; uin=o2728578956; skey=@ZF3GfLQsE; ptisp=ctc; luin=o2728578956; lskey=00010000c504a12a536ab915ce52f0ba2a3d24042adcea8e3b78ef55972477fd6d67417e4fc27cdaa8a0bd86; p_uin=o2728578956; pt4_token=YoecK598VtlFoQ7Teus8nC51UayhpD9rfitjZ6BMUkc_; p_skey=SFU7-V*Vwn3XsXtF3MF4T2OAOBbSp96ol-zzMbhcCzM_; p_luin=o2728578956; p_lskey=00040000768e027ce038844edbd57908c83024d365b4a86c9c12cf8b979d473a573567e70c30bd779d5f20cd; yqq_stat=0");
            var o = JObject.Parse(dt);
            var data = Settings.USettings.MusicGD;
            var dx = o["data"]["mydiss"]["list"];
            foreach (var ex in dx) {
                if (!data.ContainsKey(ex["dissid"].ToString()))
                {
                    var df = new MusicGData();
                    df.id = ex["dissid"].ToString();
                    df.Data = (await GetGDAsync(df.id)).Data;
                    df.name = ex["title"].ToString();
                    if (ex["picurl"].ToString() != "")
                        df.pic = ex["picurl"].ToString();
                    else df.pic = df.Data[0].ImageUrl;
                    data.Add(df.id, df);
                }
            }
            Settings.USettings.MusicGD = data;
        }
        public async Task<string> GetUrlAsync(string Musicid)
        {
            //固定GUID(随机)
            var guid = "365305415";
            //通过Musicid获取mid
            var mid = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg?songmid={Musicid}&platform=yqq&format=json"))["data"][0]["file"]["media_mid"].ToString();
            //获取key
            var key = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/base/fcgi-bin/fcg_musicexpress.fcg?json=3&guid={guid}&format=json"))["key"].ToString();
            return $"https://dl.stream.qqmusic.qq.com/M800{mid}.mp3?vkey={key}&guid={guid}&uid=0&fromtag=30";
        }
        public string GetWyUrlAsync(string mid)
        {//TODO: 原生api
            return JObject.Parse(HttpHelper.PostWeb("http://lab.mkblog.cn/music/api.php", $"types=url&id={mid}&source=netease"))["url"].ToString();
        }
        public async void GetAndPlayMusicUrlAsync(string mid, Boolean openlyric, TextBlock x, Window s,bool ispos, bool doesplay = true)
        {
            string name = mldata[mid];
            if (ispos) name = "Wy" + name + ".mp3";
            else name = name + ".mp3";
            if (!File.Exists(GetPath() + $@"Download/{name}"))
            {
                string musicurl = "";
                if (ispos)
                    musicurl=GetWyUrlAsync(GetWYIdByName(name));
                else musicurl=await GetUrlAsync(mid);
                WebClient dc = new WebClient();
                dc.DownloadFileCompleted += delegate {
                    m.Open(new Uri(GetPath() + $@"Download/{name}", UriKind.Absolute));
                    if (doesplay)
                        m.Play();
                    s.Dispatcher.Invoke(DispatcherPriority.Normal, new System.Windows.Forms.MethodInvoker(delegate ()
                    {
                        x.Text = TextHelper.XtoYGetTo("[" + name, "[", " -", 0).Replace("Wy","");
                    }));
                };
                dc.DownloadFileAsync(new Uri(musicurl), GetPath() + $@"Download/{name}");
                dc.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs e) {
                    s.Dispatcher.Invoke(DispatcherPriority.Normal, new System.Windows.Forms.MethodInvoker(delegate ()
                    {
                        x.Text = "加载中..." + e.ProgressPercentage + "%";
                    }));
                };
            }
            else
            {
                m.Open(new Uri(GetPath() + $@"Download/{name}", UriKind.Absolute));
                if (doesplay)
                    m.Play();
                s.Dispatcher.Invoke(DispatcherPriority.Normal, new System.Windows.Forms.MethodInvoker(delegate ()
                {
                    x.Text = TextHelper.XtoYGetTo("[" + name, "[", " -", 0).Replace("Wy","");
                }));
            }
            if (openlyric)
            {
                if (ispos) {
                    name = TextHelper.XtoYGetTo("[" + name, "[", ".mp3", 0);
                    var dt =await GetLyricByWYAsync(name);
                    lv.LoadLrc(dt);
                }
                else
                {
                    var dt = GetLyric(mid);
                    lv.LoadLrc(dt);
                }
            }
        }
        public string GetLyric(string McMind)
        {
            string name = mldata[McMind];
            if (!File.Exists(GetPath() + $@"Download/{name}.lrc"))
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
                if (o["trans"].ToString() == "") return t;
                else
                {
                    string x = Encoding.UTF8.GetString(Convert.FromBase64String(o["trans"].ToString())).Replace("&apos;", "\'");
                    Console.WriteLine(t + "\r\n" + x);
                    List<string> datatime = new List<string>();
                    List<string> datatext = new List<string>();
                    Dictionary<string, string> gcdata = new Dictionary<string, string>();
                    string[] dta = t.Split('\n');
                    foreach (var dt in dta)
                    {
                        try { LyricView.parserLine(dt, datatime, datatext, gcdata); } catch { }
                    }
                    List<String> dataatimes = new List<String>();
                    List<String> dataatexs = new List<String>();
                    Dictionary<String, String> fydata = new Dictionary<String, String>();
                    String[] dtaa = x.Split('\n');
                    foreach (var dt in dtaa)
                    {
                        try { LyricView.parserLine(dt, dataatimes, dataatexs, fydata); } catch { }
                    }
                    List<String> KEY = new List<String>();
                    Dictionary<String, String> gcfydata = new Dictionary<String, String>();
                    Dictionary<String, String> list = new Dictionary<String, String>();
                    foreach (var dt in datatime)
                    {
                        try
                        {
                            KEY.Add(dt);
                            gcfydata.Add(dt, "");
                        }
                        catch { }
                    }
                    for (int i = 0; i != gcfydata.Count; i++)
                    {
                        try
                        {
                            if (fydata.ContainsKey(KEY[i]))
                                gcfydata[KEY[i]] = (gcdata[KEY[i]] + "^" + fydata[KEY[i]]).Replace("\n", "").Replace("\r", "");
                            else gcfydata[KEY[i]] = (gcdata[KEY[i]] + "^").Replace("\n", "").Replace("\r", "");
                        }
                        catch { }
                    }
                    string LyricData = "";
                    for (int i = 0; i != KEY.Count; i++)
                    {
                        try
                        {
                            String value = gcfydata[KEY[i]].Replace("[", "").Replace("]", "");
                            String key = KEY[i];
                            LyricData += $"[{key}]{value}\r\n";
                        }
                        catch { }
                    }
                    File.WriteAllText(GetPath() + $@"Download/{name}.lrc", LyricData);
                    return LyricData;
                }
            }
            else
            {
                return File.ReadAllText(GetPath() + $@"Download/{name}.lrc");
            }
        }
        public async Task<List<MusicTop>> GetTopIndexAsync() {
            var dt = await HttpHelper.GetWebAsync("https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_opt.fcg?page=index&format=html&tpl=macv4&v8debug=1");
            var sh = "{\"data\":" + dt.Replace("jsonCallback(", "").Replace("}]\n)", "") + "}]" + "}";
            var o = JObject.Parse(sh);
            var data = new List<MusicTop>();
            int i = 0;
            while (i < o["data"][0]["List"].Count()) {
                data.Add(new MusicTop {
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
        public async Task<List<Music>> GetToplistAsync(int TopID) {
            JObject o = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_cp.fcg?tpl=3&page=detail&topid={TopID}&type=top&song_begin=0&song_num=30&g_tk=1206122277&loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"));
            List<Music> dt = new List<Music>();
            int i = 0;
            while (i < o["songlist"].Count())
            {
                Music m = new Music();
                m.MusicName = o["songlist"][i]["data"]["songname"].ToString().Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
                string Singer = "";
                for (int osxc = 0; osxc != o["songlist"][i]["data"]["singer"].Count(); osxc++)
                { Singer += o["songlist"][i]["data"]["singer"][osxc]["name"] + "&"; }
                m.Singer = Singer.Substring(0, Singer.LastIndexOf("&"));
                m.MusicID = o["songlist"][i]["data"]["songmid"].ToString();
                m.ImageUrl = $"http://y.gtimg.cn/music/photo_new/T002R300x300M000{o["songlist"][i]["data"]["albummid"]}.jpg";
                m.GC = o["songlist"][i]["data"]["songmid"].ToString();
                dt.Add(m);
                if (!mldata.ContainsKey(m.MusicID))
                    mldata.Add(m.MusicID, (m.MusicName+" - "+m.Singer).Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""));
                i++;
            }
            return dt;
        }
        public async Task<List<MusicSinger>> GetSingerAsync(string key, int page = 1) {
            var o = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/v8.fcg?channel=singer&page=list&key={key}&pagesize=100&pagenum={page}&g_tk=5381&loginUin=0&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"));
            var data = new List<MusicSinger>();
            int i = 0;
            while (i < o["data"]["list"].Count()) {
                data.Add(new MusicSinger
                {
                    Name = o["data"]["list"][i]["Fsinger_name"].ToString(),
                    Photo = $"https://y.gtimg.cn/music/photo_new/T001R150x150M000{o["data"]["list"][i]["Fsinger_mid"]}.jpg?max_age=2592000"
                });
                i++;
            }
            return data;
        }
        public async Task<MusicFLGDIndexItemsList> GetFLGDIndexAsync() {
            var o = JObject.Parse(await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/splcloud/fcgi-bin/fcg_get_diss_tag_conf.fcg?g_tk=1206122277&loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0",Encoding.UTF8));
            var data = new MusicFLGDIndexItemsList();
            data.Hot.Add(new MusicFLGDIndexItems { id = "10000000", name = "全部" });
            int i = 0;
            while (i < o["data"]["categories"][1]["items"].Count()) {
                data.Lauch.Add(new MusicFLGDIndexItems {
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
        public async Task<List<MusicGD>> GetFLGDAsync(int id) {
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
        public async Task<MusicRadioList> GetRadioList() {
            var o = JObject.Parse(await HttpHelper.GetWebAsync("https://c.y.qq.com/v8/fcg-bin/fcg_v8_radiolist.fcg?channel=radio&format=json&page=index&tpl=wk&new=1&p=0.8663229811059507&g_tk=5381&loginUin=0&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"));
            var data = new MusicRadioList();
            int i = 0;
            while (i < o["data"]["data"]["groupList"][0]["radioList"].Count())
            {
                data.Hot.Add(new MusicRadioListItem {
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
            return data;
        }
        public async Task<Music> GetRadioMusicAsync(string id) {
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
            else {
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
        public async Task<MusicGData> GetGDbyWYAsync(string id,Window x,TextBlock tb,ProgressBar pb) {
            string data = HttpHelper.PostWeb("http://lab.mkblog.cn/music/api.php", "types=playlist&id="+id);
            JObject o = JObject.Parse(data);
            var dt = new MusicGData();
            dt.name = o["playlist"]["name"].ToString();
            dt.id = o["playlist"]["id"].ToString();
            dt.pic = o["playlist"]["coverImgUrl"].ToString();
            x.Dispatcher.Invoke(() => { pb.Maximum = o["playlist"]["tracks"].Count(); });
            for (int i = 0; i != o["playlist"]["tracks"].Count(); i++) {
                try
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
                catch { }
                }
            return dt;
        }
        public async Task<List<MusicPL>> GetPLAsync(string name,int page=1) {
            string Page = ((page - 1) * 20).ToString();
            string id = GetWYIdByName(name);
            var data =await HttpHelper.GetWebAsync($"https://music.163.com/api/v1/resource/comments/R_SO_4_{id}?offset={Page}");
            JObject o = JObject.Parse(data);
            var d = new List<MusicPL>();
            for (int i = 0; i != o["hotComments"].Count(); i++) {
                d.Add(new MusicPL() {
                    text = o["hotComments"][i]["content"].ToString(),
                    name = o["hotComments"][i]["user"]["nickname"].ToString(),
                    img = o["hotComments"][i]["user"]["avatarUrl"].ToString(),
                    like = o["hotComments"][i]["likedCount"].ToString()
                });
            }
            return d;
        }
        public async Task<List<MusicPL>> GetPLByQQAsync(string mid) {
            var id = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg?songmid={mid}&tpl=yqq_song_detail&format=json&g_tk=268405378&loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"))["data"][0]["id"].ToString();
            var dt = await HttpHelper.GetWebAsync($"https://c.y.qq.com/base/fcgi-bin/fcg_global_comment_h5.fcg?g_tk=642290724&loginUin={qq}&hostUin=0&format=json&inCharset=utf8&outCharset=GB2312&notice=0&platform=yqq&needNewCode=0&cid=205360772&reqtype=2&biztype=1&topid={id}&cmd=8&needmusiccrit=0&pagenum=0&pagesize=25&lasthotcommentid=&domain=qq.com&ct=24&cv=101010");
            var ds = JObject.Parse(dt.Replace("\n",""));
            var data = new List<MusicPL>();
            for (int i = 0; i != ds["hot_comment"]["commentlist"].Count(); i++) {
                data.Add(new MusicPL() {
                    img = ds["hot_comment"]["commentlist"][i]["avatarurl"].ToString(),
                    like = ds["hot_comment"]["commentlist"][i]["praisenum"].ToString(),
                    name= ds["hot_comment"]["commentlist"][i]["nick"].ToString(),
                    text= ds["hot_comment"]["commentlist"][i]["rootcommentcontent"].ToString()
                });
            }
            return data;
        }
        public string GetWYIdByName(string name) {
            var ds = "{\"data\":" + HttpHelper.PostWeb("http://lab.mkblog.cn/music/api.php", "types=search&count=20&source=netease&pages=1&name=" + Uri.EscapeDataString(name)) + "}";
            var s = JObject.Parse(ds);
            return s["data"][0]["id"].ToString();
        }
        public async Task<string> GetLyricByWYAsync(string name)
        {
            string id = GetWYIdByName(name);
            if (!File.Exists(GetPath() + $@"Download/Wy{name}.lrc"))
            {
                string s = await HttpHelper.GetWebAsync($"http://music.163.com/api/song/lyric?os=pc&id={id}&lv=-1&kv=-1&tv=-1");
                Console.WriteLine(s);
                JObject o = JObject.Parse(s);
                string t = o["lrc"]["lyric"].ToString();
                if (o["tlyric"]["lyric"].ToString() == "") return t;
                else
                {
                    string x = o["tlyric"]["lyric"].ToString();
                    Console.WriteLine(t + "\r\n" + x);
                    List<string> datatime = new List<string>();
                    List<string> datatext = new List<string>();
                    Dictionary<string, string> gcdata = new Dictionary<string, string>();
                    string[] dta = t.Split('\n');
                    foreach (var dt in dta)
                    {
                        try { LyricView.parserLine(dt, datatime, datatext, gcdata); } catch { }
                    }
                    List<String> dataatimes = new List<String>();
                    List<String> dataatexs = new List<String>();
                    Dictionary<String, String> fydata = new Dictionary<String, String>();
                    String[] dtaa = x.Split('\n');
                    foreach (var dt in dtaa)
                    {
                        try { LyricView.parserLine(dt, dataatimes, dataatexs, fydata); } catch { }
                    }
                    List<String> KEY = new List<String>();
                    Dictionary<String, String> gcfydata = new Dictionary<String, String>();
                    Dictionary<String, String> list = new Dictionary<String, String>();
                    foreach (var dt in datatime)
                    {
                        try
                        {
                            KEY.Add(dt);
                            gcfydata.Add(dt, "");
                        }
                        catch { }
                    }
                    //sdm("d3");
                    //        foreach (var dt in dataatimes)
                    //            {
                    //           if (!KEY.Contains(dt))
                    //         {
                    //     KEY.Add(dt);//q
                    //          gcfydata.Add(dt, "");
                    //             }
                    //                }
                    //sdm("d4");
                    for (int i = 0; i != gcfydata.Count; i++)
                    {
                        try
                        {
                            if (fydata.ContainsKey(KEY[i]))
                                gcfydata[KEY[i]] = (gcdata[KEY[i]] + "^" + fydata[KEY[i]]).Replace("\n", "").Replace("\r", "");
                            else gcfydata[KEY[i]] = (gcdata[KEY[i]] + "^").Replace("\n", "").Replace("\r", "");
                        }
                        catch { }
                    }
                    string LyricData = "";
                    //sdm("d5   "+dataatexs.size()+"   "+dataatimes.size()+"   "+datatexs.size()+"   "+datatimes.size()+"   "+KEY.size());
                    for (int i = 0; i != KEY.Count; i++)
                    {
                        try
                        {
                            String value = gcfydata[KEY[i]].Replace("[", "").Replace("]", "");
                            String key = KEY[i];
                            LyricData += $"[{key}]{value}\r\n";
                        }
                        catch { }
                    }
                    File.WriteAllText(GetPath() + $@"Download/Wy{name}.lrc", LyricData);
                    return LyricData;
                }
            }
            else
            {
                return File.ReadAllText(GetPath() + $@"Download/Wy{name}.lrc");
            }
        }
    }
}