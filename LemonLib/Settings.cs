using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LemonLib.InfoHelper;
using System.Windows.Media;

namespace LemonLib
{
    public class Settings
    {
        private static async Task<string> ReadEncodeAsync(string qq)
        {
            string data = Encoding.UTF8.GetString(Convert.FromBase64String(TextHelper.TextDecrypt(await File.ReadAllTextAsync(USettings.DataCachePath + qq + ".st"), TextHelper.MD5.EncryptToMD5string(qq + ".st"))));
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
                await File.WriteAllTextAsync(USettings.DataCachePath + id + ".st", TextHelper.JSON.ToJSON(USettings));
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
                    if (File.Exists(USettings.DataCachePath + qq + ".st"))
                    {
                        string data =await File.ReadAllTextAsync(USettings.DataCachePath + qq + ".st");
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
            if (data.Contains("Skin_Type"))
            {
                USettings.Skin_Type =int.Parse( o["Skin_Type"].ToString());
                USettings.Skin_ThemeColor_R = byte.Parse(o["Skin_ThemeColor_R"].ToString());
                USettings.Skin_ThemeColor_G = byte.Parse(o["Skin_ThemeColor_G"].ToString());
                USettings.Skin_ThemeColor_B = byte.Parse(o["Skin_ThemeColor_B"].ToString());
                USettings.Skin_ImagePath = o["Skin_ImagePath"].ToString();
                USettings.Skin_FontColor = o["Skin_FontColor"].ToString();
            }
            if (data.Contains("MusicCachePath"))
            {
                USettings.MusicCachePath = o["MusicCachePath"].ToString();
                USettings.DownloadPath = o["DownloadPath"].ToString();
            }
            else
            {
                DriveInfo[] allDirves = DriveInfo.GetDrives();
                string dir = "C:\\";
                foreach (DriveInfo item in allDirves)
                {
                    if (item.DriveType == DriveType.Fixed && item.Name != "C:\\")
                    {
                        dir = item.Name;
                        return;
                    }
                }
                USettings.MusicCachePath = dir + "LemonAppCache\\";
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
            if (data.Contains("QuickGoToData"))
            {
                string json = o["QuickGoToData"].ToString();
                USettings.QuickGoToData= JsonConvert.DeserializeObject<Dictionary<string,QuickGoToData>>(json);
            }
            if (data.Contains("MusicGDataPlayList"))
            {
                string json = o["MusicGDataPlayList"].ToString();
                USettings.MusicGDataPlayList = JsonConvert.DeserializeObject<List<Music>>(json);
            }
            if (data.Contains("PlayingIndex"))
                USettings.PlayingIndex = int.Parse(o["PlayingIndex"].ToString());
            if (data.Contains("IsLyricImm"))
                USettings.IsLyricImm = bool.Parse(o["IsLyricImm"].ToString());
            if (data.Contains("IsMiniOpen"))
                USettings.IsMiniOpen = bool.Parse(o["IsMiniOpen"].ToString());
            if (data.Contains("PlayXHMode"))
                USettings.PlayXHMode = int.Parse(o["PlayXHMode"].ToString());
            if (data.Contains("LyricFontSize"))
                USettings.LyricFontSize = int.Parse(o["LyricFontSize"].ToString());
        }
        public class UserSettings
        {
            public UserSettings()
            {
                DriveInfo[] allDirves = DriveInfo.GetDrives();
                string dir = "C:\\";
                foreach (DriveInfo item in allDirves) {
                    if (item.DriveType == DriveType.Fixed&& item.Name!= "C:\\") {
                        dir = item.Name;
                        break;
                    }
                }
                MusicCachePath = dir + "LemonAppCache\\";
                DataCachePath = Environment.ExpandEnvironmentVariables(@"%AppData%\LemonApp\");
                DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\LemonApp\\";
            }
            #region 用户配置
            public string LemonAreeunIts { get; set; } = "0";
            public string UserName { get; set; } = "";
            public string UserImage { get; set; } = "";
            public string Cookie { get; set; } = "pgv_info=ssid=s8469992130; pgv_pvid=7657244050; pgv_pvi=1902315520; pgv_si=s2246959104";
            public string g_tk { get; set; } = "5381";
            #endregion
            #region 播放
            public Music Playing { get; set; } = new Music();
            public int PlayingIndex = -1;
            public List<Music> MusicGDataPlayList = new List<Music>();
            /// <summary>
            /// 播放模式 0列表循环 1单曲循环 2随机播放
            /// </summary>
            public int PlayXHMode = 0;
            /// <summary>
            /// 歌词字体大小..
            /// </summary>
            public int LyricFontSize = 18;
            #endregion
            #region 主题配置
            //启用圆的MusicImage?
            public int IsRoundMusicImage { get; set; } = 5;
            //启动歌词页动效
            //0=波澜 1=无 2=转动
            public int LyricAnimationMode { get; set; } = 0;
            //是否打开了沉浸歌词
            public bool IsLyricImm { get; set; } = false;
            //是否打开了mini小窗
            public bool IsMiniOpen { get; set; } = true;
            //是否打开桌面歌词
            public bool DoesOpenDeskLyric { get; set; } = true;

            /// <summary>
            /// 主题类型 0:Normal 1:Picture Theme 2.Blur For Win10 3:live theme
            /// </summary>
            public int Skin_Type = -1;
            /// <summary>
            /// 主题字体颜色 Black:黑字白底 White:白字黑底
            /// </summary>
            public string Skin_FontColor = "Black";
            /// <summary>
            /// 主题颜色
            /// </summary>
            public byte Skin_ThemeColor_R = 49, Skin_ThemeColor_G = 194, Skin_ThemeColor_B = 124;

            /// <summary>
            /// (如果有) 主题背景图/动态主题dll路径
            /// </summary>
            public string Skin_ImagePath = "";
            #endregion
            #region 缓存/下载路径
            public string DataCachePath = "";
            public string MusicCachePath = @"C:\Users\cz241\AppData\Roaming\LemonApp\";
            public string DownloadPath = "";
            public string DownloadName = "[I].  [M] - [S]";
            public bool DownloadWithLyric = true;
            #endregion

            public Dictionary<string, QuickGoToData> QuickGoToData = new Dictionary<string, QuickGoToData>();
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
                if (File.Exists(USettings.DataCachePath + "Data.st"))
                {
                    string data = await File.ReadAllTextAsync(USettings.DataCachePath + "Data.st");
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
            await File.WriteAllTextAsync(USettings.DataCachePath + "Data.st", TextHelper.JSON.ToJSON(LSettings));
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
             await File.WriteAllTextAsync(USettings.DataCachePath + "HANDLE.st", TextHelper.JSON.ToJSON(Handle));
        }
        public static async Task<HANDLE> ReadHandleAsync()
        {
            JObject o = JObject.Parse(await File.ReadAllTextAsync(USettings.DataCachePath + "HANDLE.st"));
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
