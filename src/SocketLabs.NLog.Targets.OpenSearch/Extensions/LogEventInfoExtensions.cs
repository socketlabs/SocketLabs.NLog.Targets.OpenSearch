using NLog;
using SocketLabs.NLog.Targets.OpenSearch.Models;

namespace SocketLabs.NLog.Targets.OpenSearch.Extensions
{
    internal static class LogEventInfoExtensions
    {
        public static LogEntry ToLogEntry(this LogEventInfo logEventInfo, Func<string> indexRender, Func<IDictionary<string, object?>> propertyRender)
        {
            var index = indexRender?.Invoke();

            if (string.IsNullOrEmpty(index))
                throw new ArgumentNullException(nameof(index), "No index specified for OpenSearch");

            var entry = new LogEntry(index!);
            var properties = propertyRender?.Invoke();
            properties ??= new Dictionary<string, object?>();

            properties.TryAdd("className", logEventInfo.CallerClassName);
            properties.TryAdd("lineNumber", logEventInfo.CallerLineNumber);
            properties.TryAdd("memberName", logEventInfo.CallerMemberName);
            properties.TryAdd("filePath", logEventInfo.CallerFilePath);
            properties.TryAdd("exception", logEventInfo.Exception);
            properties.TryAdd("message", logEventInfo.FormattedMessage);
            properties.TryAdd("level", logEventInfo.Level);
            properties.TryAdd("loggerName", logEventInfo.LoggerName);
            properties.TryAdd("stackTrace", logEventInfo.StackTrace);
            properties.TryAdd("timeStamp", logEventInfo.TimeStamp);

            entry.Properties = properties;

            return entry;
        }
    }
}