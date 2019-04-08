// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.PowerShell.CrossCompatibility
{
    /// <summary>
    /// A readonly set implementation that has set semantics and functions but is immutable.
    /// </summary>
    /// <typeparam name="T">The type of items in the set.</typeparam>
    public class ReadOnlySet<T> : IEnumerable<T>, IReadOnlyCollection<T>, ISet<T>
    {
        private readonly HashSet<T> _backingSet;

        /// <summary>
        /// Construct a new ReadOnlySet around the given items.
        /// </summary>
        /// <param name="items">The set elements.</param>
        public ReadOnlySet(IEnumerable<T> items)
        {
            _backingSet = new HashSet<T>(items);
        }

        /// <summary>
        /// Construct a new ReadOnlySet around the given items with the given comparer.
        /// </summary>
        /// <param name="items">The set elements.</param>
        /// <param name="itemComparer">The comparer to evaluate item equality.</param>
        public ReadOnlySet(IEnumerable<T> items, IEqualityComparer<T> itemComparer)
        {
            _backingSet = new HashSet<T>(items, itemComparer);
        }

        /// <summary>
        /// The number of items in the set.
        /// </summary>
        public int Count => _backingSet.Count;

        /// <summary>
        /// Indicates that this set is readonly.
        /// </summary>
        public bool IsReadOnly => true;

        /// <summary>
        /// DO NOT USE.
        /// </summary>
        public bool Add(T item)
        {
            throw new InvalidOperationException("Cannot add item to readonly set");
        }

        /// <summary>
        /// DO NOT USE.
        /// </summary>
        public void Clear()
        {
            throw new InvalidOperationException("Cannot modify a readonly set");
        }

        /// <summary>
        /// Indicates whether a given object is in the set.
        /// </summary>
        /// <param name="item">The object to check the set for.</param>
        /// <returns>True if the object is in the set, false otherwise.</returns>
        public bool Contains(T item)
        {
            return _backingSet.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _backingSet.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// DO NOT USE.
        /// </summary>
        public void ExceptWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("Cannot modify a readonly set");
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            return _backingSet.GetEnumerator();
        }

        /// <summary>
        /// DO NOT USE.
        /// </summary>
        public void IntersectWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("Cannot modify a readonly set");
        }

        /// <inheritdoc/>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _backingSet.IsProperSubsetOf(other);
        }

        /// <inheritdoc/>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _backingSet.IsProperSupersetOf(other);
        }

        /// <inheritdoc/>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _backingSet.IsSubsetOf(other);
        }

        /// <inheritdoc/>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _backingSet.IsSupersetOf(other);
        }

        /// <inheritdoc/>
        public bool Overlaps(IEnumerable<T> other)
        {
            return _backingSet.Overlaps(other);
        }

        /// <summary>
        /// DO NOT USE.
        /// </summary>
        public bool Remove(T item)
        {
            throw new InvalidOperationException("Cannot modify a readonly set");
        }

        /// <inheritdoc/>
        public bool SetEquals(IEnumerable<T> other)
        {
            return _backingSet.SetEquals(other);
        }

        /// <summary>
        /// DO NOT USE.
        /// </summary>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("Cannot modify a readonly set");
        }

        /// <summary>
        /// DO NOT USE.
        /// </summary>
        public void UnionWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("Cannot modify a readonly set");
        }

        /// <summary>
        /// DO NOT USE.
        /// </summary>
        void ICollection<T>.Add(T item)
        {
            throw new InvalidOperationException("Cannot modify a readonly set");
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_backingSet).GetEnumerator();
        }
    }
}