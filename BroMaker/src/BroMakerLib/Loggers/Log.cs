using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Loggers
{
    public struct Log
    {
        public const string PREFIX = "[BroMakerLib]";
        public string message;
        public string formatedLog;

        private DateTime _time;

        public Log(string message)
        {
            this.message = message;
            this._time = DateTime.Now;
            formatedLog = string.Empty;
            formatedLog = FormatLog();
        }

        public override string ToString()
        {
            return formatedLog;
        }

        private string FormatTime()
        {
            StringBuilder sb = new StringBuilder("[");
            sb.Append(_time.ToString("HH:mm:ss"))
            .Append(']')
            .Append(' ');
            return sb.ToString();
        }

        public string FormatLog()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(FormatTime());
            sb.Append(' ');
            sb.Append(message);
            return sb.ToString();
        }
    }
}
