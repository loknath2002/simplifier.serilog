using System.Globalization;
using Serilog;
using Serilog.Events;
namespace Simplifier.Serilog
{
    /// <summary>
    /// Logging configuration when setting up a logger.
    /// </summary>
    public class LoggingConfiguration
    {
        /// <summary>
        /// Gets or sets the log format. Default : "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3} {ThreadId:D2} {Elapsed,10} {CallSite,60}() {Message}{NewLine}{Exception}";
        /// </summary>
        public string LogFormat { get; set; } = LoggingSetup.DefaultLogFormat;
        
        /// <summary>
        /// Gets or sets the log file name. Default : MyApplication_.slog
        /// </summary>
        public string FileName { get; set; } = LoggingSetup.DefaultLogFileName;
        
        /// <summary>
        /// Gets or sets the rolling interval. Default : RollingInterval.Day;
        /// </summary>
        public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;
        
        /// <summary>
        /// Gets or sets the total number of log files to retain. Default : 14
        /// </summary>
        public uint RetentionFileCountLimit { get; set; } = 14;
        
        /// <summary>
        /// Gets or sets to indicate if logs should also be printed in Console Default : false
        /// </summary>
        public bool EnableConsole { get; set; }
        
        /// <summary>
        /// Gets or sets to indicate if the multiple process can write to the same log Default : true
        /// </summary>
        public bool MultiProcess { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the minimum log event level. Default : LogEventLevel.Information
        /// </summary>
        public LogEventLevel LogEventLevel { get; set; } = LogEventLevel.Information;
        
        /// <summary>
        /// Gets or sets the Localization option. Default : CultureInfo.InvariantCulture
        /// </summary>
        public CultureInfo Localization { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// A prebaked production configuration.
        /// Informational and higher level logs will be written to MyApplication_.log, which rolls over every day upto 14 days, After which the oldest file will be deleted.
        /// Sample Log : 2026-01-29 20:18:45.702 INF 13       10ms                                              MyApplication.BizLogic.Execute() Processing execute request for a total of 408 items.
        /// </summary>
        public static LoggingConfiguration ProdConfig => new LoggingConfiguration()
        {
            EnableConsole = false,
            FileName = LoggingSetup.DefaultLogFileName,
            RollingInterval = RollingInterval.Day,
            RetentionFileCountLimit = 14,
            LogFormat = LoggingSetup.DefaultLogFormat,
            LogEventLevel = LogEventLevel.Information,
            MultiProcess = true
        };

        /// <summary>
        /// A prebaked production configuration with Console sink.
        /// Informational and higher level logs will be written to MyApplication_.log and Console, which rolls over every day upto 14 days, After which the oldest file will be deleted.
        /// Sample Log : 2026-01-29 20:18:45.702 INF 13       10ms                                              MyApplication.BizLogic.Execute() Processing execute request for a total of 408 items.
        /// </summary>
        public static LoggingConfiguration ProdConfigWithConsole => new LoggingConfiguration()
        {
            EnableConsole = true,
            FileName = LoggingSetup.DefaultLogFileName,
            RollingInterval = RollingInterval.Day,
            RetentionFileCountLimit = 14,
            LogFormat = LoggingSetup.DefaultLogFormat,
            LogEventLevel = LogEventLevel.Information,
            MultiProcess = true
        };
    }
}