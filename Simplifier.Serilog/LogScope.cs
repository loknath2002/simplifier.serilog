using System;
using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace Simplifier.Serilog
{
    /// <summary>
    /// Class which encloses a scope which needs to be logged.
    /// </summary>
#pragma warning disable CA1815
    public readonly struct LogScope : IDisposable
#pragma warning restore CA1815
    {
        private readonly ILogger myLogger;
        private readonly LoggerDetail myDetail;
        private readonly DateTime myStartTime;
        private readonly string myCallerName;

        /// <summary>
        /// Creates an instance of Log Scope.
        /// </summary>
        /// <param name="theDetail">the details which are to be attached to the scope</param>
        /// <param name="theCallerName">(Optional)the Calling function name.</param>
        public LogScope(LoggerDetail theDetail, [CallerMemberName] string theCallerName = "")
        {
            Guard.AgainstNull(theDetail, nameof(theDetail));
            Guard.AgainstNull(theCallerName, nameof(theCallerName));
            if (LoggingSetup.Logger == Logger.None)
            {
                LoggingSetup.Create(LoggingConfiguration.ProdConfig);
            }

            myLogger = LoggingSetup.Logger;
            myDetail = theDetail;
            myStartTime = DateTime.UtcNow;
#pragma warning disable CA1865
            myCallerName = theCallerName.StartsWith(".", StringComparison.InvariantCulture) ? theCallerName.Remove(0, 1) : theCallerName;
#pragma warning restore CA1865
            if (LoggingSetup.InOutEnabled)
            {
                this.Info($"[I] Entering {myCallerName}");
            }
        }

        /// <summary>
        /// Gets elapsed time since the scope was constructed to this instant in time.
        /// </summary>
        public TimeSpan Elapsed => DateTime.UtcNow - myStartTime;

        #region Verbose

        /// <summary>
        /// Writes verbose level log to serilog framework only when Elapsed has crossed the deadline
        /// </summary>
        /// <param name="theDeadline">the deadline time to check for</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        public void VerboseOverTime(TimeSpan theDeadline, string theMessageTemplate)
        {
            if (this.Elapsed > theDeadline)
            {
                Verbose(theMessageTemplate);
            }
        }

        /// <summary>
        /// Writes verbose level log to serilog framework only when theCondition evaluates to true.
        /// </summary>
        /// <param name="theCondition">the Condition callback which is evaluated before writing a log</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        public void VerboseWhen(Func<bool> theCondition, string theMessageTemplate)
        {
            if (theCondition == null)
            {
                return;    
            }
            
            if (theCondition())
            {
                Verbose(theMessageTemplate);
            }
        }

        /// <summary>
        /// Writes verbose level log to serilog framework.
        /// </summary>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        public void Verbose(string theMessageTemplate)
        {
            using (LogContext.PushProperty(LoggingSetup.CallSiteConst, GetCallSight()))
            using (LogContext.PushProperty(LoggingSetup.ElapsedConst, GetElapsed(DateTime.UtcNow - myStartTime)))
            {
                myLogger.Verbose(theMessageTemplate);
            }
        }


        /// <summary>
        /// Writes verbose level log to serilog framework only when theCondition evaluates to true.
        /// </summary>
        /// <param name="theCondition">the Condition callback which is evaluated before writing a log</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        /// <param name="theFormatArgs">Positional Formatting arguments used for formatting the log event</param>
        public void VerboseWhen(Func<bool> theCondition, Func<string> theMessageTemplate, Func<object[]> theFormatArgs)
        {
            if (theCondition == null || theMessageTemplate == null || theFormatArgs == null)
            {
                return;    
            }

            if (theCondition())
            {
                Verbose(theMessageTemplate(), theFormatArgs());
            }
        }

        /// <summary>
        /// Writes verbose level log to serilog framework only when Elapsed has crossed the deadline
        /// </summary>
        /// <param name="theDeadline">the deadline time to check for</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        /// <param name="theFormatArgs">Positional Formatting arguments used for formatting the log event</param>
        public void VerboseOverTime(TimeSpan theDeadline, Func<string> theMessageTemplate, Func<object[]> theFormatArgs)
        {
            if (theMessageTemplate == null || theFormatArgs == null)
            {
                return;
            }
            
            if (this.Elapsed > theDeadline)
            {
                Verbose(theMessageTemplate(), theFormatArgs());
            }
        }

        /// <summary>
        /// Writes verbose level log to serilog framework.
        /// </summary>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        /// <param name="theFormatArgs">Positional Formatting arguments used for formatting the log event</param>
        public void Verbose(string theMessageTemplate, params object[] theFormatArgs)
        {
            if (!myLogger.IsEnabled(LogEventLevel.Verbose))
            {
                return;
            }

            using (LogContext.PushProperty(LoggingSetup.CallSiteConst, GetCallSight()))
            using (LogContext.PushProperty(LoggingSetup.ElapsedConst, GetElapsed(DateTime.UtcNow - myStartTime)))
            {
                myLogger.Verbose(theMessageTemplate, theFormatArgs);
            }
        }

        #endregion

        #region Debug

        /// <summary>
        /// Writes debug level log to serilog framework only when Elapsed has crossed the deadline
        /// </summary>
        /// <param name="theDeadline">the deadline time to check for</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        public void DebugOverTime(TimeSpan theDeadline, string theMessageTemplate)
        {
            if (this.Elapsed > theDeadline)
            {
                Debug(theMessageTemplate);
            }
        }

        /// <summary>
        /// Writes debug level log to serilog framework only when theCondition evaluates to true.
        /// </summary>
        /// <param name="theCondition">the Condition callback which is evaluated before writing a log</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        public void DebugWhen(Func<bool> theCondition, string theMessageTemplate)
        {
            if (theCondition == null)
            {
                return;    
            }

            if (theCondition())
            {
                Debug(theMessageTemplate);
            }
        }


        /// <summary>
        /// Writes debug level log to serilog framework.
        /// </summary>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        public void Debug(string theMessageTemplate)
        {
            if (!myLogger.IsEnabled(LogEventLevel.Debug))
            {
                return;
            }

            using (LogContext.PushProperty(LoggingSetup.CallSiteConst, GetCallSight()))
            using (LogContext.PushProperty(LoggingSetup.ElapsedConst, GetElapsed(DateTime.UtcNow - myStartTime)))
            {
                myLogger.Debug(theMessageTemplate);
            }
        }

        /// <summary>
        /// Writes debug level log to serilog framework only when theCondition evaluates to true.
        /// </summary>
        /// <param name="theCondition">the Condition callback which is evaluated before writing a log</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        /// <param name="theFormatArgs">Positional Formatting arguments used for formatting the log event</param>
        public void DebugWhen(Func<bool> theCondition, Func<string> theMessageTemplate, Func<object[]> theFormatArgs)
        {
            if (theCondition == null || theMessageTemplate == null || theFormatArgs == null)
            {
                return;    
            }

            if (theCondition())
            {
                Debug(theMessageTemplate(), theFormatArgs());
            }
        }

        /// <summary>
        /// Writes debug level log to serilog framework only when Elapsed has crossed the deadline
        /// </summary>
        /// <param name="theDeadline">the deadline time to check for</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        /// <param name="theFormatArgs">Positional Formatting arguments used for formatting the log event</param>
        public void DebugOverTime(TimeSpan theDeadline, Func<string> theMessageTemplate, Func<object[]> theFormatArgs)
        {
            if (theMessageTemplate == null || theFormatArgs == null)
            {
                return;    
            }

            if (this.Elapsed > theDeadline)
            {
                Debug(theMessageTemplate(), theFormatArgs());
            }
        }


        /// <summary>
        /// Writes debug level log to serilog framework.
        /// </summary>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        /// <param name="theFormatArgs">Positional Formatting arguments used for formatting the log event</param>
        public void Debug(string theMessageTemplate, params object[] theFormatArgs)
        {
            if (!myLogger.IsEnabled(LogEventLevel.Debug))
            {
                return;
            }

            using (LogContext.PushProperty(LoggingSetup.CallSiteConst, GetCallSight()))
            using (LogContext.PushProperty(LoggingSetup.ElapsedConst, GetElapsed(DateTime.UtcNow - myStartTime)))
            {
                myLogger.Debug(theMessageTemplate, theFormatArgs);
            }
        }

        #endregion

        #region Information

        /// <summary>
        /// Writes informational level log to serilog framework only when Elapsed has crossed the deadline
        /// </summary>
        /// <param name="theDeadline">the deadline time to check for</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        public void InfoWhenOverTime(TimeSpan theDeadline, string theMessageTemplate)
        {
            if (this.Elapsed > theDeadline)
            {
                Info(theMessageTemplate);
            }
        }

        /// <summary>
        /// Writes informational level log to serilog framework only when theCondition evaluates to true.
        /// </summary>
        /// <param name="theCondition">the Condition callback which is evaluated before writing a log</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        public void InfoWhen(Func<bool> theCondition, string theMessageTemplate)
        {
            if (theCondition == null)
            {
                return;    
            }

            
            if (theCondition())
            {
                Info(theMessageTemplate);
            }
        }

        /// <summary>
        /// Writes informational level log to serilog framework.
        /// </summary>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        public void Info(string theMessageTemplate)
        {
            if (!myLogger.IsEnabled(LogEventLevel.Information))
            {
                return;
            }

            using (LogContext.PushProperty(LoggingSetup.CallSiteConst, GetCallSight()))
            using (LogContext.PushProperty(LoggingSetup.ElapsedConst, GetElapsed(DateTime.UtcNow - myStartTime)))
            {
                myLogger.Information(theMessageTemplate);
            }
        }

        /// <summary>
        /// Writes informational level log to serilog framework only when theCondition evaluates to true.
        /// </summary>
        /// <param name="theCondition">the Condition callback which is evaluated before writing a log</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        /// <param name="theFormatArgs">Positional Formatting arguments used for formatting the log event</param>
        public void InfoWhen(Func<bool> theCondition, Func<string> theMessageTemplate, Func<object[]> theFormatArgs)
        {
            if (theCondition == null || theMessageTemplate == null || theFormatArgs == null)
            {
                return;    
            }

            if (theCondition())
            {
                Info(theMessageTemplate(), theFormatArgs());
            }
        }

        /// <summary>
        /// Writes informational level log to serilog framework only when Elapsed has crossed the deadline
        /// </summary>
        /// <param name="theDeadline">the deadline time to check for</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        /// <param name="theFormatArgs">Positional Formatting arguments used for formatting the log event</param>
        public void InfoWhenOverTime(TimeSpan theDeadline, Func<string> theMessageTemplate, Func<object[]> theFormatArgs)
        {
            if (theMessageTemplate == null || theFormatArgs == null)
            {
                return;    
            }

            if (this.Elapsed > theDeadline)
            {
                Info(theMessageTemplate(), theFormatArgs());
            }
        }

        /// <summary>
        /// Writes informational level log to serilog framework.
        /// </summary>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        /// <param name="theFormatArgs">Positional Formatting arguments used for formatting the log event</param>
        public void Info(string theMessageTemplate, params object[] theFormatArgs)
        {
            if (!myLogger.IsEnabled(LogEventLevel.Information))
            {
                return;
            }

            using (LogContext.PushProperty(LoggingSetup.CallSiteConst, GetCallSight()))
            using (LogContext.PushProperty(LoggingSetup.ElapsedConst, GetElapsed(DateTime.UtcNow - myStartTime)))
            {
                myLogger.Information(theMessageTemplate, theFormatArgs);
            }
        }

        #endregion

        #region Warning

        /// <summary>
        /// Writes warning level log to serilog framework only when Elapsed has crossed the deadline
        /// </summary>
        /// <param name="theDeadline">the deadline time to check for</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        public void WarningOverTime(TimeSpan theDeadline, string theMessageTemplate)
        {
            if (this.Elapsed > theDeadline)
            {
                Warning(theMessageTemplate);
            }
        }

        /// <summary>
        /// Writes warning level log to serilog framework only when theCondition evaluates to true.
        /// </summary>
        /// <param name="theCondition">the Condition callback which is evaluated before writing a log</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        public void WarningWhen(Func<bool> theCondition, string theMessageTemplate)
        {
            if (theCondition == null)
            {
                return;    
            }

            if (theCondition())
            {
                Warning(theMessageTemplate);
            }
        }

        /// <summary>
        /// Writes warning level log to serilog framework.
        /// </summary>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        public void Warning(string theMessageTemplate)
        {
            if (!myLogger.IsEnabled(LogEventLevel.Warning))
            {
                return;
            }

            using (LogContext.PushProperty(LoggingSetup.CallSiteConst, GetCallSight()))
            using (LogContext.PushProperty(LoggingSetup.ElapsedConst, GetElapsed(DateTime.UtcNow - myStartTime)))
            {
                myLogger.Warning(theMessageTemplate);
            }
        }

        /// <summary>
        /// Writes warning level log to serilog framework only when theCondition evaluates to true.
        /// </summary>
        /// <param name="theCondition">the Condition callback which is evaluated before writing a log</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        /// <param name="theFormatArgs">Positional Formatting arguments used for formatting the log event</param>
        public void WarningWhen(Func<bool> theCondition, Func<string> theMessageTemplate, Func<object[]> theFormatArgs)
        {
            if (theCondition == null || theMessageTemplate == null || theFormatArgs == null)
            {
                return;    
            }

            if (theCondition())
            {
                Warning(theMessageTemplate(), theFormatArgs());
            }
        }


        /// <summary>
        /// Writes warning level log to serilog framework only when Elapsed has crossed the deadline
        /// </summary>
        /// <param name="theDeadline">the deadline time to check for</param>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        /// <param name="theFormatArgs">Positional Formatting arguments used for formatting the log event</param>
        public void WarningOverTime(TimeSpan theDeadline, Func<string> theMessageTemplate, Func<object[]> theFormatArgs)
        {
            if (theMessageTemplate == null || theFormatArgs == null)
            {
                return;    
            }

            if (this.Elapsed > theDeadline)
            {
                Warning(theMessageTemplate(), theFormatArgs());
            }
        }

        /// <summary>
        /// Writes warning level log to serilog framework.
        /// </summary>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        /// <param name="theFormatArgs">Positional Formatting arguments used for formatting the log event</param>
        public void Warning(string theMessageTemplate, params object[] theFormatArgs)
        {
            if (!myLogger.IsEnabled(LogEventLevel.Warning))
            {
                return;
            }

            using (LogContext.PushProperty(LoggingSetup.CallSiteConst, GetCallSight()))
            using (LogContext.PushProperty(LoggingSetup.ElapsedConst, GetElapsed(DateTime.UtcNow - myStartTime)))
            {
                myLogger.Warning(theMessageTemplate, theFormatArgs);
            }
        }

        #endregion

        #region Error

        /// <summary>
        /// Writes error level log to serilog framework.
        /// </summary>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        public void Error(string theMessageTemplate)
        {
            if (!myLogger.IsEnabled(LogEventLevel.Error))
            {
                return;
            }

            using (LogContext.PushProperty(LoggingSetup.CallSiteConst, GetCallSight()))
            using (LogContext.PushProperty("Method", myCallerName))
            using (LogContext.PushProperty(LoggingSetup.ElapsedConst, GetElapsed(DateTime.UtcNow - myStartTime)))
            {
                myLogger.Error(theMessageTemplate);
            }
        }

        /// <summary>
        /// Writes error level log to serilog framework.
        /// </summary>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        /// <param name="theFormatArgs">Positional Formatting arguments used for formatting the log event</param>
        public void Error(string theMessageTemplate, params object[] theFormatArgs)
        {
            if (!myLogger.IsEnabled(LogEventLevel.Error))
            {
                return;
            }

            using (LogContext.PushProperty(LoggingSetup.CallSiteConst, GetCallSight()))
            using (LogContext.PushProperty("Method", myCallerName))
            using (LogContext.PushProperty(LoggingSetup.ElapsedConst, GetElapsed(DateTime.UtcNow - myStartTime)))
            {
                myLogger.Error(theMessageTemplate, theFormatArgs);
            }
        }

        #endregion

        #region Fatal

        /// <summary>
        /// Writes fatal level log to serilog framework.
        /// </summary>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        public void Fatal(string theMessageTemplate)
        {
            if (!myLogger.IsEnabled(LogEventLevel.Fatal))
            {
                return;
            }

            using (LogContext.PushProperty(LoggingSetup.CallSiteConst, GetCallSight()))
            using (LogContext.PushProperty("Method", myCallerName))
            using (LogContext.PushProperty(LoggingSetup.ElapsedConst, GetElapsed(DateTime.UtcNow - myStartTime)))
            {
                myLogger.Fatal(theMessageTemplate);
            }
        }

        /// <summary>
        /// Writes fatal level log to serilog framework.
        /// </summary>
        /// <param name="theMessageTemplate">Message template describing the log event.</param>
        /// <param name="theFormatArgs">Positional Formatting arguments used for formatting the log event</param>
        public void Fatal(string theMessageTemplate, params object[] theFormatArgs)
        {
            if (!myLogger.IsEnabled(LogEventLevel.Fatal))
            {
                return;
            }

            using (LogContext.PushProperty(LoggingSetup.CallSiteConst, GetCallSight()))
            using (LogContext.PushProperty("Method", myCallerName))
            using (LogContext.PushProperty(LoggingSetup.ElapsedConst, GetElapsed(DateTime.UtcNow - myStartTime)))
            {
                myLogger.Fatal(theMessageTemplate, theFormatArgs);
            }
        }

        /// <summary>
        /// Logs out trace when enabled. Otherwise, does nothing
        /// </summary>
        public void Dispose()
        {
            if (LoggingSetup.InOutEnabled)
            {
                this.Info($"[O] Exiting {myCallerName}");
            }
        }

        #endregion

        private string GetCallSight()
        {
            return $"{myDetail.ScopeName}.{myCallerName}";
        }

        private static string GetElapsed(TimeSpan t)
        {
            if (t.TotalMinutes >= 10)
                return ">10M";

            if (t.TotalMilliseconds < 1)
                return "<1ms";

            if (t.TotalMilliseconds < 2500)
                return $"{Math.Round(t.TotalMilliseconds, 1)}ms";

            if (t.TotalSeconds < 10)
                return $"{t.TotalSeconds:F2}S";

            if (t.TotalSeconds < 60)
                return $"{t.TotalSeconds:F2}S";

            if (t.TotalMinutes < 2)
                return $"{t.TotalMinutes:F2}M";

            if (t.TotalMinutes < 10)
                return $"{t.TotalMinutes:F1}M";

            return ">10M";
        }
    }
}