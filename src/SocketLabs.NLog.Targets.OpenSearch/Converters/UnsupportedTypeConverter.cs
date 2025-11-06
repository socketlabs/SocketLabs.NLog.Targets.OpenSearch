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
            // If a type is added, also add to the SourceGeneration project.

            return
                // There's no safe way to construct a Type/MemberInfo from untrusted user input.
                typeof(MemberInfo).IsAssignableFrom(type) ||
                // (De)serialization of SerializationInfo is already disallowed due to Type being disallowed
                // (the two ctors on SerializationInfo take a Type, and a Type member is present when serializing).
                // Explicitly disallowing this type provides a clear exception when ctors with
                // .ctor(SerializationInfo, StreamingContext) signatures are attempted to be used for deserialization.
                // Invoking such ctors is not safe when used with untrusted user input.
                type == typeof(SerializationInfo) ||
                type == typeof(IntPtr) ||
                type == typeof(UIntPtr) ||
                // Exclude delegates.
                typeof(Delegate).IsAssignableFrom(type);
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

        public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return default!;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteNullValue();
            // Do nothing!
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(value?.ToString() ?? string.Empty);
        }
    }
}
