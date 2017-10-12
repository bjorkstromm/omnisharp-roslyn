using System.Collections.Generic;
using OmniSharp.Mef;

namespace OmniSharp.Models.V2
{
    [OmniSharpEndpoint(OmniSharpEndpoints.V2.FilesChanged, typeof(IEnumerable<FileChangedRequest>), typeof(FilesChangedResponse))]
    public class FileChangedRequest : Request
    {
        public FileChangedAction Action { get; set; }
    }
}
