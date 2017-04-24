using System.Composition;
using OmniSharp;
using OmniSharp.Mef;
using OmniSharp.Models;

namespace Cake.OmniSharp.RequestHandlers.Signatures
{
    [OmniSharpHandler(OmnisharpEndpoints.SignatureHelp, Constants.LanguageNames.Cake), Shared]
    public class SignatureHelpHandler : CakeRequestHandler<SignatureHelpRequest, SignatureHelp>
    {
        [ImportingConstructor]
        public SignatureHelpHandler(OmniSharpWorkspace workspace) : base(workspace)
        {
        }
    }
}
