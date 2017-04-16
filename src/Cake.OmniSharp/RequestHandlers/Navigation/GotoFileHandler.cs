using System.Composition;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;
using OmniSharp.Roslyn.CSharp.Services.Navigation;

namespace Cake.OmniSharp.RequestHandlers.Navigation
{
    [OmniSharpHandler(OmnisharpEndpoints.GotoFile, Constants.LanguageNames.Cake), Shared]
    public class GotoFileHandler : CakeRequestHandler<GotoFileRequest, QuickFixResponse>
    {
        [ImportingConstructor]
        public GotoFileHandler(
            OmniSharpWorkspace workspace)
            : base(workspace)
        {
        }
    }
}
