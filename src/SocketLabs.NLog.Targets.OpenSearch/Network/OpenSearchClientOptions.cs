using System;
using System.Collections.Generic;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Layouts;

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

    }
}
