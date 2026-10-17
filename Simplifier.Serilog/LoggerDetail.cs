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
        /// Flag to hold the logging strategy.
        /// </summary>
        private static bool? mySingleLogger;
        
        /// <summary>
        ///     Creates an instance of Logger Detail
        /// </summary>
        /// <param name="theLogScope">Type that encloses the logger's scope</param>
        public LoggerDetail(Type theLogScope)
        {
            Guard.AgainstNull(theLogScope, nameof(theLogScope));
            ScopeName = theLogScope.FullName;
            AssignLogger(ScopeName);
        }

        /// <summary>
        ///     Creates an instance of Logger Detail
        /// </summary>
        /// <param name="theLogScopeName">Name/ID which encloses the logger's scope</param>
        public LoggerDetail(string theLogScopeName)
        {
            ScopeName = theLogScopeName;
            AssignLogger(theLogScopeName);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AssignLogger(string theLogScopeName)
        {
            if (mySingleLogger == null || mySingleLogger.Value)
            {
                Logger = LoggingSetup.Logger;
            }
            else
            {
                Logger = MultiInstanceLoggingSetup.Instance.SearchLogger(theLogScopeName);
            }
        }

        /// <summary>
        ///     Gets the logger instance for this log detail.
        /// </summary>
        internal ILogger Logger { get; private set; }

        /// <summary>
        /// Gets logger's scope name
        /// </summary>
        public string ScopeName { get; private set; }

        internal static void SetLoggingStrategy(bool theIsSingleInstance)
        {
            if (mySingleLogger.HasValue && theIsSingleInstance != mySingleLogger.Value)
            {
                throw new InvalidOperationException("Logging strategy can only be set once.");
            }
            
            mySingleLogger = theIsSingleInstance;
        }
    }
}