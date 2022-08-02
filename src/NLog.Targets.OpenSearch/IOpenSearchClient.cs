using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NLog.Targets.OpenSearch
{
    public interface IOpenSearchClient
    {
        Task<BulkIndexResponse> BulkIndexAsync<T>(IEnumerable<T> items, Func<T, BulkItemDescriptor, BulkItemDescriptor> bulkItem, CancellationToken token) where T : class;
    }
}