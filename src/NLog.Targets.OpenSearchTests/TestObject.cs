using System;
using System.Collections.Generic;
using System.Text;

namespace NLog.Targets.OpenSearchTests
{
    internal class TestObject
    {
        public string Foo { get; set; } = "Bar";
        public Type Type { get; set; } = typeof(TestObject);
        public TestObject ChildTestObject { get; set; } = new TestObject() { Foo = "Baz", ChildTestObject = null };
    }
}
