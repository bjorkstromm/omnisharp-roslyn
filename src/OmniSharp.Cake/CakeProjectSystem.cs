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
using OmniSharp.Cake.Internal;
using Cake.Core.Diagnostics;
using Cake.Core;
using Cake.Core.IO;
using OmniSharp.Cake.Core.Reflection;
using OmniSharp.Cake.Core.Configuration;

namespace OmniSharp.Cake
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
            throw new NotImplementedException();
        }

        public Task<object> GetWorkspaceModelAsync(WorkspaceInformationRequest request)
        {
            throw new NotImplementedException();
        }

        public void Initalize(IConfiguration configuration)
        {
            _logger.LogInformation($"Detecting CSX files in '{_env.Path}'.");

            // Nothing to do if there are no CSX files
            var allCakeFiles = Directory.GetFiles(_env.Path, "*.cake", SearchOption.AllDirectories);
            if (allCakeFiles.Length == 0)
            {
                _logger.LogInformation("Could not find any Cake files");
                return;
            }

            _logger.LogInformation($"Found {allCakeFiles.Length} Cake files.");


        }
    }
}
