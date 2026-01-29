# Simplifier For Serilog

Provides simple way to get Serilog Up and running with additional extensions to Serilog API enhancing logging experience.

## Enhancements

You get all the features of Serilog + the following

1. Enriches the logs with additional details, i.e **Namespace Name, Class name and Function name.**
2. Enriches the logs enabling applications to **measure time** consumed by each methods at the instant of logging. (10ms in the example denotes that the function has spend 10ms until it reaches the log statement)
3. Enriches the logs with In-Out Logs. This allows detailed tracing of each functions enter and exit traces.
4. Provides various API's to conditionally write the log, reducing complexity in the calling code and log file size.
5. Zero Setup required as the Logging configuration templates are shipped out of the box

## Quick Start

Add the nuget package to your project either via nuget explorer or by running ```dotnet add package Simplifier.Serilog```

The following code will log a single Information Trace in a pre-defined format to `MyApplication_YYYYMMDD.slog`. The file will be created in the working directory of the executing application.

```cs
using (LogScope aScope = new LogScope(new LoggerDetail("DemoUnOptimizedLogger")))
{
    aScope.Info("Hello From Serilogger");
}
```

However the above example can further be optimized to reuse LoggingDetail and infact one LogScope can be used to log multiple log statements at once. A more realistic example is shown below.

```cs
namespace Simplifier.Serilog.Sample
{
    public class DemoClass
    {
        // Declare the logger detail in the class
        private static readonly LoggerDetail myLoggerDetail = new LoggerDetail(typeof(DemoClass));

        public void Execute()
        {
            // Log the required info by creation LogScope withing class methods
            using (LogScope aScope = new LogScope(new LoggerDetail(this.GetType())))
            {
                aScope.Info("Processing execute request for a total of 408 items.");
                // Do Work

                aScope.Warning("Connection returned with busy status, retrying...");

                // Do Work
                aScope.Error("Connection timed out, execute failed");

                // Do Work
                aScope.Fatal("Critical error during execute function.");
            }
        }
    }
}
```

## Runtime Switches

1.```LoggingSetup.ChangeLogLevel();``` allows the application to change the Log Verbosity at runtime.
1.```LoggingSetup.InOutEnabled``` allows the application to dump IN-OUT traces at runtime.

## Default Configuration

The default configuration used ```LoggingConfiguration.ProdConfig``` which equates to **Informational and higher level logs will be written to MyApplication_.log, which rolls over every day upto 14 days, After which the oldest file will be deleted.**

```cs
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
```

## Log Format

By default the log file will produce the following columns


| Column Number | Name           | Description                                          |
| --------------- | ---------------- | ------------------------------------------------------ |
| 1             | Date           | Standard Date Column                                 |
| 2             | Time           | Standard Local Time Column                           |
| 3             | Level          | 3 Character, Serilog Trace Level.                    |
| 4             | ThreadID       | ID Of the executing thread                           |
| 5             | Executing Time | Time consumed prior to reaching the trace statement. |
| 6             | CallSite       | Namespace + Function Name of the logger              |
| 7             | Log            | The full log as written by the application           |

## Customization and Configurations

The library has few preset templates which are present in ```LoggingConfiguration``` as static members.

However library provides allows the consumers to change multiple settings as per the need and this is achieved by invoking ```LoggingSetup.Create(new LoggingConfiguration() { <Set the Properties as required> });```, **ensure that this call is made at the startup of the application before any logs are written.**
