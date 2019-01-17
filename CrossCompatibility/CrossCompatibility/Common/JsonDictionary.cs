using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.PowerShell.CrossCompatibility
{
    [DataContract]
    public class JsonDictionary<K, V> : Dictionary<K, V>, ICloneable
        where K : ICloneable
        where V : ICloneable
    {
        [JsonConstructor]
        public JsonDictionary()
        {
        }

        public JsonDictionary(IEqualityComparer<K> keyComparer)
            : base(keyComparer)
        {
        }

        public JsonDictionary(int capacity, IEqualityComparer<K> keyComparer)
            : base(capacity, keyComparer)
        {
        }

        public JsonDictionary(IDictionary<K, V> thatDictionary)
            : base(thatDictionary)
        {
        }

        public JsonDictionary(IDictionary<K, V> thatDictionary, IEqualityComparer<K> keyComparer)
            : base(thatDictionary, keyComparer)
        {
        }

        public virtual object Clone()
        {
            return CloneInto(new JsonDictionary<K, V>(Count, Comparer));
        }

        protected object CloneInto(JsonDictionary<K, V> startingDict)
        {
            foreach (KeyValuePair<K, V> item in this)
            {
                startingDict[(K)item.Key.Clone()] = (V)item.Value.Clone();
            }

            return startingDict;
        }
    }

    [DataContract]
    public class JsonCaseInsensitiveStringDictionary<TValue> : JsonDictionary<string, TValue>
        where TValue : ICloneable
    {
        [JsonConstructor]
        public JsonCaseInsensitiveStringDictionary()
            : base(keyComparer: StringComparer.OrdinalIgnoreCase)
        {
        }

        public JsonCaseInsensitiveStringDictionary(int capacity)
            : base(capacity, StringComparer.OrdinalIgnoreCase)
        {
        }

        public JsonCaseInsensitiveStringDictionary(IDictionary<string, TValue> thatDictionary)
            : base(thatDictionary, StringComparer.OrdinalIgnoreCase)
        {
        }

        public override object Clone()
        {
            return CloneInto(new JsonCaseInsensitiveStringDictionary<TValue>(Count));
        }
    }
}