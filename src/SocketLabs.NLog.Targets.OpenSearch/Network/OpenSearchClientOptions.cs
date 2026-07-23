using NLog;

namespace SocketLabs.NLog.Targets.OpenSearch.Network
{
    public class OpenSearchClientOptions
    {
        public string? Username { get; set; }
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the OpenSearch uris.
        /// </summary>
        public readonly string[] _uris;

        public OpenSearchClientOptions(string[] uris)
        {
            if (!uris.Any())
                throw new ArgumentException("No uris specified for OpenSearch hosts.", nameof(uris));

            _uris = uris;
        }

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

        public Action<Exception, LogLevel, string>? OnLogging { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of idle bytes the small-buffer pool is allowed to
        /// retain before releasing them back to the GC. Defaults to 100MB.
        /// </summary>
        public long MaximumSmallPoolFreeBytes { get; set; } = 100 * 1024 * 1024;

        /// <summary>
        /// Gets or sets the maximum number of idle bytes the large-buffer pool is allowed to
        /// retain before releasing them back to the GC. Defaults to 200MB.
        /// </summary>
        public long MaximumLargePoolFreeBytes { get; set; } = 200 * 1024 * 1024;
    }
}