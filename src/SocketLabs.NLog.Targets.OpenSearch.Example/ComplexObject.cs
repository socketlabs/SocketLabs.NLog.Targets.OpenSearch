using System;

namespace SocketLabs.NLog.Targets.OpenSearch.Example
{
    public class ComplexObject
    {
        public ComplexObject()
        {
        }

        public Foo Foo { get; set; }

        public override string ToString()
        {
            return $"Foo.Bar = {string.Join(", ", Foo.Bar)}; Foo.Baz = {Foo.Baz.ToUnixTimeSeconds()}";
        }
    }

    public class Foo
    {
        public string[] Bar { get; set; } = Array.Empty<string>();
        public DateTimeOffset Baz { get; set; }

        public ComplexObject ComplexObject { get; set; }
    }
}