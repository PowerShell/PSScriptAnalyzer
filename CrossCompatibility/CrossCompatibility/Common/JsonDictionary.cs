using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.PowerShell.CrossCompatibility
{
    [DataContract]
    public class JsonDictionary<K, V> : IDictionary<K, V>, ICloneable, IDictionary
        where K : ICloneable
        where V : ICloneable
    {
        private readonly Dictionary<K, V> _dictionary;

        [JsonConstructor]
        public JsonDictionary()
            : this(size: null, keyComparer: null)
        {
        }

        public JsonDictionary(int? size = null, IEqualityComparer<K> keyComparer = null)
        {
            // We want case-insensitive key comparison as default, so force it here
            if (typeof(K) == typeof(string) && keyComparer == null)
            {
                keyComparer = (IEqualityComparer<K>)StringComparer.OrdinalIgnoreCase;
            }

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

        public JsonDictionary(Dictionary<K, V> dictionary)
        {
            _dictionary = dictionary;
        }

        public V this[K key] { get => _dictionary[key]; set => _dictionary[key] = value; }

        public ICollection<K> Keys => _dictionary.Keys;

        public ICollection<V> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        public bool IsReadOnly => false;

        public bool IsFixedSize => throw new NotImplementedException();

        ICollection IDictionary.Keys => throw new NotImplementedException();

        ICollection IDictionary.Values => throw new NotImplementedException();

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        public object this[object key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

        public void Add(object key, object value)
        {
            Add((K)key, (V)value);
        }

        public bool Contains(object key)
        {
            return Contains((K)key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Remove(object key)
        {
            Remove((K)key);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
    }
}