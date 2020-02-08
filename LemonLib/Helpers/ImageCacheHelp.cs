using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LemonLib
{
    //三级缓存        By:Starlight
    public class ImageCacheHelp
    {
        /// <summary>
        /// 图像的三级缓存 从URL中获取图像
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="DecodePixel">高Height,宽Width</param>
        /// <returns></returns>
        public static async Task<BitmapImage> GetImageByUrl(string url, int[] DecodePixel =null) {
            try
            {
                url += DecodePixel != null ? "#"+string.Join(",", DecodePixel) : "";
                MainClass.DebugCallBack(url);
                BitmapImage bi = GetImageFormMemory(url, DecodePixel);
                if (bi != null) { return bi; }
                bi = GetImageFromFile(url);
                if (bi != null)
                {
                    AddImageToMemory(url, bi);
                    return bi;
                }
                return await GetImageFromInternet(url, DecodePixel);
            }
            catch(Exception e) {
                MainClass.DebugCallBack(e.Message);
                return null;
            }
        }
        private static ConditionalWeakTable<string, BitmapImage> MemoryData = new ConditionalWeakTable<string, BitmapImage>();
        private static BitmapImage GetImageFormMemory(string url, int[] Decode) {
            string key= TextHelper.MD5.EncryptToMD5string(url);
            BitmapImage bi;
            return MemoryData.TryGetValue(key, out bi) ? bi : null;
        }
        private static void AddImageToMemory(string url,BitmapImage data)
        {
            string key = TextHelper.MD5.EncryptToMD5string(url);
            MemoryData.AddOrUpdate(key, data);
        }

        private static BitmapImage GetImageFromFile(string url) {
            string file = Settings.USettings.CachePath + "\\Image\\" + TextHelper.MD5.EncryptToMD5string(url)+".jpg";
            if (File.Exists(file))
                return GetBitMapImageFromFile(file);
            else return null;
        }
        private static BitmapImage GetBitMapImageFromFile(string file) {
            using FileStream sr = File.OpenRead(file);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = sr;
            bi.EndInit();
            bi.Freeze();
            return bi;
        }
        private static async Task<BitmapImage> GetImageFromInternet(string url,int[] DecodePixel) {
            string file = Settings.USettings.CachePath + "\\Image\\" + TextHelper.MD5.EncryptToMD5string(url) + ".jpg";
            if (DecodePixel != null)
            {
                HttpWebRequest hwr = WebRequest.Create(url) as HttpWebRequest;
                using HttpWebResponse response = await hwr.GetResponseAsync() as HttpWebResponse;
                using Stream responseStream = response.GetResponseStream();
                Image img = Image.FromStream(responseStream);
                Bitmap bitmap = new Bitmap(DecodePixel[1], DecodePixel[0]);
                Graphics g = Graphics.FromImage(bitmap);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, bitmap.Width, bitmap.Height);
                g.Dispose();
                bitmap.Save(file, ImageFormat.Jpeg);
                bitmap.Dispose();
                img.Dispose();
            }
            else {
                await HttpHelper.HttpDownloadAsync(url, file);
            }
            BitmapImage bi = GetBitMapImageFromFile(file);
            AddImageToMemory(url, bi);
            return bi;
        }
    }
}
