using System.Text.Json.Serialization;

namespace SocketLabs.NLog.Targets.OpenSearch.Models
{
    internal class BulkItemDescriptor
    {
        public BulkItemDescriptor()
        {
        }

        [JsonPropertyName("_index")]
        public string? Index { get; set; }

        [JsonPropertyName("_id")]
        public string? Id { get; set; }
    }
}