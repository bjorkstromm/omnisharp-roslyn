using OmniSharp;
using OmniSharp.Mef;
using OmniSharp.Models;
using System.Composition;

namespace Cake.OmniSharp.RequestHandlers.Diagnostics
{
    [OmniSharpHandler(OmnisharpEndpoints.CodeCheck, Constants.LanguageNames.Cake), Shared]
    public class CodeCheckHandler : CakeRequestHandler<CodeCheckRequest, QuickFixResponse>
    {
        [ImportingConstructor]
        public CodeCheckHandler(
            OmniSharpWorkspace workspace)
            : base(workspace, false)
        {
        }
    }
}
