using System.Composition;
using System.Threading.Tasks;
using Cake.OmniSharp.Extensions;
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

        protected override Task<NavigateResponse> TranslateResponse(NavigateResponse response, NavigateUpRequest request)
        {
            return response.TranslateAsync(Workspace, request);
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

        protected override Task<NavigateResponse> TranslateResponse(NavigateResponse response, NavigateDownRequest request)
        {
            return response.TranslateAsync(Workspace, request);
        }
    }
}
