namespace SocketLabs.NLog.Targets.OpenSearch.Models
{
    internal class LogEntry : ILogEntry
    {
        public string Index { get; set; }

        public LogEntry(string index)
        {
            Index = index;
        }

        public IDictionary<string, object?>? Properties { get; set; }
    }
}