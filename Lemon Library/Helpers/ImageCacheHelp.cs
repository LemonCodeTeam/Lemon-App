using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LemonLibrary
{
    //三级缓存        By:Starlight
    public class ImageCacheHelp
    {
        public static async Task<BitmapImage> GetImageByUrl(string url) {
            try
            {
                BitmapImage bi = GetImageFormMemory(url);
                if (bi != null) { return bi; }
                bi = GetImageFromFile(url);
                if (bi != null)
                {
                    AddImageToMemory(url, bi);
                    return bi;
                }
                return await GetImageFromInternet(url);
            }
            catch {
                return null;
            }
        }
        public static MyDictionary<string, BitmapImage> MemoryData = new MyDictionary<string, BitmapImage>();
        public static BitmapImage GetImageFormMemory(string url) {
            string key= TextHelper.MD5.EncryptToMD5string(url);
            if (MemoryData.dic.ContainsKey(key))
                return MemoryData.dic[key];
            else return null;
        }
        public static void AddImageToMemory(string url,BitmapImage data)
        {
            string key = TextHelper.MD5.EncryptToMD5string(url);
            MemoryData.Add(key, data);
        }
        public static BitmapImage GetImageFromFile(string url) {
            string file = Settings.USettings.CachePath + "\\Image\\" + TextHelper.MD5.EncryptToMD5string(url)+".jpg";
            if (File.Exists(file))
                return new System.Drawing.Bitmap(file).ToBitmapImage();
            else return null;
        }
        public static async void AddImageToFile(string url, BitmapImage data) {
            await Task.Run(() => {
                string filename = TextHelper.MD5.EncryptToMD5string(url) + ".jpg";
                BitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(data));

                using (var fileStream = new FileStream(Settings.USettings.CachePath + "\\Image\\" + filename, FileMode.Create))
                    encoder.Save(fileStream);
            });
        }
        public static async Task<BitmapImage> GetImageFromInternet(string url) {
            WebClient wc = new WebClient();
            BitmapImage bi = (await wc.DownloadDataTaskAsync(url)).ToBitmapImage();
            AddImageToFile(url, bi);
            AddImageToMemory(url, bi);
            return bi;
        }
    }
}
