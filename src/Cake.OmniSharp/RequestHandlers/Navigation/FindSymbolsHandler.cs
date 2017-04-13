using System.Composition;
using System.Threading.Tasks;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;
using OmniSharp.Roslyn.CSharp.Services.Navigation;

namespace Cake.OmniSharp.RequestHandlers.Navigation
{
    [OmniSharpHandler(OmnisharpEndpoints.FindSymbols, Constants.LanguageNames.Cake)]
    public class FindSymbolsHandler : CakeRequestHandler<FindSymbolsService, FindSymbolsRequest, QuickFixResponse>
    {
        [ImportingConstructor]
        public FindSymbolsHandler(OmniSharpWorkspace workspace)
            : base(workspace, new FindSymbolsService(workspace))
        {
        }
    }
}
