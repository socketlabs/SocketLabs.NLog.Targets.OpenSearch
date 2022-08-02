using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NLog.Targets.OpenSearch
{
    internal class ExceptionConverter : JsonConverter<Exception>
    {
        public override Exception Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
        {
            int depth = 0;
            WriteException(writer, value, options, false, ref depth);
        }

        private void WriteException(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options, bool isInnerExecption, ref int depth)
        {
            if (isInnerExecption)
            {
                writer.WriteStartObject("innerException");
            }
            else
            {
                writer.WriteStartObject();
            }

            writer.WriteNumber("hResult", value.HResult);
            writer.WriteString("message", value.Message);

            if (value.HelpLink != null)
                writer.WriteString("helpLink", value.HelpLink);

            if (value.Source != null)
                writer.WriteString("source", value.Source);

            if (value.StackTrace != null)
                writer.WriteString("stackTrace", value.StackTrace);

            if (value.InnerException != null)
            {
                depth++;

                if (depth > 3)
                {
                    writer.WriteEndObject();
                    return;
                }

                WriteException(writer, value.InnerException, options, true, ref depth);
            }

            writer.WriteEndObject();
        }
    }
}
