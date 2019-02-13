using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static LemonLibrary.InfoHelper;

namespace LemonLibrary.Helpers
{
    public class MusicGDTask
    {
        public MusicGData dt = new MusicGData();
        public WrapPanel wp = new WrapPanel();
        int couf = 0;
        private int finishcount = 0;
        public delegate void Ab(MusicGData md);
        public event Ab Finished;
        public int ThreadCount = 20;
        public async void GetGDAsync(string id = "2591355982")
        {
            var s = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/qzone/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&json=1&utf8=1&onlysong=0&disstid={id}&format=json&g_tk=1157737156&loginUin={MusicLib.qq}&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0", Encoding.UTF8);
            JObject o = JObject.Parse(s);
            var c0 = o["cdlist"][0];
            dt.name = c0["dissname"].ToString();
            dt.pic = c0["logo"].ToString();
            dt.id = id;
            dt.ids = c0["songids"].ToString().Split(',').ToList();
            dt.IsOwn = c0["login"].ToString() == c0["uin"].ToString();
            JToken c0s = c0["songlist"];
            if (c0s.Count() == 0) {
                Finished(dt);
                return;
            }
            foreach (var x in c0s)
            {
                dt.Data.Add(new Music());
                wp.Children.Add(new UIElement());
            }
            int count = c0s.Count() / ThreadCount;
            Console.WriteLine(count);
            if (c0s.Count() - count * ThreadCount == 0)
                couf = count;
            else couf = count + 1;
            Console.WriteLine(couf);
            for (int i = 0; i <= couf; i++)
            {
                ExData ed = new ExData() { o = c0s, index = i * ThreadCount };
                await Task.Run(() => { Ex(ed); });
            }
        }
        public void Ex(object exdata)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ExData a = exdata as ExData;
            int i = a.index;
            for (int az = 0; i - a.index <= ThreadCount - 1 && a.index <= a.o.Count(); i++)
            {
                try
                {
                    var c0si = a.o[i];
                    string singer = "";
                    var c0sis = c0si["singer"];
                    foreach (var cc in c0sis) singer += cc["name"].ToString() + "&";
                    Music m = new Music();
                    m.MusicName = c0si["songname"].ToString();
                    m.MusicName_Lyric = c0si["albumdesc"].ToString();
                    m.Singer = singer.Substring(0, singer.Length - 1);
                    m.GC = c0si["songid"].ToString();
                    m.MusicID = c0si["songmid"].ToString();
                    var amid = c0si["albummid"].ToString();
                    if (amid == "001ZaCQY2OxVMg")
                        m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T001R300x300M000{c0si["singer"][0]["mid"].ToString()}.jpg?max_age=2592000";
                    else m.ImageUrl = $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{amid}.jpg?max_age=2592000";
                    // Callback(m,dt.IsOwn,i);
                    dt.Data[i] = m;
                }//莫名其妙的System.NullReferenceException:“未将对象引用设置到对象的实例。”
                catch { }
            }
            finishcount++;
            if (finishcount == couf) Finished(dt);
            sw.Stop();
            Console.WriteLine("当前线程:" + Thread.CurrentThread.ManagedThreadId + "    耗时:" + sw.Elapsed.TotalMilliseconds);
        }
    }

    public class ExData
    {
        public JToken o = null;
        public int index = 0;
    }
}
