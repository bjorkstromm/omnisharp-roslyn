using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OmniSharp.LanguageServerProtocol;

namespace OmniSharp.WebSocket.Driver
{
    internal class Program
    {
        static int Main(string[] args) => HostHelpers.Start(() =>
        {
            //var clientThread = new Thread(ClientThread);
            //clientThread.Start();

            var application = new WebSocketCommandLineApplication();
            application.OnExecute(() =>
            {
                var cancellation = new CancellationTokenSource();
                var loggerFactory = new LoggerFactory();
                loggerFactory.AddConsole();

                Configuration.ZeroBasedIndices = true;
                using (var server = new WebSocketServer(
                    application.Port,
                    application.Interface,
                    loggerFactory))
                {
                    using (var host = new LanguageServerHost(
                        server.Input,
                        server.Output,
                        application,
                        cancellation))
                    {
                        server.Start();
                        host.Start().Wait();
                        cancellation.Token.WaitHandle.WaitOne();
                        server.Stop();
                    }
                }

                return 0;
            });

            return application.Execute(args);
        });

        private static async void ClientThread()
        {
            while (true)
            {
                var client = new ClientWebSocket();

                try
                {
                    await client.ConnectAsync(new Uri("ws://localhost:2000/"), new CancellationTokenSource(1000).Token);

                    Console.WriteLine("client connected");

                    var init = "Content-Length: 144\r\n\r\n{ \"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"initialize\", \"params\": { \"processId\": null, \"rootUri\": null, \"capabilities\": { }, \"trace\": \"verbose\" } }";

                    var bytes = Encoding.ASCII.GetBytes(init);
                    await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

                    var buffer = new ArraySegment<byte>(new byte[1024*1024]);

                    await Task.Delay(5000);

                    await client.ReceiveAsync(buffer, CancellationToken.None);

                    Console.WriteLine($"Received: {Encoding.ASCII.GetString(buffer.Array)}");

                    await client.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);

                    return;
                }
                catch (Exception)
                {
                    Console.WriteLine("reconnecting...");
                }
            }
        }
    }
}
