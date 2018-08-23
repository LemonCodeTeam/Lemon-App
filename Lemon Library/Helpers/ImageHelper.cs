using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
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
}
