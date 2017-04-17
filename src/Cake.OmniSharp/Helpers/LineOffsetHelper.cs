using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cake.Core.IO;
using OmniSharp;

namespace Cake.OmniSharp.Helpers
{
    internal static class LineOffsetHelper
    {
        public static async Task<int> GetOffset(string fileName, OmniSharpWorkspace workspace)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return 0;
            }

            var document = workspace.GetDocument(fileName);
            if (document == null)
            {
                return 0;
            }

            var filePath = new FilePath(fileName);
            var sourceText = await document.GetTextAsync();

            var offset = sourceText.Lines.FirstOrDefault(line => line.ToString().Equals($"#line 1 \"{filePath.FullPath}\"")).LineNumber;

            return offset;
        }
    }
}
