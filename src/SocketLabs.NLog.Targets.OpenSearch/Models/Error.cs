using System.Text.Json.Serialization;

namespace SocketLabs.NLog.Targets.OpenSearch.Models
{
    internal class Error
    {
        public string? Type { get; set; }
        public string? Reason { get; set; }
        public string? Index { get; set; }
        public string? Shard { get; set; }

        [JsonPropertyName("index_uuid")]
        public string? IndexUuid { get; set; }
    }
}