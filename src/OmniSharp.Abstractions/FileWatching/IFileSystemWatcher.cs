using System;
using OmniSharp.Models.V2;

namespace OmniSharp.FileWatching
{
    // TODO: Flesh out this API more
    public interface IFileSystemWatcher
    {
        void Watch(string pattern, Action<FileChangedRequest> callback);

        void TriggerChange(FileChangedRequest request);
    }
}
