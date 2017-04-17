using System.Composition;
using System.Threading.Tasks;
using Cake.OmniSharp.Extensions;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;
using OmniSharp.Roslyn.CSharp.Services.Navigation;

namespace Cake.OmniSharp.RequestHandlers.Navigation
{
    [OmniSharpHandler(OmnisharpEndpoints.FindImplementations, Constants.LanguageNames.Cake), Shared]
    public class FindImplementationsHandler : CakeRequestHandler<FindImplementationsRequest, QuickFixResponse>
    {
        [ImportingConstructor]
        public FindImplementationsHandler(OmniSharpWorkspace workspace) 
            : base(workspace)
        {
        }

        protected override Task<QuickFixResponse> TranslateResponse(QuickFixResponse response, FindImplementationsRequest request)
        {
            return response.TranslateAsync(Workspace, request);
        }
    }
}
