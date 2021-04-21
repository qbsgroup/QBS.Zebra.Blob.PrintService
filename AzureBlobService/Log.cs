using System.IO;

namespace AzureBlobService
{
    class Log
    {
        public Log() { }

        public void Add(string contents)
        {
            Config config = new Config();
            string logFile = config.GetLogFile();
            FileStream fs = new FileStream(logFile, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine(contents + " @ " + System.DateTime.Now.ToString());
            sw.Flush();
            sw.Close();
        }
    }
}