# Simplifier For Serilog

Provides a simple way to get Serilog Up and running with additional extensions and namespace-based log filtering with multi-file creation to Serilog API enhancing the logging experience.

## Enhancements

You get all the features of Serilog + the following

1. Adds namespace-based logging feature, the application now can change settings for each namespace independently. [See Here](#namespace-based-logging)
2. Allows creation of multiple log files from the same application, each module in the application can have its own log file. [See Here](#multi-file-logging)
3. Enriches the logs with additional details, i.e. **Namespace Name, Class name and Function name.**
4. Enriches the logs enabling applications to **measure time** consumed by each method at the instant of logging. (10ms in the example denotes that the function has spend 10ms until it reaches the log statement)
5. Enriches the logs with In-Out Logs. This allows detailed tracing of each function entry and exit trace.
6. Provides various API's to conditionally write the log, reducing complexity in the calling code and log file size.
7. Zero Setup required as the Logging configuration templates are shipped out of the box

## Quick Start

Add the nuget package to your project either via nuget explorer or by running ```dotnet add package Simplifier.Serilog```

The following code will log a single Information Trace in a pre-defined format to `MyApplication_YYYYMMDD.slog`. The file will be created in the working directory of the executing application.

```cs
using (LogScope aScope = new LogScope(new LoggerDetail("DemoUnOptimizedLogger")))
{
    aScope.Info("Hello From Serilogger");
}
```

However, the above example can further be optimized to reuse LoggingDetail, and in fact one LogScope can be used to log multiple log statements at once. A more realistic example is shown below.

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
|---------------|----------------|------------------------------------------------------|
| 1             | Date           | Standard Date Column                                 |
| 2             | Time           | Standard Local Time Column                           |
| 3             | Level          | 3 Character, Serilog Trace Level.                    |
| 4             | ThreadID       | ID Of the executing thread                           |
| 5             | Execution Time | Time consumed prior to reaching the trace statement. |
| 6             | CallSite       | Namespace + Function Name of the logger              |
| 7             | Log            | The full log as written by the application           |

## Customization and Configurations

The library has few preset templates which are present in ```LoggingConfiguration``` as static members.

However, library provides allows the consumers to change multiple settings as per the need and this is achieved by invoking ```LoggingSetup.Create(new LoggingConfiguration() { <Set the Properties as required> });```, **ensure that this call is made at the startup of the application before any logs are written.**

## Namespace Based Logging

This feature allows the application to configure different log levels for different namespaces. To use this feature, we need to make use of `MultiInstanceLoggingSetup`

```
var aLoggerInstance = new MultiInstanceLoggingSetup(LoggingConfiguration.ProdConfig);
aLoggerInstance.AddSubLogger("App.StorageModule", new SubLoggingConfiguration
{
    LogEventLevel = LogEventLevel.Information,
});

aLoggerInstance.AddSubLogger("App.BusinessModule", new SubLoggingConfiguration
{
    LogEventLevel = LogEventLevel.Error,
});

// Any LogDetail with namespace App.BusinessModule will be logged at Error Level
// Any LogDetail with namespace App.StorageModule will be logged at Information Level
using (var aScope = new LogScope(new LoggerDetail("App.StorageModule.SQLiteStorage"))),
{
    aScope.Info("Connected to SQLite Database"); // Will be logged
    aScope.Verbose("Connection ID for transaction is 1234567890"); // Will NOT be logged
}

using (var aScope = new LogScope(new LoggerDetail("App.BusinessModule.DiscountRules"))),
{
    aScope.Info("A discount of 5% deduced"); // Will NOT be logged
    aScope.Error("Discount application failed. The Customer has already used a coupon."); // Will be logged 
}

// Dynamically changing the log level for a namespace
aLoggerInstance.ChangeLogLevel("App.BusinessModule", LogEventLevel.Info);
using (var aScope = new LogScope(new LoggerDetail("App.BusinessModule.DiscountRules"))),
{
    aScope.Info("A discount of 5% deduced"); // Viola : Will be logged
    aScope.Error("Discount application failed. The Customer has already used a coupon."); // Will be logged 
}
```
### Performance Considerations

Each time a new instance of LoggerDetail is created internally, a search is triggered. If there are 5000 subloggers this search can take upto 0.5ms. This maybe be small but something to keep an eye on. The recommendation is to create LoggerDetail once per class as a static readonly instance which negates this perfomance impact.

## Multi-File Logging

This feature allows the application to emit logs to multiple log files based on the instance names.

```
var aDefaultConfig = LoggingConfiguration.ProdConfig;
var aInstanceForBusiness = new MultiInstanceLoggingSetup(aDefaultConfig); // Recommended : Default instance, if everything else fails your trace will be atleast logged here.

aDefaultConfig.FileName = "MyApplication_Business_.log";
var aInstanceForBusiness = new MultiInstanceLoggingSetup(aDefaultConfig, "Business"); // Note the second parameter

aDefaultConfig.FileName = "MyApplication_Storage_.log"; // Omited other properties, at minimum we need to change the file name.
var aInstanceForStorage = new MultiInstanceLoggingSetup(aDefaultConfig, "Storage"); // Note the second parameter

// When creating a LoggerDetail we have to pass the instance name as well.
var aLogDetailForStorage = new LoggerDetail("App.StorageModule.SQLiteStorage", "Storage");
using (var aScope = new LogScope(aLogDetailForStorage)),
{
    // All of these will go to MyApplication_Storage_*.log file
    aScope.Info("Connected to SQLite Database"); // Will be logged
}

var aLogDetailWithoutName = new LoggerDetail("App.BusinessModule.DiscountRules");
using (var aScope = new LogScope(new LoggerDetail("App.BusinessModule.DiscountRules"))),
{
    // All of these will go to MyApplication_*.log file as the instance name is not provided
    aScope.Info("User forgot to enter the instance name!"); 
}
```

## Changelog
### 2.0.0

1. Added multi-file logging feature
2. Added a namespace-based logging level filters.

### 1.0.5

1. Upgrade serilog version to 4.4.0

### 1.0.4

1. Upgrade serilog version to 4.3.1
2. Fixed bug where Error API was logging at Fatal Level

### 1.0.3

Added Overloads for all the API's to accept exception object

### 1.0.2

1. Added documentation to the nuget package.
2. Changed Default Log Format to provide 60 Characters for Callsite.

### 1.0.1

Changed LogScope to class to improve compatibility.

### 1.0.0

Initial release