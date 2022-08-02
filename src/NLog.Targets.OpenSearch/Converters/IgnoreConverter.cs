using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NLog.Targets.OpenSearch.Converters
{
    public class IgnoreConverter : JsonConverter<object>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return !typeToConvert.IsGenericType;
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            // NOOP
            return;
        }
    }
}
