using System;
using System.Collections.Concurrent;
using Serilog;
using Serilog.Core;
using Serilog.Events;
namespace Simplifier.Serilog
{
    /// <summary>
    ///     Provides the ability to create multiple instances of <see cref="ILogger" /> with fuzzy search and additional
    ///     capabilities for each sub-logger
    /// </summary>
    public class MultiInstanceLoggingSetup
    {
        // ReSharper disable once ChangeFieldTypeToSystemThreadingLock, not avaliable in older frameworks.
        private readonly object myLock = new object();
        
        private readonly ConcurrentDictionary<string, (ILogger, LoggingLevelSwitch)> myLoggers = new ConcurrentDictionary<string, (ILogger, LoggingLevelSwitch)>();

        private LoggingConfiguration myGlobalConfiguration;

        private bool myGlobalLoggerInitDone;

        private MultiInstanceLoggingSetup()
        {
        }

        /// <summary>
        ///     Provides a singleton instance of <see cref="MultiInstanceLoggingSetup" />
        /// </summary>
        public static MultiInstanceLoggingSetup Instance { get; } = new MultiInstanceLoggingSetup();

        /// <summary>
        ///     Gets a logger by name, does not do a fuzzy search and will throw KeyNotFoundException if not found.
        /// </summary>
        /// <param name="theLoggerName"></param>
        public ILogger this[string theLoggerName] => myLoggers[theLoggerName].Item1;

        /// <summary>
        ///     Gets a logger by name, internally does a fuzzy search, i.e MyApplication.Module.Storage will return and exact match
        ///     or MyApplication.Module if exists.
        /// </summary>
        /// <param name="theFuzzyLoggerName">the name of the logger</param>
        /// <returns>Closely matched logger else Global instance of the logger</returns>
        internal ILogger SearchLogger(string theFuzzyLoggerName)
        {
            return myLoggers["Global"].Item1;
        }

        /// <summary>
        /// Changes the logging level of the sublogger
        /// </summary>
        /// <param name="theLoggerName">the precise logger name</param>
        /// <param name="theNewLevel">the new level to set to the sublogger</param>
        /// <returns>True if the change is done, false when sub-logger with the name is not fount.</returns>
        public bool ChangeLogLevel(string theLoggerName, LogEventLevel theNewLevel)
        {
            if (!myLoggers.TryGetValue(theLoggerName, out var theLogger))
            {
                return false;
            }
            
            theLogger.Item2.MinimumLevel = theNewLevel;
            return true;
        }
        
        /// <summary>
        ///     Creates the global logger, this creates the Global instance of the logger, this acts as a default logger when
        ///     sub loggers are not defined.
        /// </summary>
        /// <param name="theConfigurationToUse">The global configuration to use.</param>
        /// <exception cref="ArgumentException">when the configuration is null</exception>
        public void Create(LoggingConfiguration theConfigurationToUse)
        {
            lock (myLock)
            {
                Guard.AgainstNull(theConfigurationToUse, nameof(theConfigurationToUse));
                LoggingSetup.CreateLogger(theConfigurationToUse, out var theLevelSwitch, out var theLogger);
                myLoggers["Global"] = (theLogger, theLevelSwitch);
                myGlobalConfiguration = new LoggingConfiguration
                {
                    LogEventLevel = theConfigurationToUse.LogEventLevel,
                    Localization = theConfigurationToUse.Localization,
                    EnableConsole = theConfigurationToUse.EnableConsole,
                    FileName = theConfigurationToUse.FileName,
                    RollingInterval = theConfigurationToUse.RollingInterval,
                    RetentionFileCountLimit = theConfigurationToUse.RetentionFileCountLimit,
                    MultiProcess = theConfigurationToUse.MultiProcess,
                    LogFormat = theConfigurationToUse.LogFormat,
                };

                LoggerDetail.SetLoggingStrategy(false);
                myGlobalLoggerInitDone = true;
            }
        }

        /// <summary>
        ///     Adds a sub logger to the global logger.
        /// </summary>
        /// <param name="theLoggerName">the logger name, this is</param>
        /// <param name="theSubConfiguration">the configuration for this sub-logger</param>
        /// <exception cref="InvalidOperationException">
        ///     when attempt to create a sub-logger is done without calling
        ///     InitializeGlobalLogger
        /// </exception>
        public void AddSubLogger(string theLoggerName, SubLoggingConfiguration theSubConfiguration)
        {
            lock (myLock)
            {
                if (!myGlobalLoggerInitDone)
                {
                    throw new InvalidOperationException("Global logger has not been initialized. Call InitializeGlobalLogger first. Ensure that InitializeGlobalLogger");
                }

                Guard.AgainstNullOrWhiteSpace(theLoggerName, nameof(theLoggerName));
                Guard.AgainstNull(theSubConfiguration, nameof(theSubConfiguration));
                LoggingSetup.CreateLogger(CreateFullConfigFromPartial(theSubConfiguration), out var theLevelSwitch, out var theLogger);
                myLoggers[theLoggerName] = (theLogger, theLevelSwitch);
            }
        }

        private LoggingConfiguration CreateFullConfigFromPartial(SubLoggingConfiguration theSubConfiguration)
        {
            return new LoggingConfiguration
            {
                LogEventLevel = theSubConfiguration.LogEventLevel,
                Localization = theSubConfiguration.Localization,
                EnableConsole = myGlobalConfiguration.EnableConsole,
                FileName = myGlobalConfiguration.FileName,
                RollingInterval = myGlobalConfiguration.RollingInterval,
                RetentionFileCountLimit = myGlobalConfiguration.RetentionFileCountLimit,
                MultiProcess = myGlobalConfiguration.MultiProcess,
                LogFormat = myGlobalConfiguration.LogFormat,
            };
        }
    }
}