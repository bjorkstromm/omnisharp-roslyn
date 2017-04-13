using System.Composition;
using System.Threading.Tasks;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;

namespace Cake.OmniSharp.RequestHandlers.Buffer
{
    [OmniSharpHandler(OmnisharpEndpoints.UpdateBuffer, Constants.LanguageNames.Cake)]
    public class UpdateBufferHandler : RequestHandler<UpdateBufferRequest, object>
    {
        private OmniSharpWorkspace _workspace;

        [ImportingConstructor]
        public UpdateBufferHandler(OmniSharpWorkspace workspace)
        {
            _workspace = workspace;
        }

        public async Task<object> Handle(UpdateBufferRequest request)
        {
            await _workspace.BufferManager.UpdateBuffer(request);
            return true;
        }
    }
}
