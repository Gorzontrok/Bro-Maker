using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.Loggers
{
    public static class BMLogger
    {
        public static List<Log> logs = new List<Log>();
        public static List<Log> debugLogs = new List<Log>();
        public static string errorSwapingMessage
        {
            get
            {
                return _errorSwapingMessage;
            }
            set
            {
                _errorSwapingMessage = value;
                Log(value.ToString(), LogType.Warning);
            }
        }

        private static string _errorSwapingMessage = string.Empty;


        public static void Log(string message, LogType logType = LogType.Log, bool useColors = true)
        {
            StringBuilder sb = new StringBuilder(FormatLogType(logType));
            sb.AppendLine(FormatMessage(message, logType, useColors));
            sb.Remove(sb.Length - 2, 2);
            logs.Add(new Log(sb.ToString()));
        }

        public static void Log(object message, LogType logType = LogType.Log, bool useColors = true)
        {
            Log(message.ToString(), logType, useColors);
        }

        public static void Log(Exception exception, bool useColors = true)
        {
            ExceptionLog(exception, useColors);
        }

        public static void ExceptionLog(object message, bool useColors = true)
        {
            Log(message, LogType.Exception, useColors);
        }
        public static void ExceptionLog(object message, Exception exception, bool useColors = true)
        {
            StringBuilder sb = new StringBuilder(message.ToString());
            sb.AppendLine(exception.ToString());
            Log(sb.ToString(), LogType.Exception, useColors);
        }

        public static void Debug(object message, LogType logType = LogType.Log, bool useColors = true)
        {
            StringBuilder sb = new StringBuilder(FormatLogType(logType));
            sb.Insert(0, "[DEBUG] ");
            sb.AppendLine(FormatMessage(message.ToString(), logType, useColors));
            sb.Remove(sb.Length - 2, 2);
            debugLogs.Add(new Log(sb.ToString()));
        }

        private static string FormatLogType(LogType logType)
        {
            if (logType == LogType.Log) return string.Empty;

            StringBuilder sb = new StringBuilder(logType.ToString());
            sb.Insert(0, "[");
            sb.Append("] ");
            return sb.ToString();
        }

        private static string FormatMessage(string message, LogType logType, bool useColors)
        {
            if (!useColors || logType == LogType.Log) return message;

            StringBuilder sb = new StringBuilder("<color=");
            if (logType == LogType.Error || logType == LogType.Assert || logType == LogType.Exception)
            {
                sb.Append("red>");
            }
            else if (logType == LogType.Warning)
            {
                sb.Append("yellow>");
            }
            sb.Append(message);
            sb.Append("</color>");
            return sb.ToString();
        }
    }
}
