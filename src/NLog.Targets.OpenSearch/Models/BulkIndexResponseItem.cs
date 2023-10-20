using System.Text.Json.Serialization;

namespace NLog.Targets.OpenSearch.Models
{
    internal class BulkIndexResponseItem
    {
        //
        // Summary:
        //     The error associated with the bulk operation
        [JsonPropertyName("error")]
        public Error? Error { get; set; }

        //
        // Summary:
        //     The id of the document for the bulk operation
        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        //
        // Summary:
        //     The index against which the bulk operation ran
        [JsonPropertyName("_index")]
        public string? Index { get; set; }

        [JsonPropertyName("_primary_term")]
        public long PrimaryTerm { get; set; }

        //
        // Summary:
        //     The result of the bulk operation
        [JsonPropertyName("result")]
        public string? Result { get; set; }

        [JsonPropertyName("_seq_no")]
        public long SequenceNumber { get; set; }

        //
        // Summary:
        //     The shards associated with the bulk operation
        [JsonPropertyName("_shards")]
        public ShardStatistics? Shards { get; set; }

        //
        // Summary:
        //     The status of the bulk operation
        [JsonPropertyName("status")]
        public int Status { get; set; }

        //
        // Summary:
        //     The version of the document
        [JsonPropertyName("_version")]
        public long Version { get; set; }

        //
        // Summary:
        //     Specifies whether this particular bulk operation succeeded or not
        public bool IsValid
        {
            get
            {
                if (Error != null)
                {
                    return false;
                }

                return Status == 201;
            }
        }
    }
}