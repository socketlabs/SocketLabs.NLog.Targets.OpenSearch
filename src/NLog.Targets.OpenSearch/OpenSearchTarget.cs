using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace NLog.Targets.OpenSearch
{
    [Target("OpenSearch")]
    public class OpenSearchTarget : AsyncTaskTarget
    {
        private string[] _uris;
        private char[] _split = new char[] { ',' };
        private ElasticClient _client;

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
        /// Gets or sets if exceptions will be rethrown.
        ///
        /// Set it to true if the OpenSearch target should throw exceptions.
        /// </summary>
        public bool ThrowExceptions { get; set; }

        /// <summary>
        /// Gets or sets a list of additional fields to add to the OpenSearch document.
        /// </summary>
        [ArrayParameter(typeof(Field), "field")]
        public IList<Field> Fields { get; set; } = new List<Field>();

        /// <summary>
        /// Gets or sets whether the OpenSearch client should validate any server certificates.
        /// </summary>
        public bool ValidateCertificates { get; set; }

        /// <summary>
        /// Gets or sets whether the OpenSearch client should just validate a certificate thumbprint.
        /// </summary>
        public string CertificateThumprint { get; set; }

        /// <summary>
        /// Gets or sets whether the OpenSearch client should use a SniffingConnectionPool.
        /// </summary>
        public bool UseSniffingConnection { get; set; }

        protected override void CloseTarget()
        {
            base.CloseTarget();
        }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (_uris == null)
            {
                Uris = "http://localhost:9200";
            }

            var uris = _uris.Select(x => new Uri(x));
            IConnectionPool pool = null;

            if (UseSniffingConnection)
            {
                pool = new SniffingConnectionPool(uris);
            }
            else
            {
                pool = new StaticConnectionPool(uris);
            }

            var connectionSettings = new ConnectionSettings(pool,
                (builtin, settings) => new JsonNetSerializer(
                    builtin,
                    settings,
                    () => new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore },
                    contractJsonConverters: new JsonConverter[] { new StringEnumConverter() }
                    ))
                .RequestTimeout(TimeSpan.FromMinutes(2));

            if (EnableHttpCompression)
            {
                connectionSettings.EnableHttpCompression();
            }

            if (ThrowExceptions)
            {
                connectionSettings.ThrowExceptions();
            }

            if (!String.IsNullOrWhiteSpace(Password))
            {
                connectionSettings.BasicAuthentication(Username, Password);
            }

            if (!ValidateCertificates)
            {
                connectionSettings.ServerCertificateValidationCallback((sender, cert, chain, errors) => true);
            }
            else if (!String.IsNullOrWhiteSpace(CertificateThumprint))
            {
                connectionSettings.ServerCertificateValidationCallback((sender, cert, chain, errors) =>
                {
                    var x509 = cert as X509Certificate2;
                    var isValid = x509.Thumbprint.Equals(CertificateThumprint, StringComparison.OrdinalIgnoreCase);
                    return isValid;
                });
            }

            _client = new ElasticClient(connectionSettings);
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
                            return GetAllProperties(log);
                        }

                        return null;
                    })
                );

                var result = await _client.BulkAsync((selector) =>
                {
                    selector.IndexMany(logs, (bulk, log) =>
                    {
                        bulk.Index(log.Index);
                        return bulk;
                    });

                    return selector;
                },
                token);

                if (result?.ServerError?.Error != null)
                {
                    InternalLogger.Log(LogLevel.Error, result.ServerError.Error.Reason);
                }

                if (!result.IsValid)
                {
                    foreach (var item in result.ItemsWithErrors)
                    {
                        InternalLogger.Log(LogLevel.Error, "Error adding log: {0} Index: {1} StatusCode: {2}", item.Error.Reason, item.Index, item.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Log(ex, LogLevel.Error, "Error performing indexing operation");
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
