// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cake.Core;
using Cake.Core.IO;
using Cake.Core.Scripting;
using Cake.Core.Scripting.CodeGen;
using Microsoft.CodeAnalysis;

namespace Cake.OmniSharp.Scripting
{
    internal static class RoslynCodeGenerator
    {
        public static string Generate(Script context, CakeDocumentationProvider documentationProvider)
        {
            var usingDirectives = string.Join("\r\n", context.UsingAliasDirectives);
            var aliases = GetAliasCode(context, documentationProvider);
            var code = string.Join("\r\n", context.Lines);
            return string.Join("\r\n", usingDirectives, aliases, code);
        }

        private static string GetAliasCode(Script context, CakeDocumentationProvider documentationProvider)
        {
            var result = new List<string>();

            foreach (var alias in context.Aliases)
            {
                var documentation = documentationProvider.Generate(alias);

                var code = alias.Type == ScriptAliasType.Method
                    ? MethodAliasGenerator.Generate(alias.Method)
                    : PropertyAliasGenerator.Generate(alias.Method);

                result.Add(documentation + "\r\n" + code);
            }
            return string.Join("\r\n", result);
        }
    }
}