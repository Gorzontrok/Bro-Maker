using BroMakerLib.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.Editor
{
    internal static class Main
    {
        public static void Log(string message, LogType logType = LogType.Log) => BMLogger.Log(message, logType);
    }
}
