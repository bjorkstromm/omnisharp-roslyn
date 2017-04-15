using System.Composition;
﻿using System.Threading.Tasks;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;
using Cake.OmniSharp.Scripting;

namespace Cake.OmniSharp.RequestHandlers.Buffer
{
    [OmniSharpHandler(OmnisharpEndpoints.ChangeBuffer, Constants.LanguageNames.Cake)]
    public class ChangeBufferHandler : RequestHandler<ChangeBufferRequest, object>
    {
        private readonly OmniSharpWorkspace _workspace;
        private readonly ICakeScriptGenerator _generator;

        [ImportingConstructor]
        public ChangeBufferHandler(OmniSharpWorkspace workspace, ICakeScriptGenerator generator)
        {
            _workspace = workspace;
            _generator = generator;
        }

        public async Task<object> Handle(ChangeBufferRequest request)
        {
            if (request.FileName == null)
            {
                return true;
            }

            var documentIds = _workspace.CurrentSolution.GetDocumentIdsWithFilePath(request.FileName);
            if (!documentIds.IsEmpty)
            {

            }
            else
            {
                var script = _generator.Generate(request.FileName);
                request.NewText = script.ToString();
            }

            await _workspace.BufferManager.UpdateBuffer(request);
            return true;
        }
    }
}
