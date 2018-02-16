using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LemonLibrary
{
    public static class ImageHelper
    {
        /// <summary>
        /// 实现System.Drawing.Bitmap到System.Windows.Media.Imaging.BitmapImage的转换
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(ms.ToArray());
            bitmapImage.EndInit();

            return bitmapImage;
        }
        [DllImport("gdi32.dll", SetLastError = true)]

        private static extern bool DeleteObject(IntPtr hObject);
        /// <summary>
        /// 从bitmap转换成ImageSource
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static ImageSource ToImageSource(this Bitmap bitmap)
        {

            //Bitmap bitmap = icon.ToBitmap();

            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            if (!DeleteObject(hBitmap))
            {
                throw new System.ComponentModel.Win32Exception();
            }

            return wpfBitmap;

        }
        /// <summary>
        /// 从Bitmap转换成BitmapSource
        /// </summary>
        /// <param name="bitmapp"></param>
        /// <returns></returns>
        public static BitmapSource ToBitmapSource(this Bitmap bitmapp)
        {
            BitmapSource returnSource;
            try
            {
                returnSource = Imaging.CreateBitmapSourceFromHBitmap(bitmapp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                returnSource = null;
            }
            return returnSource;
        }
        /// <summary>
        /// 从Icon到ImageSource的转换
        /// </summary> 
        public static ImageSource ToImageSource(this Icon icon)
        {
            ImageSource imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            return imageSource;
        }
        /// <summary>
        /// 马赛克处理
        /// </summary>
        /// <param name="要处理的图像"></param>
        /// <param name="每一格马赛克的宽度"></param>
        /// <returns></returns>
        public static Bitmap AdjustTobitmaposaic(this Bitmap bitmap, int effectWidth)
        {
            // 差异最多的就是以照一定范围取样 玩之后直接去下一个范围
            for (int heightOfffset = 0; heightOfffset < bitmap.Height; heightOfffset += effectWidth)
            {
                for (int widthOffset = 0; widthOffset < bitmap.Width; widthOffset += effectWidth)
                {
                    int avgR = 0, avgG = 0, avgB = 0;
                    int blurPixelCount = 0;

                    for (int x = widthOffset; (x < widthOffset + effectWidth && x < bitmap.Width); x++)
                    {
                        for (int y = heightOfffset; (y < heightOfffset + effectWidth && y < bitmap.Height); y++)
                        {
                            System.Drawing.Color pixel = bitmap.GetPixel(x, y);

                            avgR += pixel.R;
                            avgG += pixel.G;
                            avgB += pixel.B;

                            blurPixelCount++;
                        }
                    }

                    // 计算范围平均
                    avgR = avgR / blurPixelCount;
                    avgG = avgG / blurPixelCount;
                    avgB = avgB / blurPixelCount;


                    // 所有范围内都设定此值
                    for (int x = widthOffset; (x < widthOffset + effectWidth && x < bitmap.Width); x++)
                    {
                        for (int y = heightOfffset; (y < heightOfffset + effectWidth && y < bitmap.Height); y++)
                        {

                            System.Drawing.Color newColor = System.Drawing.Color.FromArgb(avgR, avgG, avgB);
                            bitmap.SetPixel(x, y, newColor);
                        }
                    }
                }
            }

            return bitmap;
        }
        /// <summary>
        /// 将数组转化为Bitmap
        /// </summary>
        /// <param name="Bytes"></param>
        /// <returns></returns>
        public static Bitmap ToBitmap(this byte[] Bytes)
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(Bytes);
                return new Bitmap(new Bitmap(stream));
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            finally
            {
                stream.Close();
            }
        }
        /// <summary>
        /// 将Bitmap转化为数组
        /// </summary>
        /// <param name="Bitmap"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this Bitmap Bitmap)
        {
            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream();
                Bitmap.Save(ms, Bitmap.RawFormat);
                byte[] byteImage = new byte[ms.Length];
                byteImage = ms.ToArray();
                return byteImage;
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            finally
            {
                ms.Close();
            }
        }
        /// <summary>
        /// 将数组转化为BitmapImage
        /// </summary>
        /// <param name="Bytes"></param>
        /// <returns></returns>
        public static BitmapImage ToBitmapImage(this byte[] byteArray)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = new MemoryStream(byteArray);
            bmp.EndInit();
            return bmp;
        }
        /// <summary>
        /// 将BitmapImage转化为数组
        /// </summary>
        /// <param name="Bitmap"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this BitmapImage bmp)
        {
            byte[] byteArray = null;
            Stream sMarket = bmp.StreamSource;
            if (sMarket != null && sMarket.Length > 0)
            {
                //很重要，因为Position经常位于Stream的末尾，导致下面读取到的长度为0。 
                sMarket.Position = 0;

                using (BinaryReader br = new BinaryReader(sMarket))
                {
                    byteArray = br.ReadBytes((int)sMarket.Length);
                }
            }
            return byteArray;
        }
        /// <summary>
        /// 重设Bitmap图片大小
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="w">宽</param>
        /// <param name="h">高</param>
        /// <param name="interpolationMode"></param>
        /// <param name="smoothingMode"></param>
        /// <param name="compositingQuality"></param>
        /// <returns></returns>
        public static Bitmap SetSize(this Bitmap bm, int w, int h,
            InterpolationMode interpolationMode = InterpolationMode.HighQualityBicubic,
            SmoothingMode smoothingMode = SmoothingMode.HighQuality,
            CompositingQuality compositingQuality = CompositingQuality.HighSpeed
            )
        {
            int nowWidth = w;
            int nowHeight = h;
            Bitmap newbm = new Bitmap(nowWidth, nowHeight);//新建一个放大后大小的图片

            Graphics g = Graphics.FromImage(newbm);
            g.InterpolationMode = interpolationMode;
            g.SmoothingMode = smoothingMode;
            g.CompositingQuality = compositingQuality;
            g.DrawImage(bm, new Rectangle(0, 0, nowWidth, nowHeight), new Rectangle(0, 0, bm.Width, bm.Height), GraphicsUnit.Pixel);
            g.Dispose();
            return newbm;
        }
        /// <summary>
        /// 裁剪图像
        /// </summary>
        /// <param name="b"></param>
        /// <param name="StartX"></param>
        /// <param name="StartY"></param>
        /// <param name="iWidth"></param>
        /// <param name="iHeight"></param>
        /// <returns></returns>
        public static Bitmap Cut(this Bitmap b, int StartX, int StartY, int iWidth, int iHeight)
        {
            if (b == null)
            {
                return null;
            }
            int w = b.Width;
            int h = b.Height;
            if (StartX >= w || StartY >= h)
            {
                return null;
            }
            if (StartX + iWidth > w)
            {
                iWidth = w - StartX;
            }
            if (StartY + iHeight > h)
            {
                iHeight = h - StartY;
            }
            try
            {
                Bitmap bmpOut = new Bitmap(iWidth, iHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(bmpOut);
                g.DrawImage(b, new Rectangle(0, 0, iWidth, iHeight), new Rectangle(StartX, StartY, iWidth, iHeight), GraphicsUnit.Pixel);
                g.Dispose();
                return bmpOut;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 获取图片主色调
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static System.Drawing.Color GetMajorColor(this Bitmap bitmap)
        {
            //色调的总和
            var sum_hue = 0d;
            //色差的阈值
            var threshold = 10;
            Bitmap smaller = bitmap.SetSize((int)(bitmap.Size.Width * 0.4), (int)(bitmap.Size.Height * 0.4));
            //计算色调总和
            for (int h = 0; h < smaller.Height; h++)
            {
                for (int w = 0; w < smaller.Width; w++)
                {
                    var hue = smaller.GetPixel(w, h).GetHue();
                    sum_hue += hue;
                }
            }
            var avg_hue = sum_hue / (smaller.Width * smaller.Height);

            //色差大于阈值的颜色值
            var rgbs = new List<System.Drawing.Color>();
            for (int h = 0; h < smaller.Height; h++)
            {
                for (int w = 0; w < smaller.Width; w++)
                {
                    var color = smaller.GetPixel(w, h);
                    var hue = color.GetHue();
                    //如果色差大于阈值，则加入列表
                    if (Math.Abs(hue - avg_hue) > threshold)
                    {
                        rgbs.Add(color);
                    }
                }
            }
            if (rgbs.Count == 0)
                return System.Drawing.Color.Black;
            //计算列表中的颜色均值，结果即为该图片的主色调
            int sum_r = 0, sum_g = 0, sum_b = 0;
            foreach (var rgb in rgbs)
            {
                sum_r += rgb.R;
                sum_g += rgb.G;
                sum_b += rgb.B;
            }
            return System.Drawing.Color.FromArgb(sum_r / rgbs.Count,
                (sum_g / rgbs.Count),
                (sum_b / rgbs.Count));
        }

        public static System.Drawing.Color GetMostUsedColor(this Bitmap theBitMap)
        {
            //List<System.Drawing.Color> TenMostUsedColors;
            //List<int> TenMostUsedColorIncidences;

            System.Drawing.Color MostUsedColor;
            //int MostUsedColorIncidence;

            int pixelColor;

            Dictionary<int, int> dctColorIncidence;

            //TenMostUsedColors = new List<System.Drawing.Color>();
            //TenMostUsedColorIncidences = new List<int>();

            MostUsedColor = System.Drawing.Color.Empty;
            //MostUsedColorIncidence = 0;

            // does using Dictionary<int,int> here
            // really pay-off compared to using
            // Dictionary<Color, int> ?

            // would using a SortedDictionary be much slower, or ?

            dctColorIncidence = new Dictionary<int, int>();
            Bitmap smaller = theBitMap.SetSize((int)(theBitMap.Size.Width * 0.4), (int)(theBitMap.Size.Height * 0.4));
            // this is what you want to speed up with unmanaged code
            for (int row = 0; row < smaller.Size.Width; row++)
            {
                for (int col = 0; col < smaller.Size.Height; col++)
                {
                    pixelColor = smaller.GetPixel(row, col).ToArgb();

                    if (dctColorIncidence.Keys.Contains(pixelColor))
                    {
                        dctColorIncidence[pixelColor]++;
                    }
                    else
                    {
                        dctColorIncidence.Add(pixelColor, 1);
                    }
                }
            }

            // note that there are those who argue that a
            // .NET Generic Dictionary is never guaranteed
            // to be sorted by methods like this
            var dctSortedByValueHighToLow = dctColorIncidence.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            // this should be replaced with some elegant Linq ?
            //foreach (KeyValuePair<int, int> kvp in dctSortedByValueHighToLow.Take(10))
            //{
            //TenMostUsedColors.Add(System.Drawing.Color.FromArgb(kvp.Key));
            //TenMostUsedColorIncidences.Add(kvp.Value);
            //}

            MostUsedColor = System.Drawing.Color.FromArgb(dctSortedByValueHighToLow.First().Key);
            //MostUsedColorIncidence = dctSortedByValueHighToLow.First().Value;
            return MostUsedColor;
        }
    }
    public class HttpHelper {
        public static async Task<string> GetWebAsync(string url,Encoding e=null)
        {
            try
            {
                if (e == null)
                    e = Encoding.UTF8;
                HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
                hwr.Timeout = 20000;
                var o = await hwr.GetResponseAsync();
                StreamReader sr = new StreamReader(o.GetResponseStream(),e);
                var st = await sr.ReadToEndAsync();
                sr.Dispose();
                return st;
            }
            catch { return ""; }
        }
        public static async Task HttpDownloadFileAsync(string url, string path)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
            Stream responseStream = response.GetResponseStream();
            Stream stream = new FileStream(path, FileMode.Create);
            byte[] bArr = new byte[1024];
            int size = await responseStream.ReadAsync(bArr, 0, bArr.Length);
            while (size > 0)
            {
                await stream.WriteAsync(bArr, 0, size);
                size = await responseStream.ReadAsync(bArr, 0, bArr.Length);
            }
            stream.Close();
            responseStream.Close();
        }
        public static void SetHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (property != null)
            {
                var collection = property.GetValue(header, null) as NameValueCollection;
                collection[name] = value;
            }
        }
        public static async System.Threading.Tasks.Task<string> GetWebDatacAsync(string url, Encoding c)
        {
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
            hwr.Timeout = 20000;
            SetHeaderValue(hwr.Headers, "Connection", "keep-alive");
            hwr.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
            hwr.Headers.Add(HttpRequestHeader.Upgrade, "1");
            hwr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.110 Safari/537.36";
            SetHeaderValue(hwr.Headers, "Accept", "*/*");
            hwr.Referer = "https://y.qq.com/portal/player.html";
            hwr.Host = "c.y.qq.com";
            hwr.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.8");
            hwr.Headers.Add(HttpRequestHeader.Cookie, "pgv_pvi=1693112320; RK=DKOGai2+wu; pgv_pvid=1804673584; ptcz=3a23e0a915ddf05c5addbede97812033b60be2a192f7c3ecb41aa0d60912ff26; pgv_si=s4366031872; _qpsvr_localtk=0.3782697029073365; ptisp=ctc; luin=o2728578956; lskey=00010000863c7a430b79e2cf0263ff24a1e97b0694ad14fcee720a1dc16ccba0717d728d32fcadda6c1109ff; pt2gguin=o2728578956; uin=o2728578956; skey=@PjlklcXgw; p_uin=o2728578956; p_skey=ROnI4JEkWgKYtgppi3CnVTETY3aHAIes-2eDPfGQcVg_; pt4_token=wC-2b7WFwI*8aKZBjbBb7f4Am4rskj11MmN7bvuacJQ_; p_luin=o2728578956; p_lskey=00040000e56d131f47948fb5a2bec49de6174d7938c2eb45cb224af316b053543412fd9393f83ee26a451e15; ts_refer=ui.ptlogin2.qq.com/cgi-bin/login; ts_last=y.qq.com/n/yqq/playlist/2591355982.html; ts_uid=1420532256; yqq_stat=0");
            var o = await hwr.GetResponseAsync();
            StreamReader sr = new StreamReader(o.GetResponseStream(), c);
            var st = await sr.ReadToEndAsync();
            sr.Dispose();
            return st;
        }
        public static async System.Threading.Tasks.Task<string> GetWebDatadAsync(string url, Encoding c)
        {
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
            hwr.Timeout = 20000;
            SetHeaderValue(hwr.Headers, "Connection", "keep-alive");
            hwr.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
            hwr.Headers.Add(HttpRequestHeader.Upgrade, "1");
            hwr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.110 Safari/537.36";
            SetHeaderValue(hwr.Headers, "Accept", "*/*");
            hwr.Referer = "https://y.qq.com/portal/player.html";
            hwr.Host = "c.y.qq.com";
            hwr.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.8");
            hwr.Headers.Add(HttpRequestHeader.Cookie, "pgv_pvi=9798155264; pt2gguin=o2728578956; RK=JKKMei2V0M; ptcz=f60f58ab93a9b59848deb2d67b6a7a4302dd1208664e448f939ed122c015d8d1; pgv_pvid=4173718307; ts_uid=5327745136; ts_refer=xui.ptlogin2.qq.com/cgi-bin/xlogin; pgv_info=ssid=s3825018265; pgv_si=s8085315584; _qpsvr_localtk=0.08173445548395986; ptisp=ctc; pt4_token=ZIeo22vj6enh5MYFVRG8dcF1N9S8Y*ccVSStyU4Jquw_; yq_playdata=r_99; yq_playschange=0; player_exist=1; qqmusic_fromtag=66; yplayer_open=0; yqq_stat=0; ts_last=y.qq.com/portal/playlist.html");
            var o = await hwr.GetResponseAsync();
            StreamReader sr = new StreamReader(o.GetResponseStream(), c);
            var st = await sr.ReadToEndAsync();
            sr.Dispose();
            return st;
        }
        public static Boolean IsNetworkTrue() {
            try
            {
                Ping ping = new Ping();
                PingReply La = ping.Send("www.mi.com");
                if (La.Status == IPStatus.Success)
                    return true;
                else
                    return false;
            }
            catch { return false; }
        }
    }
    public class TextHelper
    {
        public static string XtoYGetTo(string all, string r, string l, int t)
        {

            int A = all.IndexOf(r, t);
            int B = all.IndexOf(l, A + 1);
            if (A < 0 || B < 0)
            {
                return null;
            }
            else
            {
                A = A + r.Length;
                B = B - A;
                if (A < 0 || B < 0)
                {
                    return null;
                }
                return all.Substring(A, B);
            }
        }
        public static string TextEncrypt(string encryptStr, string key)
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(encryptStr);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        public static string TextDecrypt(string decryptStr, string key)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Convert.FromBase64String(decryptStr);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return UTF8Encoding.UTF8.GetString(resultArray);
        }
        public class JSON
        {
            public static object JsonToObject(string jsonString, object obj)
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
                return serializer.ReadObject(mStream);
            }
            public static string ToJSON(object obj)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer()
                { MaxJsonLength = Int32.MaxValue };
                return serializer.Serialize(obj);
            }
        }
        public class MD5
        {
            public static byte[] EncryptToMD5(string str)
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] str1 = System.Text.Encoding.UTF8.GetBytes(str);
                byte[] str2 = md5.ComputeHash(str1, 0, str1.Length);
                md5.Clear();
                (md5 as IDisposable).Dispose();
                return str2;
            }
            public static string EncryptToMD5string(string str)
            {
                byte[] bytHash = EncryptToMD5(str);
                string sTemp = "";
                for (int i = 0; i < bytHash.Length; i++)
                {
                    sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
                }
                return sTemp.ToLower();
            }
        }
    }
    public class InfoHelper {
        ////////Music Helper////////
        public class Music
        {
            public Music() { }
            public string MusicName { set; get; } = "";
            public string Singer { set; get; } = "";
            public string MusicID { set; get; } = "";
            public string ImageUrl { set; get; } = "";
            public string GC { set; get; } = "";
        }
        public class MusicSinger {
            public string Name { set; get; }
            public string Photo { set; get; }
        }

        public class MusicGD
        {
            public string Name { set; get; }
            public string Photo { set; get; }
            public string ID { set; get; }
        }

        public class MusicTop
        {
            public string Name { set; get; }
            public string Photo { set; get; }
            public string ID { set; get; }
        }
        public class MusicRadioList {
            public List<MusicRadioListItem> Hot { set; get; } = new List<MusicRadioListItem>();
            public List<MusicRadioListItem> Evening{ set; get; } = new List<MusicRadioListItem>();
            public List<MusicRadioListItem> Love { set; get; } = new List<MusicRadioListItem>();
            public List<MusicRadioListItem> Theme { set; get; } = new List<MusicRadioListItem>();
            public List<MusicRadioListItem> Changjing { set; get; } = new List<MusicRadioListItem>();
            public List<MusicRadioListItem> Style { set; get; } = new List<MusicRadioListItem>();
            public List<MusicRadioListItem> Lauch { set; get; } = new List<MusicRadioListItem>();
            public List<MusicRadioListItem> People { set; get; } = new List<MusicRadioListItem>();
            public List<MusicRadioListItem> MusicTools { set; get; } = new List<MusicRadioListItem>();
            public List<MusicRadioListItem> Diqu { set; get; } = new List<MusicRadioListItem>();
        }
        public class MusicRadioListItem {
            public string Name { set; get; }
            public string Photo { set; get; }
            public string ID { set; get; }
        }
        public class MusicFLGDIndexItemsList
        {
            public List<MusicFLGDIndexItems> Hot { get; set; } = new List<MusicFLGDIndexItems>();
            public List<MusicFLGDIndexItems> Lauch { get; set; } = new List<MusicFLGDIndexItems>();
            public List<MusicFLGDIndexItems> LiuPai { get; set; } = new List<MusicFLGDIndexItems>();
            public List<MusicFLGDIndexItems> Theme { get; set; } = new List<MusicFLGDIndexItems>();
            public List<MusicFLGDIndexItems> Heart { get; set; } = new List<MusicFLGDIndexItems>();
            public List<MusicFLGDIndexItems> Changjing { get; set; } = new List<MusicFLGDIndexItems>();
        }
        public class MusicFLGDIndexItems {
            public string name { get; set; }
            public string id { get; set; }
        }

        public class MusicGData {
            public List<Music> Data { get; set; } = new List<Music>();
            public string name { get; set; }
            public string pic { get; set; }
        }
        ////////Weather Helper/////
        public class Weather {
            public string Qiwen { get; set; }
            public string KongQiZhiLiang { get; set; }
            public string MiaoShu { get; set; }
            public string TiGan { get; set; }
            public string FenSu { get; set; }
            public string NenJianDu { get; set; }
            public List<WeatherByDay> Data { get; set; }

        }
        public class WeatherByDay {
            public string Date { set; get; }
            public string Icon { set; get; }
            public string QiWen { set; get; }
        }
    }
    public static class FullScreenManager
    {
        public static void RepairWpfWindowFullScreenBehavior(Window wpfWindow)
        {
            if (wpfWindow == null)
            {
                return;
            }

            if (wpfWindow.WindowState == WindowState.Maximized)
            {
                wpfWindow.WindowState = WindowState.Normal;
                wpfWindow.Loaded += delegate { wpfWindow.WindowState = WindowState.Maximized; };
            }

            wpfWindow.SourceInitialized += delegate
            {
                IntPtr handle = (new WindowInteropHelper(wpfWindow)).Handle;
                HwndSource source = HwndSource.FromHwnd(handle);
                if (source != null)
                {
                    source.AddHook(WindowProc);
                }
            };
        }

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }

            return (IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
                mmi.ptMinTrackSize.x = 835;
                mmi.ptMinTrackSize.y = 615;
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }


        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        /// <summary>
        /// 
        /// </summary>
        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        #region Nested type: MINMAXINFO

        [StructLayout(LayoutKind.Sequential)]
        internal struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        #endregion

        #region Nested type: MONITORINFO

        /// <summary>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal class MONITORINFO
        {
            /// <summary>
            /// </summary>            
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            /// <summary>
            /// </summary>            
            public RECT rcMonitor;

            /// <summary>
            /// </summary>            
            public RECT rcWork;

            /// <summary>
            /// </summary>            
            public int dwFlags;
        }

        #endregion

        #region Nested type: POINT

        /// <summary>
        /// POINT aka POINTAPI
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINT
        {
            /// <summary>
            /// x coordinate of point.
            /// </summary>
            public int x;

            /// <summary>
            /// y coordinate of point.
            /// </summary>
            public int y;

            /// <summary>
            /// Construct a point of coordinates (x,y).
            /// </summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        #endregion

        #region Nested type: RECT

        /// <summary> Win32 </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal struct RECT
        {
            /// <summary> Win32 </summary>
            public int left;

            /// <summary> Win32 </summary>
            public int top;

            /// <summary> Win32 </summary>
            public int right;

            /// <summary> Win32 </summary>
            public int bottom;

            /// <summary> Win32 </summary>
            public static readonly RECT Empty;

            /// <summary> Win32 </summary>
            public int Width
            {
                get { return Math.Abs(right - left); } // Abs needed for BIDI OS
            }

            /// <summary> Win32 </summary>
            public int Height
            {
                get { return bottom - top; }
            }

            /// <summary> Win32 </summary>
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }


            /// <summary> Win32 </summary>
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }

            /// <summary> Win32 </summary>
            public bool IsEmpty
            {
                get
                {
                    // BUGBUG : On Bidi OS (hebrew arabic) left > right
                    return left >= right || top >= bottom;
                }
            }

            /// <summary> Return a user friendly representation of this struct </summary>
            public override string ToString()
            {
                if (this == Empty)
                {
                    return "RECT {Empty}";
                }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom +
                       " }";
            }

            /// <summary> Determine if 2 RECT are equal (deep compare) </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is Rect))
                {
                    return false;
                }
                return (this == (RECT)obj);
            }

            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode()
            {
                return left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            }


            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2)
            {
                return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right &&
                        rect1.bottom == rect2.bottom);
            }

            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2)
            {
                return !(rect1 == rect2);
            }
        }

        #endregion
    }

    public class PopopHelper
    {
        public static DependencyObject GetPopupPlacementTarget(DependencyObject obj)
        {
            return (DependencyObject)obj.GetValue(PopupPlacementTargetProperty);
        }

        public static void SetPopupPlacementTarget(DependencyObject obj, DependencyObject value)
        {
            obj.SetValue(PopupPlacementTargetProperty, value);
        }

        // Using a DependencyProperty as the backing store for PopupPlacementTarget.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupPlacementTargetProperty =
            DependencyProperty.RegisterAttached("PopupPlacementTarget", typeof(DependencyObject), typeof(PopopHelper), new PropertyMetadata(null, OnPopupPlacementTargetChanged));

        private static void OnPopupPlacementTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                DependencyObject popupPopupPlacementTarget = e.NewValue as DependencyObject;
                Popup pop = d as Popup;

                var w = Window.GetWindow(popupPopupPlacementTarget);
                if (null != popupPopupPlacementTarget)
                {
                    w.LocationChanged += delegate
                    {
                        var offset = pop.HorizontalOffset;
                        pop.HorizontalOffset = offset + 1;
                        pop.HorizontalOffset = offset;
                    };
                }
            }
        }

    }
}
