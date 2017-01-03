// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Cake.Core.Diagnostics;
using Cake.Core.Reflection;
using Cake.Core.Scripting;

namespace OmniSharp.Cake
{
    internal sealed class CakeXPlatScriptEngine : IScriptEngine
    {
        private readonly IAssemblyLoader _loader;
        private readonly ICakeLog _log;

        public CakeXPlatScriptEngine(IAssemblyLoader loader, ICakeLog log)
        {
            _loader = loader;
            _log = log;
        }

        public IScriptSession CreateSession(IScriptHost host, IDictionary<string, string> arguments)
        {
            return new CakeXPlatScriptSession(host, _loader, _log);
        }
    }
}