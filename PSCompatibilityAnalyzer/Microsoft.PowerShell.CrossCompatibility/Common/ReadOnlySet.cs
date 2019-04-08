// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.PowerShell.CrossCompatibility
{
    public class ReadOnlySet<T> : IEnumerable<T>, IReadOnlyCollection<T>, ISet<T>
    {
        private readonly HashSet<T> _backingSet;

        public ReadOnlySet(IEnumerable<T> items)
        {
            _backingSet = new HashSet<T>(items);
        }

        public ReadOnlySet(IEnumerable<T> items, IEqualityComparer<T> itemComparer)
        {
            _backingSet = new HashSet<T>(items, itemComparer);
        }

        public int Count => _backingSet.Count;

        public bool IsReadOnly => true;

        public bool Add(T item)
        {
            throw new InvalidOperationException("Cannot add item to readonly set");
        }

        public void Clear()
        {
            throw new InvalidOperationException("Cannot modify a readonly set");
        }

        public bool Contains(T item)
        {
            return _backingSet.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _backingSet.CopyTo(array, arrayIndex);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("Cannot modify a readonly set");
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _backingSet.GetEnumerator();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("Cannot modify a readonly set");
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return _backingSet.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return _backingSet.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return _backingSet.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return _backingSet.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return _backingSet.Overlaps(other);
        }

        public bool Remove(T item)
        {
            throw new InvalidOperationException("Cannot modify a readonly set");
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return _backingSet.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("Cannot modify a readonly set");
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new InvalidOperationException("Cannot modify a readonly set");
        }

        void ICollection<T>.Add(T item)
        {
            throw new InvalidOperationException("Cannot modify a readonly set");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_backingSet).GetEnumerator();
        }
    }
}