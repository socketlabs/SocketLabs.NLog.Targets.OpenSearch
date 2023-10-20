using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IO;
using NLog.Targets.OpenSearch.Converters;
using NLog.Targets.OpenSearch.Extensions;
using NLog.Targets.OpenSearch.Models;
using NLog.Targets.OpenSearch.Utilities;

namespace NLog.Targets.OpenSearch.Network
{
    internal class OpenSearchClient : IOpenSearchClient
    {
        private static readonly byte[] _newline = "\n"u8.ToArray();
        private readonly HttpClient _client;
        private readonly RecyclableMemoryStreamManager _memoryStreamManager;
        private readonly string[] _opensearchHosts;
        private readonly bool _useCompression;
        private readonly Action<Exception, LogLevel, string>? _onLog;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public OpenSearchClient(OpenSearchClientOptions options)
        {
            _memoryStreamManager = new RecyclableMemoryStreamManager();
            _client = new HttpClient(ConfigureClientHandler(options.CertificateThumprint, options.ValidateCertificates));

            if (!string.IsNullOrEmpty(options.Username) && !string.IsNullOrEmpty(options.Password))
                _client.DefaultRequestHeaders.Authorization = BasicAuthenticationHeaderValue.Create(options.Username!, options.Password!);

            _opensearchHosts = options._uris;
            _useCompression = options.EnableHttpCompression;
            _onLog = options.OnLogging;
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
            };
#if NETCOREAPP
            _jsonSerializerOptions.Converters.Add(new RouteEndpointConverter());
#endif
            _jsonSerializerOptions.Converters.Add(new ExceptionConverter());
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            _jsonSerializerOptions.Converters.Add(new LogLevelConverter());
        }

        public async Task<BulkIndexResponse?> BulkIndexAsync<T>(IEnumerable<T> items, Func<T, BulkItemDescriptor, BulkItemDescriptor> bulkItem, CancellationToken token) where T : ILogEntry
        {
            Stream stream = default!;
            try
            {
                var ms = (RecyclableMemoryStream)_memoryStreamManager.GetStream();

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
#if NETCOREAPP
                        await stream.WriteAsync(_newline, token);
#else
                        await stream.WriteAsync(_newline, 0, _newline.Length, token);
#endif
                        await JsonSerializer.SerializeAsync(stream, item.Properties, _jsonSerializerOptions, token);
#if NETCOREAPP
                        await stream.WriteAsync(_newline, token);
#else
                        await stream.WriteAsync(_newline, 0, _newline.Length, token);
#endif
                    }
                    catch (Exception ex)
                    {
                        _onLog?.Invoke(ex, LogLevel.Error, "Error serializing object.");
                    }
                }

                await stream.FlushAsync(token);

                if (stream is GZipStream gz)
                {
                    gz.Close();
                }

                List<Exception>? errors = null;

                ms.TryGetBuffer(out var buffer);
                foreach (var host in _opensearchHosts.Randomize())
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        ms.Position = 0;
                        var request = new HttpRequestMessage(HttpMethod.Post, $"{host}/_bulk");
#if NETCOREAPP
                        request.Content = new System.Net.Http.ReadOnlyMemoryContent(buffer.AsMemory());
#else
                        request.Content = new ReadOnlyMemoryContent(buffer.AsMemory());
#endif
                        if (_useCompression)
                            request.Content.Headers.ContentEncoding.Add("gzip");

                        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                        var response = await _client.SendAsync(request, token);

                        if (!response.IsSuccessStatusCode)
                        {
#if NETCOREAPP
                            var content = await response.Content.ReadAsStringAsync(token);
#else
                            var content = await response.Content.ReadAsStringAsync();
#endif
                            throw new HttpRequestException($"Error sending bulk index request {response.ReasonPhrase}: {content}");
                        }
#if NETCOREAPP
                        var utf8Json = await response.Content.ReadAsStreamAsync(token);
#else
                        var utf8Json = await response.Content.ReadAsStreamAsync();
#endif
                        var bir = JsonSerializer.Deserialize<BulkIndexResponse>(utf8Json, _jsonSerializerOptions);
                        return bir;
                    }
                    catch (Exception ex)
                    {
                        errors ??= new List<Exception>();

                        errors.Add(ex);
                    }
                }

                throw new AggregateException("Unable to send bulk index request to any of the OpenSearch hosts. See InnerException for details.", errors!);

            }
            finally
            {
                stream?.Dispose();
            }
        }

        private static HttpMessageHandler ConfigureClientHandler(string? certificateThumbprint, bool validateCertificates)
        {
            if (validateCertificates && string.IsNullOrEmpty(certificateThumbprint))
                return new HttpClientHandler();
            else
                return new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, certChain, policyErrors) =>
                    {
                        if (validateCertificates && !string.IsNullOrEmpty(certificateThumbprint))
                        {
                            if (cert is null) return false;

                            return cert.Thumbprint.Equals(certificateThumbprint, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                            return true;
                    }
                };
        }
    }
}
