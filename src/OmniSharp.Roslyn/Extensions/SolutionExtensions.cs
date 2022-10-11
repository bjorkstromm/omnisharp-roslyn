using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using OmniSharp.Models;
using OmniSharp.Models.v1.SourceGeneratedFile;
using Microsoft.CodeAnalysis.ExternalAccess.OmniSharp.NavigateTo;

#nullable enable

namespace OmniSharp.Extensions
{
    public static class SolutionExtensions
    {
        private static readonly IImmutableSet<string> _supportedKinds =
            ImmutableHashSet.Create(
                "Class",
                "Constant",
                "Delegate",
                "Enum",
                "EnumItem",
                "Event",
                "Field",
                "Interface",
                "Method",
                "Module",
                "Property",
                "Structure");

        public static async Task<QuickFixResponse> FindSymbols(this Solution solution,
            string? pattern,
            string projectFileExtension,
            int maxItemsToReturn,
            SymbolFilter symbolFilter = SymbolFilter.TypeAndMember)
        {
            var projectsToInclude = solution.Projects.Where(p =>
                (p.FilePath?.EndsWith(projectFileExtension, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.Name?.EndsWith(projectFileExtension, StringComparison.OrdinalIgnoreCase) ?? false))
                .Select(p => p.Id)
                .ToImmutableHashSet();

            var filteredSolution = solution;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var project in solution.Projects)
            {
                if (!projectsToInclude.Contains(project.Id))
                {
                    filteredSolution = filteredSolution.RemoveProject(project.Id);
                }
            }

            var callbackHandler = new OmniSharpNavigateToSearcherCallbackHandler(
                filteredSolution,
                maxItemsToReturn);

            await OmniSharpNavigateToSearcher.SearchAsync(
                filteredSolution,
                callbackHandler.HandleCallback,
                pattern ?? string.Empty,
                _supportedKinds,
                callbackHandler.CancellationToken);

            return callbackHandler.Results;
        }

        private class OmniSharpNavigateToSearcherCallbackHandler
        {
            private readonly Solution _solution;
            private readonly int _maxItemsToReturn;
            private readonly CancellationTokenSource _cts;
            private readonly List<SymbolLocation> _symbolLocations;
            public OmniSharpNavigateToSearcherCallbackHandler(Solution solution, int maxItemsToReturn)
            {
                _solution = solution;
                _maxItemsToReturn = maxItemsToReturn;
                _cts = new CancellationTokenSource();
                _symbolLocations = new List<SymbolLocation>(maxItemsToReturn);
            }

            public CancellationToken CancellationToken => _cts.Token;
            public QuickFixResponse Results => new(_symbolLocations);

            public Task HandleCallback(
                Project project,
                in OmniSharpNavigateToSearchResult result,
                CancellationToken cancellationToken) =>
                ConvertSymbol(project, result, cancellationToken)
                    .ContinueWith(task =>
                    {
                        _symbolLocations.Add(task.Result);

                        if (ShouldStopSearching(_maxItemsToReturn, _symbolLocations))
                        {
                            _cts.Cancel();
                        }

                        return Task.CompletedTask;
                    }, cancellationToken);

            private async Task<SymbolLocation> ConvertSymbol(
                Project project,
                OmniSharpNavigateToSearchResult result,
                CancellationToken cancellationToken)
            {
                var syntaxTree = await result.NavigableItem.Document.GetSyntaxTreeAsync(cancellationToken);
                var location = Location.Create(syntaxTree!, result.NavigableItem.SourceSpan);
                var lineSpan = location.GetLineSpan();
                var path = lineSpan.Path;
                var projects = _solution.GetDocumentIdsWithFilePath(path)
                    .Select(documentId => _solution.GetProject(documentId.ProjectId)!.Name)
                    .ToArray();

                var semanticModel = await result.NavigableItem.Document.GetSemanticModelAsync(cancellationToken);
                var node = (await syntaxTree!.GetRootAsync(cancellationToken)).FindNode(result.NavigableItem.SourceSpan);
                var symbol = semanticModel!.GetDeclaredSymbol(node, cancellationToken: cancellationToken);

                var format = SymbolDisplayFormat.MinimallyQualifiedFormat;
                format = format.WithMemberOptions(format.MemberOptions
                                                  ^ SymbolDisplayMemberOptions.IncludeContainingType
                                                  ^ SymbolDisplayMemberOptions.IncludeType);

                format = format.WithKindOptions(SymbolDisplayKindOptions.None);

                return new SymbolLocation
                {
                    Text = symbol!.ToDisplayString(format),
                    Kind = symbol!.GetKind(),
                    FileName = path,
                    Line = lineSpan.StartLinePosition.Line,
                    Column = lineSpan.StartLinePosition.Character,
                    EndLine = lineSpan.EndLinePosition.Line,
                    EndColumn = lineSpan.EndLinePosition.Character,
                    Projects = projects,
                    ContainingSymbolName = symbol!.ContainingSymbol?.Name ?? "",
                    GeneratedFileInfo = GetSourceGeneratedFileInfo(_solution, location),
                };
            }

            private static bool ShouldStopSearching(int maxItemsToReturn, ICollection<SymbolLocation> symbolLocations) =>
                maxItemsToReturn > 0 && symbolLocations.Count >= maxItemsToReturn;
        }

        public static async Task<QuickFixResponse> __FindSymbolsOld(this Solution solution,
            string pattern,
            string projectFileExtension,
            int maxItemsToReturn,
            SymbolFilter symbolFilter = SymbolFilter.TypeAndMember)
        {
            var projects = solution.Projects.Where(p =>
                (p.FilePath?.EndsWith(projectFileExtension, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.Name?.EndsWith(projectFileExtension, StringComparison.OrdinalIgnoreCase) ?? false));

            var symbolLocations = new List<QuickFix>();

            foreach (var project in projects)
            {
                var symbols = !string.IsNullOrEmpty(pattern) ?
                    await SymbolFinder.FindSourceDeclarationsWithPatternAsync(project, pattern, symbolFilter) :
                    await SymbolFinder.FindSourceDeclarationsAsync(project, candidate => true, symbolFilter);

                foreach (var symbol in symbols)
                {
                    // for partial methods, pick the one with body
                    var s = symbol;
                    if (s is IMethodSymbol method)
                    {
                        s = method.PartialImplementationPart ?? symbol;
                    }

                    foreach (var location in s.Locations)
                    {
                        symbolLocations.Add(ConvertSymbol(solution, symbol, location));
                    }

                    if (ShouldStopSearching(maxItemsToReturn, symbolLocations))
                    {
                        break;
                    }
                }

                if (ShouldStopSearching(maxItemsToReturn, symbolLocations))
                {
                    break;
                }
            }

            return new QuickFixResponse(symbolLocations.Distinct().ToList());
        }

        private static bool ShouldStopSearching(int maxItemsToReturn, List<QuickFix> symbolLocations)
        {
            return maxItemsToReturn > 0 && symbolLocations.Count >= maxItemsToReturn;
        }

        private static QuickFix ConvertSymbol(Solution solution, ISymbol symbol, Location location)
        {
            var lineSpan = location.GetLineSpan();
            var path = lineSpan.Path;
            var projects = solution.GetDocumentIdsWithFilePath(path)
                                    .Select(documentId => solution.GetProject(documentId.ProjectId)!.Name)
                                    .ToArray();

            var format = SymbolDisplayFormat.MinimallyQualifiedFormat;
            format = format.WithMemberOptions(format.MemberOptions
                                              ^ SymbolDisplayMemberOptions.IncludeContainingType
                                              ^ SymbolDisplayMemberOptions.IncludeType);

            format = format.WithKindOptions(SymbolDisplayKindOptions.None);

            return new SymbolLocation
            {
                Text = symbol.ToDisplayString(format),
                Kind = symbol.GetKind(),
                FileName = path,
                Line = lineSpan.StartLinePosition.Line,
                Column = lineSpan.StartLinePosition.Character,
                EndLine = lineSpan.EndLinePosition.Line,
                EndColumn = lineSpan.EndLinePosition.Character,
                Projects = projects,
                ContainingSymbolName = symbol.ContainingSymbol?.Name ?? "",
                GeneratedFileInfo = GetSourceGeneratedFileInfo(solution, location),
            };
        }

        internal static SourceGeneratedFileInfo? GetSourceGeneratedFileInfo(this Solution solution, Location location)
        {
            Debug.Assert(location.IsInSource);
            var document = solution.GetDocument(location.SourceTree);
            if (document is not SourceGeneratedDocument)
            {
                return null;
            }

            return new SourceGeneratedFileInfo
            {
                ProjectGuid = document.Project.Id.Id,
                DocumentGuid = document.Id.Id
            };
        }
    }
}
