using Microsoft.Extensions.Logging;
using TestUtility;
using TestUtility.Logging;
using Xunit.Abstractions;

namespace OmniSharp.LanguageServerProtocol.Tests
{
    public abstract class LanguageServerTestFixture
    {
        protected readonly ITestOutputHelper TestOutput;
        protected readonly ILoggerFactory LoggerFactory;


        protected LanguageServerTestFixture(ITestOutputHelper output)
        {
            TestOutput = output;
            LoggerFactory = new LoggerFactory()
                .AddXunit(output);
        }

        public LanguageServerTestHost CreateHost()
        {
            var host = LanguageServerTestHost.Create(LoggerFactory);

            return host;
        }
    }
}
