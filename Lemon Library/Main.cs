using System;

namespace LemonLibrary
{
    class MainClass
    {
        static void Main() {
            var ml = new MusicLib();
            ml.mldata.Add("003PEHXL1JkgM3", "TEST");
            var s = ml.GetLyric("003PEHXL1JkgM3");
            Console.WriteLine(s);
            Console.ReadLine();
        }
    }
}
