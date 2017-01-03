// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;
using Cake.Core.IO;
using Cake.Core.Reflection;

namespace OmniSharp.Cake
{
    internal sealed class CakeAssemblyLoader : IAssemblyLoader
    {
        private readonly IFileSystem _fileSystem;

        public CakeAssemblyLoader(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public Assembly Load(AssemblyName assemblyName)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException(nameof(assemblyName));
            }

            return Assembly.Load(assemblyName);
        }

        public Assembly Load(FilePath path)
        {
#if NET46
            return Assembly.LoadFrom(path.FullPath);
#else
            
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.Segments.Length == 1 && !_fileSystem.Exist(path))
            {
                // Not a valid path. Try loading it by its name.
                return Assembly.Load(new AssemblyName(path.FullPath));
            }

            var loader = new CakeAssemblyLoadContext(_fileSystem, path.GetDirectory());
            return loader.LoadFromAssemblyPath(path.FullPath);
#endif
        }
    }
}