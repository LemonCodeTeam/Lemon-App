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
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + id + @".st", TextHelper.TextEncrypt(Convert.ToBase64String(Encoding.Default.GetBytes(TextHelper.JSON.ToJSON(Settings.USettings))), TextHelper.MD5.EncryptToMD5string(id + ".st")));
        }
        public static void SaveLoadSettings()
        {
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"Settings.st", TextHelper.TextEncrypt(Convert.ToBase64String(Encoding.Default.GetBytes(TextHelper.JSON.ToJSON(Settings.LSettings))),TextHelper.MD5.EncryptToMD5string("Settings.st")));
        }
        public class UserSettings {
            public List<Music> Music_Like { get; set; } = new List<Music>();
            public List<MusicGD> Music_GD { get; set; } = new List<MusicGD>();
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
