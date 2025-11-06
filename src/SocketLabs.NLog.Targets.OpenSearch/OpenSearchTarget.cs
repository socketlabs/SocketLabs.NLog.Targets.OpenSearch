using System.Text.Json;
using System.Text.Json.Serialization;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using SocketLabs.NLog.Targets.OpenSearch.Converters;
using SocketLabs.NLog.Targets.OpenSearch.Extensions;
using SocketLabs.NLog.Targets.OpenSearch.Network;

namespace SocketLabs.NLog.Targets.OpenSearch
{
    [Target("OpenSearch")]
    public class OpenSearchTarget : AsyncTaskTarget
    {
        private string[]? _uris;
        private readonly char[] _split = new char[] { ',' };
        private OpenSearchClient _client = default!;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        public OpenSearchTarget()
        {
            RetryCount = 10;
            BatchSize = 1000;
            TaskDelayMilliseconds = 5000;
            OverflowAction = AsyncTargetWrapperOverflowAction.Grow;
            IncludeEventProperties = true;
            IncludeCallSite = true;
            IncludeCallSiteStackTrace = true;
            IncludeScopeProperties = true;

            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            _jsonSerializerOptions.Converters.Add(new HttpMethodConverter());
            _jsonSerializerOptions.Converters.Add(new ExceptionConverter());
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            _jsonSerializerOptions.Converters.Add(new LogLevelConverter());
            _jsonSerializerOptions.Converters.Add(new CustomUnsupportedTypeConverterFactory());
        }

        public string? Username { get; set; }
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the OpenSearch uris, can be multiple comma separated.
        /// </summary>
        public string? Uris
        {
            get
            {
                if (_uris == null)
                    return null;
                else
                    return String.Join(",", _uris);
            }
            set
            {
                _uris = value?.Split(_split, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        /// <summary>
        /// Gets or sets the name of the OpenSearch index to write to.
        /// </summary>
        [RequiredParameter]
        public Layout Index { get; set; } = "nlog-${date:format=yyyy.MM.dd}";

        /// <summary>
        /// Set it to true to enable HttpCompression (Must be enabled on server)
        /// </summary>
        public bool EnableHttpCompression { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the OpenSearch client should validate any server certificates.
        /// </summary>
        public bool ValidateCertificates { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the OpenSearch client should just validate a certificate thumbprint.
        /// </summary>
        public string? CertificateThumprint { get; set; }

        protected override void CloseTarget()
        {
            base.CloseTarget();
        }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (_uris?.Any() != true)
            {
                throw new ArgumentNullException(nameof(Uris), "No OpenSearch hosts specified in config.");
            }

            var options = new OpenSearchClientOptions(_uris)
            {
                EnableHttpCompression = EnableHttpCompression,
                ValidateCertificates = ValidateCertificates,
                OnLogging = InternalLogger.Log,
                CertificateThumprint = CertificateThumprint,
                Password = Password,
                Username = Username
            };

            _client = new OpenSearchClient(options);
        }

        protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
        {
            await BulkIndexAsync(new[] { logEvent }, cancellationToken);
        }

        private async Task BulkIndexAsync(IEnumerable<LogEventInfo> logEvents, CancellationToken token)
        {
            try
            {
                var logs = logEvents.Select(log => log.ToLogEntry(
                    indexRender: () =>
                    {
                        return Index.Render(log);
                    },
                    propertyRender: () =>
                    {
                        var properties = GetAllProperties(log);

                        FilterProperties(properties);
                        FixProperties(properties);
                        TruncateLongStringValues(properties);
                        RemoveUnserializablePropertyTypes(properties);
                        return properties;
                    })
                );

                var result = await _client.BulkIndexAsync(logs, (doc, descriptor) =>
                {
                    descriptor.Index = doc.Index;
                    return descriptor;
                },
                token);

                if (result?.Errors == true)
                {
                    InternalLogger.Log(LogLevel.Error, "Error adding {0} log events to OpenSearch", logEvents.Count());

                    if (result.Items.Any(x => x.Index?.Error != null))
                    {
                        foreach (var item in result.Items.Where(x => !x.Index?.IsValid == true))
                        {
                            if (item.Index?.Error == null) continue;

                            InternalLogger.Log(LogLevel.Error, "Error adding log - Type: {0}, Reason: {1}, Index: {2}, StatusCode: {3}",
                                item.Index.Error.Type, item.Index.Error.Reason, item.Index.Index, item.Index.Status);

                            if (InternalLogger.IsTraceEnabled)
                            {
                                var rawEventsJson = JsonSerializer.Serialize(logs, _jsonSerializerOptions);
                                InternalLogger.Log(LogLevel.Trace, "Events that were attempted:\r\n{0}", rawEventsJson);
                            }
                        }
                    }
                }
                else
                {
                    InternalLogger.Log(LogLevel.Trace, "Successfully added {0} log events to OpenSearch", logEvents.Count());
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Log(ex, LogLevel.Error, "Error performing indexing operation.");
                if (ex.InnerException != null)
                    InternalLogger.Log(ex.InnerException, LogLevel.Error, "Inner Exception: ");
            }
        }

        private static void FilterProperties(IDictionary<string, object> properties)
        {
            properties.Remove("EventId_Id");
            properties.Remove("EventId_Name");

            foreach (var item in properties)
            {
                if (item.Value == null)
                {
                    properties.Remove(item);
                }
            }
        }

        private static void FixProperties(IDictionary<string, object> properties)
        {
            // Opensearch must use this internally and causes an error when it exists
            // so rename it.
            if (properties.TryGetValue("requestId", out var requestIdValue))
            {
                properties.Remove("requestId");
                properties.TryAdd("request.requestId", requestIdValue);
            }

            if (properties.TryGetValue("EventId", out var value))
            {
                if (value is Microsoft.Extensions.Logging.EventId eventId)
                {
                    properties["EventId"] = eventId.Id;

                    if (!string.IsNullOrEmpty(eventId.Name))
                    {
                        properties.TryAdd("EventName", eventId.Name!);
                    }
                }
            }

            if (properties.TryGetValue("Host", out var hostValue))
            {
                if (hostValue is not string)
                {
                    properties["Host"] = hostValue.ToString()!;
                }
            }

            if (properties.TryGetValue("PathBase", out var pathBaseValue))
            {
                if (pathBaseValue is not string)
                {
                    properties["PathBase"] = pathBaseValue.ToString()!;
                }
            }

            if (properties.TryGetValue("Path", out var pathValue))
            {
                if (pathValue is not string and not null)
                {
                    properties["Path"] = pathValue.ToString()!;
                }
            }
        }

        private static void TruncateLongStringValues(IDictionary<string, object?> properties)
        {
            foreach (var item in properties)
            {
                if (item.Value is string value)
                {
                    properties[item.Key] = value.Truncate(32768);
                }
            }
        }

        protected override async Task WriteAsyncTask(IList<LogEventInfo> logEvents, CancellationToken cancellationToken)
        {
            await BulkIndexAsync(logEvents, cancellationToken);
        }

        private static void RemoveUnserializablePropertyTypes(IDictionary<string, object> properties)
        {
            //            foreach (var item in properties)
            //            {
            //                switch (item.Value)
            //                {
            //                    case bool _:
            //                    case byte _:
            //                    case DateTime _:
            //                    case DateTimeOffset _:
            //                    case decimal _:
            //                    case double _:
            //                    case Enum _:
            //                    case Guid _:
            //                    case short _:
            //                    case int _:
            //                    case long _:
            //                    case sbyte _:
            //                    case float _:
            //                    case string _:
            //                    case ushort _:
            //                    case uint _:
            //                    case ulong _:
            //                    case LogLevel _:
            //#if NETCOREAPP
            //                    case RouteEndpoint _:
            //#endif
            //                    case TimeSpan _:
            //                        break;
            //                    case IPAddress ip:
            //                        properties[item.Key] = ip.ToString();
            //                        break;
            //                    default:
            //                        InternalLogger.Log(LogLevel.Trace, "Removed unserializable property of type {0}", item.Value.GetType().FullName);
            //                        properties.Remove(item);
            //                        break;
            //                }
            //            }
        }

        //protected override IDictionary<string, object?>? CaptureScopeContextProperties(LogEventInfo logEvent, IDictionary<string, object?> contextProperties)
        //{
        //    var properties = base.CaptureScopeContextProperties(logEvent, contextProperties);

        //    if (IncludeScopeProperties && properties != null)
        //    {
        //        foreach (var item in properties)
        //        {
        //            if (!logEvent.Properties.TryGetValue(item.Key, out _))
        //            {
        //                logEvent.Properties.Add(item.Key, item.Value);
        //            }
        //        }
        //    }

        //    return properties;
        //}
    }
}