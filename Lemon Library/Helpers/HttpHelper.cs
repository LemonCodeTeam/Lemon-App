using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace LemonLibrary
{
    public class HttpHelper {
        public static async Task<int> GetWebCode(String url) {
            try
            {
                HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
                var o = (await hwr.GetResponseAsync()) as HttpWebResponse;
                return (int)o.StatusCode;
            }
            catch { return 404; }
        }
        public static async Task<string> GetWebForCodingAsync(string url) {
            HttpWebRequest hwr = (HttpWebRequest)WebRequest.Create(url);
            SetHeaderValue(hwr.Headers, "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            hwr.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
            hwr.Headers.Add("Cache-Control", "max-age=0");
            SetHeaderValue(hwr.Headers, "Connection", "keep-alive");
            hwr.Headers.Add("Cookie", "sid=5129400e-3ae5-4d8f-b4c6-55a2d8dcf866; c=access-token%3Dtrue%2Ccoding-cli%3Dfalse%2Ccoding-ocd-pages%3Dtrue%2Ccoding-ocd%3Dtrue%2Ccoding-owas%3Dtrue%2Cdepot-sharing%3Dtrue%2Cmarkdown-graph%3Dtrue%2Cnew-home%3Dtrue%2Cqc-v2%3Dtrue%2Ctask-comment%3Dfalse%2Ctask-history%3Dtrue%2Cvip%3Dtrue%2Czip-download%3Dtrue%2C5d095651; exp=89cd78c2; frontlog_sample_rate=1");
            hwr.Host="coding.net";
            hwr.Referer=url;
            hwr.Headers.Add("Upgrade-Insecure-Requests", "1");
            hwr.UserAgent="Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";
            hwr.Timeout = 20000;
            StreamReader sr = new StreamReader((await hwr.GetResponseAsync()).GetResponseStream(), Encoding.UTF8);
            var st = await sr.ReadToEndAsync();
            sr.Dispose();
            return st;
        }
        public static async Task<string> GetWebAsync(string url,Encoding e=null)
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
        public static string PostWeb(string url, string data,WebHeaderCollection Header=null)
        {
            byte[] postData = Encoding.UTF8.GetBytes(data);
            WebClient webClient = new WebClient();
            if (Header == null)
            {
                webClient.Headers.Add("Accept", "text/javascript, application/javascript, application/ecmascript, application/x-ecmascript, */*; q=0.01");
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                webClient.Headers.Add("Cookie", "Hm_lvt_6e8dac14399b608f633394093523542e=1522910113; Hm_lpvt_6e8dac14399b608f633394093523542e=1522910122; Hm_lvt_ea4269d8a00e95fdb9ee61e3041a8f98=1522910125; Hm_lpvt_ea4269d8a00e95fdb9ee61e3041a8f98=1522910125");
                webClient.Headers.Add("Host", "lab.mkblog.cn");
                webClient.Headers.Add("Origin", "http://lab.mkblog.cn");
                webClient.Headers.Add("Referer", "http://lab.mkblog.cn/music/");
                webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36");
            }
            else webClient.Headers = Header;
            byte[] responseData = webClient.UploadData(url, "POST", postData);
            webClient.Dispose();
            return Encoding.UTF8.GetString(responseData);
        }
        public static async Task HttpDownloadFileAsync(string url, string path)
        {
            HttpWebRequest hwr = WebRequest.Create(url) as HttpWebRequest;
            hwr.Accept="text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            hwr.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
            hwr.Headers.Add("Cache-Control", "max-age=0");
            hwr.KeepAlive = true;
            hwr.Referer = url;
            hwr.Headers.Add("Upgrade-Insecure-Requests", "1");
            hwr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";
            hwr.Timeout = 20000;
            HttpWebResponse response = await hwr.GetResponseAsync() as HttpWebResponse;
            Stream responseStream = response.GetResponseStream();
            Stream stream = new FileStream(path, FileMode.Create,FileAccess.ReadWrite);
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
        public static async Task<string> GetWebDatacAsync(string url, Encoding c=null,string cookie="")
        {
            if (c == null) c = Encoding.UTF8;
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
            if(cookie=="")
                 hwr.Headers.Add(HttpRequestHeader.Cookie, "pgv_pvi=1693112320; RK=DKOGai2+wu; pgv_pvid=1804673584; ptcz=3a23e0a915ddf05c5addbede97812033b60be2a192f7c3ecb41aa0d60912ff26; pgv_si=s4366031872; _qpsvr_localtk=0.3782697029073365; ptisp=ctc; luin=o2728578956; lskey=00010000863c7a430b79e2cf0263ff24a1e97b0694ad14fcee720a1dc16ccba0717d728d32fcadda6c1109ff; pt2gguin=o2728578956; uin=o2728578956; skey=@PjlklcXgw; p_uin=o2728578956; p_skey=ROnI4JEkWgKYtgppi3CnVTETY3aHAIes-2eDPfGQcVg_; pt4_token=wC-2b7WFwI*8aKZBjbBb7f4Am4rskj11MmN7bvuacJQ_; p_luin=o2728578956; p_lskey=00040000e56d131f47948fb5a2bec49de6174d7938c2eb45cb224af316b053543412fd9393f83ee26a451e15; ts_refer=ui.ptlogin2.qq.com/cgi-bin/login; ts_last=y.qq.com/n/yqq/playlist/2591355982.html; ts_uid=1420532256; yqq_stat=0");
            else hwr.Headers.Add(HttpRequestHeader.Cookie, cookie);
            var o = await hwr.GetResponseAsync();
            StreamReader sr = new StreamReader(o.GetResponseStream(), c);
            var st = await sr.ReadToEndAsync();
            sr.Dispose();
            return st;
        }
        public static async Task<string> GetWebDatadAsync(string url, Encoding c)
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
}
