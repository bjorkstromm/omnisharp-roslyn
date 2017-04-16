using Cake.Core.IO;

namespace Cake.OmniSharp.IO
{
    public interface IBufferedFileSystem : IFileSystem
    {
        void AddOrUpdateFile(FilePath path, string content);

        void RemoveFile(FilePath path);
    }
}
