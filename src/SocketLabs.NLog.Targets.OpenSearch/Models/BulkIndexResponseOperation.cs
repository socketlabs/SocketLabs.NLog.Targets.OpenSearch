using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketLabs.NLog.Targets.OpenSearch.Models
{
    internal class BulkIndexResponseOperation
    {
        public BulkIndexResponseItem? Index { get; set; }
    }
}
