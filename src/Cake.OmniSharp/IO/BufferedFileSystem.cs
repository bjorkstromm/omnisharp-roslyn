using Cake.Core.IO;
using System.Collections.Concurrent;
using System.Composition;

namespace Cake.OmniSharp.IO
{
    [Export(typeof(IFileSystem)), Export(typeof(IBufferedFileSystem)), Shared]
    public class BufferedFileSystem : IBufferedFileSystem
    {
        private readonly IFileSystem _fileSystem;
        private readonly ConcurrentDictionary<string, IFile> _buffer;

        public BufferedFileSystem()
        {
            _fileSystem = new FileSystem();
            _buffer = new ConcurrentDictionary<string, IFile>();
        }

        public IDirectory GetDirectory(DirectoryPath path)
        {
            return _fileSystem.GetDirectory(path);
        }

        public IFile GetFile(FilePath path)
        {
            if(_buffer.TryGetValue(path.FullPath, out IFile file))
            {
                return file;
            }

            return _fileSystem.GetFile(path);
        }

        public void AddOrUpdateFile(FilePath path, string content)
        {
            var file = new BufferedFile(path, content);
            _buffer.AddOrUpdate(path.FullPath, file, (k, v) => file);
        }

        public void RemoveFile(FilePath path)
        {
            _buffer.TryRemove(path.FullPath, out IFile file);
        }
    }
}
