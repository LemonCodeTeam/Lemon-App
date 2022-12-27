using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LemonLib
{
    public class TranslateAirHelper
    {
        public static async Task<string> GetLang(string text) {
            string json = await HttpHelper.PostWeb("https://fanyi.baidu.com/langdetect", "query=" + HttpUtility.UrlEncode(text), HttpHelper.GetWebHeader_BaiduFY);
            JObject obj = JObject.Parse(json);
            return obj["lan"].ToString();
        }

        public static async Task<string> GetSug(string text)
        {
            string json = await HttpHelper.PostWeb("https://fanyi.baidu.com/sug", "kw=" + HttpUtility.UrlEncode(text), HttpHelper.GetWebHeader_BaiduFY);
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
            using var hc = new HttpClient(new SocketsHttpHandler() { KeepAlivePingPolicy = HttpKeepAlivePingPolicy.WithActiveRequests });
            hc.DefaultRequestHeaders.Accept.TryParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            hc.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            hc.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            hc.DefaultRequestHeaders.Add("Referer", "https://fanyi.youdao.com/");
            hc.DefaultRequestHeaders.Host = "dict.youdao.com";
            hc.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.66 Safari/537.36 Edg/80.0.361.40");
            var myrp = await hc.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            using Stream st = await myrp.Content.ReadAsStreamAsync();
            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            {
                byte[] bArr = new byte[1024];
                int size = await st.ReadAsync(bArr, 0, bArr.Length);
                while (size > 0)
                {
                    await stream.WriteAsync(bArr, 0, size);
                    size = await st.ReadAsync(bArr, 0, bArr.Length);
                }
                stream.Close();
            }
            st.Close();
        }
    }
}
