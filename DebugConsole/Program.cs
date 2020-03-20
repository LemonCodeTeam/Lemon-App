using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace DebugConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "LemonApp Debug Console";
            Console.WriteLine("Hello World!");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("LemonApp Debug Console");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Developer Mode Alt+C]");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("NamedPipeServerStream Using DebugConsolePipeForLemonApp");
            Console.WriteLine("Powered by TwilightLemon");
            Console.ForegroundColor = ConsoleColor.White;
            new Thread(Start).Start();
            Console.ReadLine();
        }

        static NamedPipeServerStream pipe;
        static StreamReader sr;
        static async void Start()
        {
            pipe = new NamedPipeServerStream("DebugConsolePipeForLemonApp", PipeDirection.InOut, 1);
            try
            {
                pipe.WaitForConnection();
                Console.WriteLine("Connect Successfully!");
                pipe.ReadMode = PipeTransmissionMode.Byte;
                sr = new StreamReader(pipe);
                while (true)
                {
                    string text = await sr.ReadLineAsync();
                    Console.WriteLine(text);
                }
            }
            catch { }
        }
    }
}
