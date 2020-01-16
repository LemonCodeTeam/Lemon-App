using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LemonLib
{
    public class HttpDownloadHelper
    {
        private string Id = "";
        private string Path = "";
        public bool Downloading = true;
        private int Progress = 0;

        public delegate void Pr(int pr);
        public event Pr ProgressChanged;
        public delegate void finish();
        public event finish Finished;
        public delegate void x(string z);
        public event x GetSize;
        public HttpDownloadHelper(string id, string pa)
        {
            Id = id;
            Path = pa;
        }
        public async void Download()
        {
            await Task.Run(async () =>
            {
                string Url = await MusicLib.GetUrlAsync(Id);
                Console.WriteLine(Path + "  " + Downloading + "\r\n" + Url);
                HttpWebRequest Myrq = (HttpWebRequest)WebRequest.Create(Url);
                var myrp = (HttpWebResponse)Myrq.GetResponse();
                Console.WriteLine(myrp.StatusCode.ToString());
                var totalBytes = myrp.ContentLength;
                GetSize(Getsize(totalBytes));
                Stream st = myrp.GetResponseStream();
                Stream so = new FileStream(Path, FileMode.Create);
                long totalDownloadedByte = 0;
                byte[] by = new byte[1048576];
                int osize = await st.ReadAsync(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    if (stop) break;
                    if (Downloading)
                    {
                        totalDownloadedByte = osize + totalDownloadedByte;
                        await so.WriteAsync(by, 0, osize);
                        osize = await st.ReadAsync(by, 0, (int)by.Length);
                        Progress = (int)((float)totalDownloadedByte / (float)totalBytes * 100);
                        ProgressChanged(Progress);
                        Console.WriteLine("downloading:" + Progress);
                    }
                }
                st.Close();
                so.Close();
                myrp.Close();
                Finished();
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
        public void Stop() {
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
