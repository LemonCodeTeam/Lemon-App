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
    /// 图像的三级缓存 网络-内存-储存
    /// </summary>
    public class ImageCacheHelper
    {
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
                MainClass.DebugCallBack("IMAGE GETER",url);
                BitmapImage bi = GetImageFromFile(url);
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
            string file = Settings.USettings.MusicCachePath + "\\Image\\" + TextHelper.MD5.EncryptToMD5string(url) + ".jpg";
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
            string file = Settings.USettings.MusicCachePath + "\\Image\\" + TextHelper.MD5.EncryptToMD5string(url) + ".jpg";
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
