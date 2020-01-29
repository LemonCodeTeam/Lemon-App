using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LemonLib
{
    //三级缓存        By:Starlight
    public class ImageCacheHelp
    {
        public static async Task<BitmapImage> GetImageByUrl(string url, int[] DecodePixel =null) {
            try
            {
                BitmapImage bi = GetImageFormMemory(url);
                if (bi != null) { return bi; }
                bi = GetImageFromFile(url, DecodePixel);
                if (bi != null)
                {
                    AddImageToMemory(url, bi);
                    return bi;
                }
                return await GetImageFromInternet(url, DecodePixel);
            }
            catch {
                return null;
            }
        }
        private static MyDictionary<string, BitmapImage> MemoryData = new MyDictionary<string, BitmapImage>();
        private static BitmapImage GetImageFormMemory(string url) {
            string key= TextHelper.MD5.EncryptToMD5string(url);
            if (MemoryData.dic.ContainsKey(key))
                return MemoryData.dic[key];
            else return null;
        }
        private static void AddImageToMemory(string url,BitmapImage data)
        {
            string key = TextHelper.MD5.EncryptToMD5string(url);
            MemoryData.Add(key, data);
        }
        private static BitmapImage GetImageFromFile(string url,int[] DecodePixel) {
            string file = Settings.USettings.CachePath + "\\Image\\" + TextHelper.MD5.EncryptToMD5string(url)+".jpg";
            if (File.Exists(file))
                return new BitmapImage(new Uri(file, UriKind.Absolute));
            else return null;
        }

        private static async Task<BitmapImage> GetImageFromInternet(string url,int[] DecodePixel) {
            string file = Settings.USettings.CachePath + "\\Image\\" + TextHelper.MD5.EncryptToMD5string(url) + ".jpg";
            await HttpHelper.HttpDownloadFileAsync(url, file);
            BitmapImage bi = new BitmapImage(new Uri(file, UriKind.Absolute));
            AddImageToMemory(url, bi);
            return bi;
        }
    }
}
