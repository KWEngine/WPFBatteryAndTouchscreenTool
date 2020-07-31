using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFBatteryAndTouchscreenTool
{
    public class LoggerSimple
    {
        public enum LogLevel { Warning, Error };

        private StreamWriter _fileHandle = null;
        private string _fileName = "log.txt";

        public LoggerSimple(string name)
        {
            try
            {
                _fileHandle = new StreamWriter(@".\" + name, true, Encoding.UTF8);
                
            }
            catch(Exception ex)
            {
                Close();
            }

        }

        public void Close()
        {
            if (_fileHandle != null)
            {
                _fileHandle.Dispose();
            }
        }

        public void Log(string message, LogLevel level)
        {
            if (_fileHandle != null)
            {
                _fileHandle.WriteLine(DateTime.Now.ToShortTimeString() + " [" + level.ToString() + "]: " + message);
            }
        }
    }
}
