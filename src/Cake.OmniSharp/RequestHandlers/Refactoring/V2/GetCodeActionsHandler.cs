using Microsoft.Extensions.Logging;
using OmniSharp;
using OmniSharp.Mef;
using OmniSharp.Models.V2;
using OmniSharp.Roslyn.CSharp.Services.CodeActions;
using OmniSharp.Roslyn.CSharp.Services.Refactoring.V2;
using OmniSharp.Services;
using System.Collections.Generic;
using System.Composition;

namespace Cake.OmniSharp.RequestHandlers.Refactoring.V2
{
    [OmniSharpHandler(OmnisharpEndpoints.V2.GetCodeActions, Constants.LanguageNames.Cake)]
    public class GetCodeActionsHandler : CakeRequestHandler<GetCodeActionsService, GetCodeActionsRequest, GetCodeActionsResponse>
    {
        [ImportingConstructor]
        public GetCodeActionsHandler(
            OmniSharpWorkspace workspace,
            CodeActionHelper helper,
            [ImportMany] IEnumerable<ICodeActionProvider> providers,
            ILoggerFactory loggerFactory) 
            : base(workspace, new GetCodeActionsService(workspace, helper, providers, loggerFactory))
        {
        }
    }
}
