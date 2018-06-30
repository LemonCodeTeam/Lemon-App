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
            File.WriteAllText(InfoHelper.GetPath() + id + ".st", TextHelper.TextEncrypt(Convert.ToBase64String(Encoding.Default.GetBytes(TextHelper.JSON.ToJSON(Settings.USettings))), TextHelper.MD5.EncryptToMD5string(id + ".st")));
        }
        public static void LoadUSettings(string data)
        {
            JObject o = JObject.Parse(data);
            USettings.LemonAreeunIts = o["LemonAreeunIts"].ToString();
            USettings.UserImage = o["UserImage"].ToString();
            USettings.UserName = o["UserName"].ToString();
            USettings.Playing.GC= o["Playing"]["GC"].ToString();
            USettings.Playing.ImageUrl = o["Playing"]["ImageUrl"].ToString();
            USettings.Playing.MusicID = o["Playing"]["MusicID"].ToString();
            USettings.Playing.MusicName= o["Playing"]["MusicName"].ToString();
            USettings.Playing.Singer = o["Playing"]["Singer"].ToString();
            USettings.jd = Double.Parse(o["jd"].ToString());
            USettings.alljd = Double.Parse(o["alljd"].ToString());
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
                foreach (var jm in jcm) {
                    if (!USettings.MusicGD.ContainsKey(jm["id"].ToString()))
                    {
                        var datae = new List<Music>();
                        foreach (var dt in jm["Data"]) {
                            datae.Add(new Music() {
                                GC = dt["GC"].ToString(),
                                Singer = dt["Singer"].ToString(),
                                ImageUrl = dt["ImageUrl"].ToString(),
                                MusicID = dt["MusicID"].ToString(),
                                MusicName = dt["MusicName"].ToString()
                            });
                        }
                        USettings.MusicGD.Add(jm["id"].ToString(), new MusicGData()
                        {
                            id = jm["id"].ToString(),
                            name = jm["name"].ToString(),
                            pic = jm["pic"].ToString(),
                            Data=datae
                        });
                    }
                }
            }
            int.TryParse(o["skin"].ToString(), out int skin);
            USettings.skin = skin;
        }
        public static void LoadLSettings(string data) {
            JObject o = JObject.Parse(data);
            LSettings.NAME = o["NAME"].ToString();
            LSettings.RNBM = Boolean.Parse(o["RNBM"].ToString());
            LSettings.TX = o["TX"].ToString();
            int.TryParse(o["skin"].ToString(), out int skin);
            LSettings.skin = skin;
        }
        public static void SaveLoadSettings()
        {
            File.WriteAllText(InfoHelper.GetPath() + "Settings.st", TextHelper.TextEncrypt(Convert.ToBase64String(Encoding.Default.GetBytes(TextHelper.JSON.ToJSON(Settings.LSettings))), TextHelper.MD5.EncryptToMD5string("Settings.st")));
        }
        public class UserSettings {
            public UserSettings() {

            }
            public SortedDictionary<string, Music> MusicLike { get; set; } = new SortedDictionary<string, Music>();
            public SortedDictionary<string, MusicGData> MusicGD { get; set; } = new SortedDictionary<string, MusicGData>();
            public string LemonAreeunIts { get; set; } = "你的QQ";
            public string UserName { get; set; } = "";
            public string UserImage { get; set; } = "";
            public Music Playing { get; set; } = new Music();
            public double jd { get; set; } = 0;
            public double alljd { get; set; } = 0;
            public int skin { get; set; } = 0;
        }
        public class LoadSettingsData
        {
            public string TX { get; set; } = "";
            public string NAME { get; set; } = "QQ账号";
            public bool RNBM { get; set; } = false;
            public int skin { get; set; } = 0;
        }
    }
}
