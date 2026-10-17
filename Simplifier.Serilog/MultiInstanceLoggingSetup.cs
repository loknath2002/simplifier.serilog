using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private const int myLoggerNameRequiredLength = 2;

        private const string myGlobalLoggerInstanceKey = "Global";

        private readonly ConcurrentDictionary<string, (ILogger, LoggingLevelSwitch)> myLoggers = new ConcurrentDictionary<string, (ILogger, LoggingLevelSwitch)>();

        private LoggingConfiguration myGlobalConfiguration;
        
        internal const string DefaultMultiInstanceLoggerName = "Default";
        
        internal static readonly IDictionary<string, MultiInstanceLoggingSetup> Instances = new Dictionary<string, MultiInstanceLoggingSetup>();

        /// <summary>
        ///     Gets a logger by name, does not do a fuzzy search and will throw KeyNotFoundException if not found.
        /// </summary>
        /// <param name="theLoggerName"></param>
        public ILogger this[string theLoggerName] => myLoggers[theLoggerName].Item1;

        /// <summary>
        /// Creates an instance of logger which supports sub-loggers.
        /// </summary>
        /// <param name="theConfigurationToUse">the configuration to use.</param>
        /// <param name="theInstanceName">Used to resolve the instance, when creating LoggingDetail, set to 'Default'</param>
        public MultiInstanceLoggingSetup(LoggingConfiguration theConfigurationToUse, string theInstanceName = DefaultMultiInstanceLoggerName)
        {
            Guard.AgainstNull(theConfigurationToUse, nameof(theConfigurationToUse));
            LoggingSetup.CreateLogger(theConfigurationToUse, out var theLevelSwitch, out var theLogger);
            myLoggers[myGlobalLoggerInstanceKey] = (theLogger, theLevelSwitch);
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
            Instances[theInstanceName] = this;
        }
        /// <summary>
        ///     Gets a logger by name, internally does a fuzzy search, i.e MyApplication.Module.Storage will return and exact match
        ///     or MyApplication.Module if exists.
        /// </summary>
        /// <param name="theFuzzyLoggerNameSpace">the name of the logger</param>
        /// <returns>Closely matched logger else Global instance of the logger</returns>
        internal ILogger SearchLogger(string theFuzzyLoggerNameSpace)
        {
            Guard.AgainstNullOrWhiteSpace(theFuzzyLoggerNameSpace, nameof(theFuzzyLoggerNameSpace));
            if (myLoggers.TryGetValue(theFuzzyLoggerNameSpace, out var theLogger))
            {
                return theLogger.Item1;
            }

            string[] aAllNamespaces = myLoggers.Keys.ToArray();
            string aClosestMatch = myGlobalLoggerInstanceKey;
            int aClosestMatchCount = myLoggerNameRequiredLength - 1;
            foreach (string aNameSpace in aAllNamespaces)
            {
                int aMatchCount = 0;
                int minLength = Math.Min(aNameSpace.Length, theFuzzyLoggerNameSpace.Length);
                for (int i = 0; i < minLength; i++)
                {
                    if (aNameSpace[i] != theFuzzyLoggerNameSpace[i])
                    {
                        break;
                    }
                    
                    aMatchCount++;
                }

                if (aMatchCount > aClosestMatchCount)
                {
                    aClosestMatch = aNameSpace;
                    aClosestMatchCount = aMatchCount;
                }
            }

            return myLoggers[aClosestMatch].Item1;
        }

        /// <summary>
        ///     Changes the logging level of the sublogger
        /// </summary>
        /// <param name="theLoggerName">the precise logger name</param>
        /// <param name="theNewLevel">the new level to set to the sublogger</param>
        /// <returns>True if the change is done, false when sub-logger with the name is not found.</returns>
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
        ///     Adds a sub logger to the global logger.
        /// </summary>
        /// <param name="theLoggerNamespace">the logger name, this is</param>
        /// <param name="theSubConfiguration">the configuration for this sub-logger</param>
        /// <exception cref="InvalidOperationException">
        ///     when attempt to create a sub-logger is done without calling
        ///     InitializeGlobalLogger
        /// </exception>
        public void AddSubLogger(string theLoggerNamespace, SubLoggingConfiguration theSubConfiguration)
        {
            Guard.AgainstNullOrWhiteSpace(theLoggerNamespace, nameof(theLoggerNamespace));
            Guard.AgainstNull(theSubConfiguration, nameof(theSubConfiguration));
            if (theLoggerNamespace.Length < myLoggerNameRequiredLength)
            {
                throw new ArgumentException("Logger name must be at-least 2 characters long", nameof(theLoggerNamespace));
            }
            
            LoggingSetup.CreateLogger(CreateFullConfigFromPartial(theSubConfiguration), out var theLevelSwitch, out var theLogger);
            myLoggers[theLoggerNamespace] = (theLogger, theLevelSwitch);
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