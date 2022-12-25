using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LemonLib
{
    public class TranslateAirHelper
    {
        public static async Task<string> GetLang(string text) {
            string json = await HttpHelper.PostWeb("https://fanyi.baidu.com/langdetect", "query=" + HttpUtility.UrlEncode(text), HttpHelper.GetWebHeader_BaiduFY());
            JObject obj = JObject.Parse(json);
            return obj["lan"].ToString();
        }

        public static async Task<string> GetSug(string text)
        {
            string json = await HttpHelper.PostWeb("https://fanyi.baidu.com/sug", "kw=" + HttpUtility.UrlEncode(text), HttpHelper.GetWebHeader_BaiduFY());
            JObject obj = JObject.Parse(json);
            StringBuilder sb = new StringBuilder();
            foreach (var j in obj["data"]) {
                sb.Append(j["k"] + "\r\n");
                sb.Append(j["v"] + "\r\n");
            }
            return sb.ToString();
        }
        public static async Task<string> Speech(string text)
        {
            var url = $"https://dict.youdao.com/dictvoice?audio={HttpUtility.UrlEncode(text)}&le={(await GetLang(text)).Replace("jp","ja")}";
            MainClass.DebugCallBack(url,"blue");
            string path = Settings.USettings.MusicCachePath + "\\ta.mp3";
            await HttpDownloadFileAsync(url, path);
            return path;
        }
        public static async Task HttpDownloadFileAsync(string url, string path)
        {
            HttpWebRequest hwr = WebRequest.Create(url) as HttpWebRequest;
            hwr.Accept = "*/*";
            hwr.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
            hwr.Headers.Add("Accept-Encoding", "identity;q=1, *;q=0");
            hwr.Headers.Add("Cache-Control", "no-cache");
            hwr.KeepAlive = true;
            hwr.Referer = "https://fanyi.youdao.com/";
            hwr.Host = "dict.youdao.com";
            hwr.Headers.Add("Pragma", "no-cache");
            hwr.Headers.Add("Range", "bytes=0-");
            hwr.Headers.Add("Sec-Fetch-Dest", "audio");
            hwr.Headers.Add("Upgrade-Insecure-Requests", "1");
            hwr.Headers.Add("Sec-Fetch-Mode", "no-cors");
            hwr.Headers.Add("Sec-Fetch-Site", "same-origin");
            hwr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";
            using HttpWebResponse response = await hwr.GetResponseAsync() as HttpWebResponse;
            using Stream responseStream = response.GetResponseStream();
            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            {
                byte[] bArr = new byte[1024];
                int size = await responseStream.ReadAsync(bArr, 0, bArr.Length);
                while (size > 0)
                {
                    await stream.WriteAsync(bArr, 0, size);
                    size = await responseStream.ReadAsync(bArr, 0, bArr.Length);
                }
                stream.Close();
            }
            responseStream.Close();
        }
        public static async Task<string> Speech_AITTS(string text) {
            var data =await PostWeb("http://ai.baidu.com/aidemo",
                    $"type=tns&spd=2&pit=5&vol=5&lan=jp&per=1&tex=" + HttpUtility.HtmlEncode(text));
            JObject o = JObject.Parse(data);
            var dt = o["data"].ToString().Replace("data:audio/x-mpeg;base64,", "");
            string path = Settings.USettings.DataCachePath + "\\ta.mp3";
            Base64ToOriFile(dt, path);
            return path;
        }
        public static async Task<string> PostWeb(string url, string data)
        {
            byte[] postData = Encoding.UTF8.GetBytes(data);
            WebClient webClient = new WebClient();
            webClient.Headers.Add("Accept", "*/*");
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            webClient.Headers.Add("Cookie", "BAIDUID=CF1A84B8FDD881260D93DECF5A8A33C3:FG=1; BIDUPSID=CF1A84B8FDD881260D93DECF5A8A33C3; PSTM=1549463409; MCITY=-41%3A; H_PS_PSSID=1459_21119_28414; BDORZ=FFFB88E999055A3F8A630C64834BD6D0; delPer=0; PSINO=7; BDSFRCVID=v-KsJeCCxG3JyDT9jc-uynbPAZc9k64iSmQ73J; H_BDCLCKID_SF=tR3KB6rtKRTffjrnhPF35CuUKP6-3MJO3b7ZXlbmfRcjKt5wh45mLqISef4qtPcgaGCfohFLK-oj-DDwDjK23J; Hm_lvt_8b973192450250dd85b9011320b455ba=1550309932; Hm_lpvt_8b973192450250dd85b9011320b455ba=1550309932; seccode=0d31296fec64f8e311278ce31edf2b25");
            webClient.Headers.Add("Host", "ai.baidu.com");
            webClient.Headers.Add("Origin", "http://ai.baidu.com");
            webClient.Headers.Add("Referer", "http://ai.baidu.com/tech/speech/tts?track=cp:aipinzhuan|pf:pc|pp:AIpingtai|pu:1-3|ci:|kw:10005801");
            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36");
            byte[] responseData =await  webClient.UploadDataTaskAsync(url, "POST", postData);
            webClient.Dispose();
            return Encoding.UTF8.GetString(responseData);
        }

        public static void Base64ToOriFile(string base64Str, string outPath)
        {
            var contents = Convert.FromBase64String(base64Str);
            using (var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(contents, 0, contents.Length);
                fs.Flush();
            }
        }
    }
}
