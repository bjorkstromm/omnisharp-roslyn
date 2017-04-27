using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Core;
using Cake.Core.Scripting;
using Cake.Core.Diagnostics;
using Cake.Core.Configuration;
using Cake.Core.Reflection;
using Cake.Core.Scripting.Analysis;
using Cake.OmniSharp.Configuration;
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
        private readonly IGlobber _globber;
        private readonly CakeDocumentationProvider _documentationProvider;

        [ImportingConstructor]
        public CakeScriptGenerator(ILoggerFactory loggerFactory, IFileSystem fileSystem, CakeDocumentationProvider documentationProvider)
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

            // Globber
            _globber = new Globber(_fileSystem, _environment);

            // Processor
            var toolRepository = new ToolRepository(_environment);
            var toolResolutionStrategy = new ToolResolutionStrategy(_fileSystem, _environment, _globber, configuration);
            var toolLocator = new ToolLocator(_environment, toolRepository, toolResolutionStrategy);
            _processor = new ScriptProcessor(_fileSystem, _environment, _log, toolLocator, new[] { new CakePackageInstaller() });

            // Conventions
            _conventions = new ScriptConventions(_fileSystem, _assemblyLoader, _log);

            _documentationProvider = documentationProvider;
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
            var cakeRoot = GetCakePath(toolsPath);
            var addinRoot = GetAddinPath(cakeRoot);
            var addinReferences = _processor.InstallAddins(result, addinRoot);
            foreach (var addinReference in addinReferences)
            {
                result.References.Add(addinReference.FullPath);
            }

            // Load all references.
            var metadataReferences = new HashSet<MetadataReference>();
            var assemblies = new HashSet<Assembly>();

            // TODO: Don't load Cake.Core here... Just add it as a Metadata reference...
            //assemblies.AddRange(_conventions.GetDefaultAssemblies(cakeRoot));
            assemblies.Add(_assemblyLoader.Load(cakeRoot.CombineWithFilePath("Cake.Common.dll"), false));

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
                    var referenceFullPath = referencePath.MakeAbsolute(_environment).FullPath;
                    metadataReferences.Add(MetadataReference.CreateFromFile(referenceFullPath, documentation: CreateDocumentationProvider(referenceFullPath)));
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
                    metadataReferences.Add(MetadataReference.CreateFromFile(assembly.Location, documentation: CreateDocumentationProvider(assembly.Location)));

                    _documentationProvider.AddDocumentation(assembly.Location);
                }

                metadataReferences.AddRange(GetDefaultReferences(_environment.ApplicationRoot));
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
                Usings = namespaces,
            };
        }

        public CakeDocumentationProvider DocumentationProvider => _documentationProvider;

        private DocumentationProvider CreateDocumentationProvider(string assemblyLocation)
        {
            var documentationPath = new FilePath(assemblyLocation).ChangeExtension("xml");

            if (_fileSystem.Exist(documentationPath))
            {
                return XmlDocumentationProvider.CreateFromFile(documentationPath.FullPath);
            }

            return Microsoft.CodeAnalysis.DocumentationProvider.Default;
        }

        private IEnumerable<MetadataReference> GetDefaultReferences(DirectoryPath root)
        {
            // Prepare the default assemblies.
            var result = new HashSet<string>();
            result.Add(typeof(Action).GetTypeInfo().Assembly.Location); // mscorlib or System.Private.Core
            result.Add(typeof(IQueryable).GetTypeInfo().Assembly.Location); // System.Core or System.Linq.Expressions

            // Load other Cake-related assemblies that we need.
            result.Add(root.CombineWithFilePath("Cake.Core.dll").FullPath);

#if NET46
            result.Add(typeof(Uri).GetTypeInfo().Assembly.Location); // System
            result.Add(typeof(System.Xml.XmlReader).GetTypeInfo().Assembly.Location); // System.Xml
            result.Add(typeof(System.Xml.Linq.XDocument).GetTypeInfo().Assembly.Location); // System.Xml.Linq
            result.Add(typeof(System.Data.DataTable).GetTypeInfo().Assembly.Location); // System.Data
#endif

            // Return the assemblies.
            return result.Select(assemblyLocation =>
            {
                _documentationProvider.AddDocumentation(assemblyLocation);
                return MetadataReference.CreateFromFile(assemblyLocation, documentation: CreateDocumentationProvider(assemblyLocation));
            });
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

        private DirectoryPath GetCakePath(DirectoryPath toolPath)
        {
            var pattern = string.Concat(toolPath.FullPath, "/**/Cake.Core.dll");
            var cakeCorePath = _globber.GetFiles(pattern).FirstOrDefault();

            return cakeCorePath?.GetDirectory().MakeAbsolute(_environment);
        }

        private DirectoryPath GetAddinPath(DirectoryPath cakeRoot)
        {
            var addinPath = _configuration.GetValue(Constants.Paths.Addins);
            if (!string.IsNullOrWhiteSpace(addinPath))
            {
                return new DirectoryPath(addinPath).MakeAbsolute(_environment);
            }

            return cakeRoot.Combine("../Addins").Collapse();
        }
    }
}
