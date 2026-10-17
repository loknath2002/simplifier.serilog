using System;
using System.Runtime.CompilerServices;
using Serilog;
namespace Simplifier.Serilog
{
    /// <summary>
    /// Details of the logger holds info about the log scope
    /// </summary>
    public class LoggerDetail
    {
        /// <summary>
        /// Lock for writing logger state
        /// </summary>
        // ReSharper disable once ChangeFieldTypeToSystemThreadingLock
        private static readonly object myWriterLock = new object();
        
        /// <summary>
        /// Flag to hold the logging strategy.
        /// </summary>
        private static bool? mySingleLogger;

        /// <summary>
        ///     Creates an instance of Logger Detail
        /// </summary>
        /// <param name="theLogScope">Type that encloses the logger's scope</param>
        /// <param name="theLoggingInstance">The logging instance name to use in case of <see cref="MultiInstanceLoggingSetup"/></param>
        public LoggerDetail(Type theLogScope, string theLoggingInstance = MultiInstanceLoggingSetup.DefaultMultiInstanceLoggerName)
        {
            Guard.AgainstNull(theLogScope, nameof(theLogScope));
            ScopeName = theLogScope.FullName;
            AssignLogger(ScopeName, theLoggingInstance);
        }

        /// <summary>
        ///     Creates an instance of Logger Detail
        /// </summary>
        /// <param name="theLogScopeName">Name/ID which encloses the logger's scope</param>
        /// <param name="theLoggingInstance">The logging instance name to use in case of <see cref="MultiInstanceLoggingSetup"/></param>
        public LoggerDetail(string theLogScopeName, string theLoggingInstance = MultiInstanceLoggingSetup.DefaultMultiInstanceLoggerName)
        {
            Guard.AgainstNullOrWhiteSpace(theLogScopeName, nameof(theLogScopeName));
            ScopeName = theLogScopeName;
            AssignLogger(theLogScopeName, theLoggingInstance);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AssignLogger(string theLogScopeName, string theInstanceName)
        {
            if (mySingleLogger == null || mySingleLogger.Value)
            {
                if (LoggingSetup.Logger == global::Serilog.Core.Logger.None)
                {
                    LoggingSetup.Create(LoggingConfiguration.ProdConfig);
                }

                Logger = LoggingSetup.Logger;
            }
            else
            {
                if (!MultiInstanceLoggingSetup.Instances.TryGetValue(theInstanceName, out var aInstance))
                {
                    throw new ArgumentException($"Logger instance {theInstanceName} not found. Please create one using MultiInstanceLoggingSetup or use the 'Default' instance");
                }
                
                Logger = aInstance.SearchLogger(theLogScopeName);
            }
        }

        /// <summary>
        ///     Gets the logger instance for this log detail.
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// Gets logger's scope name
        /// </summary>
        public string ScopeName { get; }

        internal static void SetLoggingStrategy(bool theIsSingleInstance)
        {
            lock (myWriterLock)
            {
                if (mySingleLogger.HasValue && theIsSingleInstance != mySingleLogger.Value)
                {
                    throw new InvalidOperationException("Logging strategy can only be changed once.");
                }

                mySingleLogger = theIsSingleInstance;
            }
        }
    }
}