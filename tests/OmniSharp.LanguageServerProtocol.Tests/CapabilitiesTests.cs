using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestUtility;
using Xunit;
using Xunit.Abstractions;

namespace OmniSharp.LanguageServerProtocol.Tests
{
    public class CapabilitiesTests : LanguageServerTestFixture
    {
        public CapabilitiesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task Capabilities_Are_Signaled()
        {
            using (var project = await TestAssets.Instance.GetTestProjectAsync("HelloWorld"))
            using (var host = CreateHost())
            {
                await host.Client.Initialize(project.Directory);
            }
        }
    }
}
