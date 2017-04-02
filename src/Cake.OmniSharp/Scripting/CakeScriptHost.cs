// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.Scripting;

namespace Cake.OmniSharp.Scripting
{
    /// <summary>
    /// The script host used to execute Cake scripts.
    /// </summary>
    internal sealed class CakeScriptHost : ScriptHost
    {
        //private readonly ICakeReportPrinter _reportPrinter;
        private readonly ICakeLog _log;
        private ICakeContext _cakeContext;

        public CakeScriptHost(ICakeContext cakeContext)
            : base(new CakeEngine(cakeContext.Log), cakeContext)
        {
            _cakeContext = cakeContext;
            _log = cakeContext.Log;
        }

        /// <summary>
        /// Runs the specified target.
        /// </summary>
        /// <param name="target">The target to run.</param>
        /// <returns>The resulting report.</returns>
        public override CakeReport RunTarget(string target)
        {
            var strategy = new DefaultExecutionStrategy(_log);
            var report = Engine.RunTarget(Context, strategy, target);
            //if (report != null && !report.IsEmpty)
            //{
            //    _reportPrinter.Write(report);
            //}
            return report;
        }
    }
}