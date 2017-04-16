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
    [OmniSharpHandler(OmnisharpEndpoints.V2.GetCodeActions, Constants.LanguageNames.Cake), Shared]
    public class GetCodeActionsHandler : CakeRequestHandler<GetCodeActionsRequest, GetCodeActionsResponse>
    {
        [ImportingConstructor]
        public GetCodeActionsHandler(
            OmniSharpWorkspace workspace) 
            : base(workspace)
        {
        }
    }
}
