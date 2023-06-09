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
/*
 USettings的使用:
     1.在程序启动时载入数据
     2.使用时直接对USettings对象读取和赋值
     3.在程序退出时保存

更改和添加USettings:
     1.在 class UserSettings中编辑需要的类型(及默认值) 在构造函数中更新默认值等(例如检索磁盘目录)
     2.在 方法 XDUsettings(string data) 中从json中读取
 */
namespace LemonLib
{
    /// <summary>
    /// Settings 保存和读取用户配置、实例信息
    /// </summary>
    public class Settings
    {
        #region USettings
        public static UserSettings USettings = new UserSettings();
        public static async Task SaveSettingsTaskAsync(string id =null)
        {
            try
            {
               id ??=USettings.LemonAreeunIts;
                await File.WriteAllTextAsync(USettings.DataCachePath + id + ".st", TextHelper.JSON.ToJSON(USettings));
            }
            catch { }
        }
        public static async void SaveSettingsAsync(string id = null)
        {
            try
            {
                id ??= USettings.LemonAreeunIts;
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
                    else SaveSettingsAsync(qq);
                }
                catch{}
            });
        }
        /// <summary>
        /// 从json中解析出Settings项
        /// </summary>
        /// <param name="data"></param>
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
            if (data.Contains("TransLyric"))
                USettings.TransLyric = bool.Parse(o["TransLyric"].ToString());
            if (data.Contains("RomajiLyric"))
                USettings.RomajiLyric = bool.Parse(o["RomajiLyric"].ToString());
            if (data.Contains("LyricAppBarOpen"))
                USettings.LyricAppBarOpen = bool.Parse(o["LyricAppBarOpen"].ToString());
        }
        public class UserSettings
        {
            public UserSettings()
            {
                //加载默认的缓存和下载文件夹
                //1. Settings配置文件放在用户目录 %AppData% (C:\User\xxx\AppData\Roaming\LemonApp)
                //2. 音乐、歌词、图片等缓存目录放在 第二个盘符(如果存在)\LemonAppCache\ 
                //3. 下载目录放在用户目录 我的音乐 文件夹下
                //以上2、3项的目录可在应用中设置..
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

            /// <summary>
            /// 开启歌词翻译
            /// </summary>
            public bool TransLyric = true;
            /// <summary>
            /// 开启罗马音译
            /// </summary>
            public bool RomajiLyric = true;
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
            //是否打开歌词AppBar
            public bool LyricAppBarOpen { get; set; } = false;
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

        #region LSettings 用于保存上次登录的qq 在启动时读取
        public static LocaSettings LSettings = new LocaSettings();
        public static async Task LoadLocaSettings()
        {
            try
            {
                if (File.Exists(USettings.DataCachePath + "Data.st"))
                {
                    string data = await File.ReadAllTextAsync(USettings.DataCachePath + "Data.st");
                    JObject o = JObject.Parse(data);
                    LSettings.qq = o["qq"].ToString();
                }
                else await SaveLocaSettings();
            }
            catch{}
        }
        public async static Task SaveLocaSettings()
        {
            await File.WriteAllTextAsync(USettings.DataCachePath + "Data.st", TextHelper.JSON.ToJSON(LSettings));
        }
        public class LocaSettings
        {
            public string qq { get; set; } = "";
        }
        #endregion

        #region WINDOW_HANDLE
        //用于储存当前应用实例的 主窗口句柄 和 进程pid
        //便于当打开2个实例时退出并呼唤主窗口

        public static HANDLE Handle = new HANDLE();
        public async static void SaveHandle()
        {
             await File.WriteAllTextAsync(USettings.DataCachePath + "HANDLE.st", TextHelper.JSON.ToJSON(Handle));
        }
        public static async Task<HANDLE> ReadHandleAsync()
        {
            if (File.Exists(USettings.DataCachePath + "HANDLE.st"))
            {
                JObject o = JObject.Parse(await File.ReadAllTextAsync(USettings.DataCachePath + "HANDLE.st"));
                Handle.ProcessId = int.Parse(o["ProcessId"].ToString());
                Handle.WINDOW_HANDLE = int.Parse(o["WINDOW_HANDLE"].ToString());
                return Handle;
            }
            return null;
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
