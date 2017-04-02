using System;
using Cake.Core.Diagnostics;
using Microsoft.Extensions.Logging;

using LogLevel = Cake.Core.Diagnostics.LogLevel;

namespace Cake.OmniSharp.Diagnostics
{
    class CakeLog : ICakeLog
    {
        private readonly ILogger _logger;

        public CakeLog(ILogger logger)
        {
            _logger = logger;
        }

        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            switch (level)
            {
                case LogLevel.Fatal:
                    _logger.LogCritical(format, args);
                    break;
                case LogLevel.Error:
                    _logger.LogError(format, args);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(format, args);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(format, args);
                    break;
                case LogLevel.Verbose:
                    _logger.LogTrace(format, args);
                    break;
                case LogLevel.Debug:
                    _logger.LogDebug(format, args);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public Verbosity Verbosity { get; set; }
    }
}
