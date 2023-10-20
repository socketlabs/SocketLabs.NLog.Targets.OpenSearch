using System;

namespace SocketLabs.NLog.Targets.OpenSearchTests
{
    internal class TestObject
    {
        public string Foo { get; set; } = "Bar";
        public Type Type { get; set; } = typeof(TestObject);
        public TestObject ChildTestObject { get; set; }
    }
}