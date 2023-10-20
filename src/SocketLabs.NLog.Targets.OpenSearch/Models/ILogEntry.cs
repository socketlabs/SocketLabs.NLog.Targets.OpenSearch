namespace SocketLabs.NLog.Targets.OpenSearch.Models
{
    internal interface ILogEntry
    {
        string Index { get; set; }
        IDictionary<string, object?>? Properties { get; set; }
    }
}