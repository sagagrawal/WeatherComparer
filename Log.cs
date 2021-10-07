using System;
using System.IO;

namespace BlueStacksAssignment
{
    public class Log
    {
        private StreamWriter _log;

        public Log(string fileaName, bool append)
        {
            _log = new StreamWriter(fileaName, append);
        }

        public Log(string fileName) : this(fileName, false)
        {

        }

        public void Dispose()
        {
            if(_log != null)
                _log.Close();
        }

        public void WriteLine(string message, LogType logType)
        {
            _log.WriteLine(string.Format("{0} | {1} | {2}", DateTime.Now, logType, message));
            _log.Flush();
        }

        public void WriteLine(string message)
        {
            WriteLine(message, LogType.INFO);
        }
    }

    public enum LogType
    {
        DEBUG,
        INFO,
        ERROR
    }
}
