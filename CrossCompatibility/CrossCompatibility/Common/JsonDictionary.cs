using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.PowerShell.CrossCompatibility
{
    [DataContract]
    public class JsonDictionary<K, V> : IDictionary<K, V>, ICloneable
        where K : ICloneable
        where V : ICloneable
    {
        private readonly Dictionary<K, V> _dictionary;

        public JsonDictionary(int? size = null, IEqualityComparer<K> keyComparer = null)
        {
            if (size == null)
            {
                if (keyComparer == null)
                {
                    _dictionary = new Dictionary<K, V>();
                    return;
                }

                _dictionary = new Dictionary<K, V>(keyComparer);
                return;
            }

            if (keyComparer == null)
            {
                _dictionary = new Dictionary<K, V>(size.Value);
                return;
            }

            _dictionary = new Dictionary<K, V>(size.Value, keyComparer);
        }

        [JsonConstructor()]
        public JsonDictionary(Dictionary<K, V> dictionary)
        {
            _dictionary = dictionary;
        }

        public V this[K key] { get => _dictionary[key]; set => _dictionary[key] = value; }

        public ICollection<K> Keys => _dictionary.Keys;

        public ICollection<V> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        public bool IsReadOnly => false;

        public void Add(K key, V value)
        {
            _dictionary.Add(key, value);
        }

        public void Add(KeyValuePair<K, V> item)
        {
            _dictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public object Clone()
        {
            var clone = new Dictionary<K, V>(_dictionary.Count, _dictionary.Comparer);

            foreach (KeyValuePair<K, V> item in _dictionary)
            {
                clone[(K)item.Key.Clone()] = (V)item.Value.Clone();
            }

            return new JsonDictionary<K, V>(clone);
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return ((IDictionary<K, V>)_dictionary).Contains(item);
        }

        public bool ContainsKey(K key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            ((IDictionary<K, V>)_dictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public bool Remove(K key)
        {
            return _dictionary.Remove(key);
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            return ((IDictionary<K, V>)_dictionary).Remove(item);
        }

        public bool TryGetValue(K key, out V value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_dictionary).GetEnumerator();
        }
    }
}