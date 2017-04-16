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
    [OmniSharpHandler(OmnisharpEndpoints.MembersTree, Constants.LanguageNames.Cake), Shared]
    public class MembersAsTreeHandler : CakeRequestHandler<MembersTreeRequest, FileMemberTree>
    {
        [ImportingConstructor]
        public MembersAsTreeHandler(
            OmniSharpWorkspace workspace) 
            : base(workspace)
        {
        }
    }
}
