using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis.Text;
using OmniSharp;
using OmniSharp.Extensions;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp.Options;
using OmniSharp.Roslyn.CSharp.Services.Documentation;
using OmniSharp.Roslyn.CSharp.Services.Intellisense;

namespace Cake.OmniSharp.RequestHandlers.Intellisense
{
    [OmniSharpHandler(OmnisharpEndpoints.AutoComplete, Constants.LanguageNames.Cake)]
    public class AutoCompleteHandler : CakeRequestHandler<IntellisenseService, AutoCompleteRequest, IEnumerable<AutoCompleteResponse>>
    {
        [ImportingConstructor]
        public AutoCompleteHandler(OmniSharpWorkspace workspace, FormattingOptions formattingOptions)
            : base(workspace, new IntellisenseService(workspace, formattingOptions))
        {
        }
    }
}
