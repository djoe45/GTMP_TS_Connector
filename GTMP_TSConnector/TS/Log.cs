using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTMP_TSConnector
{
    class Log
    {
        public static void Info(String log)
        {
            TSPlugin.Instance.Functions.printMessageToCurrentTab(log);
        }
    }
}
