using System.Net;
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

            FixUpKnownProperties(properties);

            entry.Properties = properties;

            return entry;
        }

        private static void FixUpKnownProperties(IDictionary<string, object?> knownProperties)
        {

            foreach (var item in knownProperties)
            {
                if (_numbers.Contains(item.Key))
                {
                    knownProperties[item.Key] = TryConvertToNumber(knownProperties[item.Key]);
                }
                else if (_booleans.Contains(item.Key))
                {
                    knownProperties[item.Key] = TryConvertToBoolean(knownProperties[item.Key]);
                }
            }

            FixupIpForwarded(knownProperties);
        }

        private static void FixupIpForwarded(IDictionary<string, object?> knownProperties)
        {
            // the ip-forwarded property is actually an endpoint like this 127.0.0.1:1234 so we split it out 
            const string ipForwardedKey = "aspnet.request.ip-forwarded";
            const string ipForwardedPort = "aspnet.request.port-forwarded";

            if (knownProperties.TryGetValue(ipForwardedKey, out var value) && value is string ipForwarded)
            {
                var parts = ipForwarded.Split([':'], options: StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2)
                {
                    knownProperties[ipForwardedKey] = parts[0];
                    knownProperties.TryAdd(ipForwardedPort, TryConvertToNumber(parts[1]));
                }
            }
        }

        private static object? TryConvertToIPAddress(object? value)
        {
            try
            {
                if (value is string stringValue)
                {
                    if (IPAddress.TryParse(stringValue, out var address))
                    {
                        return address;
                    }
                }
            }
            catch (Exception)
            {
                // do nothing.
            }

            return value;
        }

        private static object? TryConvertToBoolean(object? value)
        {
            if (value is string stringValue)
            {
                return stringValue == "1";
            }

            return value;
        }

        private static readonly string[] _numbers =
        [
            "aspnet.request.duration",
            "aspnet.request.local-port",
            "aspnet.request.remote-port",
            "aspnet.response.statuscode",
            "gc.collectionCount0",
            "gc.collectionCount1",
            "gc.collectionCount2",
            "gc.maxGeneration",
            "gc.totalMemory",
            "gc.totalMemoryForceCollection",
            "process.handleCount",
            "process.maxWorkingSet",
            "process.minWorkingSet",
            "process.pid",
            "process.peakWorkingSet64",
            "process.privateMemorySize64",
            "process.virtualMemorySize64",
            "process.workingSet64",
            "threadId"
        ];

        private static readonly string[] _booleans =
        [
            "aspnet.user.isauthenticated",
            "aspnet.response.has-started"
        ];

        private static readonly string[] _ips =
        [
            "aspnet.request.ip",
            "aspnet.request.ip-forwarded",
            "aspnet.request.local-ip"
        ];

        private static object? TryConvertToNumber(object? value)
        {
            try
            {
                if (value is string stringValue)
                {
                    return Convert.ChangeType(stringValue, TypeCode.Double);
                }
            }
            catch (Exception)
            {
                // do nothing.
            }

            return value;
        }
    }
}