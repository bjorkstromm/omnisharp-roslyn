using System.Composition;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;
using OmniSharp.Roslyn.CSharp.Services.Navigation;

namespace Cake.OmniSharp.RequestHandlers.Navigation
{
    [OmniSharpHandler(OmnisharpEndpoints.FindImplementations, Constants.LanguageNames.Cake)]
    public class FindImplementationsHandler : CakeRequestHandler<FindImplementationsService, FindImplementationsRequest, QuickFixResponse>
    {
        [ImportingConstructor]
        public FindImplementationsHandler(OmniSharpWorkspace workspace) 
            : base(workspace, new FindImplementationsService(workspace))
        {
        }
    }
}
