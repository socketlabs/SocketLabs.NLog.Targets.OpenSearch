using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets.OpenSearch.Extensions;
using NLog.Targets.OpenSearch.Network;

namespace NLog.Targets.OpenSearch
{
    [Target("OpenSearch")]
    public class OpenSearchTarget : AsyncTaskTarget
    {
        private string[] _uris;
        private char[] _split = new char[] { ',' };
        private OpenSearchClient _client;

        public string Username { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the OpenSearch uris, can be multiple comma separated.
        /// </summary>
        public string Uris
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
        /// Set it to true if OpenSearch uses BasicAuth
        /// </summary>
        public bool RequireAuth { get; set; }

        /// <summary>
        /// Gets or sets the name of the OpenSearch index to write to.
        /// </summary>
        [RequiredParameter]
        public Layout Index { get; set; } = "nlog-${date:format=yyyy.MM.dd}";

        /// <summary>
        /// Set it to true to enable HttpCompression (Must be enabled on server)
        /// </summary>
        public bool EnableHttpCompression { get; set; }


        /// <summary>
        /// Gets or sets whether the OpenSearch client should validate any server certificates.
        /// </summary>
        public bool ValidateCertificates { get; set; }

        /// <summary>
        /// Gets or sets whether the OpenSearch client should just validate a certificate thumbprint.
        /// </summary>
        public string CertificateThumprint { get; set; }

        protected override void CloseTarget()
        {
            base.CloseTarget();
        }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            var rmsm = new RecyclableMemoryStreamManager();

            Action<Exception, LogLevel, string> onLog = (ex, level, message) => InternalLogger.Log(ex, level, message);

            _client = new OpenSearchClient(rmsm, _uris, EnableHttpCompression, CertificateThumprint, Username, Password, onLog);
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
                        if (IncludeEventProperties)
                        {
                            var properties = GetAllProperties(log);
                            FilterProperties(properties);
                            FixProperties(properties);
                            return properties;
                        }

                        return null;
                    })
                );

                var result = await _client.BulkIndexAsync(logs, (doc, descriptor) =>
                {
                    descriptor.Index = doc.Index;
                    return descriptor;
                },
                token);

                if (result.Errors)
                {
                    if (result.Items.Any(x => x.Index.Error != null))
                    {
                        foreach (var item in result.Items.Where(x => !x.Index.IsValid))
                        {
                            if (item.Index.Error == null) continue;

                            InternalLogger.Log(LogLevel.Error, "Error adding log: {0} Index: {1} StatusCode: {2}", item.Index.Error.Reason, item.Index.Index, item.Index.Status);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Log(ex, LogLevel.Error, "Error performing indexing operation.");
                if (ex.InnerException != null)
                    InternalLogger.Log(ex.InnerException, LogLevel.Error, "Inner Exception: ");
            }
        }

        private void FilterProperties(IDictionary<string, object> properties)
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

        private void FixProperties(IDictionary<string, object> properties)
        {
            if (properties.TryGetValue("EventId", out var value))
            {
                if (value is EventId eventId)
                {
                    properties["EventId"] = eventId.Id;

                    if (!String.IsNullOrEmpty(eventId.Name))
                    {
                        properties.Add("EventName", eventId.Name);
                    }
                }
            }
        }

        protected override async Task WriteAsyncTask(IList<LogEventInfo> logEvents, CancellationToken cancellationToken)
        {
            await BulkIndexAsync(logEvents, cancellationToken);
        }


        //private IEnumerable<object> RenderLogEvents(IEnumerable<AsyncLogEventInfo> logEvents)
        //{
        //    foreach (var item in logEvents)
        //    {
        //        RenderLogEvent()
        //    }
        //}

    }
}
