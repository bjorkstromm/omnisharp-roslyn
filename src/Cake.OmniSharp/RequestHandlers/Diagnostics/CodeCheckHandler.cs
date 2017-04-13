using OmniSharp;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp.Roslyn.CSharp.Services.Diagnostics;
using System.Composition;

namespace Cake.OmniSharp.RequestHandlers.Diagnostics
{
    [OmniSharpHandler(OmnisharpEndpoints.CodeCheck, Constants.LanguageNames.Cake)]
    public class CodeCheckHandler : CakeRequestHandler<CodeCheckService, CodeCheckRequest, QuickFixResponse>
    {
        [ImportingConstructor]
        public CodeCheckHandler(
            OmniSharpWorkspace workspace)
            : base(workspace, new CodeCheckService(workspace))
        {
        }
    }
}
