using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LemonLibrary
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
                string Url = "";
                while (true) {
                    if (Url != "")
                        break;
                    else Url= await MusicLib.GetUrlAsync(Id);
                    await Task.Delay(1000);
                }
                Console.WriteLine(Path+"  "+Downloading+"\r\n"+Url);
                long totalBytes = 0;
                HttpWebResponse myrp = null;
                while (true)
                {
                    if (totalBytes != 0)
                        break;
                    else
                    {
                        try
                        {
                            HttpWebRequest Myrq = (HttpWebRequest)WebRequest.Create(Url);
                            Myrq.Timeout = 500;
                            myrp = (HttpWebResponse)Myrq.GetResponse();
                            Console.WriteLine(myrp.StatusCode.ToString());
                            totalBytes = myrp.ContentLength;
                        }
                        catch { await Task.Delay(1000); }
                    }
                }
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
            return Math.Round(size) + units[i];
        }
    }
}
