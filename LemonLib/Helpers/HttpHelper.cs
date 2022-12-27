using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LemonLib
{
    public class HttpHelper
    {
        public static SocketsHttpHandler GetSta()=> new SocketsHttpHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip
        };
        public static async Task<string> GetRedirectUrl(string url)
        {
            try
            {
                using var hc = new HttpClient(new SocketsHttpHandler() { AllowAutoRedirect = false });
                var headers = await hc.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                return headers.Headers.Location.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static async Task<long> GetHTTPFileSize(string url)
        {
            long size = 0L;
            try
            {
                using var hc = new HttpClient();
                var headers = await hc.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                return (long)headers.Content.Headers.ContentLength;
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
        public static async Task<string> GetWebAsync(string url)
        {
            using var hc = new HttpClient(GetSta());
            var a = await hc.GetAsync(url);
            return await a.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// 带上简单Header的Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> GetWebWithHeaderAsync(string url)
        {
            using var hc = new HttpClient(GetSta());
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Accept","text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("accept-language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("cookie", "__cfduid=dbb44318d9a3e9b739a80230a206d3d8f1592021136; io=xGIaLcCN2fsSC37pADUn");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua", "\"\\\\Not\\\"A;Brand\";v=\"99\", \"Chromium\";v=\"84\", \"Microsoft Edge\";v=\"84\"");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-dest", "document");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-mode", "navigate");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-site", "none");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("sec-fetch-user", "?1");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("upgrade-insecure-requests", "1");
            hc.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.38 Safari/537.36 Edg/84.0.522.15");
            return await (await hc.GetAsync(url)).Content.ReadAsStringAsync();
        }
        public static void GetWebHeader_YQQCOM(HttpClient hc)
        {
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("AcceptLanguage", "zh-CN,zh;q=0.9");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("ContentType", "application/x-www-form-urlencoded; charset=UTF-8");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", Settings.USettings.Cookie);
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://y.qq.com/n/yqq/singer/0020PeOh4ZaCw1.html");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("UserAgent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Host", "c.y.qq.com");
        }
        public static void GetWebHeader_BaiduFY(HttpClient hc)
        {
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "zh-CN,zh;q=0.9");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Host", "fanyi.baidu.com");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://fanyi.baidu.com");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://fanyi.baidu.com/");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.75 Safari/537.36");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
        }
        /// <summary>
        /// 发送一个简单的POST请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="Header"></param>
        /// <returns></returns>
        public static async Task<string> PostWeb(string url, string data, Action<HttpClient> headers = null)
        {
            byte[] postData = Encoding.UTF8.GetBytes(data);
            using var hc = new HttpClient();
            if (headers != null)
                headers(hc);
            var result = await hc.PostAsync(url, new ByteArrayContent(postData));
            return await result.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// 针对wk_v17 y.qq.com的反防盗链 发送POST请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<string> PostInycAsync(string url, string data)
        {
            using var hc = new HttpClient(GetSta());
            hc.DefaultRequestHeaders.TryAddWithoutValidation("ContentType", "application/x-www-form-urlencoded");
            hc.DefaultRequestHeaders.Host = "u.y.qq.com";
            hc.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            hc.DefaultRequestHeaders.Add("Origin", "http://y.qq.com");
            hc.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.47.134 Safari/537.36 QBCore/3.53.47.400 QQBrowser/9.0.2524.400 pcqqmusic/17.10.5105.0801 SkinId/10001|1ecc94|145|1|||1fd4af");
            hc.DefaultRequestHeaders.Add("Referer", "http://y.qq.com/wk_v17/");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("AcceptLanguage","zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.5;q=0.4");
            hc.DefaultRequestHeaders.Add("Cookie", Settings.USettings.Cookie);
            var byteData = Encoding.UTF8.GetBytes(data);
            var result = await hc.PostAsync(url, new ByteArrayContent(byteData));
            return await result.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// 模拟Chrome请求 下载文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task HttpDownloadFileAsync(string url, string path)
        {
            using var hc = new HttpClient(new SocketsHttpHandler() { KeepAlivePingPolicy = HttpKeepAlivePingPolicy.WithActiveRequests });
            hc.DefaultRequestHeaders.Accept.TryParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            hc.DefaultRequestHeaders.TryAddWithoutValidation("AcceptLanguage","zh-CN,zh;q=0.8,en-US;q=0.6,en;q=0.5;q=0.4");
            hc.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
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
        /// <summary>
        /// y.qq.com fcg客户端 Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static async Task<string> GetWebDatacAsync(string url)
        {
            Console.WriteLine(Settings.USettings.Cookie + "\r\n" + Settings.USettings.g_tk);
            using var hc = new HttpClient(GetSta());
            hc.DefaultRequestHeaders.Add("CacheControl", "max-age=0");
            hc.DefaultRequestHeaders.Add("Upgrade", "1");
            hc.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.110 Safari/537.36");
            hc.DefaultRequestHeaders.Add("Referer", "https://y.qq.com/portal/player.html");
            hc.DefaultRequestHeaders.Host = "c.y.qq.com";
            hc.DefaultRequestHeaders.TryAddWithoutValidation("AcceptLanguage","zh-CN,zh;q=0.8");
            hc.DefaultRequestHeaders.Add("Cookie", Settings.USettings.Cookie);
            hc.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
            hc.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            hc.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");
            return await (await hc.GetAsync(url)).Content.ReadAsStringAsync();
        }
        /// <summary>
        /// y.qq.com musicu.fcg客户端 Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static async Task<string> GetWebDataqAsync(string url)
        {
            Console.WriteLine(Settings.USettings.Cookie + "\r\n" + Settings.USettings.g_tk);
            using var hc = new HttpClient(GetSta());
            hc.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.110 Safari/537.36");
            hc.DefaultRequestHeaders.Add("Cookie", Settings.USettings.Cookie);
            return await (await hc.GetAsync(url)).Content.ReadAsStringAsync();
        }

        public static async Task<string> PostWebSiteEzlang(string url, string content)
        {
            using var hc = new HttpClient(GetSta());
            hc.DefaultRequestHeaders.Add("ContentType", "application/x-www-form-urlencoded; charset=UTF-8");
            hc.DefaultRequestHeaders.Host = "www.ezlang.net";
            hc.DefaultRequestHeaders.Add("Origin", "https://www.ezlang.net");
            hc.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.120 Safari/537.36 Core/1.77.119.400 QQBrowser/10.9.4817.400");
            hc.DefaultRequestHeaders.Add("Referer", "https://www.ezlang.net/zh-Hans/tool/romaji");
            hc.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9");
            hc.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
            hc.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
            hc.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            var byteData = Encoding.UTF8.GetBytes(content);
            var result = await hc.PostAsync(url, new ByteArrayContent(byteData));
            return await result.Content.ReadAsStringAsync();
        }
    }
}
