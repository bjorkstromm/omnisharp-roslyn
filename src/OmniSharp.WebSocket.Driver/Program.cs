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
                    server.Start(cancellation.Token).ConfigureAwait(false);
                    using (var host = new LanguageServerHost(
                        server.Input,
                        server.Output,
                        application,
                        cancellation))
                    {
                        host.Start().Wait();
                        cancellation.Token.WaitHandle.WaitOne();
                    }
                }
                

                return 0;
            });

            return application.Execute(args);
        });
    }
}
