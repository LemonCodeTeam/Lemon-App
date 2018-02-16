using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LemonLibrary.InfoHelper;

namespace LemonLibrary
{
    public class Settings
    {
        public static UserSettings USettings = new Settings.UserSettings();
        public static LoadSettingsData LSettings = new LoadSettingsData();
        public static void SaveSettings(string id = "id")
        {
            if (id == "id") id = Settings.USettings.LemonAreeunIts;
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + id + ".st", TextHelper.TextEncrypt(Convert.ToBase64String(Encoding.Default.GetBytes(TextHelper.JSON.ToJSON(Settings.USettings))), TextHelper.MD5.EncryptToMD5string(id + ".st")));
        }
        public static void LoadUSettings(string data)
        {
            JObject o = JObject.Parse(data);
            USettings.LemonAreeunIts = o["LemonAreeunIts"].ToString();
            USettings.UserImage = o["UserImage"].ToString();
            USettings.UserName = o["UserName"].ToString();
            foreach (var jx in o["MusicLike"].ToArray())
            {
                foreach (var jm in jx)
                {
                    if(!USettings.MusicLike.ContainsKey(jm["MusicID"].ToString()))
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
            foreach (var jcm in o["MusicGD"])
            {
                if (!USettings.MusicGD.ContainsKey(jcm["ID"].ToString()))
                    USettings.MusicGD.Add(jcm["ID"].ToString(), new MusicGD() {ID= jcm["ID"].ToString(),Name= jcm["Name"].ToString(),Photo= jcm["Photo"].ToString()});
            }
        }
        public static void LoadLSettings(string data) {
            JObject o = JObject.Parse(data);
            LSettings.NAME = o["NAME"].ToString();
            LSettings.RNBM = Boolean.Parse(o["RNBM"].ToString());
            LSettings.TX = o["TX"].ToString();
        }
        public static void SaveLoadSettings()
        {
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Settings.st", TextHelper.TextEncrypt(Convert.ToBase64String(Encoding.Default.GetBytes(TextHelper.JSON.ToJSON(Settings.LSettings))), TextHelper.MD5.EncryptToMD5string("Settings.st")));
        }
        public class UserSettings {
            public UserSettings() {

            }
            public SortedDictionary<string, Music> MusicLike { get; set; } = new SortedDictionary<string, Music>();
            public SortedDictionary<string, MusicGD> MusicGD { get; set; } = new SortedDictionary<string, MusicGD>();
            public string LemonAreeunIts { get; set; } = "你的QQ";
            public string UserName { get; set; } = "";
            public string UserImage { get; set; } = "";
        }
        public class LoadSettingsData
        {
            public string TX { get; set; } = "";
            public string NAME { get; set; } = "QQ账号";
            public bool RNBM { get; set; } = false;
        }
    }
}
