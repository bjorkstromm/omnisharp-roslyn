using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Client.Processes;
using System.IO;
using System.Threading.Tasks;

namespace OmniSharp.LanguageServerProtocol.Tests
{
    internal class LanguageServerProcess : ServerProcess
    {
        private Stream _inputStream;
        private Stream _outputStream;
        private bool _isRunning;

        public LanguageServerProcess(Stream input, Stream output, ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            _inputStream = input;
            _outputStream = output;
        }

        public override bool IsRunning => _isRunning;

        public override Stream InputStream => _inputStream;

        public override Stream OutputStream => _outputStream;

        public override Task Start()
        {
            _isRunning = true;
            return Task.CompletedTask;
        }

        public override Task Stop()
        {
            _isRunning = false;
            return Task.CompletedTask;
        }
    }
}
