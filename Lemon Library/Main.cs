using System;

namespace LemonLibrary
{
    class MainClass
    {
        static void Main() {
            var ml = new MusicLib();
            ml.mldata.Add("001mgo6u1tLENZ", "test");
            Console.WriteLine(ml.GetLyric("001mgo6u1tLENZ"));
            Console.ReadLine();
        }
    }
}
