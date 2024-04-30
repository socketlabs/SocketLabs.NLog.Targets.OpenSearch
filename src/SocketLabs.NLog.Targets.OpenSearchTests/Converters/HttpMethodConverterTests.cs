using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketLabs.NLog.Targets.OpenSearchTests;

namespace SocketLabs.NLog.Targets.OpenSearch.Converters.Tests
{
    [TestClass()]
    public class HttpMethodConverterTests
    {
        private readonly HttpMethod _testObj;
        private JsonSerializerOptions _jsonSerializerOptions;

        public HttpMethodConverterTests()
        {
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            _jsonSerializerOptions.Converters.Add(new HttpMethodConverter());
            _testObj = new HttpMethod("Post");
        }

        [TestMethod()]
        public void CanConvertTest()
        {
            var converter = new HttpMethodConverter();
            Assert.IsTrue(converter.CanConvert(_testObj.GetType()));
        }

        [TestMethod()]
        public void ReadTest()
        {
            var converter = new IgnoreTypeConverter();
            Assert.ThrowsException<NotSupportedException>(action: () =>
            {
                var reader = new Utf8JsonReader();
                converter.Read(ref reader, null, _jsonSerializerOptions);
            });
        }

        [TestMethod()]
        public void WriteTest()
        {
            var converter = new IgnoreTypeConverter();
            var writer = new Utf8JsonWriter(new MemoryStream(), new JsonWriterOptions());
            converter.Write(writer, _testObj.GetType(), _jsonSerializerOptions);
        }

        [TestMethod]
        public void SerializationTest()
        {
            var log = new { HttpMethod = _testObj };
            var json = JsonSerializer.Serialize(log, _jsonSerializerOptions);

            Assert.AreEqual("{\"httpMethod\":\"Post\"}", json);
        }
    }
}