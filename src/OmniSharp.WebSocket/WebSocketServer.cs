using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        private readonly ILogger _logger;
        private readonly Thread _listenerThread;
        private HttpListener _listener;
        private System.Net.WebSockets.WebSocket _connection;
        private bool _closing = false;
        private CancellationTokenSource _cancellation;

        public Stream Input { get; } = new BlockingMemoryStream();
        public Stream Output { get; } = new BlockingMemoryStream();

        private readonly string _listenerPrefix;

        public WebSocketServer(int serverPort, string serverInterface, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory?.CreateLogger(typeof(WebSocketServer)) ?? NullLogger.Instance;
            _listenerPrefix = $"http://{serverInterface}:{serverPort}/";
            _listenerThread = new Thread(ProcessListener) { IsBackground = true, Name = "ProcessHttpListener" };
        }

        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(_listenerPrefix);
            _listener.Start();
            _listenerThread.Start();
        }

        public void Stop()
        {
            _cancellation?.Cancel();
            _closing = true;
        }

        private void ProcessListener()
        {
            while (!_closing)
            {
                var listenerContext = _listener.GetContext();

                if (listenerContext.Request.IsWebSocketRequest)
                {
                    WebSocketContext webSocketContext;
                    try
                    {
                        webSocketContext = listenerContext.AcceptWebSocketAsync(null).Result;
                        _connection = webSocketContext.WebSocket;
                        _cancellation = new CancellationTokenSource();

                        _logger.LogInformation("Websocket connected.");

                        var tasks = new[]
                        {
                            ProcessInput(_cancellation.Token),
                            ProcessOutput(_cancellation.Token)
                        };

                        Task.WaitAll(tasks);
                        _logger.LogInformation("Websocket disconnected.");
                    }
                    catch (WebSocketException)
                    {
                        listenerContext.Response.StatusCode = 500;
                        listenerContext.Response.Close();
                    }
                }
                else
                {
                    listenerContext.Response.StatusCode = 400;
                    listenerContext.Response.Close();
                }
            }
        }

        private async Task ProcessInput(CancellationToken cancellationToken)
        {
            while (_connection.State == WebSocketState.Open)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    await _connection.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
                    return;
                }

                var buffer = new byte[1024];

                // Process Input
                using (var receiveStream = new MemoryStream())
                {
                    var size = 0;
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _connection.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _connection.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
                            return;
                        }
                        if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            await _connection.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "", cancellationToken);
                            return;
                        }

                        size += result.Count;
                        await receiveStream.WriteAsync(buffer, 0, result.Count, cancellationToken);

                    } while (!result.EndOfMessage);

                    receiveStream.Position = 0;
                    //TODO: Figure out why Monaco language client doesn't send headers?
                    var headerBuffer = Encoding.ASCII.GetBytes($"Content-Length: {size}\r\n\r\n");
                    await Input.WriteAsync(headerBuffer, 0, headerBuffer.Length);
                    await Input.WriteAsync(receiveStream.GetBuffer(), 0, size);

                    _logger.LogInformation($"Sending: {Encoding.ASCII.GetString(receiveStream.GetBuffer())}");
                }
            }
        }

        private async Task ProcessOutput(CancellationToken cancellationToken)
        {
            const char CR = '\r';
            const char LF = '\n';
            char[] CRLF = { CR, LF };
            char[] HeaderKeys = { CR, LF, ':' };
            const short MinBuffer = 21; // Minimum size of the buffer "Content-Length: X\r\n\r\n"

            while (_connection.State == WebSocketState.Open)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    await _connection.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
                    return;
                }

                var buffer = new byte[300];
                var current = await Output.ReadAsync(buffer, 0, MinBuffer);
                if (current == 0) return; // no more _input
                while (current < MinBuffer ||
                       buffer[current - 4] != CR || buffer[current - 3] != LF ||
                       buffer[current - 2] != CR || buffer[current - 1] != LF)
                {
                    var n = await Output.ReadAsync(buffer, current, 1);
                    if (n == 0) return; // no more _input, mitigates endless loop here.
                    current += n;
                }

                var headersContent = System.Text.Encoding.ASCII.GetString(buffer, 0, current);
                var headers = headersContent.Split(HeaderKeys, StringSplitOptions.RemoveEmptyEntries);
                long length = 0;
                for (var i = 1; i < headers.Length; i += 2)
                {
                    // starting at i = 1 instead of 0 won't throw, if we have uneven headers' length
                    var header = headers[i - 1];
                    var value = headers[i].Trim();
                    if (header.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                    {
                        length = 0;
                        long.TryParse(value, out length);
                    }
                }

                if (length != 0 || length < int.MaxValue)
                {
                    var requestBuffer = new byte[length];
                    var received = 0;
                    while (received < length)
                    {
                        var n = Output.Read(requestBuffer, received, requestBuffer.Length - received);
                        if (n == 0) return; // no more _input
                        received += n;
                    }

                    await _connection.SendAsync(new ArraySegment<byte>(requestBuffer), WebSocketMessageType.Text, true, cancellationToken);
                    _logger.LogInformation($"Received: {Encoding.ASCII.GetString(requestBuffer)}");
                }

                // Process Output
                //using (var receiveStream = new MemoryStream())
                //{
                //    bool sent = false;
                //    while (!sent)
                //    {
                //        int count;
                //        while ((count = await Output.ReadAsync(buffer, 0, 1024, cancellationToken)) > 0)
                //        {
                //            await receiveStream.WriteAsync(buffer, 0, count, cancellationToken);
                //        }

                //        var array = receiveStream.ToArray();

                //        if (array.Length > 0)
                //        {
                //            // TODO: Remove any headers.
                //            var contentStart = Array.IndexOf(array, Convert.ToByte('{'));
                //            await _connection.SendAsync(new ArraySegment<byte>(array.Skip(contentStart).ToArray()), WebSocketMessageType.Text, true, cancellationToken);
                //            sent = true;
                //            _logger.LogInformation(Encoding.ASCII.GetString(array.Skip(contentStart).ToArray()));
                //        }
                //    }
                //}
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
            Stop();
            _listenerThread.Join();
            _listener.Stop();
            Input?.Dispose();
            Output?.Dispose();
        }
    }
}
