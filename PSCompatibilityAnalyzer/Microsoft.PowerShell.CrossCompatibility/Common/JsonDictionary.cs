// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.PowerShell.CrossCompatibility
{
    /// <summary>
    /// A deep-cloneable dictionary for convenient deserialization.
    /// </summary>
    /// <typeparam name="K">The type of keys of the dictionary.</typeparam>
    /// <typeparam name="V">The type of values of the dictionary.</typeparam>
    [DataContract]
    public class JsonDictionary<K, V> : Dictionary<K, V>, ICloneable
        where K : ICloneable
        where V : ICloneable
    {
        /// <summary>
        /// Default constructor, used by the JSON deserializer.
        /// </summary>
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

        /// <summary>
        /// Deep clones the dictionary, recursively cloning all keys and values within it.
        /// </summary>
        /// <returns>A fresh copy of the dictionary.</returns>
        public virtual object Clone()
        {
            return CloneInto(new JsonDictionary<K, V>(Count, Comparer));
        }

        /// <summary>
        /// Clones all key/value pairs in this dictionary into the given dictionary instance.
        /// </summary>
        /// <param name="startingDict">The new dictionary instance to clone items into.</param>
        /// <returns>The new dictionary with clones of all items in this dictionary.</returns>
        protected object CloneInto(JsonDictionary<K, V> startingDict)
        {
            foreach (KeyValuePair<K, V> item in this)
            {
                startingDict[(K)item.Key.Clone()] = (V)item.Value.Clone();
            }

            return startingDict;
        }
    }

    /// <summary>
    /// A case-insensitive string-keyed dictionary, created as a type for deserialization purposes
    /// (simpler than other ways of forcing deserialization to case-insensitive dictionaries).
    /// </summary>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    [DataContract]
    public class JsonCaseInsensitiveStringDictionary<TValue> : JsonDictionary<string, TValue>
        where TValue : ICloneable
    {
        /// <summary>
        /// Creates this dictionary with just case-insensitive key-string comparisons.
        /// This is the default constructor used by the JSON deserializer.
        /// </summary>
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

        /// <summary>
        /// Deep clones this dictionary, with clones of all the values in it.
        /// </summary>
        /// <returns>A recursively cloned copy of this dictionary with clones of all its values.</returns>
        public override object Clone()
        {
            return CloneInto(new JsonCaseInsensitiveStringDictionary<TValue>(Count));
        }
    }

    internal static class DictionaryExtension
    {
        public static void AddAll<K, V>(this IDictionary<K, V> thisDict, IEnumerable<KeyValuePair<K, V>> entries)
        {
            foreach (KeyValuePair<K, V> entry in entries)
            {
                thisDict[entry.Key] = entry.Value;
            }
        }
    }
}
