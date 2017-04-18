using System.Composition;
using OmniSharp;
using OmniSharp.Mef;
using OmniSharp.Models;

namespace Cake.OmniSharp.RequestHandlers.Types
{
    [OmniSharpHandler(OmnisharpEndpoints.TypeLookup, Constants.LanguageNames.Cake), Shared]
    public class TypeLookupHandler : CakeRequestHandler<TypeLookupRequest, TypeLookupResponse>
    {
        [ImportingConstructor]
        public TypeLookupHandler(OmniSharpWorkspace workspace) : base(workspace)
        {
        }
    }
}
