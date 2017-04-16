using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cake.Core;
using Cake.Core.Scripting;
using Cake.Core.Diagnostics;
using Cake.Core.Configuration;
using Cake.Core.Reflection;
using Cake.Core.Scripting.Analysis;
using Cake.OmniSharp.Configuration;
using Cake.OmniSharp.Scripting;
using Cake.OmniSharp.Reflection;
using Cake.Core.Tooling;
using Cake.Core.IO;
using System.Reflection;
using Cake.OmniSharp.Extensions;
using Microsoft.CodeAnalysis;
using System.Composition;
using Microsoft.Extensions.Logging;
using Cake.OmniSharp.Diagnostics;

namespace Cake.OmniSharp.Scripting
{
    [Export(typeof(ICakeScriptGenerator)), Shared]
    public class CakeScriptGenerator : ICakeScriptGenerator
    {
        private readonly IFileSystem _fileSystem;
        private readonly ICakeEnvironment _environment;
        private readonly ICakeLog _log;
        private readonly ICakeConfiguration _configuration;
        private readonly IScriptAliasFinder _aliasFinder;
        private readonly IScriptAnalyzer _analyzer;
        private readonly IScriptProcessor _processor;
        private readonly IScriptConventions _conventions;
        private readonly IAssemblyLoader _assemblyLoader;

        [ImportingConstructor]
        public CakeScriptGenerator(ILoggerFactory loggerFactory, IFileSystem fileSystem)
        {
            // Log
            _log = new CakeLog(loggerFactory.CreateLogger<CakeProjectSystem>());

            // Configuration
            _configuration = new CakeConfiguration(new Dictionary<string, string>());

            // Environment
            var cakePlatform = new CakePlatform();
            var cakeRuntime = new CakeRuntime();
            _environment = new CakeEnvironment(cakePlatform, cakeRuntime, _log);

            // Filesystem
            _fileSystem = fileSystem;

            // Assemblyloader
            var configuration = new CakeConfiguration(new Dictionary<string, string>());
            var assemblyVerifier = new AssemblyVerifier(configuration, _log);
            _assemblyLoader = new AssemblyLoader(_fileSystem, assemblyVerifier);

            // Aliasfinder
            _aliasFinder = new ScriptAliasFinder(_log);

            // Analyzer
            _analyzer = new ScriptAnalyzer(_fileSystem, _environment, _log, null);

            // Processor
            var toolRepository = new ToolRepository(_environment);
            var globber = new Globber(_fileSystem, _environment);
            var toolResolutionStrategy = new ToolResolutionStrategy(_fileSystem, _environment, globber, configuration);
            var toolLocator = new ToolLocator(_environment, toolRepository, toolResolutionStrategy);
            _processor = new ScriptProcessor(_fileSystem, _environment, _log, toolLocator, new[] { new CakePackageInstaller() });

            // Conventions
            _conventions = new ScriptConventions(_fileSystem, _assemblyLoader, _log);
        }

        public CakeScriptGenerator(
            IFileSystem fileSystem,
            ICakeEnvironment environment,
            ICakeLog log,
            ICakeConfiguration configuration,
            IScriptAliasFinder aliasFinder,
            IScriptAnalyzer analyzer,
            IScriptProcessor processor,
            IScriptConventions conventions,
            IAssemblyLoader assemblyLoader)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _aliasFinder = aliasFinder ?? throw new ArgumentNullException(nameof(aliasFinder));
            _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _conventions = conventions ?? throw new ArgumentNullException(nameof(conventions));
            _assemblyLoader = assemblyLoader ?? throw new ArgumentNullException(nameof(assemblyLoader));
        }

        public CakeScript Generate(FilePath scriptPath)
        {
            if (scriptPath == null)
            {
                throw new ArgumentNullException(nameof(scriptPath));
            }

            // Make the script path absolute.
            scriptPath = scriptPath.MakeAbsolute(_environment);

            // Prepare the environment.
            _environment.WorkingDirectory = scriptPath.GetDirectory();

            // Analyze the script file.
            _log.Verbose("Analyzing build script...");
            var result = _analyzer.Analyze(scriptPath.GetFilename());

            // Install tools.
            _log.Verbose("Processing build script...");
            var toolsPath = GetToolPath(scriptPath.GetDirectory());
            _processor.InstallTools(result, toolsPath);

            // Install addins.
            var applicationRoot = _environment.ApplicationRoot;
            var addinRoot = GetAddinPath(applicationRoot);
            var addinReferences = _processor.InstallAddins(result, addinRoot);
            foreach (var addinReference in addinReferences)
            {
                result.References.Add(addinReference.FullPath);
            }

            // Load all references.
            var metadataReferences = new HashSet<MetadataReference>();
            var assemblies = new HashSet<Assembly>();
            assemblies.AddRange(_conventions.GetDefaultAssemblies(applicationRoot));

            foreach (var reference in result.References)
            {
                var referencePath = new FilePath(reference);
                if (_fileSystem.Exist(referencePath))
                {
                    var assembly = _assemblyLoader.Load(referencePath, true);
                    assemblies.Add(assembly);
                }
                else
                {
                    // Add the metadatareference
                    metadataReferences.Add(MetadataReference.CreateFromFile(referencePath.MakeAbsolute(_environment).ToString()));
                }
            }

            var aliases = new List<ScriptAlias>();

            // Got any assemblies?
            if (assemblies.Count > 0)
            {
                // Find all script aliases.
                var foundAliases = _aliasFinder.FindAliases(assemblies);
                if (foundAliases.Count > 0)
                {
                    aliases.AddRange(foundAliases);
                }

                // Add assembly references to the session.
                foreach (var assembly in assemblies)
                {
                    metadataReferences.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }

            // Import all namespaces.
            var namespaces = new HashSet<string>(result.Namespaces, StringComparer.Ordinal);
            namespaces.AddRange(_conventions.GetDefaultNamespaces());
            namespaces.AddRange(aliases.SelectMany(alias => alias.Namespaces));


            // Return the script.
            return new CakeScript
            {
                Script = new Script(result.Namespaces, result.Lines, aliases, result.UsingAliases),
                MetadataReferences = metadataReferences,
                Usings = namespaces
            };
        }

        private DirectoryPath GetToolPath(DirectoryPath root)
        {
            var toolPath = _configuration.GetValue(Constants.Paths.Tools);
            if (!string.IsNullOrWhiteSpace(toolPath))
            {
                return new DirectoryPath(toolPath).MakeAbsolute(_environment);
            }

            return root.Combine("tools");
        }

        private DirectoryPath GetAddinPath(DirectoryPath applicationRoot)
        {
            var addinPath = _configuration.GetValue(Constants.Paths.Addins);
            if (!string.IsNullOrWhiteSpace(addinPath))
            {
                return new DirectoryPath(addinPath).MakeAbsolute(_environment);
            }

            return applicationRoot.Combine("../Addins").Collapse();
        }
    }
}
