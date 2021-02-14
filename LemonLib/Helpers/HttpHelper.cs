using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LemonLib
{
    public class HttpHelper
    {
        public static async Task<long> GetHTTPFileSize(string sURL)
        {
            long size = 0L;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(sURL);
                request.Method = "HEAD";
                using var response = (HttpWebResponse)await request.GetResponseAsync();
                size = response.ContentLength;
                response.Close();
            }
            catch
            {
                size = 0L;
            }
            return size;
        }
        /// <summary>
        /// 不带Header直接发送Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static async Task<string> GetWebAsync(string url, Encoding e = null)
        {
            if (e == null)
                e = Encoding.UTF8;
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
            using WebResponse res = await hwr.GetResponseAsync();
            using StreamReader sr = new StreamReader(res.GetResponseStream(), e);
            var st = await sr.ReadToEndAsync();
            return st;
        }
        /// <summary>
        /// 带上简单Header的Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> GetWebWithHeaderAsync(string url)
        {
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
            hwr.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            hwr.Headers.Add("accept-language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            hwr.Headers.Add("cookie","__cfduid=dbb44318d9a3e9b739a80230a206d3d8f1592021136; io=xGIaLcCN2fsSC37pADUn");
            hwr.Headers.Add("sec-ch-ua", "\"\\\\Not\\\"A;Brand\";v=\"99\", \"Chromium\";v=\"84\", \"Microsoft Edge\";v=\"84\"");
            hwr.Headers.Add("sec-ch-ua-mobile", "?0");
            hwr.Headers.Add("sec-fetch-dest", "document");
            hwr.Headers.Add("sec-fetch-mode", "navigate");
            hwr.Headers.Add("sec-fetch-site","none");
            hwr.Headers.Add("sec-fetch-user","?1");
            hwr.Headers.Add("upgrade-insecure-requests", "1");
            hwr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.38 Safari/537.36 Edg/84.0.522.15";
            using WebResponse res = await hwr.GetResponseAsync();
            using StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
            var st = await sr.ReadToEndAsync();
            return st;
        }
        public static WebHeaderCollection GetWebHeader_YQQCOM() => new WebHeaderCollection
            {
                { HttpRequestHeader.Accept, "*/*" },
                { HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.9" },
                { HttpRequestHeader.ContentType, "application/x-www-form-urlencoded; charset=UTF-8" },
                { HttpRequestHeader.Cookie, Settings.USettings.Cookie },
                { HttpRequestHeader.Referer, "https://y.qq.com/n/yqq/singer/0020PeOh4ZaCw1.html" },
                { HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36" },
                { HttpRequestHeader.Host, "c.y.qq.com" }
            };
        /// <summary>
        /// 发送一个简单的POST请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="Header"></param>
        /// <returns></returns>
        public static async Task<string> PostWeb(string url, string data, WebHeaderCollection Header = null)
        {
            byte[] postData = Encoding.UTF8.GetBytes(data);
            using WebClient webClient = new WebClient();
            if (Header != null)
                webClient.Headers = Header;
            byte[] responseData = await webClient.UploadDataTaskAsync(new Uri(url), "POST", postData);

            return Encoding.UTF8.GetString(responseData);
        }
        /// <summary>
        /// 针对wk_v17 y.qq.com的反防盗链 发送POST请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<string> PostInycAsync(string url, string data)
        {
            var r = (HttpWebRequest)WebRequest.Create(url);
            r.Method = "POST";
            r.ContentType = " application/x-www-form-urlencoded";
            r.Host = "u.y.qq.com";
            r.KeepAlive = true;
            r.Accept = "application/json";
            r.Headers.Add("Origin", "http://y.qq.com");
            r.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.47.134 Safari/537.36 QBCore/3.53.47.400 QQBrowser/9.0.2524.400 pcqqmusic/17.10.5105.0801 SkinId/10001|1ecc94|145|1|||1fd4af";
            r.Referer = "http://y.qq.com/wk_v17/";
            r.Headers.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.5;q=0.4");
            r.Headers.Add("Cookie", Settings.USettings.Cookie);
            var byteData = Encoding.UTF8.GetBytes(data);
            var length = byteData.Length;
            r.ContentLength = length;
            var writer = await r.GetRequestStreamAsync();
            await writer.WriteAsync(byteData, 0, length);
            writer.Close();
            using HttpWebResponse response = (HttpWebResponse)await r.GetResponseAsync();
            using var stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string dt = await stream.ReadToEndAsync();
            return dt;
        }
        /// <summary>
        /// 模拟Chrome请求 下载文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task HttpDownloadFileAsync(string url, string path)
        {
            HttpWebRequest hwr = WebRequest.Create(url) as HttpWebRequest;
            hwr.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            hwr.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
            hwr.Headers.Add("Cache-Control", "max-age=0");
            hwr.KeepAlive = true;
            hwr.Referer = url;
            hwr.Headers.Add("Upgrade-Insecure-Requests", "1");
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
        /// <summary>
        /// y.qq.com fcg客户端 Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static async Task<string> GetWebDatacAsync(string url, Encoding c = null)
        {
            Console.WriteLine(Settings.USettings.Cookie + "\r\n" + Settings.USettings.g_tk);
            if (c == null) c = Encoding.UTF8;
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
            hwr.KeepAlive = true;
            hwr.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
            hwr.Headers.Add(HttpRequestHeader.Upgrade, "1");
            hwr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.110 Safari/537.36";
            hwr.Accept = "*/*";
            hwr.Referer = "https://y.qq.com/portal/player.html";
            hwr.Host = "c.y.qq.com";
            hwr.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.8");
            hwr.Headers.Add(HttpRequestHeader.Cookie, Settings.USettings.Cookie);
            hwr.Headers.Add("sec-fetch-dest", "empty");
            hwr.Headers.Add("sec-fetch-mode", "cors");
            hwr.Headers.Add("sec-fetch-site", "same-site");
            using WebResponse o = await hwr.GetResponseAsync();
            using StreamReader sr = new StreamReader(o.GetResponseStream(), c);
            var st = await sr.ReadToEndAsync();
            sr.Dispose();
            return st;
        }
        /// <summary>
        /// y.qq.com musicu.fcg客户端 Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static async Task<string> GetWebDataqAsync(string url, Encoding c = null)
        {
            Console.WriteLine(Settings.USettings.Cookie + "\r\n" + Settings.USettings.g_tk);
            if (c == null) c = Encoding.UTF8;
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
            hwr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.110 Safari/537.36";
            hwr.Headers.Add(HttpRequestHeader.Cookie, Settings.USettings.Cookie);
            using WebResponse o = await hwr.GetResponseAsync();
            using StreamReader sr = new StreamReader(o.GetResponseStream(), c);
            var st = await sr.ReadToEndAsync();
            sr.Dispose();
            return st;
        }
    }
}
