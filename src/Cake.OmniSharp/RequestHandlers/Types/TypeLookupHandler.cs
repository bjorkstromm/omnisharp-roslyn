using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniSharp;
using OmniSharp.Mef;
using OmniSharp.Models;
using Cake.OmniSharp.Extensions;

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
