using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LemonLibrary
{
    class MainClass
    {
        static void Main() {
            var ml = new MusicLib();
            var s = ml.GetTopIndexAsync().GetAwaiter().GetResult();
            Console.ReadLine();
            Console.ReadLine();
        }
    }
}
