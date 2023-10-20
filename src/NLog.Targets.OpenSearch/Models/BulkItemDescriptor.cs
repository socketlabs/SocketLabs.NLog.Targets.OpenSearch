using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NLog.Targets.OpenSearch.Models
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
