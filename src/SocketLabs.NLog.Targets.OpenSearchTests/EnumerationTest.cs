using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SocketLabs.NLog.Targets.OpenSearchTests
{
    [TestClass]
    public class EnumerationTest
    {
        public Dictionary<string, object> Enumeration { get; set; } =
            new Dictionary<string, object> { { "foo", "bar" }, { "baz", "qux" }, { "bingo", 1 } };

        [TestMethod]
        public void TestEnumerationDoesNotCausesException()
        {
#if NETCOREAPP
                foreach (var item in Enumeration)
                {
                    if (item.Value is string value)
                    {
                        Enumeration[item.Key] = "bax";
                    }
                }

#elif NETFRAMEWORK

            var keys = Enumeration.Keys.ToList();
                foreach (var key in keys)
                {
                    if (Enumeration[key] is string value)
                    {
                        Enumeration[key] = "zix";
                    }
                }

#endif
        }
    }
}
