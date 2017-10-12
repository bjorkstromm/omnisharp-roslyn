using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using OmniSharp.FileWatching;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp.Models.V2;

namespace OmniSharp.Roslyn.CSharp.Services.Files.V2
{
    [OmniSharpHandler(OmniSharpEndpoints.V2.FilesChanged, LanguageNames.CSharp)]
    public class FilesChangedService : IRequestHandler<IEnumerable<FileChangedRequest>, FilesChangedResponse>
    {
        private readonly IFileSystemWatcher _watcher;

        [ImportingConstructor]
        public FilesChangedService(IFileSystemWatcher watcher)
        {
            _watcher = watcher;
        }

        public Task<FilesChangedResponse> Handle(IEnumerable<FileChangedRequest> requests)
        {
            foreach (var request in requests)
            {
                _watcher.TriggerChange(request);
            }
            return Task.FromResult(new FilesChangedResponse());
        }
    }
}
