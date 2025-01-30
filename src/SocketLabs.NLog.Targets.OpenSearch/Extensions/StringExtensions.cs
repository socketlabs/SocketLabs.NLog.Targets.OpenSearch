using System;
using System.Collections.Generic;
using System.Text;

namespace SocketLabs.NLog.Targets.OpenSearch.Extensions
{
    public static class StringExtensions
    {
        public static string? Truncate(this string? @string, int length)
        {
            if (@string is null) return @string;

            if (@string.Length <= length) return @string;

            return @string.Substring(0, length);
        }
    }
}
