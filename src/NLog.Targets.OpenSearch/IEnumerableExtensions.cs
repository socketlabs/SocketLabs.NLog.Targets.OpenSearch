using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLog.Targets.OpenSearch
{
    internal static class IEnumerableExtensions
    {
        private static readonly Random _rng = new Random();

        public static T GetRandomElement<T>(this IEnumerable<T> collection)
        {
            var count = collection.Count();
            var random = _rng.Next(count);
            var enumerator = collection.GetEnumerator();
            enumerator.MoveNext();

            for (int i = 0; i <= random; i++)
            {
                enumerator.MoveNext();
            }

            return enumerator.Current;
        }

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> collection)
        {
            IList<T> buffer = null;

            if (collection is IList<T>)
            {
                buffer = collection as IList<T>;
            }
            else
            {
                buffer = collection.ToList();
            }

            for (int i = 0; i < buffer.Count; i++)
            {
                int j = _rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }
}
