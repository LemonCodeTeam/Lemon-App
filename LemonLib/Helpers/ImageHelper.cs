using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LemonLib
{
    public static class ImageHelper
    {
        #region Image类互转的拓展方法
        public static Bitmap Snapshot(int x, int y, int width, int height)
        {
            Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);
            }
            return bitmap;
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);
        public static ImageSource ToImageSource(this Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            if (!DeleteObject(hBitmap))
                throw new System.ComponentModel.Win32Exception();

            return wpfBitmap;

        }
        public static Bitmap ToBitmap(this ImageSource imageSource)
        {
            BitmapSource m = (BitmapSource)imageSource;

            Bitmap bmp = new Bitmap(m.PixelWidth, m.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb); // 坑点：选Format32bppRgb将不带透明度

            BitmapData data = bmp.LockBits(
            new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            m.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);

            return bmp;
        }
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
        public static BitmapImage ToBitmapImage(this byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }
        #endregion
        #region 处理模糊图像
        [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdipBitmapApplyEffect(IntPtr bitmap, IntPtr effect, ref Rectangle rectOfInterest, bool useAuxData, IntPtr auxData, int auxDataSize);
        /// <summary>
        /// 获取对象的私有字段的值，感谢Aaron Lee Murgatroyd
        /// </summary>
        /// <typeparam name="TResult">字段的类型</typeparam>
        /// <param name="obj">要从其中获取字段值的对象</param>
        /// <param name="fieldName">字段的名称.</param>
        /// <returns>字段的值</returns>
        /// <exception cref="System.InvalidOperationException">无法找到该字段.</exception>
        /// 
        internal static TResult GetPrivateField<TResult>(this object obj, string fieldName)
        {
            if (obj == null) return default(TResult);
            Type ltType = obj.GetType();
            FieldInfo lfiFieldInfo = ltType.GetField(fieldName, System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (lfiFieldInfo != null)
                return (TResult)lfiFieldInfo.GetValue(obj);
            else
                throw new InvalidOperationException(string.Format("Instance field '{0}' could not be located in object of type '{1}'.", fieldName, obj.GetType().FullName));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BlurParameters
        {
            internal float Radius;
            internal bool ExpandEdges;
        }
        [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdipCreateEffect(Guid guid, out IntPtr effect);
        private static Guid BlurEffectGuid = new Guid("{633C80A4-1843-482B-9EF2-BE2834C5FDD4}");
        [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdipSetEffectParameters(IntPtr effect, IntPtr parameters, uint size);
        public static IntPtr NativeHandle(this Bitmap Bmp)
        {
            // 通过反射获取Bitmap的私有字段nativeImage的值，该值为GDI+的内部图像句柄
            //在.NET 8中字段由 nativeImage变更为_nativeImage
            return Bmp.GetPrivateField<IntPtr>("_nativeImage");
        }
        [DllImport("gdiplus.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GdipDeleteEffect(IntPtr effect);
        public static void GaussianBlur(this Bitmap Bmp, ref Rectangle Rect, float Radius = 10, bool ExpandEdge = false)
        {
            int Result;
            IntPtr BlurEffect;
            BlurParameters BlurPara;
            if ((Radius < 0) || (Radius > 255))
            {
                throw new ArgumentOutOfRangeException("半径必须在[0,255]范围内");
            }
            BlurPara.Radius = Radius;
            BlurPara.ExpandEdges = ExpandEdge;
            Result = GdipCreateEffect(BlurEffectGuid, out BlurEffect);
            if (Result == 0)
            {
                IntPtr Handle = Marshal.AllocHGlobal(Marshal.SizeOf(BlurPara));
                Marshal.StructureToPtr(BlurPara, Handle, true);
                GdipSetEffectParameters(BlurEffect, Handle, (uint)Marshal.SizeOf(BlurPara));
                GdipBitmapApplyEffect(Bmp.NativeHandle(), BlurEffect, ref Rect, false, IntPtr.Zero, 0);
                // 使用GdipBitmapCreateApplyEffect函数可以不改变原始的图像，而把模糊的结果写入到一个新的图像中
                GdipDeleteEffect(BlurEffect);
                Marshal.FreeHGlobal(Handle);
            }
            else
            {
                throw new ExternalException("不支持的GDI+版本，必须为GDI+1.1及以上版本，且操作系统要求为Win Vista及之后版本.");
            }
        }
        #endregion
        #region Main Color of Image
        public static System.Windows.Media.Color get_major_color(this Bitmap bitmap,int threshold = 30)
        {
            //色调的总和
            var sum_hue = 0d;
            //色差的阈值
            //计算色调总和
            for (int h = 0; h < bitmap.Height; h++)
            {
                for (int w = 0; w < bitmap.Width; w++)
                {
                    var hue = bitmap.GetPixel(w, h).GetHue();
                    sum_hue += hue;
                }
            }
            var avg_hue = sum_hue / (bitmap.Width * bitmap.Height);

            //色差大于阈值的颜色值
            var rgbs = new List<System.Drawing.Color>();
            for (int h = 0; h < bitmap.Height; h++)
            {
                for (int w = 0; w < bitmap.Width; w++)
                {
                    var color = bitmap.GetPixel(w, h);
                    var hue = color.GetHue();
                    //如果色差大于阈值，则加入列表
                    if (Math.Abs(hue - avg_hue) > threshold)
                    {
                        rgbs.Add(color);
                    }
                }
            }
            if (rgbs.Count == 0)
                return System.Windows.Media.Colors.Black;
            //计算列表中的颜色均值，结果即为该图片的主色调
            int sum_r = 0, sum_g = 0, sum_b = 0;
            foreach (var rgb in rgbs)
            {
                sum_r += rgb.R;
                sum_g += rgb.G;
                sum_b += rgb.B;
            }
            return System.Windows.Media.Color.FromRgb((byte)(sum_r / rgbs.Count),
                (byte)(sum_g / rgbs.Count),
                (byte)(sum_b / rgbs.Count));
        }
        #endregion 
    }
}
