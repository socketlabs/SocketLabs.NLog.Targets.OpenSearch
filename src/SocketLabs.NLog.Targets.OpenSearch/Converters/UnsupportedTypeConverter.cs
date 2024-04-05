using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SocketLabs.NLog.Targets.OpenSearch.Converters
{
    internal class CustomUnsupportedTypeConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type type)
        {
            if (!typeof(MemberInfo).IsAssignableFrom(type) && !(type == typeof(SerializationInfo)) && !(type == typeof(IntPtr)) && !(type == typeof(UIntPtr)))
            {
                return typeof(Delegate).IsAssignableFrom(type);
            }

            return true;
        }

        public override JsonConverter? CreateConverter(Type type, JsonSerializerOptions options)
        {
            return (JsonConverter?)Activator.CreateInstance(typeof(CustomUnsupportedTypeConverter<>).MakeGenericType(type), BindingFlags.Instance | BindingFlags.Public, null, null, null);
        }
    }

    internal class CustomUnsupportedTypeConverter<T> : JsonConverter<T>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteNullValue();
            // Do nothing!
        }
    }
}
