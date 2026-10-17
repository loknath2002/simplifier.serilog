using System.Reflection;
using Serilog.Events;
namespace Simplifier.Serilog.Tests
{
    [TestFixture]
    public class MultiInstanceLoggingTests
    {
        private string myTargetFile;
        
        private MultiInstanceLoggingSetup myLoggingInstance;
        
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
            myLoggingInstance = new MultiInstanceLoggingSetup(aConf);
            myLoggingInstance.AddSubLogger("App.StorageModule", new SubLoggingConfiguration
            {
                LogEventLevel = LogEventLevel.Verbose,
            });

            myLoggingInstance.AddSubLogger("App.DataModule", new SubLoggingConfiguration
            {
                LogEventLevel = LogEventLevel.Debug,
            });

            myLoggingInstance.AddSubLogger("App.BusinessModule", new SubLoggingConfiguration
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
        [Test]
        public void Verify_SubLogger_NamespaceResolves_Fuzzyly()
        {
            using (var aBusinessScope = new LogScope(new LoggerDetail("App.BusinessModule.Payment"), ""))
            using (var aDataModuleScope = new LogScope(new LoggerDetail("App.DataModule.Invoices"), ""))
            using (var aStorageModule = new LogScope(new LoggerDetail("App.StorageModule.SQLite"), ""))
            {
                aStorageModule.Verbose("ZThis is a verbose trace from storage module, This should appear. Verify_SubLogger_NamespaceResolves_Fuzzyly");
                aStorageModule.Debug("ZThis is a debug trace from storage module, This should appear. Verify_SubLogger_NamespaceResolves_Fuzzyly");
                
                aDataModuleScope.Verbose("ZThis is a verbose trace from data module, This won't appear. Verify_SubLogger_NamespaceResolves_Fuzzyly");
                aDataModuleScope.Debug("ZThis is a debug trace from data module, This should appear. Verify_SubLogger_NamespaceResolves_Fuzzyly");
                aDataModuleScope.Info("ZThis is a info trace from data module, This should appear. Verify_SubLogger_NamespaceResolves_Fuzzyly");
                
                aBusinessScope.Warning("ZThis is a warning trace from business module, This won't appear. Verify_SubLogger_NamespaceResolves_Fuzzyly");
                aBusinessScope.Error("ZThis is a error trace from business module, This should appear. Verify_SubLogger_NamespaceResolves_Fuzzyly");
                aBusinessScope.Fatal("ZThis is a fatal trace from business module, This should appear. Verify_SubLogger_NamespaceResolves_Fuzzyly");
            }

            string[] aLines = ReadLogFile();
            LoggingTests.AssertLinePresent(aLines, "ZThis is a verbose trace from storage module, This should appear. Verify_SubLogger_NamespaceResolves_Fuzzyly", "VRB", "App.StorageModule.SQLite.()");
            LoggingTests.AssertLinePresent(aLines, "ZThis is a debug trace from storage module, This should appear. Verify_SubLogger_NamespaceResolves_Fuzzyly", "DBG", "App.StorageModule.SQLite.()");

            LoggingTests.AssertLineAbsent(aLines, "ZThis is a verbose trace from data module, This won't appear. Verify_SubLogger_NamespaceResolves_Fuzzyly");
            LoggingTests.AssertLinePresent(aLines, "ZThis is a debug trace from data module, This should appear. Verify_SubLogger_NamespaceResolves_Fuzzyly", "DBG", "App.DataModule.Invoices.()");
            LoggingTests.AssertLinePresent(aLines, "ZThis is a info trace from data module, This should appear. Verify_SubLogger_NamespaceResolves_Fuzzyly", "INF", "App.DataModule.Invoices.()");
            
            LoggingTests.AssertLineAbsent(aLines, "ZThis is a warning trace from business module, This won't appear. Verify_SubLogger_NamespaceResolves_Fuzzyly");
            LoggingTests.AssertLinePresent(aLines, "ZThis is a error trace from business module, This should appear. Verify_SubLogger_NamespaceResolves_Fuzzyly", "ERR", "App.BusinessModule.Payment.()");
            LoggingTests.AssertLinePresent(aLines, "ZThis is a fatal trace from business module, This should appear. Verify_SubLogger_NamespaceResolves_Fuzzyly", "FTL", "App.BusinessModule.Payment.()");
        }

        [Test]
        public void Verify_SubLogger_TraceLevels_CanSwitchOnTheFly()
        {
            using (var aBusinessScope = new LogScope(new LoggerDetail("App.BusinessModule"), ""))
            using (var aDataModuleScope = new LogScope(new LoggerDetail("App.DataModule"), ""))
            using (var aStorageModule = new LogScope(new LoggerDetail("App.StorageModule"), ""))
            {
                aStorageModule.Verbose("XThis is a verbose trace from storage module, This should appear.");
                aStorageModule.Debug("XThis is a debug trace from storage module, This should appear.");
                
                aDataModuleScope.Verbose("XThis is a verbose trace from data module, This won't appear.");
                aDataModuleScope.Debug("XThis is a debug trace from data module, This should appear.");
                aDataModuleScope.Info("XThis is a info trace from data module, This should appear.");
                
                aBusinessScope.Warning("XThis is a warning trace from business module, This won't appear.");
                aBusinessScope.Error("XThis is a error trace from business module, This should appear.");
                aBusinessScope.Fatal("XThis is a fatal trace from business module, This should appear.");

                myLoggingInstance.ChangeLogLevel("App.BusinessModule", LogEventLevel.Warning);
                aBusinessScope.Warning("This is a warning trace from business module, This should appear after switching.");
                myLoggingInstance.ChangeLogLevel("App.BusinessModule", LogEventLevel.Error);
            }

            string[] aLines = ReadLogFile();
            LoggingTests.AssertLinePresent(aLines, "XThis is a verbose trace from storage module, This should appear.", "VRB", "App.StorageModule.()");
            LoggingTests.AssertLinePresent(aLines, "XThis is a debug trace from storage module, This should appear.", "DBG", "App.StorageModule.()");

            LoggingTests.AssertLineAbsent(aLines, "XThis is a verbose trace from data module, This won't appear.");
            LoggingTests.AssertLinePresent(aLines, "XThis is a debug trace from data module, This should appear.", "DBG", "App.DataModule.()");
            LoggingTests.AssertLinePresent(aLines, "XThis is a info trace from data module, This should appear.", "INF", "App.DataModule.()");
            
            LoggingTests.AssertLineAbsent(aLines, "XThis is a warning trace from business module, This won't appear.");
            LoggingTests.AssertLinePresent(aLines, "This is a warning trace from business module, This should appear after switching.", "WRN", "App.BusinessModule.()");
            LoggingTests.AssertLinePresent(aLines, "XThis is a error trace from business module, This should appear.", "ERR", "App.BusinessModule.()");
            LoggingTests.AssertLinePresent(aLines, "XThis is a fatal trace from business module, This should appear.", "FTL", "App.BusinessModule.()");
        }
        
        [Test]
        public void Verify_SubLogger_TraceLevels_ApplyToEachNamespace()
        {
            using (var aBusinessScope = new LogScope(new LoggerDetail("App.BusinessModule"), ""))
            using (var aDataModuleScope = new LogScope(new LoggerDetail("App.DataModule"), ""))
            using (var aStorageModule = new LogScope(new LoggerDetail("App.StorageModule"), ""))
            {
                aStorageModule.Verbose("This is a verbose trace from storage module, This should appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace");
                aStorageModule.Debug("This is a debug trace from storage module, This should appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace");
                
                aDataModuleScope.Verbose("This is a verbose trace from data module, This won't appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace");
                aDataModuleScope.Debug("This is a debug trace from data module, This should appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace");
                aDataModuleScope.Info("This is a info trace from data module, This should appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace");
                
                aBusinessScope.Warning("This is a warning trace from business module, This won't appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace");
                aBusinessScope.Error("This is a error trace from business module, This should appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace");
                aBusinessScope.Fatal("This is a fatal trace from business module, This should appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace");
            }

            string aFuncName = "Simplifier.Serilog.Tests.LoggingTests.()";
            string[] aLines = ReadLogFile();
            LoggingTests.AssertLinePresent(aLines, "This is a verbose trace from storage module, This should appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace", "VRB", "App.StorageModule.()");
            LoggingTests.AssertLinePresent(aLines, "This is a debug trace from storage module, This should appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace", "DBG", "App.StorageModule.()");

            LoggingTests.AssertLineAbsent(aLines, "This is a verbose trace from data module, This won't appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace");
            LoggingTests.AssertLinePresent(aLines, "This is a debug trace from data module, This should appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace", "DBG", "App.DataModule.()");
            LoggingTests.AssertLinePresent(aLines, "This is a info trace from data module, This should appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace", "INF", "App.DataModule.()");
            
            LoggingTests.AssertLineAbsent(aLines, "This is a warning trace from business module, This won't appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace");
            LoggingTests.AssertLinePresent(aLines, "This is a error trace from business module, This should appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace", "ERR", "App.BusinessModule.()");
            LoggingTests.AssertLinePresent(aLines, "This is a fatal trace from business module, This should appear. Verify_SubLogger_TraceLevels_ApplyToEachNamespace", "FTL", "App.BusinessModule.()");
        }
        
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