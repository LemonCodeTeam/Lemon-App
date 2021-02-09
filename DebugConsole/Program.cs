using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

/// <summary>
/// 调试模式 Alt+C
/// </summary>
namespace DebugConsole
{
    class Program
    {
        static Socket socket;
        static void Main(string[] args)
        {
            Console.Title = "LemonApp Debug Console";
            Console.WriteLine("Hello World!");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("LemonApp Debug Console");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Developer Mode Alt+C]");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("127.0.0.1:3239");
            Console.WriteLine("Powered by TwilightLemon");
            Console.ForegroundColor = ConsoleColor.White;


            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3239));
            socket.Listen(100);
            //接收客户端的 Socket请求
            socket.BeginAccept(OnAccept, socket);

            while (true)
                Console.ReadLine();
        }
        public static object JsonToObject(string jsonString, object obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            return serializer.ReadObject(mStream);
        }
        private static void OnAccept(IAsyncResult async)
        {
            var serverSocket = async.AsyncState as Socket;
            //获取到客户端的socket
            var clientSocket = serverSocket.EndAccept(async);
            var bytes = new byte[10000];
            //获取socket的内容
            var len = clientSocket.Receive(bytes);
            //将 bytes[] 转换 string
            var request = Encoding.UTF8.GetString(bytes, 0, len);
            try
            {
                DebugData dt = new DebugData();
                dt= (DebugData)JsonToObject(request, dt);
                Console.ForegroundColor = dt.color switch { 
                "blue"=>ConsoleColor.Blue,
                "red"=>ConsoleColor.Red,
                "yellow"=>ConsoleColor.Yellow,
                _=>ConsoleColor.White
                };
                Console.WriteLine(dt.title);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(dt.data);
            }
            catch {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(request);
            }
            socket.Listen(100);
            //接收客户端的 Socket请求
            socket.BeginAccept(OnAccept, socket);
        }
    }

    public class DebugData {
        /// <summary>
        /// blue red yellow
        /// </summary>
        public string color = "";
        public string title = "";
        public string data = "";
    }
}
