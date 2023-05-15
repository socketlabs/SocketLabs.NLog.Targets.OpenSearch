using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IO;
using NLog.Targets.OpenSearch.Converters;
using NLog.Targets.OpenSearch.Extensions;
using NLog.Targets.OpenSearch.Models;
using NLog.Targets.OpenSearch.Utilities;

namespace NLog.Targets.OpenSearch.Network
{
    internal class OpenSearchClient : IOpenSearchClient
    {
        private static byte[] _newline = new byte[] { (byte)'\n' };
        private readonly HttpClient _client;
        private readonly RecyclableMemoryStreamManager _memoryStreamManager;
        private readonly string[] _opensearchHosts;
        private readonly bool _useCompression;
        private readonly Action<Exception, LogLevel, string> _onLog;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public OpenSearchClient(
            RecyclableMemoryStreamManager memoryStreamManager,
            string[] opensearchHosts,
            bool useCompression,
            string certificateThumbprint = null,
            string openSearchUser = null,
            string openSearchPass = null,
            Action<Exception, LogLevel, string> onLog = null
            )
        {
            _client = new HttpClient(ConfigureClientHandler(certificateThumbprint));

            if (!string.IsNullOrEmpty(openSearchUser) && !string.IsNullOrEmpty(openSearchPass))
                _client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(openSearchUser, openSearchPass);

            _memoryStreamManager = memoryStreamManager;
            _opensearchHosts = opensearchHosts;
            _useCompression = useCompression;
            _onLog = onLog;
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
            };
            //_jsonSerializerOptions.Converters.Add(new IgnoreTypeConverter());
#if NETCOREAPP
            _jsonSerializerOptions.Converters.Add(new RouteEndpointConverter());
#endif
            _jsonSerializerOptions.Converters.Add(new ExceptionConverter());
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            _jsonSerializerOptions.Converters.Add(new LogLevelConverter());
        }

        public async Task<BulkIndexResponse> BulkIndexAsync<T>(IEnumerable<T> items, Func<T, BulkItemDescriptor, BulkItemDescriptor> bulkItem, CancellationToken token) where T : class
        {
            Stream stream = null;
            try

            {
                var ms = _memoryStreamManager.GetStream() as RecyclableMemoryStream;

                if (_useCompression)
                {
                    var gzip = new GZipStream(ms, CompressionLevel.Fastest, true);
                    stream = gzip;
                }
                else
                {
                    stream = ms;
                }

                foreach (var item in items)
                {
                    var bulkIndexItem = new BulkItemDescriptor();
                    var itemDescriptor = bulkItem(item, bulkIndexItem);
                    var op = new { index = itemDescriptor };
                    try
                    {
                        await JsonSerializer.SerializeAsync(stream, op, _jsonSerializerOptions, token);
                        await stream.WriteAsync(_newline, 0, _newline.Length);
                        await JsonSerializer.SerializeAsync(stream, item, _jsonSerializerOptions, token);
                        await stream.WriteAsync(_newline, 0, _newline.Length);
                    }
                    catch (Exception ex)
                    {
                        _onLog.Invoke(ex, LogLevel.Error, "Error serializing object.");
                    }
                }

                await stream.FlushAsync(token);

                if (stream is GZipStream gz)
                {
                    gz.Close();
                }

                List<Exception> errors = null;

                ms.TryGetBuffer(out var buffer);
                foreach (var host in _opensearchHosts.Randomize())
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        ms.Position = 0;
                        var request = new HttpRequestMessage(HttpMethod.Post, $"{host}/_bulk");
#if NET5_0_OR_GREATER
                        request.Content = new System.Net.Http.ReadOnlyMemoryContent(buffer.AsMemory());
#else
                        request.Content = new ReadOnlyMemoryContent(buffer.AsMemory());
#endif
                        if (_useCompression)
                            request.Content.Headers.ContentEncoding.Add("gzip");

                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var response = await _client.SendAsync(request);

                        if (!response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            throw new HttpRequestException($"Error sending bulk index request {response.ReasonPhrase}: {content}");
                        }

                        var utf8Json = await response.Content.ReadAsStreamAsync();
                        var bir = JsonSerializer.Deserialize<BulkIndexResponse>(utf8Json, _jsonSerializerOptions);
                        return bir;
                    }
                    catch (Exception ex)
                    {
                        if (errors == null) errors = new List<Exception>();

                        errors.Add(ex);
                    }
                }

                throw new AggregateException("Unable to send bulk index request to any of the OpenSearch hosts. See InnerException for details.", errors);

            }
            finally
            {
                stream?.Dispose();
            }
        }

        private HttpMessageHandler ConfigureClientHandler(string certificateThumbprint)
        {
            return new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, certChain, policyErrors) =>
                {
                    if (!string.IsNullOrEmpty(certificateThumbprint))
                    {
                        return cert.Thumbprint.Equals(certificateThumbprint, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                        return true;
                }
            };
        }
    }
}
