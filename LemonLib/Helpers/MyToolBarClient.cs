using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LemonLib.Helpers
{
    public class MyToolBarClient:IDisposable
    {
        public static bool DetectIsCreated()
        {
            bool success = false;
            Mutex mutex = new Mutex(false, "MyToolBar.Plugin.BasicPackage//LemonAppMusicServier", out success);
            return !success;
        }

        public TcpClient tcpClient;
        public MyToolBarClient()
        {
            tcpClient = new TcpClient("localhost",12587);
        }
        public async Task SendMsgAsync(string message)
        {
            message += '\n';
            if (!tcpClient.Connected) return;
            byte[] data = Encoding.UTF8.GetBytes(message);
            var stream=tcpClient.GetStream();
            await stream.WriteAsync(data);
            await stream.FlushAsync();
        }

        public void Dispose()
        {
            tcpClient.Dispose();
        }
    }
}
