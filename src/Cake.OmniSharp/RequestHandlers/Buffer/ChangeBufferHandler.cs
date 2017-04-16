using System.Composition;
﻿using System.Threading.Tasks;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;
using Cake.OmniSharp.Scripting;
using Cake.OmniSharp.IO;

namespace Cake.OmniSharp.RequestHandlers.Buffer
{
    [OmniSharpHandler(OmnisharpEndpoints.ChangeBuffer, Constants.LanguageNames.Cake)]
    public class ChangeBufferHandler : RequestHandler<ChangeBufferRequest, object>
    {
        private readonly OmniSharpWorkspace _workspace;
        private readonly ICakeScriptGenerator _generator;
        private readonly IBufferedFileSystem _fileSystem;

        [ImportingConstructor]
        public ChangeBufferHandler(
            OmniSharpWorkspace workspace,
            IBufferedFileSystem fileSystem,
            ICakeScriptGenerator generator)
        {
            _workspace = workspace;
            _generator = generator;
            _fileSystem = fileSystem;
        }

        public async Task<object> Handle(ChangeBufferRequest request)
        {
            if (request.FileName == null)
            {
                return true;
            }

            var documentIds = _workspace.CurrentSolution.GetDocumentIdsWithFilePath(request.FileName);
            if (documentIds.IsEmpty)
            {
                _fileSystem.AddOrUpdateFile(request.FileName, request.NewText);
            }

            var script = _generator.Generate(request.FileName);

            var offset = script.GetLineDirectivePosition(request.FileName) + 1;
            request.StartLine += offset;
            request.EndLine += offset;

            await _workspace.BufferManager.UpdateBuffer(request);
            return true;
        }
    }
}
