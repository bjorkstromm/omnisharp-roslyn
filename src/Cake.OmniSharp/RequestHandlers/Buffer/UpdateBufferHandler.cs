using System.Composition;
using System.Threading.Tasks;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;
using Cake.OmniSharp.IO;
using Cake.OmniSharp.Scripting;

namespace Cake.OmniSharp.RequestHandlers.Buffer
{
    [OmniSharpHandler(OmnisharpEndpoints.UpdateBuffer, Constants.LanguageNames.Cake)]
    public class UpdateBufferHandler : RequestHandler<UpdateBufferRequest, object>
    {
        private readonly OmniSharpWorkspace _workspace;
        private readonly ICakeScriptGenerator _generator;
        private readonly IBufferedFileSystem _fileSystem;
        private readonly CakeDocumentationProvider _documentationProvider;

        [ImportingConstructor]
        public UpdateBufferHandler(
            OmniSharpWorkspace workspace,
            IBufferedFileSystem fileSystem,
            ICakeScriptGenerator generator,
            CakeDocumentationProvider provider)
        {
            _workspace = workspace;
            _generator = generator;
            _fileSystem = fileSystem;
            _documentationProvider = provider;
        }

        public async Task<object> Handle(UpdateBufferRequest request)
        {
            if (request.FileName == null)
            {
                return true;
            }

            // Avoid having buffer manager reading from disk
            if (request.FromDisk)
            {
                _fileSystem.RemoveFile(request.FileName);
                request.FromDisk = false;
            }
            else if(request.Changes == null)
            {
                _fileSystem.AddOrUpdateFile(request.FileName, request.Buffer);
            }

            var script = _generator.Generate(request.FileName);
            request.Buffer = script.ToString(_documentationProvider);
            var offset = script.GetLineDirectivePosition(request.FileName) + 1;

            if(request.Changes != null)
            {
                foreach(var change in request.Changes)
                {
                    change.StartLine += offset;
                    change.EndLine += offset;
                }
            }

            await _workspace.BufferManager.UpdateBuffer(request);
            return true;
        }
    }
}
