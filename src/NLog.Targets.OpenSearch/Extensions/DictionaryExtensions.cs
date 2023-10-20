using System;
using System.Collections.Generic;
using System.Text;

namespace NLog.Targets.OpenSearch.Extensions
{

#if !NET5_0_OR_GREATER 
    public static class DictionaryExtensions
    {
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary is null) 
                return false;

            if (dictionary.ContainsKey(key))
                return false;

            dictionary.Add(key, value);
            
            return true;
        }
    }
#endif
}
