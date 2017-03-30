// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/////////////////////////////////////////////////////////////////////////////////////////////////////
// NOTE: Portions of this code was taken from the ScriptCS project
// which is licensed under the MIT license. https://github.com/scriptcs/scriptcs
/////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
//using System.Runtime.Remoting.Contexts;
//using System.Runtime.Loader;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Reflection;
using Cake.Core.Scripting;
//using Cake.Scripting.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;

using Script = global::Cake.Core.Scripting.Script;

namespace OmniSharp.Cake.Internal
{
    internal sealed class CakeXPlatScriptSession : IScriptSession
    {
        private const string CompiledType = "Submission#0";
        private const string CompiledMethod = "<Factory>";

        private readonly IScriptHost _host;
        private readonly ICakeLog _log;
        private readonly IAssemblyLoader _loader;

        public HashSet<FilePath> ReferencePaths { get; }

        public HashSet<Assembly> References { get; }

        public HashSet<string> Namespaces { get; }

        public CakeXPlatScriptSession(IScriptHost host, IAssemblyLoader loader, ICakeLog log)
        {
            _host = host;
            _log = log;
            _loader = loader;

            ReferencePaths = new HashSet<FilePath>(PathComparer.Default);
            References = new HashSet<Assembly>();
            Namespaces = new HashSet<string>(StringComparer.Ordinal);
        }

        public void AddReference(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }
            _log.Debug("Adding reference to {0}...", new FilePath(assembly.Location).GetFilename().FullPath);
            References.Add(assembly);
        }

        public void AddReference(FilePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            _log.Debug("Adding reference to {0}...", path.GetFilename().FullPath);

            References.Add(_loader.Load(path, true));
        }

        public void ImportNamespace(string @namespace)
        {
            if (!Namespaces.Contains(@namespace))
            {
                _log.Debug("Importing namespace {0}...", @namespace);
                Namespaces.Add(@namespace);
            }
        }

        public Tuple<ProjectInfo, string> GetProjectInfo(Script script, FilePath scriptPath)
        {
            // Generate the script code.
            var generator = new CakeRoslynCodeGenerator();
            var code = generator.Generate(script);

            // Create the script options dynamically.
            //var options = Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default
            //    .AddImports(Namespaces)
            //    .AddReferences(References)
            //    .AddReferences(ReferencePaths.Select(r => r.FullPath));

            //var roslynScript = CSharpScript.Create(code, options, _host.GetType());
            //var compilation = roslynScript.GetCompilation();
            //compilation = compilation.WithOptions(compilation.Options
            //    .WithOptimizationLevel(OptimizationLevel.Debug)
            //    .WithOutputKind(OutputKind.DynamicallyLinkedLibrary));

            var project = ProjectInfo.Create(
                id: ProjectId.CreateNewId(Guid.NewGuid().ToString()),
                version: VersionStamp.Create(),
                name: "build.cake",
                filePath: scriptPath.MakeAbsolute(_host.Context.Environment).ToString(),
                assemblyName: "build.cake.dll",
                language: LanguageNames.CSharp,
                compilationOptions: GetCompilationOptions(),
                parseOptions: new CSharpParseOptions(LanguageVersion.Default, DocumentationMode.Parse, SourceCodeKind.Script),
                metadataReferences: GetMetadataReferences(),
                //projectReferences: Context.CsxLoadReferences[csxPath].Select(p => new ProjectReference(p.Id)),
                isSubmission: true,
                hostObjectType: _host.GetType());

            return Tuple.Create(project, code);
        }

        private IEnumerable<MetadataReference> GetMetadataReferences()
        {
            var references = References.Select(r => MetadataReference.CreateFromFile(r.Location)).ToList();
            references.AddRange(ReferencePaths.Select(r => MetadataReference.CreateFromFile(r.MakeAbsolute(_host.Context.Environment).ToString())));
            references.Add(MetadataReference.CreateFromFile(typeof(CakeScriptHost).GetTypeInfo().Assembly.Location));

            return references;
        }

        private CompilationOptions GetCompilationOptions()
        {
            var compilationOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                usings: Namespaces,
                allowUnsafe: true,
                metadataReferenceResolver: new CachingScriptMetadataResolver(),
                sourceReferenceResolver: ScriptSourceResolver.Default,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default).
                WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
                {
                    // ensure that specific warnings about assembly references are always suppressed
                    // https://github.com/dotnet/roslyn/issues/5501
                    { "CS1701", ReportDiagnostic.Suppress },
                    { "CS1702", ReportDiagnostic.Suppress },
                    { "CS1705", ReportDiagnostic.Suppress }
                });

            var topLevelBinderFlagsProperty = typeof(CSharpCompilationOptions).GetProperty("TopLevelBinderFlags", BindingFlags.Instance | BindingFlags.NonPublic);
            var binderFlagsType = typeof(CSharpCompilationOptions).GetTypeInfo().Assembly.GetType("Microsoft.CodeAnalysis.CSharp.BinderFlags");

            var ignoreCorLibraryDuplicatedTypesMember = binderFlagsType?.GetField("IgnoreCorLibraryDuplicatedTypes", BindingFlags.Static | BindingFlags.Public);
            var ignoreCorLibraryDuplicatedTypesValue = ignoreCorLibraryDuplicatedTypesMember?.GetValue(null);
            if (ignoreCorLibraryDuplicatedTypesValue != null)
            {
                topLevelBinderFlagsProperty?.SetValue(compilationOptions, ignoreCorLibraryDuplicatedTypesValue);
            }

            return compilationOptions;
        }

        public void Execute(Script script)
        {
            // Generate the script code.
            var generator = new CakeRoslynCodeGenerator();
            var code = generator.Generate(script);

            // Create the script options dynamically.
            var options = Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default
                .AddImports(Namespaces)
                .AddReferences(References)
                .AddReferences(ReferencePaths.Select(r => r.FullPath));

            var roslynScript = CSharpScript.Create(code, options, _host.GetType());
            var compilation = roslynScript.GetCompilation();
            compilation = compilation.WithOptions(compilation.Options
                .WithOptimizationLevel(OptimizationLevel.Debug)
                .WithOutputKind(OutputKind.DynamicallyLinkedLibrary));

            //using (var assemblyStream = new MemoryStream())
            //using (var symbolStream = new MemoryStream())
            //{
            //    _log.Verbose("Compiling build script for debugging...");
            //    var emitOptions = new EmitOptions(false, DebugInformationFormat.PortablePdb);
            //    var result = compilation.Emit(assemblyStream, symbolStream, options: emitOptions);
            //    if (result.Success)
            //    {
            //        // Rewind the streams.
            //        assemblyStream.Seek(0, SeekOrigin.Begin);
            //        symbolStream.Seek(0, SeekOrigin.Begin);

            //        var assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream, symbolStream);
            //        var type = assembly.GetType(CompiledType);
            //        var method = type.GetMethod(CompiledMethod, BindingFlags.Static | BindingFlags.Public);

            //        var submissionStates = new object[2];
            //        submissionStates[0] = _host;

            //        method.Invoke(null, new object[] { submissionStates });
            //    }
            //    else
            //    {
            //        var errors = string.Join(Environment.NewLine, result.Diagnostics.Select(x => x.ToString()));
            //        var message = string.Format(CultureInfo.InvariantCulture, "Error occurred when compiling: {0}", errors);
            //        throw new CakeException(message);
            //    }
            //}
        }
    }
}