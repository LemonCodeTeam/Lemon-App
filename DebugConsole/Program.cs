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
            Console.Title = "LemonApp Debug Console    [Developer Mode Alt+C]";
            Console.SetWindowSize(54, 30);
            Console.WriteLine("Hello World!");
            Console.WriteLine("LemonApp Debug Console");
            Console.WriteLine("NamedPipeServerStream Using DebugConsolePipeForLemonApp");
            Console.WriteLine("Powered by TwilightLemon");
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
