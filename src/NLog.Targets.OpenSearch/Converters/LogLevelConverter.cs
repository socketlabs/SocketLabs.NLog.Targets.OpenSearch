using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NLog.Targets.OpenSearch.Converters
{
    public class LogLevelConverter : JsonConverter<LogLevel>
    {
        public override LogLevel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var level = reader.GetString();

            if (!string.IsNullOrEmpty(level))
            {
                return LogLevel.FromString(level);
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, LogLevel value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
