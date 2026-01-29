using Serilog;
using Serilog.Context;
using Serilog.Events;
using Simplifier.Serilog.Sample;
namespace Simplifier.Serilog.Tests;

public class LoggingTests
{
    private static readonly LoggerDetail myLoggerDetail = new LoggerDetail(typeof(LoggingTests));
    
    private string myTargetFile;

    [OneTimeTearDown]
    public void Teardown()
    {
        LoggingSetup.ChangeLogLevel(LoggingConfiguration.ProdConfig.LogEventLevel);
        new DemoClass().Execute();
    }
    
    [OneTimeSetUp]
    public void Setup()
    {
        foreach (var aFile in Directory.EnumerateFiles("./", "*.slog"))
        {
            if (File.Exists(aFile))
            {
                File.Delete(aFile);
            }
        }

        LoggingSetup.Create(LoggingConfiguration.ProdConfig);
        using (LogScope aScope = new LogScope(new LoggerDetail(typeof(LoggingTests))))
        {
            aScope.Info("Warmup");
        }

        myTargetFile = Directory.EnumerateFiles("./", "*.slog").FirstOrDefault();
        LoggingSetup.ChangeLogLevel(LogEventLevel.Verbose);
    }

    [Test]
    public void Verify_All_Parts_Of_Logs_Are_Present_War_API()
    {
        using (LogScope aScope = new LogScope(new LoggerDetail(typeof(LoggingTests))))
        {
            aScope.Warning("TraceMarker1Verify_All_Parts_Of_Logs_Are_Present_War_API");
            aScope.WarningWhen(() => false, "TraceMarker2Verify_All_Parts_Of_Logs_Are_Present_War_API");
            aScope.WarningWhen(() => true, "TraceMarker3Verify_All_Parts_Of_Logs_Are_Present_War_API");
            aScope.WarningOverTime(TimeSpan.FromMilliseconds(100), "TraceMarker4Verify_All_Parts_Of_Logs_Are_Present_War_API");
            Thread.Sleep(100);
            aScope.WarningOverTime(TimeSpan.FromMilliseconds(100), "TraceMarker5Verify_All_Parts_Of_Logs_Are_Present_War_API");
            aScope.Warning("TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_War_API", 6);
            aScope.WarningWhen(() => false, () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_War_API", () => [7]);
            aScope.WarningWhen(() => true, () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_War_API", () => [8]);
            aScope.WarningOverTime(TimeSpan.FromMilliseconds(200), () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_War_API", () => [9]);
            Thread.Sleep(100);
            aScope.WarningOverTime(TimeSpan.FromMilliseconds(200), () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_War_API", () => [10]);
        }

        string aFuncName = "Simplifier.Serilog.Tests.LoggingTests.Verify_All_Parts_Of_Logs_Are_Present_War_API()";
        string[] aLines = ReadLogFile();
        AssertLinePresent(aLines, "TraceMarker1Verify_All_Parts_Of_Logs_Are_Present_War_API", "WRN", aFuncName);
        AssertLinePresent(aLines, "TraceMarker3Verify_All_Parts_Of_Logs_Are_Present_War_API", "WRN", aFuncName);
        AssertLinePresent(aLines, "TraceMarker5Verify_All_Parts_Of_Logs_Are_Present_War_API", "WRN", aFuncName);
        AssertLinePresent(aLines, "TraceMarker6Verify_All_Parts_Of_Logs_Are_Present_War_API", "WRN", aFuncName);
        AssertLinePresent(aLines, "TraceMarker8Verify_All_Parts_Of_Logs_Are_Present_War_API", "WRN", aFuncName);
        AssertLinePresent(aLines, "TraceMarker10Verify_All_Parts_Of_Logs_Are_Present_War_API", "WRN", aFuncName);
        AssertLineAbsent(aLines, "TraceMarker2Verify_All_Parts_Of_Logs_Are_Present_War_API");
        AssertLineAbsent(aLines, "TraceMarker4Verify_All_Parts_Of_Logs_Are_Present_War_API");
        AssertLineAbsent(aLines, "TraceMarker7Verify_All_Parts_Of_Logs_Are_Present_War_API");
        AssertLineAbsent(aLines, "TraceMarker9Verify_All_Parts_Of_Logs_Are_Present_War_API");
    }

    [Test]
    public void Verify_All_Parts_Of_Logs_Are_Present_Ftl_API()
    {
        using (LogScope aScope = new LogScope(new LoggerDetail(typeof(LoggingTests))))
        {
            aScope.Fatal("TraceMarker1Verify_All_Parts_Of_Logs_Are_Present_Ftl_API");
            aScope.Fatal("TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Ftl_API", 6);
        }

        string aFuncName = "Simplifier.Serilog.Tests.LoggingTests.Verify_All_Parts_Of_Logs_Are_Present_Ftl_API()";
        string[] aLines = ReadLogFile();
        AssertLinePresent(aLines, "TraceMarker1Verify_All_Parts_Of_Logs_Are_Present_Ftl_API", "FTL", aFuncName);
        AssertLinePresent(aLines, "TraceMarker6Verify_All_Parts_Of_Logs_Are_Present_Ftl_API", "FTL", aFuncName);
    }

    [Test]
    public void Verify_All_Parts_Of_Logs_Are_Present_Dbg_API()
    {
        using (LogScope aScope = new LogScope(new LoggerDetail(typeof(LoggingTests))))
        {
            aScope.Debug("TraceMarker1Verify_All_Parts_Of_Logs_Are_Present_Dbg_API");
            aScope.DebugWhen(() => false, "TraceMarker2Verify_All_Parts_Of_Logs_Are_Present_Dbg_API");
            aScope.DebugWhen(() => true, "TraceMarker3Verify_All_Parts_Of_Logs_Are_Present_Dbg_API");
            aScope.DebugOverTime(TimeSpan.FromMilliseconds(100), "TraceMarker4Verify_All_Parts_Of_Logs_Are_Present_Dbg_API");
            Thread.Sleep(100);
            aScope.DebugOverTime(TimeSpan.FromMilliseconds(100), "TraceMarker5Verify_All_Parts_Of_Logs_Are_Present_Dbg_API");
            aScope.Debug("TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Dbg_API", 6);
            aScope.DebugWhen(() => false, () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Dbg_API", () => [7]);
            aScope.DebugWhen(() => true, () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Dbg_API", () => [8]);
            aScope.DebugOverTime(TimeSpan.FromMilliseconds(200), () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Dbg_API", () => [9]);
            Thread.Sleep(100);
            aScope.DebugOverTime(TimeSpan.FromMilliseconds(200), () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Dbg_API", () => [10]);
        }

        string aFuncName = "Simplifier.Serilog.Tests.LoggingTests.Verify_All_Parts_Of_Logs_Are_Present_Dbg_API()";
        string[] aLines = ReadLogFile();
        AssertLinePresent(aLines, "TraceMarker1Verify_All_Parts_Of_Logs_Are_Present_Dbg_API", "DBG", aFuncName);
        AssertLinePresent(aLines, "TraceMarker3Verify_All_Parts_Of_Logs_Are_Present_Dbg_API", "DBG", aFuncName);
        AssertLinePresent(aLines, "TraceMarker5Verify_All_Parts_Of_Logs_Are_Present_Dbg_API", "DBG", aFuncName);
        AssertLinePresent(aLines, "TraceMarker6Verify_All_Parts_Of_Logs_Are_Present_Dbg_API", "DBG", aFuncName);
        AssertLinePresent(aLines, "TraceMarker8Verify_All_Parts_Of_Logs_Are_Present_Dbg_API", "DBG", aFuncName);
        AssertLinePresent(aLines, "TraceMarker10Verify_All_Parts_Of_Logs_Are_Present_Dbg_API", "DBG", aFuncName);
        AssertLineAbsent(aLines, "TraceMarker2Verify_All_Parts_Of_Logs_Are_Present_Dbg_API");
        AssertLineAbsent(aLines, "TraceMarker4Verify_All_Parts_Of_Logs_Are_Present_Dbg_API");
        AssertLineAbsent(aLines, "TraceMarker7Verify_All_Parts_Of_Logs_Are_Present_Dbg_API");
        AssertLineAbsent(aLines, "TraceMarker9Verify_All_Parts_Of_Logs_Are_Present_Dbg_API");
    }

    [Test]
    public void Verify_All_Parts_Of_Logs_Are_Present_Ver_API()
    {
        using (LogScope aScope = new LogScope(new LoggerDetail(typeof(LoggingTests))))
        {
            aScope.Verbose("TraceMarker1Verify_All_Parts_Of_Logs_Are_Present_Ver_API");
            aScope.VerboseWhen(() => false, "TraceMarker2Verify_All_Parts_Of_Logs_Are_Present_Ver_API");
            aScope.VerboseWhen(() => true, "TraceMarker3Verify_All_Parts_Of_Logs_Are_Present_Ver_API");
            aScope.VerboseOverTime(TimeSpan.FromMilliseconds(100), "TraceMarker4Verify_All_Parts_Of_Logs_Are_Present_Ver_API");
            Thread.Sleep(100);
            aScope.VerboseOverTime(TimeSpan.FromMilliseconds(100), "TraceMarker5Verify_All_Parts_Of_Logs_Are_Present_Ver_API");
            aScope.Verbose("TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Ver_API", 6);
            aScope.VerboseWhen(() => false, () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Ver_API", () => [7]);
            aScope.VerboseWhen(() => true, () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Ver_API", () => [8]);
            aScope.VerboseOverTime(TimeSpan.FromMilliseconds(200), () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Ver_API", () => [9]);
            Thread.Sleep(100);
            aScope.VerboseOverTime(TimeSpan.FromMilliseconds(200), () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Ver_API", () => [10]);
        }

        string aFuncName = "Simplifier.Serilog.Tests.LoggingTests.Verify_All_Parts_Of_Logs_Are_Present_Ver_API()";
        string[] aLines = ReadLogFile();
        AssertLinePresent(aLines, "TraceMarker1Verify_All_Parts_Of_Logs_Are_Present_Ver_API", "VRB", aFuncName);
        AssertLinePresent(aLines, "TraceMarker3Verify_All_Parts_Of_Logs_Are_Present_Ver_API", "VRB", aFuncName);
        AssertLinePresent(aLines, "TraceMarker5Verify_All_Parts_Of_Logs_Are_Present_Ver_API", "VRB", aFuncName);
        AssertLinePresent(aLines, "TraceMarker6Verify_All_Parts_Of_Logs_Are_Present_Ver_API", "VRB", aFuncName);
        AssertLinePresent(aLines, "TraceMarker8Verify_All_Parts_Of_Logs_Are_Present_Ver_API", "VRB", aFuncName);
        AssertLinePresent(aLines, "TraceMarker10Verify_All_Parts_Of_Logs_Are_Present_Ver_API", "VRB", aFuncName);
        AssertLineAbsent(aLines, "TraceMarker2Verify_All_Parts_Of_Logs_Are_Present_Ver_API");
        AssertLineAbsent(aLines, "TraceMarker4Verify_All_Parts_Of_Logs_Are_Present_Ver_API");
        AssertLineAbsent(aLines, "TraceMarker7Verify_All_Parts_Of_Logs_Are_Present_Ver_API");
        AssertLineAbsent(aLines, "TraceMarker9Verify_All_Parts_Of_Logs_Are_Present_Ver_API");
    }

    [Test]
    public void Verify_All_Parts_Of_Logs_Are_Present_Err_API()
    {
        using (LogScope aScope = new LogScope(new LoggerDetail(typeof(LoggingTests))))
        {
            aScope.Error("TraceMarker1Verify_All_Parts_Of_Logs_Are_Present_Err_API");
            aScope.Error("TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Err_API", 6);
        }

        string aFuncName = "Simplifier.Serilog.Tests.LoggingTests.Verify_All_Parts_Of_Logs_Are_Present_Err_API()";
        string[] aLines = ReadLogFile();
        AssertLinePresent(aLines, "TraceMarker1Verify_All_Parts_Of_Logs_Are_Present_Err_API", "ERR", aFuncName);
        AssertLinePresent(aLines, "TraceMarker6Verify_All_Parts_Of_Logs_Are_Present_Err_API", "ERR", aFuncName);
    }

    [Test]
    public void Verify_All_Parts_Of_Logs_Are_Present_Inf_API()
    {
        using (LogScope aScope = new LogScope(new LoggerDetail(typeof(LoggingTests))))
        {
            aScope.Info("TraceMarker1Verify_All_Parts_Of_Logs_Are_Present_Inf_API");
            aScope.InfoWhen(() => false, "TraceMarker2Verify_All_Parts_Of_Logs_Are_Present_Inf_API");
            aScope.InfoWhen(() => true, "TraceMarker3Verify_All_Parts_Of_Logs_Are_Present_Inf_API");
            aScope.InfoWhenOverTime(TimeSpan.FromMilliseconds(100), "TraceMarker4Verify_All_Parts_Of_Logs_Are_Present_Inf_API");
            Thread.Sleep(100);
            aScope.InfoWhenOverTime(TimeSpan.FromMilliseconds(100), "TraceMarker5Verify_All_Parts_Of_Logs_Are_Present_Inf_API");
            aScope.Info("TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Inf_API", 6);
            aScope.InfoWhen(() => false, () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Inf_API", () => [7]);
            aScope.InfoWhen(() => true, () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Inf_API", () => [8]);
            aScope.InfoWhenOverTime(TimeSpan.FromMilliseconds(200), () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Inf_API", () => [9]);
            Thread.Sleep(100);
            aScope.InfoWhenOverTime(TimeSpan.FromMilliseconds(200), () => "TraceMarker{0}Verify_All_Parts_Of_Logs_Are_Present_Inf_API", () => [10]);
        }

        string aFuncName = "Simplifier.Serilog.Tests.LoggingTests.Verify_All_Parts_Of_Logs_Are_Present_Inf_API()";
        string[] aLines = ReadLogFile();
        AssertLinePresent(aLines, "TraceMarker1Verify_All_Parts_Of_Logs_Are_Present_Inf_API", "INF", aFuncName);
        AssertLinePresent(aLines, "TraceMarker3Verify_All_Parts_Of_Logs_Are_Present_Inf_API", "INF", aFuncName);
        AssertLinePresent(aLines, "TraceMarker5Verify_All_Parts_Of_Logs_Are_Present_Inf_API", "INF", aFuncName);
        AssertLinePresent(aLines, "TraceMarker6Verify_All_Parts_Of_Logs_Are_Present_Inf_API", "INF", aFuncName);
        AssertLinePresent(aLines, "TraceMarker8Verify_All_Parts_Of_Logs_Are_Present_Inf_API", "INF", aFuncName);
        AssertLinePresent(aLines, "TraceMarker10Verify_All_Parts_Of_Logs_Are_Present_Inf_API", "INF", aFuncName);
        AssertLineAbsent(aLines, "TraceMarker2Verify_All_Parts_Of_Logs_Are_Present_Inf_API");
        AssertLineAbsent(aLines, "TraceMarker4Verify_All_Parts_Of_Logs_Are_Present_Inf_API");
        AssertLineAbsent(aLines, "TraceMarker7Verify_All_Parts_Of_Logs_Are_Present_Inf_API");
        AssertLineAbsent(aLines, "TraceMarker9Verify_All_Parts_Of_Logs_Are_Present_Inf_API");
    }

    [Test]
    public void Verify_In_Out_Traces_Are_Logged()
    {
        LoggingSetup.InOutEnabled = true;
        using (LogScope aScope = new LogScope(new LoggerDetail(typeof(LoggingTests))))
        {
            
        }
        
        string aFuncName = "Simplifier.Serilog.Tests.LoggingTests.Verify_In_Out_Traces_Are_Logged()";
        string[] aLines = ReadLogFile();
        AssertLinePresent(aLines, "[I] Entering Verify_In_Out_Traces_Are_Logged", "INF", aFuncName);
        AssertLinePresent(aLines, "[O] Exiting Verify_In_Out_Traces_Are_Logged", "INF", aFuncName);
        LoggingSetup.InOutEnabled = false;
    }
    
    [Test]
    public void Verify_In_Out_Traces_Are_Not_Logged()
    {
        LoggingSetup.InOutEnabled = false;
        using (LogScope aScope = new LogScope(new LoggerDetail(typeof(LoggingTests))))
        {
            
        }
        
        string aFuncName = "Simplifier.Serilog.Tests.LoggingTests.Verify_In_Out_Traces_Are_Not_Logged()";
        string[] aLines = ReadLogFile();
        AssertLineAbsent(aLines, "[I] Entering Verify_In_Out_Traces_Are_Not_Logged");
        AssertLineAbsent(aLines, "[O] Exiting Verify_In_Out_Traces_Are_Not_Logged");
    }
    
    private void AssertLineAbsent(string[] theLines, string theMatchMarker)
    {
        Assert.That(theLines.Any(x => x.Contains(theMatchMarker)), Is.False);
    }

    private void AssertLinePresent(string[] theLines, string theMatchMarker, string theLevel, string theFunctionName)
    {
        string[] aParts = theLines.FirstOrDefault(x => x.Contains(theMatchMarker))!.Split(" ", StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
        Assert.That(aParts.Length, Is.AtLeast(7));
        Assert.That(aParts[0], Does.Match(@"^\d{4}-\d{2}-\d{2}$"));
        Assert.That(aParts[1], Does.Match(@"^\d{2}:\d{2}:\d{2}\.\d{3}$"));
        Assert.That(aParts[2], Is.EqualTo(theLevel));
        Assert.That(aParts[3], Does.Match(@"^\d+$"));
        // Assert.That(aParts[4], Does.Match(
        //     @"^(>10M|<1ms|\d+ms|\d+\.\d{2}S|\d+\.\dS|\d+\.\d{2}M|\d+\.\dM)$"
        // ));
        Assert.That(aParts[5], Is.EqualTo(theFunctionName));
        Assert.That(string.Join(" ", aParts[6..]), Is.EqualTo(theMatchMarker));
    }

    private string[] ReadLogFile()
    {
        lock (myTargetFile)
        {
            string aCopy = "LogCopy.log";
            File.Copy(myTargetFile, aCopy, true);
            string[] aFileLines = File.ReadAllLines(aCopy);
            return aFileLines;
        }
    }
}