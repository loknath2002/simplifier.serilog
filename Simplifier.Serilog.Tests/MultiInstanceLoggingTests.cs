using System.Reflection;
using Serilog.Events;
namespace Simplifier.Serilog.Tests
{
    [TestFixture]
    public class MultiInstanceLoggingTests
    {
        [OneTimeTearDown]
        public void Teardown()
        {
            LoggingSetup.ChangeLogLevel(LoggingConfiguration.ProdConfig.LogEventLevel);
            typeof(LoggerDetail)
                .GetField("mySingleLogger", BindingFlags.Static | BindingFlags.NonPublic)
                ?.SetValue(null, null);
        }

        [OneTimeSetUp]
        public void Setup()
        {
            foreach (string aFile in Directory.EnumerateFiles("./", "*.mlog"))
            {
                if (File.Exists(aFile))
                {
                    File.Delete(aFile);
                }
            }

            typeof(LoggerDetail)
                .GetField("mySingleLogger", BindingFlags.Static | BindingFlags.NonPublic)
                ?.SetValue(null, null);

            var aConf = LoggingConfiguration.ProdConfig;
            aConf.LogEventLevel = LogEventLevel.Verbose;
            aConf.FileName = "MultiProc_.mlog";
            MultiInstanceLoggingSetup.Instance.Create(aConf);
            MultiInstanceLoggingSetup.Instance.AddSubLogger("App.StorageModule", new SubLoggingConfiguration
            {
                LogEventLevel = LogEventLevel.Verbose,
            });

            MultiInstanceLoggingSetup.Instance.AddSubLogger("App.DataModule", new SubLoggingConfiguration
            {
                LogEventLevel = LogEventLevel.Debug,
            });

            MultiInstanceLoggingSetup.Instance.AddSubLogger("App.BusinessModule", new SubLoggingConfiguration
            {
                LogEventLevel = LogEventLevel.Error,
            });

            using (var aScope = new LogScope(new LoggerDetail(typeof(LoggingTests))))
            {
                aScope.Info("Create The File...");
            }

            myTargetFile = Directory.EnumerateFiles("./", "MultiProc_*.mlog").FirstOrDefault()!;
            LoggingSetup.ChangeLogLevel(LogEventLevel.Verbose);
        }
        private string myTargetFile;

        [Test]
        public void Verify_SubLogger_Is_WorkingWithGlobalInstance()
        {
            using (var aScope = new LogScope(new LoggerDetail(typeof(LoggingTests))))
            {
                aScope.Info("Processing execute request for a total of 999 items.");
                aScope.Debug("Attempting to connect to the @{Database2}");
                aScope.Warning("Connection entered to busy status, retrying...");
                aScope.Verbose("Retrying connection to database, attempt number 1, wait time 25ms");
                aScope.Error("Connection timed out, execute failed. Try again.");
                aScope.Fatal("Critical error during execute function. Exit the application.");
            }

            string aFuncName = "Simplifier.Serilog.Tests.LoggingTests.Verify_SubLogger_Is_WorkingWithGlobalInstance()";
            string[] aLines = ReadLogFile();
            LoggingTests.AssertLinePresent(aLines, "Processing execute request for a total of 999 items.", "INF", aFuncName);
            LoggingTests.AssertLinePresent(aLines, "Attempting to connect to the @{Database2}", "DBG", aFuncName);
            LoggingTests.AssertLinePresent(aLines, "Connection entered to busy status, retrying...", "WRN", aFuncName);
            LoggingTests.AssertLinePresent(aLines, "Retrying connection to database, attempt number 1, wait time 25ms", "VRB", aFuncName);
            LoggingTests.AssertLinePresent(aLines, "Connection timed out, execute failed. Try again.", "ERR", aFuncName);
            LoggingTests.AssertLinePresent(aLines, "Critical error during execute function. Exit the application.", "FTL", aFuncName);
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
}