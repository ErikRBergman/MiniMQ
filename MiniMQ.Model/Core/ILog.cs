using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniMQ.Model.Core
{
    public interface ILog
    {
        void Log(LogType logType, string message);
    }

    public enum LogType
    {
        Verbose, 
        Information,
        Warning,
        Error,
    }
}
