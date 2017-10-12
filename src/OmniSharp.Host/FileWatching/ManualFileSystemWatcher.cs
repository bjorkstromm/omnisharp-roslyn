using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.FileSystemGlobbing;
using OmniSharp.Models.V2;

namespace OmniSharp.FileWatching
{
    public class ManualFileSystemWatcher : IFileSystemWatcher
    {
        private readonly IOmniSharpEnvironment _environment;
        private readonly Dictionary<string, ICollection<Action<FileChangedRequest>>> _callbacks;

        public ManualFileSystemWatcher(IOmniSharpEnvironment environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _callbacks = new Dictionary<string, ICollection<Action<FileChangedRequest>>>();
        }

        public void TriggerChange(FileChangedRequest request)
        {
            var directoryInfo = new InMemoryDirectoryInfo(_environment.TargetDirectory, new[] { request.FileName });
            foreach (var key in _callbacks.Keys)
            {
                var matcher= new Matcher(StringComparison.OrdinalIgnoreCase)
                    .AddInclude(key);

                if (!matcher.Execute(directoryInfo).HasMatches) continue;
                foreach (var action in _callbacks[key])
                {
                    action(request);
                }
            }
        }

        public void Watch(string pattern, Action<FileChangedRequest> callback)
        {
            if (!_callbacks.ContainsKey(pattern))
            {
                _callbacks[pattern] = new Collection<Action<FileChangedRequest>>();
            }
            _callbacks[pattern].Add(callback);
        }
    }
}
