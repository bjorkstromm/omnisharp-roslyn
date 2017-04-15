using Cake.Core.Scripting;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

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
    }
}
