using OmniSharp.Services;
using System;
using System.Composition;
using Microsoft.Extensions.Configuration;
using OmniSharp.Models.v1;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using System.IO;
using Cake.Core;
using Cake.Core.IO;
using OmniSharp;
using Cake.OmniSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
using Cake.Core.Scripting;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Cake.OmniSharp
{
    [Export(typeof(IProjectSystem)), Shared]
    public class CakeProjectSystem : IProjectSystem
    {
        public string Key => "Cake";
        public string Language => Constants.LanguageNames.Cake;//LanguageNames.CSharp;
        public IEnumerable<string> Extensions => new[] { ".cake" };

        private readonly IMetadataFileReferenceCache _metadataFileReferenceCache;

        // used for tracking purposes only
        private readonly HashSet<string> _assemblyReferences = new HashSet<string>();

        private readonly Dictionary<string, ProjectInfo> _projects;
        private readonly OmniSharpWorkspace _workspace;
        private readonly IOmniSharpEnvironment _env;
        private readonly ILogger _logger;

        private readonly ICakeContext _cakeContext;
        private readonly ICakeScriptGenerator _generator;
        private readonly ConcurrentDictionary<DocumentId, ImmutableArray<byte>> _documentChecksums;

        [ImportingConstructor]
        public CakeProjectSystem(OmniSharpWorkspace workspace, IOmniSharpEnvironment env, ILoggerFactory loggerFactory, IMetadataFileReferenceCache metadataFileReferenceCache, ICakeScriptGenerator generator)
        {
            _metadataFileReferenceCache = metadataFileReferenceCache;
            _workspace = workspace;
            _env = env;
            _logger = loggerFactory.CreateLogger<CakeProjectSystem>();
            _projects = new Dictionary<string, ProjectInfo>();
            _documentChecksums = new ConcurrentDictionary<DocumentId, ImmutableArray<byte>>();
            _generator = generator;
        }

        public Task<object> GetProjectModelAsync(string filePath)
        {
            var document = _workspace.GetDocument(filePath);
            var projectFilePath = document != null
                ? document.Project.FilePath
                : filePath;

            var projectInfo = GetProjectFileInfo(projectFilePath);
            if (projectInfo == null)
            {
                _logger.LogDebug($"Could not locate project for '{projectFilePath}'");
                return Task.FromResult<object>(null);
            }

            return Task.FromResult<object>(new CakeContextModel(filePath, projectInfo, _assemblyReferences));
        }

        public Task<object> GetWorkspaceModelAsync(WorkspaceInformationRequest request)
        {
            var cakeContextModels = new List<CakeContextModel>();
            foreach (var project in _projects)
            {
                cakeContextModels.Add(new CakeContextModel(project.Key, project.Value, _assemblyReferences));
            }
            return Task.FromResult<object>(new CakeContextModelCollection(cakeContextModels));
        }

        public void Initalize(IConfiguration configuration)
        {
            _logger.LogInformation($"Detecting Cake files in '{_env.Path}'.");

            // Nothing to do if there are no Cake files
            var allCakeFiles = Directory.GetFiles(_env.Path, "*.cake", SearchOption.AllDirectories);
            if (allCakeFiles.Length == 0)
            {
                _logger.LogInformation("Could not find any Cake files");
                return;
            }

            _logger.LogInformation($"Found {allCakeFiles.Length} Cake files.");

            foreach (var cakePath in allCakeFiles)
            {
                try
                {
                    var cakeScript = _generator.Generate(new FilePath(cakePath));
                    var project = GetProject(cakeScript, cakePath);

                    // add Cake project to workspace
                    _workspace.AddProject(project);
                    var documentId = DocumentId.CreateNewId(project.Id);
                    var loader = new CakeScriptLoader(cakePath, _generator);
                    var documentInfo = DocumentInfo.Create(
                        documentId,
                        cakePath,
                        filePath: cakePath, 
                        loader: loader, 
                        sourceCodeKind: SourceCodeKind.Script);

                    _workspace.AddDocument(documentInfo);
                    _projects[cakePath] = project;
                    _logger.LogInformation($"Added Cake project '{cakePath}' to the workspace.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{cakePath} will be ignored due to an following error");
                }
            }
        }

        private ProjectInfo GetProject(CakeScript cakeScript, string filePath)
        {
            var name = System.IO.Path.GetFileName(filePath);

            return ProjectInfo.Create(
                id: ProjectId.CreateNewId(Guid.NewGuid().ToString()),
                version: VersionStamp.Create(),
                name: name,
                filePath: filePath,
                assemblyName: $"{name}.dll",
                language: LanguageNames.CSharp,
                compilationOptions: GetCompilationOptions(cakeScript.Usings),
                parseOptions: new CSharpParseOptions(LanguageVersion.Default, DocumentationMode.Parse, SourceCodeKind.Script),
                metadataReferences: cakeScript.MetadataReferences,
                //projectReferences: ,
                isSubmission: true,
                hostObjectType: typeof(IScriptHost));
        }

        private ProjectInfo GetProjectFileInfo(string path)
        {
            ProjectInfo projectFileInfo;
            if (!_projects.TryGetValue(path, out projectFileInfo))
            {
                return null;
            }

            return projectFileInfo;
        }

        private CompilationOptions GetCompilationOptions(IEnumerable<string> usings)
        {
            var compilationOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                usings: usings,
                allowUnsafe: true,
                metadataReferenceResolver: new CachingScriptMetadataResolver(),
                sourceReferenceResolver: ScriptSourceResolver.Default,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default).
                WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
                {
                    // ensure that specific warnings about assembly references are always suppressed
                    // https://github.com/dotnet/roslyn/issues/5501
                    { "CS1701", ReportDiagnostic.Suppress },
                    { "CS1702", ReportDiagnostic.Suppress },
                    { "CS1705", ReportDiagnostic.Suppress }
                });

            var topLevelBinderFlagsProperty = typeof(CSharpCompilationOptions).GetProperty("TopLevelBinderFlags", BindingFlags.Instance | BindingFlags.NonPublic);
            var binderFlagsType = typeof(CSharpCompilationOptions).GetTypeInfo().Assembly.GetType("Microsoft.CodeAnalysis.CSharp.BinderFlags");

            var ignoreCorLibraryDuplicatedTypesMember = binderFlagsType?.GetField("IgnoreCorLibraryDuplicatedTypes", BindingFlags.Static | BindingFlags.Public);
            var ignoreCorLibraryDuplicatedTypesValue = ignoreCorLibraryDuplicatedTypesMember?.GetValue(null);
            if (ignoreCorLibraryDuplicatedTypesValue != null)
            {
                topLevelBinderFlagsProperty?.SetValue(compilationOptions, ignoreCorLibraryDuplicatedTypesValue);
            }

            return compilationOptions;
        }
    }
}
