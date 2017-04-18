using System.Composition;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;

namespace Cake.OmniSharp.RequestHandlers.Navigation
{
    [OmniSharpHandler(OmnisharpEndpoints.Metadata, Constants.LanguageNames.Cake), Shared]
    public class MetadataHandler : CakeRequestHandler<MetadataRequest, MetadataResponse>
    {
        [ImportingConstructor]
        public MetadataHandler(OmniSharpWorkspace workspace)
            : base(workspace)
        {
        }
    }
}
