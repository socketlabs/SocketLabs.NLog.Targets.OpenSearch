using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketLabs.NLog.Targets.OpenSearchTests;

namespace SocketLabs.NLog.Targets.OpenSearch.Converters.Tests
{
    [TestClass()]
    public class IgnoreConverterTests
    {
        private readonly TestObject _testObj;
        private JsonSerializerOptions _jsonSerializerOptions;

        public IgnoreConverterTests()
        {
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
            //_jsonSerializerOptions.Converters.Add(new TestObjectConverter());
            _jsonSerializerOptions.Converters.Add(new IgnoreTypeConverter());
            _jsonSerializerOptions.Converters.Add(new ExceptionConverter());
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            _jsonSerializerOptions.Converters.Add(new LogLevelConverter());
            _testObj = new TestObject();
            _testObj.ChildTestObject = new TestObject();
        }

        [TestMethod()]
        public void CanConvertTest()
        {
            var converter = new IgnoreTypeConverter();
            Assert.IsFalse(converter.CanConvert(_testObj.Foo.GetType()));
            Assert.IsFalse(converter.CanConvert(_testObj.ChildTestObject.GetType()));
        }

        [TestMethod()]
        public void ReadTest()
        {
            var converter = new IgnoreTypeConverter();

            Assert.Throws<NotSupportedException>(action: () =>
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
            converter.Write(writer, _testObj.Type, _jsonSerializerOptions);
        }

        [TestMethod]
        public void SerializationTest()
        {
            var json = JsonSerializer.Serialize(_testObj, _jsonSerializerOptions);
        }
    }
}