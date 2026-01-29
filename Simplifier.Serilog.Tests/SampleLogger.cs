namespace Simplifier.Serilog.Sample
{
    public class DemoClass
    {
        // Declare the logger detail in the class
        private static readonly LoggerDetail myLoggerDetail = new LoggerDetail(typeof(DemoClass));

        public void Execute()
        {
            // Log the required info by creation LogScope withing class methods
            using (LogScope aScope = new LogScope(myLoggerDetail))
            {
                aScope.Info("Processing execute request for a total of 408 items.");
                aScope.Debug("Attempting to connect to the @{Database}");
                aScope.Warning("Connection returned with busy status, retrying...");
                aScope.Verbose("Retrying connection to database, attempt number 4, wait time 25ms");
                aScope.Error("Connection timed out, execute failed");
                aScope.Fatal("Critical error during execute function.");
            }

            using (LogScope aScope = new LogScope(new LoggerDetail("DemoUnOptimizedLogger")))
            {
                aScope.Info("Hello From Serilogger");
            }
        }
    }
}