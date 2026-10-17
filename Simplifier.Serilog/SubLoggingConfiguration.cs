// /*
// * Copyright (c) 2026 Loknath, Quantum Technology
// * All rights reserved.
// */

using System.Globalization;
using Serilog;
using Serilog.Events;
namespace Simplifier.Serilog
{
    /// <summary>
    ///     Configuration for a sub logger. This allows each sub-logger to have its own configuration.
    ///     Few of the configuration options are common to all sub-loggers.
    /// </summary>
    public class SubLoggingConfiguration
    {
        /// <summary>
        ///     Gets the log format. 
        /// </summary>
        public string LogFormat { get; internal set; }

        /// <summary>
        ///     Gets the log file name.
        /// </summary>
        public string FileName { get; internal set; }

        /// <summary>
        ///     Gets the rolling interval. 
        /// </summary>
        public RollingInterval RollingInterval { get; internal set; }

        /// <summary>
        ///     Gets the total number of log files to retain. 
        /// </summary>
        public uint RetentionFileCountLimit { get; internal set; }

        /// <summary>
        ///     Gets to indicate if logs should also be printed in Console 
        /// </summary>
        public bool EnableConsole { get; internal set; }

        /// <summary>
        ///     Gets to indicate if the multiple process can write to the same log 
        /// </summary>
        public bool MultiProcess { get; internal set; }

        /// <summary>
        ///     Gets or sets the minimum log event level. Default: LogEventLevel.Information
        /// </summary>
        public LogEventLevel LogEventLevel { get; set; } = LogEventLevel.Information;

        /// <summary>
        ///     Gets or sets the Localization option. Default: CultureInfo.InvariantCulture
        /// </summary>
        public CultureInfo Localization { get; set; } = CultureInfo.InvariantCulture;
    }
}