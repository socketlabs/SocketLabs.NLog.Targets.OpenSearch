using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NLog.Targets.OpenSearch
{
    public static class LogEventInfoExtensions
    {
        public static LogEntry ToLogEntry(this LogEventInfo logEventInfo, Func<string> indexRender, Func<IDictionary<string,object>> propertyRender)
        {
            var entry = new LogEntry()
            {
                ClassName = logEventInfo.CallerClassName,
                LineNumber = logEventInfo.CallerLineNumber,
                MemberName = logEventInfo.CallerMemberName,
                FilePath = logEventInfo.CallerFilePath,
                Exception = logEventInfo.Exception,
                Message = logEventInfo.FormattedMessage,
                Level = logEventInfo.Level,
                LoggerName = logEventInfo.LoggerName,
                StackTrace = logEventInfo.StackTrace,
                TimeStamp = logEventInfo.TimeStamp,
                Index = indexRender?.Invoke(),
                Properties = propertyRender?.Invoke()
            };

            return entry;
        }
    }
}
