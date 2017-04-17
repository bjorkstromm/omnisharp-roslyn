using System.Composition;
using System.Threading.Tasks;
using Cake.OmniSharp.Extensions;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;
using OmniSharp.Roslyn.CSharp.Services.Navigation;
using OmniSharp.Roslyn;

namespace Cake.OmniSharp.RequestHandlers.Navigation
{
    [OmniSharpHandler(OmnisharpEndpoints.GotoDefinition, Constants.LanguageNames.Cake), Shared]
    public class GotoDefinitionHandler : CakeRequestHandler<GotoDefinitionRequest, GotoDefinitionResponse>
    {
        [ImportingConstructor]
        public GotoDefinitionHandler(
            OmniSharpWorkspace workspace)
            : base(workspace)
        {
        }

        protected override Task<GotoDefinitionResponse> TranslateResponse(GotoDefinitionResponse response, GotoDefinitionRequest request)
        {
            return response.TranslateAsync(Workspace);
        }
    }
}