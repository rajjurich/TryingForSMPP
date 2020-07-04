using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public class Enumerations
    {
        public enum LogType
        {
            Audit,
            Exception,
            Transaction,
            ListenerAudit,
            ListenerException
        };
    }
}
