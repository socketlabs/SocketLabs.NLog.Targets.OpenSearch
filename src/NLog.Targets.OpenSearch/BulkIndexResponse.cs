using System.Collections.Generic;

namespace NLog.Targets.OpenSearch
{
    public class BulkIndexResponse
    {
        public long Took { get; set; }
        public bool Errors { get; set; }
        public IEnumerable<BulkIndexResponseOperation> Items { get; set; }
    }
}