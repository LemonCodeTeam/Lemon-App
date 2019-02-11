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
    public class MusicGDCacheHelper
    {
        public static int type = 0;
        public static async Task<MusicGData> GetMGData(string id)
        {
            MusicGData mg = GetDataFormMemory(id);
            type = 0;
            if (mg != null) return mg;
            mg = await GetDataFromFileAsync(id);
            type = 1;
            if (mg != null)
            {
                AddDataToMemory(id, mg);
                return mg;
            }
            type = 2;
            return await GetDataFromInternet(id);
        }

        public static MyDictionary<string, MusicGData> MemoryData = new MyDictionary<string, MusicGData>();
        public static MusicGData GetDataFormMemory(string id)
        {
            if (MemoryData.dic.ContainsKey(id))
                return MemoryData.dic[id];
            else return null;
        }
        public static void AddDataToMemory(string id,MusicGData data)
        {
            if (MemoryData.dic.ContainsKey(id))
                MemoryData.dic[id] = data;
            else
                MemoryData.Add(id, data);
        }
        public static async Task<MusicGData> GetDataFromFileAsync(string id)
        {
            string file = Settings.USettings.CachePath + "\\GData\\" + id;
            if (File.Exists(file))
            {
                StreamReader srReadFile = new StreamReader(file,Encoding.UTF8);
                string x=await srReadFile.ReadToEndAsync();
                srReadFile.Close();
                return StringToMusicGData(id, x);
            }
            else return null;
        }
        public static async void AddDataToFile(string id, string txt)
        {
            await Task.Run(() => {
                string file = Settings.USettings.CachePath + "\\GData\\" + id;
                FileStream fs = new FileStream(file, FileMode.Create);
                byte[] data = Encoding.UTF8.GetBytes(txt);
                fs.WriteAsync(data, 0, data.Length);
                fs.FlushAsync();
                fs.Close();
            });
        }
        public static async Task<MusicGData> GetDataFromInternet(string id)
        {
            string file = Settings.USettings.CachePath + "\\GData\\" + id;
            var s = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/qzone/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&json=1&utf8=1&onlysong=0&disstid={id}&format=json&g_tk=1157737156&loginUin={MusicLib.qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0", Encoding.UTF8);
            var dt = StringToMusicGData(id, s);
            AddDataToFile(id, s);
            AddDataToMemory(id, dt);
            return dt;
        }
        public static void UpdataData(string id,MusicGData dt,string s) {
            AddDataToFile(id, s);
            AddDataToMemory(id, dt);
        }
        public static MusicGData StringToMusicGData(string id,string s) {
            JObject o = JObject.Parse(s);
            var dt = new MusicGData();
            var c0 = o["cdlist"][0];
            dt.name = c0["dissname"].ToString();
            dt.pic = c0["logo"].ToString();
            dt.id = id;
            dt.ids = c0["songids"].ToString().Split(',').ToList();
            dt.IsOwn = c0["login"].ToString() == c0["uin"].ToString();
            var c0s = c0["songlist"];
            foreach (var c0si in c0s)
            {
                string singer = "";
                var c0sis = c0si["singer"];
                foreach (var cc in c0sis) singer += cc["name"].ToString() + "&";
                Music m = new Music();
                try
                {
                    m.MusicName = c0si["songname"].ToString();
                    m.MusicName_Lyric = c0si["albumdesc"].ToString();
                    m.Singer = singer.Substring(0, singer.Length - 1);
                    m.GC = c0si["songid"].ToString();
                    m.MusicID = c0si["songmid"].ToString();
                    var amid = c0si["albummid"].ToString();
                    if (amid == "001ZaCQY2OxVMg")
                        m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R300x300M000{c0si["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                    else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{amid}.jpg?max_age=2592000";
                }
                catch { }
                dt.Data.Add(m);
            }
            return dt;
        }
    }
}
