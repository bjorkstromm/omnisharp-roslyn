using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Utilities;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OmniSharp.LanguageServerProtocol.Tests
{
    public class LanguageServerTestHost : DisposableObject
    {
        public Stream Input { get; private set; }
        public Stream Output { get; private set; }
        public LanguageClient Client { get; private set; }

        private LanguageServerHost _host;
        private CancellationTokenSource _cts;
        private Task _hostTask;



        private LanguageServerTestHost(ILoggerFactory loggerFactory)
        {
            Input = new BlockingMemoryStream();
            Output = new BlockingMemoryStream();
            _cts = new CancellationTokenSource();

            _host = new LanguageServerHost(Input, Output, new CommandLineApplication(), _cts);
            _hostTask = _host.Start();

            Client = new LanguageClient(loggerFactory, new LanguageServerProcess(Input, Output, loggerFactory));
        }

        public static LanguageServerTestHost Create(ILoggerFactory loggerFactory)
            => new LanguageServerTestHost(loggerFactory);

        protected override void DisposeCore(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            _cts.Cancel();
            _hostTask.Wait();
            _host.Dispose();
            Client.Shutdown().GetAwaiter().GetResult();
        }
    }
}
