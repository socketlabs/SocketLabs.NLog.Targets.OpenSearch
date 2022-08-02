using System.Collections.Generic;

namespace NLog.Targets.OpenSearch.Models
{
    internal class BulkIndexResponse
    {
        public long Took { get; set; }
        public bool Errors { get; set; }
        public IEnumerable<BulkIndexResponseOperation> Items { get; set; }
    }
}