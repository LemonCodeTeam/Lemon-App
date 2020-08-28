using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;

namespace LemonLib
{
    public static class TextHelper
    {
        /// <summary>
        /// 将数字转换为 "xx.x万"
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string IntToWn(this int num)
        {
            if (num < 10000)
                return num.ToString();
            else
            {
                double d = (double)num / (double)10000;
                return Math.Round(d, 2, MidpointRounding.AwayFromZero) + "万";
            }
        }
        /// <summary>
        /// 过滤掉路径文件名中的非法字符
        /// </summary>
        /// <param name="text"></param>
        /// <param name="replacement">替换的字符</param>
        /// <returns></returns>
        public static string MakeValidFileName(string text, string replacement = "_")
        {
            StringBuilder str = new StringBuilder();
            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            foreach (var c in text)
            {
                if (invalidFileNameChars.Contains(c))
                    str.Append(replacement ?? "");
                else
                    str.Append(c);
            }

            return str.ToString();
        }
        /// <summary>
        /// 去除emoji表情信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Exem(string str)
        {
            string s = str;
            while (s.Contains("[em]"))
            {
                string em = "[em]" + FindTextByAB(s, "[em]", "[/em]", 0) + "[/em]";
                s = s.Replace(em, "");
            }
            return s;
        }

        /// <summary>
        /// 查找中间文本
        /// </summary>
        /// <param name="all"></param>
        /// <param name="r">前面的文本</param>
        /// <param name="l">后面的文本</param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string FindTextByAB(string all, string r, string l, int t)
        {

            int A = all.IndexOf(r, t);
            int B = all.IndexOf(l, A + 1);
            if (A < 0 || B < 0)
            {
                return null;
            }
            else
            {
                A += r.Length;
                B -= A;
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
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Convert.FromBase64String(decryptStr);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Encoding.UTF8.GetString(resultArray);
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
                return JsonConvert.SerializeObject(obj);
            }
        }
        public class MD5
        {
            public static byte[] EncryptToMD5(string str)
            {
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] str1 = Encoding.UTF8.GetBytes(str);
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
}
