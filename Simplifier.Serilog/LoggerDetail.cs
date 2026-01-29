namespace Simplifier.Serilog
{
    using System;

    /// <summary>
    /// Detail's of the logger holds info about the log scope
    /// </summary>
    public class LoggerDetail
    {
        /// <summary>
        /// Gets logger's scope name
        /// </summary>
        public string ScopeName { get; private set; }

        /// <summary>
        /// Creates an instance of Logger Detail
        /// </summary>
        /// <param name="theLogScope">Type which encloses the logger's scope</param>
        public LoggerDetail(Type theLogScope)
        {
            Guard.AgainstNull(theLogScope, nameof(theLogScope));
            ScopeName = theLogScope.FullName;
        }

        /// <summary>
        /// Creates an instance of Logger Detail
        /// </summary>
        /// <param name="theLogScopeName">Name/ID which encloses the logger's scope</param>
        public LoggerDetail(string theLogScopeName)
        {
            ScopeName = theLogScopeName;
        }
    }
}