using System.Composition;
using System.Threading.Tasks;
using Cake.OmniSharp.Extensions;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;
using OmniSharp.Roslyn.CSharp.Services.Navigation;

namespace Cake.OmniSharp.RequestHandlers.Navigation
{
    [OmniSharpHandler(OmnisharpEndpoints.FindUsages, Constants.LanguageNames.Cake), Shared]
    public class FindUsagesHandler : CakeRequestHandler<FindUsagesRequest, QuickFixResponse>
    {
        [ImportingConstructor]
        public FindUsagesHandler(
            OmniSharpWorkspace workspace)
            : base(workspace)
        {
        }

        protected override Task<QuickFixResponse> TranslateResponse(QuickFixResponse response, FindUsagesRequest request)
        {
            return response.TranslateAsync(Workspace, request);
        }
    }
}
