using System.Composition;
﻿using System.Threading.Tasks;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;

namespace Cake.OmniSharp.RequestHandlers.Buffer
{
    [OmniSharpHandler(OmnisharpEndpoints.ChangeBuffer, Constants.LanguageNames.Cake)]
    public class ChangeBufferHandler : RequestHandler<ChangeBufferRequest, object>
    {
        private OmniSharpWorkspace _workspace;

        [ImportingConstructor]
        public ChangeBufferHandler(OmniSharpWorkspace workspace)
        {
            _workspace = workspace;
        }

        public async Task<object> Handle(ChangeBufferRequest request)
        {
            // Todo: Codegen
            await _workspace.BufferManager.UpdateBuffer(request);
            return true;
        }
    }
}
