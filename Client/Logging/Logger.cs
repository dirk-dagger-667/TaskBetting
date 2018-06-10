using System.IO;

namespace Client.Logging
{
    public class Logger : ILogger
    {
        private readonly string logFileName;

        protected static readonly object lockObject = new object();

        public Logger(string logFileName)
        {
            this.logFileName = logFileName;
        }

        public void Log(string message)
        {
            string destPath = Path.Combine(Path.GetFullPath(@"..\..\"), this.logFileName);

            lock (lockObject)
            {
                using (var logWriter = new StreamWriter(destPath, true))
                {
                    logWriter.WriteLine(message);
                    logWriter.Close();
                }
            }
        }
    }
}
