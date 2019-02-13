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
            await Task.Run(() => {
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
            try
            {
                USettings = new UserSettings();
                if (File.Exists(USettings.CachePath + qq + ".st"))
                {
                    string data = Encoding.UTF8.GetString(Convert.FromBase64String(TextHelper.TextDecrypt(File.ReadAllText(USettings.CachePath + qq + ".st"), TextHelper.MD5.EncryptToMD5string(qq + ".st"))));
                    JObject o = JObject.Parse(data);
                    USettings.LemonAreeunIts = o["LemonAreeunIts"].ToString();
                    USettings.UserImage = o["UserImage"].ToString();
                    USettings.UserName = o["UserName"].ToString();
                    USettings.Cookie = o["Cookie"].ToString();
                    USettings.g_tk = o["g_tk"].ToString();
                    USettings.Playing.GC = o["Playing"]["GC"].ToString();
                    USettings.Playing.ImageUrl = o["Playing"]["ImageUrl"].ToString();
                    USettings.Playing.MusicID = o["Playing"]["MusicID"].ToString();
                    USettings.Playing.MusicName = o["Playing"]["MusicName"].ToString();
                    USettings.Playing.Singer = o["Playing"]["Singer"].ToString();
                    foreach (var jx in o["MusicLike"].ToArray())
                    {
                        foreach (var jm in jx)
                        {
                            if (!USettings.MusicLike.ContainsKey(jm["MusicID"].ToString()))
                                USettings.MusicLike.Add(jm["MusicID"].ToString(), new Music()
                                {
                                    GC = jm["GC"].ToString(),
                                    MusicID = jm["MusicID"].ToString(),
                                    Singer = jm["Singer"].ToString(),
                                    ImageUrl = jm["ImageUrl"].ToString(),
                                    MusicName = jm["MusicName"].ToString()
                                });
                        }
                    }
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
            catch { SaveSettings(qq);}
        }
        public class UserSettings {
            public UserSettings() {
                CachePath = Environment.ExpandEnvironmentVariables(@"%AppData%\LemonApp\");
                DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\LemonApp\\";
            }
            #region 歌单
            public SortedDictionary<string, Music> MusicLike { get; set; } = new SortedDictionary<string, Music>();
             #endregion
            #region 用户配置
            public string LemonAreeunIts { get; set; } = "你的QQ";
            public string UserName { get; set; } = "";
            public string UserImage { get; set; } = "";
            public string Cookie { get; set; } = "";
            public string g_tk { get; set; } = "";
            #endregion
            #region 上一次播放
            public Music Playing { get; set; } = new Music();
            #endregion
            #region 主题配置
            public string Skin_Path { get; set; } = "";
            public string Skin_txt { get; set; } = "";
            public string Skin_Theme_R { get; set; } = "";
            public string Skin_Theme_G { get; set; } = "";
            public string Skin_Theme_B { get; set; } = "";
            #endregion
            #region 缓存/下载路径
            public string CachePath = @"C:\Users\cz241\AppData\Roaming\LemonApp\";
            public string DownloadPath = "";
            #endregion
        }
        #endregion

        #region LSettings
        public static LocaSettings LSettings = new LocaSettings();
        public static void LoadLocaSettings() {
            if (File.Exists(USettings.CachePath + "Data.st")){
                string data = Encoding.Default.GetString(Convert.FromBase64String(TextHelper.TextDecrypt(File.ReadAllText(USettings.CachePath + "Data.st"), TextHelper.MD5.EncryptToMD5string("Data.st"))));
                JObject o = JObject.Parse(data);
                LSettings.qq = o["qq"].ToString();
            }
            else SaveLocaSettings();
        }
        public static void SaveLocaSettings(){
            File.WriteAllText(USettings.CachePath + "Data.st", TextHelper.TextEncrypt(Convert.ToBase64String(Encoding.Default.GetBytes(TextHelper.JSON.ToJSON(LSettings))), TextHelper.MD5.EncryptToMD5string("Data.st")));
        }
        public class LocaSettings {
            public string qq { get; set; } = "EX";
        }
        #endregion

        #region WINDOW_HANDLE
        public static void SaveWINDOW_HANDLE(int WINDOW_HANDLE) {
            File.WriteAllText(USettings.CachePath + "WINDOW_HANDLE.INT", WINDOW_HANDLE.ToString());
        }
        public static int ReadWINDOW_HANDLE(){
          return int.Parse(File.ReadAllText(USettings.CachePath + "WINDOW_HANDLE.INT"));
        }
        #endregion
    }
}
