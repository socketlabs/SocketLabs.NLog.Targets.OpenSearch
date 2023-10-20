using SocketLabs.NLog.Targets.OpenSearch.Models;

namespace SocketLabs.NLog.Targets.OpenSearch.Network
{
    internal interface IOpenSearchClient
    {
        Task<BulkIndexResponse?> BulkIndexAsync<T>(IEnumerable<T> items, Func<T, BulkItemDescriptor, BulkItemDescriptor> bulkItem, CancellationToken token) where T : ILogEntry;
    }
}