using System.Composition;
using System.Threading.Tasks;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;
using OmniSharp.Roslyn.CSharp.Services.Navigation;
using OmniSharp.Roslyn;

namespace Cake.OmniSharp.RequestHandlers.Navigation
{
    [OmniSharpHandler(OmnisharpEndpoints.GotoDefinition, Constants.LanguageNames.Cake)]
    public class GotoDefinitionHandler : CakeRequestHandler<GotoDefinitionService, GotoDefinitionRequest, GotoDefinitionResponse>
    {
        [ImportingConstructor]
        public GotoDefinitionHandler(
            OmniSharpWorkspace workspace,
            MetadataHelper metadataHelper)
            : base(workspace, new GotoDefinitionService(workspace, metadataHelper))
        {
        }
    }
}