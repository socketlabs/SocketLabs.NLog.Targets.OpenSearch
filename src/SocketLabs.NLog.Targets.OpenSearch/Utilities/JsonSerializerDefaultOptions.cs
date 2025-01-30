using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SocketLabs.NLog.Targets.OpenSearch.Utilities
{
    public static class JsonSerializerDefaultOptions
    {
        public static JsonSerializerOptions Indented => new() { WriteIndented = true };
    }
}
