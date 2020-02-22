using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LemonLib
{
    public class MainClass
    {
        /// <summary>
        /// API调试台
        /// </summary>
        static async Task Main()
        {
            await Settings.LoadUSettings("2728578956");
            var _ =await MusicLib.GetSongListAboutSong("000MW82Z2j20g2");
            Console.WriteLine(_.Count);
            Console.ReadLine();
        }
        public static Action<string> DebugCallBack;
        #region #DEBUG 评论获取/点赞测试:
        /*
            await Settings.LoadUSettings("2728578956");
            List<InfoHelper.MusicPL> a = await MusicLib.GetPLByQQAsync("0034Ge6Q2b504d");
            int i = 0;
            foreach (var b in a)
            {
                Console.WriteLine($"[{i}]  {b.text}  Like:{b.like}");
                i++;
            }
            int index = int.Parse(Console.ReadLine());
            Console.WriteLine(await MusicLib.PraiseMusicPLAsync("0034Ge6Q2b504d", a[index]));
            Console.ReadLine();
        */
        #endregion
    }
}
