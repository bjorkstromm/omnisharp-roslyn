using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OmniSharp.WebSocket
{
    internal class WebSocketServer : IDisposable
    {
        public Stream Input { get; } = new MemoryStream();
        public Stream Output { get; } = new MemoryStream();

        private readonly string _listenerPrefix;

        public WebSocketServer(int serverPort, string serverInterface)
        {
            _listenerPrefix = $"http://{serverInterface}:{serverPort}";
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add(_listenerPrefix);
                listener.Start();

                while (!cancellationToken.IsCancellationRequested)
                {
                    var listenerContext = await listener.GetContextAsync();
                    if (listenerContext.Request.IsWebSocketRequest)
                    {
                        await ProcessRequest(listenerContext, cancellationToken);
                    }
                    else
                    {
                        listenerContext.Response.StatusCode = 400;
                        listenerContext.Response.Close();
                    }
                }
            }
        }

        private async Task ProcessRequest(HttpListenerContext listenerContext, CancellationToken cancellationToken)
        {

            WebSocketContext webSocketContext;
            try
            {
                webSocketContext = await listenerContext.AcceptWebSocketAsync("lsp");
            }
            catch (WebSocketException)
            {
                listenerContext.Response.StatusCode = 500;
                listenerContext.Response.Close();
                return;
            }

            using (var webSocket = webSocketContext.WebSocket)
            {
                var buffer = new byte[1024];

                while (webSocket.State == WebSocketState.Open)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
                        return;
                    }

                    // Process Input
                    using (var receiveStream = new MemoryStream())
                    {
                        var size = 0;
                        WebSocketReceiveResult result;
                        do
                        {
                            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
                                return;
                            }
                            if (result.MessageType == WebSocketMessageType.Binary)
                            {
                                await webSocket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "", cancellationToken);
                                return;
                            }

                            size += result.Count;
                            await receiveStream.WriteAsync(buffer, 0, result.Count, cancellationToken);

                        } while (!result.EndOfMessage);

                        await receiveStream.CopyToAsync(Input, size, cancellationToken);
                    }

                    // Process Output
                    using (var receiveStream = new MemoryStream())
                    {
                        int count;
                        while((count = await Output.ReadAsync(buffer, 0, 1024, cancellationToken)) > 0)
                        {
                            await receiveStream.WriteAsync(buffer, 0, count, cancellationToken);
                        }

                        var array = receiveStream.ToArray();
                        await webSocket.SendAsync(new ArraySegment<byte>(array), WebSocketMessageType.Text, true, cancellationToken);
                    }
                }
            }
        }

        public void Dispose()
        {
            Input?.Dispose();
            Output?.Dispose();
        }
    }
}
