using System.Composition;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;
using OmniSharp.Roslyn.CSharp.Services.Navigation;

namespace Cake.OmniSharp.RequestHandlers.Navigation
{
    [OmniSharpHandler(OmnisharpEndpoints.NavigateUp, Constants.LanguageNames.Cake)]
    public class NavigateUpHandler : CakeRequestHandler<NavigateUpService, NavigateUpRequest, NavigateResponse>
    {
        [ImportingConstructor]
        public NavigateUpHandler(OmniSharpWorkspace workspace)
            : base(workspace, new NavigateUpService(workspace))
        {
        }
    }

    [OmniSharpHandler(OmnisharpEndpoints.NavigateDown, Constants.LanguageNames.Cake)]
    public class NavigateDownHandler : CakeRequestHandler<NavigateDownService, NavigateDownRequest, NavigateResponse>
    {
        [ImportingConstructor]
        public NavigateDownHandler(OmniSharpWorkspace workspace)
            : base(workspace, new NavigateDownService(workspace))
        {
        }
    }
}
