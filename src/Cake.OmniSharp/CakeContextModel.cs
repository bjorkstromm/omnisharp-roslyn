using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Cake.OmniSharp
{
    public class CakeContextModel
    {
        public CakeContextModel(string cakePath, ProjectInfo project, HashSet<string> implicitAssemblyReferences)
        {
            Path = cakePath;
            ImplicitAssemblyReferences = implicitAssemblyReferences;
            CommonUsings = new List<string>();//CakeProjectSystem.DefaultNamespaces;
            GlobalsType = project.HostObjectType;
        }

        public string Path { get; }

        public HashSet<string> ImplicitAssemblyReferences { get; }

        public Type GlobalsType { get; }

        public IEnumerable<string> CommonUsings { get; }
    }
}