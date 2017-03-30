using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cake.Core;
using Microsoft.Extensions.Logging;
using Cake.Core.IO;
using OmniSharp.Cake.Core.Configuration;
using OmniSharp.Cake.Core.Reflection;
using Cake.Core.Tooling;

namespace OmniSharp.Cake.Internal
{
    internal static class CakeContextFactory
    {
        public static ICakeContext CreateContext(ILogger logger)
        {
            var log = new CakeLog(logger);
            var cakePlatform = new CakePlatform();
            var cakeRuntime = new CakeRuntime();
            var environment = new CakeEnvironment(cakePlatform, cakeRuntime, log);
            var fileSystem = new FileSystem();
            var globber = new Globber(fileSystem, environment);
            var configuration = new CakeConfiguration(new Dictionary<string, string>()); //TODO: Get configuration
            var assemblyVerifier = new AssemblyVerifier(configuration, log);
            var assemblyLoader = new AssemblyLoader(fileSystem, assemblyVerifier);
            var toolRepository = new ToolRepository(environment);
            var toolResolutionStrategy = new ToolResolutionStrategy(fileSystem, environment, globber, configuration);
            var toolLocator = new ToolLocator(environment, toolRepository, toolResolutionStrategy);
            var cakeArguments = new CakeArguments();
            var processRunner = new ProcessRunner(environment, log);
            var windowsRegistry = new WindowsRegistry();

            return new CakeContext(fileSystem, environment, globber, log, cakeArguments, processRunner, windowsRegistry, toolLocator);
        }
    }
}
