using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace NLog.Targets.OpenSearch.Models
{
    internal class LogEntry
    {
        public string ClassName { get; set; }
        public int LineNumber { get; set; }
        public string MemberName { get; set; }
        public string FilePath { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; set; }
        public LogLevel Level { get; set; }
        public string LoggerName { get; set; }
        public StackTrace StackTrace { get; set; }
        public DateTime TimeStamp { get; set; }

        [JsonIgnore]
        public string Index { get; set; }
        public IDictionary<string, object> Properties { get; set; }
    }
}