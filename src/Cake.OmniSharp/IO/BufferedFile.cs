using Cake.Core.IO;
using System.IO;

namespace Cake.OmniSharp.IO
{
    internal class BufferedFile : IFile
    {
        private readonly string _content;
        private readonly FilePath _filePath;

        public BufferedFile(FilePath filePath, string content)
        {
            _filePath = filePath;
            _content = content;
        }

        public FilePath Path => _filePath;

        public long Length => _content.Length;

        public bool Exists => true;

        public bool Hidden => false;

        Core.IO.Path IFileSystemInfo.Path => _filePath;

        public void Copy(FilePath destination, bool overwrite)
        {
            return;
        }

        public void Delete()
        {
            return;
        }

        public void Move(FilePath destination)
        {
            return;
        }

        public Stream Open(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(_content);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }
    }
}
