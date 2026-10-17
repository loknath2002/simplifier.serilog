using System.Reflection;
using Serilog.Events;
namespace Simplifier.Serilog.Tests
{
    [TestFixture]
    public class MultiInstanceMultiFileLoggingTests
    {
        private Lock myFileReadLock = new Lock();
        private string myTargetFile1;
        private string myTargetFile2;
        
        private MultiInstanceLoggingSetup myLoggingInstance1;
        private MultiInstanceLoggingSetup myLoggingInstance2;

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
            foreach (string aFile in Directory.EnumerateFiles("./", "*.xlog"))
            {
                if (File.Exists(aFile))
                {
                    File.Delete(aFile);
                }
            }
            
            foreach (string aFile in Directory.EnumerateFiles("./", "*.ylog"))
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
            aConf.FileName = "MultiProc_.xlog";
            myLoggingInstance1 = new MultiInstanceLoggingSetup(aConf);
            
            aConf.FileName = "MultiProc_.ylog";
            myLoggingInstance2 = new MultiInstanceLoggingSetup(aConf, "NonDefault");
            using (var aScope = new LogScope(new LoggerDetail(typeof(LoggingTests))))
            {
                aScope.Info("Create The XFile...");
            }
            
            using (var aScope = new LogScope(new LoggerDetail(typeof(LoggingTests), "NonDefault")))
            {
                aScope.Info("Create The YFile...");
            }

            myTargetFile1 = Directory.EnumerateFiles("./", "MultiProc_*.xlog").FirstOrDefault()!;
            myTargetFile2 = Directory.EnumerateFiles("./", "MultiProc_*.ylog").FirstOrDefault()!;
        }
        
        [Test]
        public void Verify_SubLogger_Is_WorkingWithNamed_Instances()
        {
            using (var aScopeX = new LogScope(new LoggerDetail(typeof(LoggingTests))))
            using (var aScopeY = new LogScope(new LoggerDetail(typeof(LoggingTests), "NonDefault")))
            {
                aScopeX.Info("Processing execute request for a total of 998 items.");
                aScopeY.Info("Processing execute request for a total of 1005 items.");
            }

            string aFuncName = "Simplifier.Serilog.Tests.LoggingTests.Verify_SubLogger_Is_WorkingWithNamed_Instances()";
            string[] aLines = ReadLogFile(myTargetFile1);
            LoggingTests.AssertLinePresent(aLines, "Processing execute request for a total of 998 items.", "INF", aFuncName);
            LoggingTests.AssertLineAbsent(aLines, "Processing execute request for a total of 1005 items.");
            
            aLines = ReadLogFile(myTargetFile2);
            LoggingTests.AssertLinePresent(aLines, "Processing execute request for a total of 1005 items.", "INF", aFuncName);
            LoggingTests.AssertLineAbsent(aLines, "Processing execute request for a total of 998 items.");
        }
        

        private string[] ReadLogFile(string theName)
        {
            lock (myFileReadLock)
            {
                string aCopy = "LogCopy.log";
                File.Copy(theName, aCopy, true);
                string[] aFileLines = File.ReadAllLines(aCopy);
                return aFileLines;
            }
        }
    }
}