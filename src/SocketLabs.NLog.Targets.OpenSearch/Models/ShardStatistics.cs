namespace SocketLabs.NLog.Targets.OpenSearch.Models
{
    internal class ShardStatistics
    {
        public long Total { get; set; }
        public long Successful { get; set; }
        public long Failed { get; set; }
    }
}