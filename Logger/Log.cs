using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Logger.Enumerations;

namespace Logger
{
    public interface ILog
    {
        string Logger(string message, LogType logType);
        string CaughtExceptions(Exception ex, string src);
    }
    public sealed class Log : ILog
    {
        private static readonly Lazy<Log> instance = new Lazy<Log>(() => new Log());

        private Log()
        {

        }
        public static Log GetInstance
        {
            get
            {
                return instance.Value;
            }
        }

        public string Logger(string message, LogType logType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "Empty Message";
                }
                string filename = string.Format("{0}_{1}.log", logType, DateTime.Now.ToString("ddMMMyyyy"));
                string logDirectory = string.Format(@"{0}\{1}\{2}", System.AppDomain.CurrentDomain.BaseDirectory, "Logs", logType);
                if (!(Directory.Exists(logDirectory)))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now.ToString("HH:mm:ss.fff tt "));
                //sb.Append("  >  ");
                sb.Append(string.Format("{0}", message));
                //sb.AppendLine("_____________________________________________________");
                using (StreamWriter writer = new StreamWriter(logDirectory + @"\" + filename, true))
                {
                    writer.WriteLine(sb.ToString());
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
            return message;
        }
        public string CaughtExceptions(Exception ex, string src)
        {
            string message = string.Empty;

            int exCounter = 1;
            message = ex.Message;
            //message += ex.StackTrace;
            bool loopCondition = true;
            Exception nex = ex.InnerException;
            while (loopCondition)
            {
                if (nex != null)
                {
                    //message += "\r\n" + exCounter + ".) " + nex.Message;
                    message = nex.Message;
                    nex = nex.InnerException;
                    exCounter++;
                }
                else
                {
                    loopCondition = false;
                }
            }
            string str = string.Format("{0}\r\n{1}", src, message);
            Logger(str, LogType.Exception);
            return message;
        }
    }
}
