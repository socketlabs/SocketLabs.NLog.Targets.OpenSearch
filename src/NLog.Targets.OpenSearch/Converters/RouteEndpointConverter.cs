using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Routing;

namespace NLog.Targets.OpenSearch.Converters
{
    internal class RouteEndpointConverter : JsonConverter<RouteEndpoint>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return base.CanConvert(typeToConvert);
        }

        public override RouteEndpoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, RouteEndpoint value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.DisplayName);
        }
    }
}
