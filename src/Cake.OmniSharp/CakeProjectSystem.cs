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
using Cake.OmniSharp;
using Cake.Core.Diagnostics;
using Cake.Core;
using Cake.Core.IO;
using Cake.OmniSharp.Reflection;
using Cake.OmniSharp.Configuration;
using OmniSharp;
using Cake.OmniSharp.Scripting;

namespace Cake.OmniSharp
{
    [Export(typeof(IProjectSystem)), Shared]
    public class CakeProjectSystem : IProjectSystem
    {
        public string Key => "Cake";
        public string Language => LanguageNames.CSharp;
        public IEnumerable<string> Extensions => new[] { ".cake" };

        private readonly IMetadataFileReferenceCache _metadataFileReferenceCache;

        // used for tracking purposes only
        private readonly HashSet<string> _assemblyReferences = new HashSet<string>();

        private readonly Dictionary<string, ProjectInfo> _projects;
        private readonly OmniSharpWorkspace _workspace;
        private readonly IOmniSharpEnvironment _env;
        private readonly ILogger _logger;

        private readonly ICakeContext _cakeContext;
        private readonly CakeScriptRunner _scriptRunner;
        private readonly CakeScriptHost _scripHost;

        [ImportingConstructor]
        public CakeProjectSystem(OmniSharpWorkspace workspace, IOmniSharpEnvironment env, ILoggerFactory loggerFactory, IMetadataFileReferenceCache metadataFileReferenceCache)
        {
            _metadataFileReferenceCache = metadataFileReferenceCache;
            _workspace = workspace;
            _env = env;
            _logger = loggerFactory.CreateLogger<CakeProjectSystem>();
            _projects = new Dictionary<string, ProjectInfo>();

            _cakeContext = CakeContextFactory.CreateContext(_logger);
            _scripHost = new CakeScriptHost(_cakeContext);
            _scriptRunner = new CakeScriptRunner(_cakeContext);
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
            _logger.LogInformation($"Detecting build.cake in '{_env.Path}'.");

            // Nothing to do if there is no build.cake
            var cakePath = System.IO.Path.Combine(_env.Path, "build.cake");
            if (!File.Exists(cakePath))
            {
                _logger.LogInformation("Could not find build.cake");
                return;
            }

            _logger.LogInformation($"Found {cakePath}");

            var projectAndCode = _scriptRunner.CreateProjectInfo(_scripHost, new FilePath(cakePath), new Dictionary<string, string>());
            var project = projectAndCode.Item1;
            var code = projectAndCode.Item2;

            // add Cake project to workspace
            _workspace.AddProject(project);
            var documentId = DocumentId.CreateNewId(project.Id);
            var loader = new CakeTextLoader(cakePath, code);
            var documentInfo = DocumentInfo.Create(documentId, cakePath, filePath: cakePath, loader: loader, sourceCodeKind: SourceCodeKind.Script);
            _workspace.AddDocument(documentInfo);
            //_workspace.AddDocument(project.Id, cakePath, SourceCodeKind.Script);
            _projects[cakePath] = project;
            _logger.LogInformation($"Added Cake project '{cakePath}' to the workspace.");
        }

        private ProjectInfo GetProjectFileInfo(string path)
        {
            ProjectInfo projectFileInfo;
            if (!_projects.TryGetValue(path, out projectFileInfo))
            {
                if (!_projects.TryGetValue(path.Replace("/","\\"), out projectFileInfo))
                {
                    return null;
                }
            }

            return projectFileInfo;
        }
    }
}
