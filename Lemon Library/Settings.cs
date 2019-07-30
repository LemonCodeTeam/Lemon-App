using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using static LemonLibrary.InfoHelper;

namespace LemonLibrary
{
    public class Settings
    {
        #region USettings
        public static UserSettings USettings = new UserSettings();
        public static async void SaveSettings(string id = "id")
        {
            await Task.Run(() =>
            {
                try
                {
                    if (id == "id") id = USettings.LemonAreeunIts;
                    File.WriteAllText(USettings.CachePath + id + ".st", TextHelper.TextEncrypt(Convert.ToBase64String(Encoding.UTF8.GetBytes(TextHelper.JSON.ToJSON(Settings.USettings))), TextHelper.MD5.EncryptToMD5string(id + ".st")));
                }
                catch { }
            });
        }
        public static void LoadUSettings(string qq)
        {
            USettings = new UserSettings();
            if (File.Exists(USettings.CachePath + qq + ".st"))
            {
                string data = Encoding.UTF8.GetString(Convert.FromBase64String(TextHelper.TextDecrypt(File.ReadAllText(USettings.CachePath + qq + ".st"), TextHelper.MD5.EncryptToMD5string(qq + ".st"))));
                Console.WriteLine(data);
                JObject o = JObject.Parse(data);
                USettings.LemonAreeunIts = o["LemonAreeunIts"].ToString();
                USettings.UserImage = o["UserImage"].ToString();
                USettings.UserName = o["UserName"].ToString();
                USettings.Cookie = o["Cookie"].ToString();
                USettings.g_tk = o["g_tk"].ToString();
                USettings.Playing.ImageUrl = o["Playing"]["ImageUrl"].ToString();
                USettings.Playing.MusicID = o["Playing"]["MusicID"].ToString();
                USettings.Playing.MusicName = o["Playing"]["MusicName"].ToString();
                USettings.Playing.SingerText = o["Playing"]["SingerText"].ToString();
                foreach (var xs in o["Playing"]["Singer"]) {
                    USettings.Playing.Singer.Add(new MusicSinger() {Name=xs["Name"].ToString(),
                        Mid=xs["Mid"].ToString()
                    });
                }
                foreach (var jx in o["MusicLike"].ToArray())
                {
                    foreach (var jm in jx)
                    {
                        if (!USettings.MusicLike.ContainsKey(jm["MusicID"].ToString()))
                        {
                            List<MusicSinger> lm = new List<MusicSinger>();
                            foreach (var xs in jm["Singer"])
                            {
                                lm.Add(new MusicSinger(){
                                    Name = xs["Name"].ToString(),
                                    Mid = xs["Mid"].ToString()
                                });
                            }
                            USettings.MusicLike.Add(jm["MusicID"].ToString(), new Music()
                            {
                                MusicID = jm["MusicID"].ToString(),
                                SingerText = jm["SingerText"].ToString(),
                                Singer=lm,
                                ImageUrl = jm["ImageUrl"].ToString(),
                                MusicName = jm["MusicName"].ToString()
                            });
                        }
                    }
                }
                if (data.Contains("IsRoundMusicImage"))
                    USettings.IsRoundMusicImage = int.Parse(o["IsRoundMusicImage"].ToString());
                if (data.Contains("DownloadName"))
                    USettings.DownloadName = o["DownloadName"].ToString();
                if (data.Contains("DownloadWithLyric"))
                    USettings.DownloadWithLyric= bool.Parse(o["DownloadWithLyric"].ToString());
                if (data.Contains("Skin_Path"))
                {
                    USettings.Skin_Path = o["Skin_Path"].ToString();
                    USettings.Skin_txt = o["Skin_txt"].ToString();
                    USettings.Skin_Theme_R = o["Skin_Theme_R"].ToString();
                    USettings.Skin_Theme_G = o["Skin_Theme_G"].ToString();
                    USettings.Skin_Theme_B = o["Skin_Theme_B"].ToString();
                }
                if (data.Contains("CachePath"))
                {
                    USettings.CachePath = o["CachePath"].ToString();
                    USettings.DownloadPath = o["DownloadPath"].ToString();
                }
                else
                {
                    USettings.CachePath = Environment.ExpandEnvironmentVariables(@"%AppData%\LemonApp\Cache\");
                    USettings.DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\LemonApp\\";
                }
            }
            else SaveSettings(qq);
        }
        public class UserSettings
        {
            public UserSettings()
            {
                CachePath = Environment.ExpandEnvironmentVariables(@"%AppData%\LemonApp\");
                DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\LemonApp\\";
            }
            #region 歌单
            public SortedDictionary<string, Music> MusicLike { get; set; } = new SortedDictionary<string, Music>();
            #endregion
            #region 用户配置
            public string LemonAreeunIts { get; set; } = "";
            public string UserName { get; set; } = "";
            public string UserImage { get; set; } = "";
            public string Cookie { get; set; } = "pgv_pvid=3531479395; euin_cookie=41BD1C4435DC375FBEDB49054DB76D6A7E144504ED74E35D; ptcz=b6a78a1389245b1d160bd02b1bd65a22d62fe28d6c0914e7264b6c74f1216b1f; pgv_pvi=4809115648; uin_cookie=2728578956; sensorsdata2015jssdkcross=%7B%22distinct_id%22%3A%22168c5952d117ea-0a44be39a4fa52-4f7b614a-1049088-168c5952d1218a%22%2C%22%24device_id%22%3A%22168c5952d117ea-0a44be39a4fa52-4f7b614a-1049088-168c5952d1218a%22%2C%22props%22%3A%7B%22%24latest_traffic_source_type%22%3A%22%E7%9B%B4%E6%8E%A5%E6%B5%81%E9%87%8F%22%2C%22%24latest_referrer%22%3A%22%22%2C%22%24latest_referrer_host%22%3A%22%22%2C%22%24latest_search_keyword%22%3A%22%E6%9C%AA%E5%8F%96%E5%88%B0%E5%80%BC_%E7%9B%B4%E6%8E%A5%E6%89%93%E5%BC%80%22%7D%7D; luin=o2728578956; RK=sKKMfg2M0M; ptui_loginuin=3545039435; lskey=00010000d1e86791d11afac56238fb6c21b4cd18569d8cfcb9408efb9e6655484611b1b5866e11b41a54c33c; pgv_si=s2290829312; _qpsvr_localtk=0.2537786283402402; ptisp=cm; uin=o2728578956; skey=@ZkYewUA2E; ts_last=y.qq.com/portal/profile.html; p_lskey=0004000037d15eb69d9dd01f3d1e0a3944c9698d5273a3ec8195a08e74454a505dc4420591f6b65f7514105e; ts_refer=xui.ptlogin2.qq.com/cgi-bin/xlogin; ts_uid=3700488506; p_luin=o2728578956; p_uin=o2728578956; pt4_token=4r1g3HfBwvrtAtxNBu0HdtWblkUbd4bNzJza-bU3LPo_; p_skey=sf61L*8fbh52eBNoq47ywuHrS*5VwShQ5K9svtDkONU_; yqq_stat=0";
            public string g_tk { get; set; } = "778593258";
            #endregion
            #region 上一次播放
            public Music Playing { get; set; } = new Music();
            #endregion
            #region 主题配置
            //启用圆的MusicImage?
            public int IsRoundMusicImage { get; set; } = 5;
            public string Skin_Path { get; set; } = "";
            public string Skin_txt { get; set; } = "";
            public string Skin_Theme_R { get; set; } = "";
            public string Skin_Theme_G { get; set; } = "";
            public string Skin_Theme_B { get; set; } = "";
            #endregion
            #region 缓存/下载路径
            public string CachePath = @"C:\Users\cz241\AppData\Roaming\LemonApp\";
            public string DownloadPath = "";
            public string DownloadName = "[I].  [M] - [S]";
            public bool DownloadWithLyric = true;
            #endregion
        }
        #endregion

        #region LSettings
        public static LocaSettings LSettings = new LocaSettings();
        public static void LoadLocaSettings()
        {
            if (File.Exists(USettings.CachePath + "Data.st"))
            {
                string data = Encoding.Default.GetString(Convert.FromBase64String(TextHelper.TextDecrypt(File.ReadAllText(USettings.CachePath + "Data.st"), TextHelper.MD5.EncryptToMD5string("Data.st"))));
                JObject o = JObject.Parse(data);
                LSettings.qq = o["qq"].ToString();
            }
            else SaveLocaSettings();
        }
        public static void SaveLocaSettings()
        {
            File.WriteAllText(USettings.CachePath + "Data.st", TextHelper.TextEncrypt(Convert.ToBase64String(Encoding.Default.GetBytes(TextHelper.JSON.ToJSON(LSettings))), TextHelper.MD5.EncryptToMD5string("Data.st")));
        }
        public class LocaSettings
        {
            public string qq { get; set; } = "";
        }
        #endregion

        #region WINDOW_HANDLE
        public static HANDLE Handle = new HANDLE();
        public static void SaveHandle()
        {
            File.WriteAllText(USettings.CachePath + "HANDLE.st",TextHelper.JSON.ToJSON(Handle));
        }
        public static HANDLE ReadHandle()
        {
            JObject o = JObject.Parse(File.ReadAllText(USettings.CachePath + "HANDLE.st"));
            Handle.ProcessId = int.Parse(o["ProcessId"].ToString());
            Handle.WINDOW_HANDLE = int.Parse(o["WINDOW_HANDLE"].ToString());
            return Handle;
        }
        public class HANDLE {
            //公共运行常量处理类..
            public int WINDOW_HANDLE { get; set; } = 0;
            public int ProcessId { get; set; } = 0;
        }
        #endregion
    }
}
