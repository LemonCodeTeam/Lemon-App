using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace LemonLibrary
{
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
}
