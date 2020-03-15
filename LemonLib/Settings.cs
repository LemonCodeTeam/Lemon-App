using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LemonLib.InfoHelper;

namespace LemonLib
{
    public class Settings
    {
        private static async Task<string> ReadEncodeAsync(string qq)
        {
            string data = Encoding.UTF8.GetString(Convert.FromBase64String(TextHelper.TextDecrypt(await File.ReadAllTextAsync(USettings.CachePath + qq + ".st"), TextHelper.MD5.EncryptToMD5string(qq + ".st"))));
            Console.WriteLine(data);
            return data;
        }
        #region USettings
        public static UserSettings USettings = new UserSettings();
        public static async void SaveSettings(string id = "id")
        {
            try
            {
                if (id == "id") id = USettings.LemonAreeunIts;
                await File.WriteAllTextAsync(USettings.CachePath + id + ".st", TextHelper.JSON.ToJSON(USettings));
            }
            catch { }
        }
        public static async Task LoadUSettings(string qq)
        {
            await Task.Run(async () =>
            {
                try
                {
                    USettings = new UserSettings();
                    if (File.Exists(USettings.CachePath + qq + ".st"))
                    {
                        string data =await File.ReadAllTextAsync(USettings.CachePath + qq + ".st");
                        Console.WriteLine(data);
                        XDUsettings(data);
                    }
                    else SaveSettings(qq);
                }
                catch
                {
                    XDUsettings(await ReadEncodeAsync(qq));
                    SaveSettings(qq);
                }
            });
        }
        private static void XDUsettings(string data)
        {
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
            foreach (var xs in o["Playing"]["Singer"])
            {
                USettings.Playing.Singer.Add(new MusicSinger()
                {
                    Name = xs["Name"].ToString(),
                    Mid = xs["Mid"].ToString()
                });
            }
            if (data.Contains("LyricAnimationMode"))
                USettings.LyricAnimationMode = int.Parse(o["LyricAnimationMode"].ToString());
            if (data.Contains("DoesOpenDeskLyric"))
                USettings.DoesOpenDeskLyric = bool.Parse(o["DoesOpenDeskLyric"].ToString());
            if (data.Contains("IsRoundMusicImage"))
                USettings.IsRoundMusicImage = int.Parse(o["IsRoundMusicImage"].ToString());
            if (data.Contains("DownloadName"))
                USettings.DownloadName = o["DownloadName"].ToString();
            if (data.Contains("DownloadWithLyric"))
                USettings.DownloadWithLyric = bool.Parse(o["DownloadWithLyric"].ToString());
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
            if (data.Contains("MusicGDataLike"))
            {
                string json=o["MusicGDataLike"]["ids"].ToString();
                USettings.MusicGDataLike.ids=JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            if (data.Contains("HotKeys")) {
                string json = o["HotKeys"].ToString();
                USettings.HotKeys = JsonConvert.DeserializeObject<List<HotKeyInfo>>(json);
            }
            if (data.Contains("MusicGDataPlayList"))
            {
                string json = o["MusicGDataPlayList"].ToString();
                USettings.MusicGDataPlayList = JsonConvert.DeserializeObject<List<Music>>(json);
            }
            if (data.Contains("PlayingIndex"))
                USettings.PlayingIndex = int.Parse(o["PlayingIndex"].ToString());
        }
        public class UserSettings
        {
            public UserSettings()
            {
                CachePath = Environment.ExpandEnvironmentVariables(@"%AppData%\LemonApp\");
                DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\LemonApp\\";
            }
            #region 用户配置
            public string LemonAreeunIts { get; set; } = "0";
            public string UserName { get; set; } = "";
            public string UserImage { get; set; } = "";
            public string Cookie { get; set; } = "pgv_info=ssid=s8469992130; pgv_pvid=7657244050; pgv_pvi=1902315520; pgv_si=s2246959104";
            public string g_tk { get; set; } = "5381";
            #endregion
            #region 上一次播放
            public Music Playing { get; set; } = new Music();
            public int PlayingIndex = -1;
            public List<Music> MusicGDataPlayList = new List<Music>();
            #endregion
            #region 主题配置
            //启用圆的MusicImage?
            public int IsRoundMusicImage { get; set; } = 5;
            //启动歌词页动效
            //0=波澜 1=无 2=转动
            public int LyricAnimationMode { get; set; } = 0;
            //是否打开桌面歌词
            public bool DoesOpenDeskLyric { get; set; } = true;
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
            public List<HotKeyInfo> HotKeys = new List<HotKeyInfo>();
            public MusicGLikeData MusicGDataLike = new MusicGLikeData();
        }
        #endregion

        #region LSettings
        public static LocaSettings LSettings = new LocaSettings();
        public static async void LoadLocaSettings()
        {
            try
            {
                if (File.Exists(USettings.CachePath + "Data.st"))
                {
                    string data = await File.ReadAllTextAsync(USettings.CachePath + "Data.st");
                    JObject o = JObject.Parse(data);
                    LSettings.qq = o["qq"].ToString();
                }
                else SaveLocaSettings();
            }
            catch
            {
                string data =await ReadEncodeAsync("Data");
                JObject o = JObject.Parse(data);
                LSettings.qq = o["qq"].ToString();
                SaveLocaSettings();
            }
        }
        public async static void SaveLocaSettings()
        {
            await File.WriteAllTextAsync(USettings.CachePath + "Data.st", TextHelper.JSON.ToJSON(LSettings));
        }
        public class LocaSettings
        {
            public string qq { get; set; } = "";
        }
        #endregion

        #region WINDOW_HANDLE
        public static HANDLE Handle = new HANDLE();
        public async static void SaveHandle()
        {
             await File.WriteAllTextAsync(USettings.CachePath + "HANDLE.st", TextHelper.JSON.ToJSON(Handle));
        }
        public static async Task<HANDLE> ReadHandleAsync()
        {
            JObject o = JObject.Parse(await File.ReadAllTextAsync(USettings.CachePath + "HANDLE.st"));
            Handle.ProcessId = int.Parse(o["ProcessId"].ToString());
            Handle.WINDOW_HANDLE = int.Parse(o["WINDOW_HANDLE"].ToString());
            return Handle;
        }
        public class HANDLE
        {
            //公共运行常量处理类..
            public int WINDOW_HANDLE { get; set; } = 0;
            public int ProcessId { get; set; } = 0;
        }
        #endregion
    }
}
