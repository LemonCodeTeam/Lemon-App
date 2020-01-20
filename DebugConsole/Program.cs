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
            Console.SetWindowSize(48, 30);
            Console.WriteLine("Hello World!");
            Console.WriteLine("LemonApp Debug Console");
            Console.WriteLine("Power by TwilightLemon");
            new Thread(Start).Start();
            Console.ReadLine();
        }
        static async void Start() {

            using (NamedPipeServerStream pipe = new NamedPipeServerStream("DebugConsolePipeForLemonApp", PipeDirection.InOut, 1))
            {
                try
                {
                    pipe.WaitForConnection();
                    Console.WriteLine("Connect Successfully!");
                    pipe.ReadMode = PipeTransmissionMode.Byte;
                    using (var sr = new StreamReader(pipe))
                    {
                        while (true)
                        {
                            string text = await sr.ReadLineAsync();
                            Console.WriteLine(text);
                        }
                    }
                }
                catch { }
            }

        }
    }
}
