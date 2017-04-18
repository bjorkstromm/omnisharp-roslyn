using Cake.Core.IO;
using Cake.Core.Scripting;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Cake.OmniSharp.Scripting
{
    public class CakeScript
    {
        public Script Script { get; set; }

        public IEnumerable<MetadataReference> MetadataReferences { get; set; }

        public IEnumerable<string> Usings { get; set; }

        public override string ToString()
        {
            return RoslynCodeGenerator.Generate(Script);
        }

        public int GetLineDirectivePosition(FilePath filePath)
        {
            return Script.Lines.Select((value, index) => new { value, index })
                        .Where(pair => pair.value.Equals($"#line 1 \"{filePath.FullPath}\""))
                        .Select(pair => pair.index)
                        .FirstOrDefault();
        }
    }
}
