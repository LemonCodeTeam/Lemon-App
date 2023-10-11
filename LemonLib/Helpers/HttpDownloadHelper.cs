using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Un4seen.Bass;
using static LemonLib.InfoHelper;

namespace LemonLib
{
    public class HttpDownloadHelper
    {
        private Music mData;
        private string Path = "";
        public bool Downloading = true;
        private int Progress = 0;

        public delegate void Pr(int pr);
        public event Pr ProgressChanged;
        public delegate void finish();
        public event finish Finished;
        public event finish Fail;
        public delegate void x(string z);
        public event x GetSize;
        public HttpDownloadHelper(Music data, string pa)
        {
            mData = data;
            Path = pa;
        }
        public async void Download()
        {
            await Task.Run(async () =>
            {
                var Url = await MusicLib.GetUrlAsync(mData);
                if (Url != null)
                {
                    Console.WriteLine(Path + "  " + Downloading + "\r\n" + Url.Url);
                    using var hc=new HttpClient(new SocketsHttpHandler(){KeepAlivePingPolicy=HttpKeepAlivePingPolicy.WithActiveRequests});
                   
                    hc.DefaultRequestHeaders.Accept.TryParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    hc.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
                    hc.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                    hc.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.66 Safari/537.36 Edg/80.0.361.40");
                   
                    var myrp = await hc.GetAsync(Url.Url,HttpCompletionOption.ResponseHeadersRead);
                    Console.WriteLine(myrp.StatusCode.ToString());
                    var totalBytes = (long)myrp.Content.Headers.ContentLength;
                    GetSize(Getsize(totalBytes));
                    Stream st = await myrp.Content.ReadAsStreamAsync();
                    Stream so = new FileStream(Path, FileMode.Create);
                    long totalDownloadedByte = 0;
                    byte[] by = new byte[1048576];
                    int osize = await st.ReadAsync(by, 0, by.Length);
                    while (osize > 0)
                    {
                        if (stop) break;
                        if (Downloading)
                        {
                            totalDownloadedByte = osize + totalDownloadedByte;
                            await so.WriteAsync(by, 0, osize);
                            osize = await st.ReadAsync(by, 0, by.Length);
                            Progress = (int)(totalDownloadedByte / totalBytes * 100);
                            ProgressChanged(Progress);
                            Console.WriteLine("downloading:" + Progress);
                        }
                    }
                    st.Close();
                    so.Close();
                    if (!stop) Finished();
                }
                else Fail();
            });
        }
        public void Pause()
        {
            Downloading = false;
            Console.WriteLine("下载暂停");
        }
        public void Start()
        {
            Downloading = true;
            Console.WriteLine("Start.");
        }
        public bool stop = false;
        public void Stop()
        {
            stop = true;
        }
        private string Getsize(double size)
        {
            string[] units = new string[] { "B", "KB", "MB", "GB", "TB", "PB" };
            double mod = 1024.0;
            int i = 0;
            while (size >= mod)
            {
                size /= mod;
                i++;
            }
            return size.ToString("0.00") + units[i];
        }
    }
}
