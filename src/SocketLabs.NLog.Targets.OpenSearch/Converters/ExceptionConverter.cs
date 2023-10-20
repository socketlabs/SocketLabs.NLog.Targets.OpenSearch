using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SocketLabs.NLog.Targets.OpenSearch.Converters
{
    internal class ExceptionConverter : JsonConverter<object>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(Exception);
        }

        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            int depth = 0;
            WriteException(writer, value, options, false, ref depth);
        }

        private void WriteException(Utf8JsonWriter writer, object value, JsonSerializerOptions options, bool isInnerExecption, ref int depth)
        {
            if (value is not Exception ex) return;

            if (isInnerExecption)
            {
                writer.WriteStartObject("innerException");
            }
            else
            {
                writer.WriteStartObject();
            }

            writer.WriteNumber("hResult", ex.HResult);
            writer.WriteString("message", ex.Message);

            if (ex.HelpLink != null)
                writer.WriteString("helpLink", ex.HelpLink);

            if (ex.Source != null)
                writer.WriteString("source", ex.Source);

            if (ex.StackTrace != null)
                writer.WriteString("stackTrace", ex.StackTrace);

            if (ex.InnerException != null)
            {
                depth++;

                if (depth > 3)
                {
                    writer.WriteEndObject();
                    return;
                }

                WriteException(writer, ex.InnerException, options, true, ref depth);
            }

            writer.WriteEndObject();
        }
    }
}
