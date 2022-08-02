using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog.Targets.OpenSearch.Models;

namespace NLog.Targets.OpenSearch.Network
{
    internal interface IOpenSearchClient
    {
        Task<BulkIndexResponse> BulkIndexAsync<T>(IEnumerable<T> items, Func<T, BulkItemDescriptor, BulkItemDescriptor> bulkItem, CancellationToken token) where T : class;
    }
}