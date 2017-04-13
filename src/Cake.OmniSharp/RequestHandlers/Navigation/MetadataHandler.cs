using System.Composition;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp;
using OmniSharp.Roslyn.CSharp.Services.Navigation;
using OmniSharp.Roslyn;

namespace Cake.OmniSharp.RequestHandlers.Navigation
{
    [OmniSharpHandler(OmnisharpEndpoints.Metadata, Constants.LanguageNames.Cake)]
    public class MetadataHandler : CakeRequestHandler<MetadataService, MetadataRequest, MetadataResponse>
    {
        [ImportingConstructor]
        public MetadataHandler(OmniSharpWorkspace workspace, MetadataHelper metadataHelper)
            : base(workspace, new MetadataService(workspace, metadataHelper))
        {
        }
    }
}
