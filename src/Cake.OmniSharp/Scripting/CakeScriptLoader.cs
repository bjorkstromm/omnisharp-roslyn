using Microsoft.CodeAnalysis;
using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.CodeAnalysis.Text;
using System.IO;

namespace Cake.OmniSharp.Scripting
{
    class CakeScriptLoader : TextLoader
    {
        private readonly string _filePath;
        private readonly CakeScriptGenerator _generator;

        public CakeScriptLoader(string filePath, CakeScriptGenerator generator)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!Path.IsPathRooted(filePath))
            {
                throw new ArgumentException("Expected an absolute file path", nameof(filePath));
            }

            _filePath = filePath;
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        }

        public override Task<TextAndVersion> LoadTextAndVersionAsync(Workspace workspace, DocumentId documentId, CancellationToken cancellationToken)
        {
            var prevLastWriteTime = File.GetLastWriteTimeUtc(_filePath);

            TextAndVersion textAndVersion;

            var script = _generator.GetCakeScript(_filePath);
            var code = RoslynCodeGenerator.Generate(script.Script);
            var version = VersionStamp.Create(prevLastWriteTime);
            var text = SourceText.From(code);
            textAndVersion = TextAndVersion.Create(text, version, _filePath);

            var newLastWriteTime = File.GetLastWriteTimeUtc(_filePath);
            if (!newLastWriteTime.Equals(prevLastWriteTime))
            {
                throw new IOException($"File was externally modified: {_filePath}");
            }

            return Task.FromResult(textAndVersion);
        }
    }
}
