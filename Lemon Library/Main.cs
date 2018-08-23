using System;

namespace LemonLibrary
{
    class MainClass
    {
        static void Main() {
            var ml = new MusicLib();
            var s = ml.GetGDAsync("2623830636");
            Console.ReadLine();
            Console.ReadLine();
        }
    }
}
