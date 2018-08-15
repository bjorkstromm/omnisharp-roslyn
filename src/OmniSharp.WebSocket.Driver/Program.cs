using System;
using System.Threading;
using OmniSharp.LanguageServerProtocol;

namespace OmniSharp.WebSocket.Driver
{
    internal class Program
    {
        static int Main(string[] args) => HostHelpers.Start(() =>
        {
            var application = new WebSocketCommandLineApplication();
            application.OnExecute(() =>
            {
                var cancellation = new CancellationTokenSource();

                Configuration.ZeroBasedIndices = true;
                using (var server = new WebSocketServer(
                    application.Port,
                    application.Interface))
                {
                    server.OnConnected(async () =>
                    {
                        using (var host = new LanguageServerHost(
                            server.Input,
                            server.Output,
                            application,
                            cancellation))
                        {
                            await host.Start();
                            cancellation.Token.WaitHandle.WaitOne();
                        }
                    });

                    server.Start(cancellation.Token).Wait(CancellationToken.None);
                    cancellation.Token.WaitHandle.WaitOne();
                }

                return 0;
            });

            return application.Execute(args);
        });
    }
}
