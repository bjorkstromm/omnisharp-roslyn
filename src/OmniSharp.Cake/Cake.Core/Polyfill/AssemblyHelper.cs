// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;
using Cake.Core.IO;
using Cake.Core.Reflection;
using Cake.Core;
using OmniSharp.Cake.Core.Reflection;

namespace OmniSharp.Cake.Core.Polyfill
{
    internal static class AssemblyHelper
    {
        public static Assembly GetExecutingAssembly()
        {
#if !NET46
            return typeof(CakeEnvironment).GetTypeInfo().Assembly;
#else
            return Assembly.GetExecutingAssembly();
#endif
        }

        public static Assembly LoadAssembly(IFileSystem fileSystem, FilePath path)
        {
#if !NET46
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.Segments.Length == 1 && !fileSystem.Exist(path))
            {
                // Not a valid path. Try loading it by its name.
                return Assembly.Load(new AssemblyName(path.FullPath));
            }

            var loader = new CakeAssemblyLoadContext(fileSystem, path.GetDirectory());
            return loader.LoadFromAssemblyPath(path.FullPath);
#else
            return Assembly.LoadFrom(path.FullPath);
#endif
        }
    }
}
