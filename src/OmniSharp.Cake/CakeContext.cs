using System;
using System.Collections.Generic;
using System.Composition;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace OmniSharp.Cake
{
    [Export, Shared]
    public class CakeContext
    {
        public HashSet<string> CakeFilesBeingProcessed { get; } = new HashSet<string>();

        // All of the followings are keyed with the file path
        // Each .cake file is wrapped into a project
        public Dictionary<string, ProjectInfo> CakeFileProjects { get; } = new Dictionary<string, ProjectInfo>();
        public Dictionary<string, HashSet<MetadataReference>> CakeReferences { get; } = new Dictionary<string, HashSet<MetadataReference>>();
        public Dictionary<string, List<ProjectInfo>> CakeLoadReferences { get; } = new Dictionary<string, List<ProjectInfo>>();
        public Dictionary<string, List<string>> CakeUsings { get; } = new Dictionary<string, List<string>>();
        public HashSet<MetadataReference> CommonReferences { get; } = new HashSet<MetadataReference>();
        public HashSet<string> CommonUsings { get; } = new HashSet<string> { "System" };
        public string RootPath { get; set; }
    }
}
