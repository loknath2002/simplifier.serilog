using System;
using System.Threading;
using Serilog;
using Serilog.Core;
using Serilog.Events;
namespace Simplifier.Serilog
{
    /// <summary>
    /// Class to help Set up serilog.
    /// </summary>
    public static class LoggingSetup
    {
        internal const string CallSiteConst = "CallSite";
        internal const string ElapsedConst = "Elapsed";
        internal const string DefaultLogFormat = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3} {ThreadId:D2} {Elapsed,10} {CallSite,60}() {Message}{NewLine}{Exception}";
        internal const string DefaultLogFileName = "MyApplication_.slog";

        private static long myThreadSafeBool;
        private static LoggingLevelSwitch myLogLevelSwitch = new LoggingLevelSwitch();

        /// <summary>
        /// Gets the current serilog logger instance.
        /// </summary>
        public static ILogger Logger => Log.Logger;

        /// <summary>
        /// Gets or sets a status which enables in-out traces of each Log Scope
        /// </summary>
        public static bool InOutEnabled
        {
            get => Interlocked.Read(ref myThreadSafeBool) == 1;
            set => Interlocked.Exchange(ref myThreadSafeBool, value ? 1 : 0);
        }

        /// <summary>
        /// Creates a global logger using Serilog with the provided configuration.
        /// </summary>
        /// <param name="theConfiguration">The configuration to use to set up Serilog</param>
        /// <exception cref="ArgumentException">when the configuration is null</exception>
        public static void Create(LoggingConfiguration theConfiguration)
        {
            Guard.AgainstNull(theConfiguration, nameof(theConfiguration));
            CreateLogger(theConfiguration, out myLogLevelSwitch, out var aLogger);
            Log.Logger = aLogger;
            LoggerDetail.SetLoggingStrategy(true);
        }

        /// <summary>
        /// Helper function to create a logger.
        /// </summary>
        /// <param name="theConfiguration">the configuration detail</param>
        /// <param name="theLevelSwitch">the out logging switch controller</param>
        /// <param name="theLogger">the instance of the logger</param>
        /// <exception cref="ArgumentException">when the configuration is null</exception>
        internal static void CreateLogger(LoggingConfiguration theConfiguration, out LoggingLevelSwitch theLevelSwitch, out ILogger theLogger)
        {
            Guard.AgainstNull(theConfiguration, nameof(theConfiguration));
            Guard.AgainstNullOrWhiteSpace(theConfiguration.LogFormat, nameof(theConfiguration.LogFormat));
            Guard.AgainstNullOrWhiteSpace(theConfiguration.FileName, nameof(theConfiguration.FileName));
            var aTemp = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.FromLogContext()
                .WriteTo.File(theConfiguration.FileName, rollingInterval: theConfiguration.RollingInterval, retainedFileCountLimit: (int)theConfiguration.RetentionFileCountLimit,
                    outputTemplate: theConfiguration.LogFormat,
                    formatProvider: theConfiguration.Localization,
                    shared: theConfiguration.MultiProcess);

            theLevelSwitch = new LoggingLevelSwitch(theConfiguration.LogEventLevel);
            aTemp.MinimumLevel.ControlledBy(myLogLevelSwitch);
            if (theConfiguration.EnableConsole)
            {
                aTemp.WriteTo.Console(outputTemplate: theConfiguration.LogFormat,
                    formatProvider: theConfiguration.Localization);
            }

            theLogger = aTemp.CreateLogger();
        }

        /// <summary>
        /// Function to change log level at runtime.
        /// </summary>
        /// <param name="theNewLevel">The new log level to apply.</param>
        public static void ChangeLogLevel(LogEventLevel theNewLevel)
        {
            if (myLogLevelSwitch.MinimumLevel != theNewLevel)
            {
                myLogLevelSwitch.MinimumLevel = theNewLevel;
            }
        }
    }
}