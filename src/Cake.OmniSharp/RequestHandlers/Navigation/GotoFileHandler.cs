using System.Composition;
using System.Threading.Tasks;
using Cake.OmniSharp.Extensions;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;

namespace Cake.OmniSharp.RequestHandlers.Navigation
{
    [OmniSharpHandler(OmnisharpEndpoints.GotoFile, Constants.LanguageNames.Cake), Shared]
    public class GotoFileHandler : CakeRequestHandler<GotoFileRequest, QuickFixResponse>
    {
        [ImportingConstructor]
        public GotoFileHandler(
            OmniSharpWorkspace workspace)
            : base(workspace)
        {
        }

        protected override Task<QuickFixResponse> TranslateResponse(QuickFixResponse response, GotoFileRequest request)
        {
            return response.TranslateAsync(Workspace, request);
        }
    }
}
