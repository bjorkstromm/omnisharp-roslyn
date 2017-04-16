using System.Composition;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;
using OmniSharp.Roslyn.CSharp.Services.Navigation;

namespace Cake.OmniSharp.RequestHandlers.Navigation
{
    [OmniSharpHandler(OmnisharpEndpoints.NavigateUp, Constants.LanguageNames.Cake), Shared]
    public class NavigateUpHandler : CakeRequestHandler<NavigateUpRequest, NavigateResponse>
    {
        [ImportingConstructor]
        public NavigateUpHandler(OmniSharpWorkspace workspace)
            : base(workspace)
        {
        }
    }

    [OmniSharpHandler(OmnisharpEndpoints.NavigateDown, Constants.LanguageNames.Cake), Shared]
    public class NavigateDownHandler : CakeRequestHandler<NavigateDownRequest, NavigateResponse>
    {
        [ImportingConstructor]
        public NavigateDownHandler(OmniSharpWorkspace workspace)
            : base(workspace)
        {
        }
    }
}
