using OmniSharp;
using OmniSharp.Mef;
using OmniSharp.Roslyn.CSharp.Services.Structure;
using System.Composition;
using OmniSharp.Models;
using System;
using System.Collections.Generic;
using OmniSharp.Abstractions.Services;

namespace Cake.OmniSharp.RequestHandlers.Structure
{
    [OmniSharpHandler(OmnisharpEndpoints.MembersTree, Constants.LanguageNames.Cake)]
    public class MembersAsTreeHandler : CakeRequestHandler<MembersAsTreeService, MembersTreeRequest, FileMemberTree>
    {
        [ImportingConstructor]
        public MembersAsTreeHandler(
            OmniSharpWorkspace workspace,
            [ImportMany] IEnumerable<ISyntaxFeaturesDiscover> featureDiscovers) 
            : base(workspace, new MembersAsTreeService(workspace, featureDiscovers))
        {
        }
    }
}
