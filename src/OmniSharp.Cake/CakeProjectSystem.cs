using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Threading.Tasks;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Scripting;
using Cake.Core.Scripting.Analysis;
using Cake.Core.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OmniSharp.Models.v1;
using OmniSharp.Services;

namespace OmniSharp.Cake
{
    [Export(typeof(IProjectSystem)), Shared]
    public class CakeProjectSystem : IProjectSystem
    {
        private readonly IMetadataFileReferenceCache _metadataFileReferenceCache;
        private readonly CakeScriptRunner _scriptRunner;
        private readonly CakeScriptHost _scripHost;
        private CSharpParseOptions CsxParseOptions { get; } = new CSharpParseOptions(LanguageVersion.Default, DocumentationMode.Parse, SourceCodeKind.Script);
        private OmnisharpWorkspace Workspace { get; }
        private IOmnisharpEnvironment Env { get; }
        private CakeContext Context { get; }
        private ILogger Logger { get; }

        [ImportingConstructor]
        public CakeProjectSystem(
            OmnisharpWorkspace workspace,
            IOmnisharpEnvironment env,
            ILoggerFactory loggerFactory,
            CakeContext context,
            IMetadataFileReferenceCache metadataFileReferenceCache)
        {
            _metadataFileReferenceCache = metadataFileReferenceCache;
            Workspace = workspace;
            Env = env;
            Context = context;
            Logger = loggerFactory.CreateLogger<CakeProjectSystem>();

            var environment = new CakeEnvironment(new CakePlatform(), new CakeRuntime(), new CakeLog(Logger));
            var fileSystem = new FileSystem();
            var cakeLog = new CakeLog(Logger);
            var globber = new Globber(fileSystem, environment);
            var assemblyLoader = new CakeAssemblyLoader(fileSystem);
            var toolLocator = new ToolLocator(environment, new ToolRepository(environment), new ToolResolutionStrategy(fileSystem, environment, globber, new CakeConfiguration()));
            var cakeContext = new global::Cake.Core.CakeContext(fileSystem, environment, globber, cakeLog, new CakeArguments(), 
                new ProcessRunner(environment, cakeLog), new WindowsRegistry(),
                toolLocator);

            _scripHost = new CakeScriptHost(
                new CakeEngine(cakeLog), 
                cakeContext,
                null,
                cakeLog);

            _scriptRunner = new CakeScriptRunner(
                environment,
                cakeLog,
                new CakeConfiguration(), 
                new CakeXPlatScriptEngine(null, cakeLog),
                new ScriptAliasFinder(cakeLog), 
                new ScriptAnalyzer(fileSystem, environment, cakeLog),
                new ScriptProcessor(fileSystem, environment, cakeLog, toolLocator, new []{ new CakePackageInstaller() }), 
                new ScriptConventions(fileSystem, assemblyLoader, cakeLog),
                assemblyLoader);
        }

        public string Key => "Cake";
        public string Language => LanguageNames.CSharp;
        public IEnumerable<string> Extensions { get; } = new[] { ".cake" };

        public void Initalize(IConfiguration configuration)
        {
            Logger.LogInformation($"Detecting CSX files in '{Env.Path}'.");

            // Nothing to do if there are no Cake files
            var allCakeFiles = Directory.GetFiles(Env.Path, "build.cake", SearchOption.TopDirectoryOnly);
            if (allCakeFiles.Length == 0)
            {
                Logger.LogInformation("Could not find any Cake files");
                return;
            }

            //Context.RootPath = Env.Path;
            Logger.LogInformation($"Found {allCakeFiles.Length} Cake files.");

            var cakeLog = new CakeLog(Logger);

            var scriptAnalyzer = new ScriptAnalyzer(
                new FileSystem(), 
                new CakeEnvironment(new CakePlatform(), new CakeRuntime(), cakeLog),
                cakeLog);

            foreach (var cakeFile in allCakeFiles)
            {
                //var analyzeResult = scriptAnalyzer.Analyze(new FilePath(cakeFile));

                Workspace.AddProject(_scriptRunner.CreateProjectInfo(_scripHost, new FilePath(cakeFile), new Dictionary<string, string>()));
            }
        }

        private ProjectInfo CreateProjectInfo(ScriptAnalyzerResult analyzeResult)
        {
            return null;
            //Logger.LogInformation($"Processing script {analyzeResult.Script.Path.FullPath}...");

            //var compilationOptions = new CSharpCompilationOptions(
            //    outputKind: OutputKind.DynamicallyLinkedLibrary,
            //    usings: Context.CsxUsings[csxPath]);

            //// #r references
            //var metadataReferencesDeclaredInCsx = new HashSet<MetadataReference>();
            //foreach (var assemblyReference in processResult.References)
            //{
            //    AddMetadataReference(metadataReferencesDeclaredInCsx, assemblyReference);
            //}

            //Context.CsxReferences[csxPath] = metadataReferencesDeclaredInCsx;
            //Context.CsxLoadReferences[csxPath] =
            //    processResult
            //        .LoadedScripts
            //        .Distinct()
            //        .Except(new[] { csxPath })
            //        .Select(loadedCsxPath => CreateCsxProject(loadedCsxPath))
            //        .ToList();

            //// Create the wrapper project and add it to the workspace
            //Logger.LogDebug($"Creating project for script {csxPath}.");
            //var csxFileName = Path.GetFileName(csxPath);
            //var project = ProjectInfo.Create(
            //    id: ProjectId.CreateNewId(Guid.NewGuid().ToString()),
            //    version: VersionStamp.Create(),
            //    name: csxFileName,
            //    assemblyName: $"{csxFileName}.dll",
            //    language: LanguageNames.CSharp,
            //    compilationOptions: compilationOptions,
            //    parseOptions: CsxParseOptions,
            //    metadataReferences: Context.CommonReferences.Union(Context.CsxReferences[csxPath]),
            //    projectReferences: Context.CsxLoadReferences[csxPath].Select(p => new ProjectReference(p.Id)),
            //    isSubmission: true,
            //    hostObjectType: typeof(InteractiveScriptGlobals));

            //Workspace.AddProject(project);
            //AddFile(csxPath, project.Id);

            ////----------LOG ONLY------------
            //Logger.LogDebug($"All references by {csxFileName}: \n{string.Join("\n", project.MetadataReferences.Select(r => r.Display))}");
            //Logger.LogDebug($"All #load projects by {csxFileName}: \n{string.Join("\n", Context.CsxLoadReferences[csxPath].Select(p => p.Name))}");
            //Logger.LogDebug($"All usings in {csxFileName}: \n{string.Join("\n", (project.CompilationOptions as CSharpCompilationOptions)?.Usings ?? new ImmutableArray<string>())}");
            ////------------------------------

            //// Traversal administration
            //Context.CsxFileProjects[csxPath] = project;
            //Context.CsxFilesBeingProcessed.Remove(csxPath);

            //return project;
        }

        Task<object> IProjectSystem.GetProjectModelAsync(string filePath)
        {
            return Task.FromResult<object>(null);
        }

        Task<object> IProjectSystem.GetWorkspaceModelAsync(WorkspaceInformationRequest request)
        {
            return Task.FromResult<object>(null);
        }
    }
}
