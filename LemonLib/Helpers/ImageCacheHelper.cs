using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LemonLib
{
    /// <summary>
    /// 图像的三级缓存 MemoryCache->FileCache->Internet
    /// </summary>
    public class ImageCacheHelper
    {
        private static ConditionalWeakTable<string, BitmapImage> MemoryCache = new();
        /// <summary>
        /// 图像的三级缓存 从URL中获取图像
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="DecodePixel">高Height,宽Width</param>
        /// <returns></returns>
        public static async Task<BitmapImage> GetImageByUrl(string url, int[] DecodePixel = null)
        {
            try
            {
                url += DecodePixel != null ? "#" + string.Join(",", DecodePixel) : "";
                //signed by HashCode, swifter than MD5.
                string id = url.GetHashCode().ToString();
                MainClass.DebugCallBack("IMAGE GETER",url);
                //Get from MemoryCache
                if (MemoryCache.TryGetValue(id, out BitmapImage bi))
                    return bi;
                //Get from FileCache
                BitmapImage bf = GetImageFromFile(url);
                if (bf != null)
                {
                    MemoryCache.Add(id, bf);
                    return bf;
                }
                //Get from Internet
                return await GetImageFromInternet(url, DecodePixel);
            }
            catch(Exception e)
            {
                MainClass.DebugCallBack("IMAGE ERROR",e.StackTrace+"\r\n"+e.ToString());
                return null;
            }
        }
        private static BitmapImage GetImageFromFile(string url)
        {
            //id signed by MD5 for long-time use
            string file = Path.Combine(Settings.USettings.MusicCachePath, "Image", TextHelper.MD5.EncryptToMD5string(url)+ ".jpg");
            if (File.Exists(file))
                return GetBitMapImageFromFile(file);
            else return null;
        }
        private static BitmapImage GetBitMapImageFromFile(string file)
        {
            using FileStream sr = File.OpenRead(file);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.StreamSource = sr;
            bi.EndInit();
            bi.Freeze();
            return bi;
        }
        private static async Task<BitmapImage> GetImageFromInternet(string url, int[] DecodePixel)
        {
            string file = Path.Combine(Settings.USettings.MusicCachePath, "Image", TextHelper.MD5.EncryptToMD5string(url) + ".jpg");
            if (DecodePixel != null)
            {
                Image img = Image.FromStream(await(await new HttpClient().GetAsync(url)).Content.ReadAsStreamAsync());
                Bitmap bitmap = new Bitmap(DecodePixel[1], DecodePixel[0]);
                Graphics g = Graphics.FromImage(bitmap);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, bitmap.Width, bitmap.Height);
                g.Dispose();
                bitmap.Save(file, ImageFormat.Jpeg);
                bitmap.Dispose();
                img.Dispose();
            }
            else
            {
                await HttpHelper.HttpDownloadFileAsync(url, file);
            }
            BitmapImage bi = GetBitMapImageFromFile(file);
            return bi;
        }
    }
}
