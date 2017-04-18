using System.Composition;
using System.Threading.Tasks;
using Cake.OmniSharp.Extensions;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;

namespace Cake.OmniSharp.RequestHandlers.Navigation
{
    [OmniSharpHandler(OmnisharpEndpoints.GotoRegion, Constants.LanguageNames.Cake), Shared]
    public class GotoRegionHandler : CakeRequestHandler<GotoRegionRequest, QuickFixResponse>
    {
        [ImportingConstructor]
        public GotoRegionHandler(OmniSharpWorkspace workspace) : base(workspace)
        {
        }

        protected override Task<QuickFixResponse> TranslateResponse(QuickFixResponse response, GotoRegionRequest request)
        {
            return response.TranslateAsync(Workspace, request);
        }
    }
}
