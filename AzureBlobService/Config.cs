using System;
using System.IO;

namespace AzureBlobService
{
    class Config
    {
        private string logFile;
        private string configFile;

        public Config()
        {
#if DEBUG
            string baseDir = Environment.CurrentDirectory;
#else
            string baseDir = @"C:\Program Files\AzureBlobService";
#endif
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);
            baseDir += @"\";
            logFile = baseDir + @"log.txt";
            configFile = baseDir + @"config.xml";
        }

        public string GetLogFile()
        {
            return logFile;
        }

        public string GetConfigFile()
        {
            return configFile;
        }
    }
}
