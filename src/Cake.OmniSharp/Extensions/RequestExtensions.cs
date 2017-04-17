using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniSharp;
using OmniSharp.Models;
using Cake.Core.IO;
using Cake.OmniSharp.Helpers;

namespace Cake.OmniSharp.Extensions
{
    internal static class RequestExtensions
    {
        public static async Task<TRequest> TranslateAsync<TRequest>(this TRequest request, OmniSharpWorkspace workspace) where TRequest : Request
        {
            var offset = await GetOffset(request.FileName, workspace);

            request.Line += offset;

            return request;
        }

        private static async Task<int> GetOffset(string fileName, OmniSharpWorkspace workspace)
        {
            return await LineOffsetHelper.GetOffset(fileName, workspace) + 1;
        }
    }
}
