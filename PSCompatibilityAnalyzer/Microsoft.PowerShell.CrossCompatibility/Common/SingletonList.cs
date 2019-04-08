// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.PowerShell.CrossCompatibility.Common
{
    public struct ImmutableSingletonList<T> : IReadOnlyList<T>
    {
        public struct Enumerator : IEnumerator<T>
        {
            private readonly ImmutableSingletonList<T> _singletonList;

            private int _index;

            public Enumerator(ImmutableSingletonList<T> singletonList)
            {
                _singletonList = singletonList;
                _index = -1;
            }

            public T Current
            {
                get
                {
                    if (_index == 0)
                    {
                        return _singletonList._item;
                    }

                    return default(T);
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if (_index != 0)
                    {
                        return default(T);
                    }

                    return _singletonList._item;
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_index >= 0)
                {
                    return false;
                }

                _index++;
                return true;
            }

            public void Reset()
            {
                _index = -1;
            }
        }

        private T _item;

        public ImmutableSingletonList(T item)
        {
            _item = item;
        }

        public T this[int index]
        { 
            get
            {
                if (index == 0)
                {
                    return _item;
                }

                throw new IndexOutOfRangeException();
            }
        }

        public int Count => 1;

        public bool Contains(T item)
        {
            return EqualityComparer<T>.Default.Equals(item, _item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}