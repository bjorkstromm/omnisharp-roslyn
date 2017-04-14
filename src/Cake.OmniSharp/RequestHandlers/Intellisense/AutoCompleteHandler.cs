using System.Collections.Generic;
using System.Composition;
using OmniSharp;
using OmniSharp.Mef;
using OmniSharp.Models;

namespace Cake.OmniSharp.RequestHandlers.Intellisense
{
    [OmniSharpHandler(OmnisharpEndpoints.AutoComplete, Constants.LanguageNames.Cake), Shared]
    public class AutoCompleteHandler : CakeRequestHandler<AutoCompleteRequest, IEnumerable<AutoCompleteResponse>>
    {
        [ImportingConstructor]
        public AutoCompleteHandler(OmniSharpWorkspace workspace)
            : base(workspace)
        {
        }
    }
}
