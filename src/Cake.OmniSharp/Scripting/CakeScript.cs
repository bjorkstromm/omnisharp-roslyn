using Cake.Core.Scripting;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake.OmniSharp.Scripting
{
    internal class CakeScript
    {
        public Script Script { get; set; }

        public IEnumerable<MetadataReference> MetadataReferences { get; set; }

        public IEnumerable<string> Usings { get; set; }
    }
}
