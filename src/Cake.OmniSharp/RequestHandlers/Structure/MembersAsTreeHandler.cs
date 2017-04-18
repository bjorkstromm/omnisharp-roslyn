using OmniSharp;
using OmniSharp.Mef;
using System.Composition;
using OmniSharp.Models;
using System.Threading.Tasks;
using Cake.OmniSharp.Extensions;

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

        protected override Task<FileMemberTree> TranslateResponse(FileMemberTree response, MembersTreeRequest request)
        {
            return response.TranslateAsync(Workspace, request);
        }
    }
}
