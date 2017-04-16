using System.Composition;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;
using OmniSharp.Roslyn.CSharp.Services.Navigation;

namespace Cake.OmniSharp.RequestHandlers.Navigation
{
    [OmniSharpHandler(OmnisharpEndpoints.GotoRegion, Constants.LanguageNames.Cake), Shared]
    public class GotoRegionHandler : CakeRequestHandler<GotoRegionRequest, QuickFixResponse>
    {
        [ImportingConstructor]
        public GotoRegionHandler(OmniSharpWorkspace workspace) : base(workspace)
        {
        }
    }
}
