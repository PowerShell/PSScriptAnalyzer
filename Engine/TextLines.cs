// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Class to represent lines in a text.
    ///
    /// An instance of this class inserts and deletes an item (line) in O(1) time complexity as it uses
    /// a linkedlist as the underlying type to store the items. A drawback of using linkedlist as the
    /// underlying data type is that each indexed operations can be of the order of O(n), where n is the number of
    /// items in the list. To mitigate this inefficiency, each instance maintains state of the last accessed
    /// index and linkedlist node. Typically, successive text edits happen around a neighborhood of lines and as such
    /// each subsequent indexed operations can take advantage of previous state to minimize linkedlist
    /// traversal to find the requested item.
    /// </summary>
    internal class TextLines : IList<string>
    {
        private LinkedList<string> lines;
        private int lastAccessedIndex;
        private LinkedListNode<string> lastAccessedNode;

        /// <summary>
        /// Construct an instance of TextLines type.
        /// </summary>
        public TextLines()
        {
            lines = new LinkedList<string>();
            Count = 0;
            InvalidateLastAccessed();
        }

        /// <summary>
        /// Construct an instance for TextLines type from an IEnumerable type.
        /// </summary>
        /// <param name="inputLines">An IEnumerable type that represent lines in a text.</param>
        public TextLines(IEnumerable<string> inputLines) : this()
        {
            ThrowIfNull(inputLines, nameof(inputLines));
            if (inputLines.Any(line => line == null))
            {
                throw new ArgumentException(String.Format(
                    CultureInfo.CurrentCulture,
                    Strings.TextLinesNoNullItem));

            }

            lines = new LinkedList<string>(inputLines);
            Count = lines.Count;
        }

        /// <summary>
        /// A readonly property describing how many elements are in the list.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// If the object is ReadOnly or not.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Sets or gets the element at the given index.
        /// </summary>
        public string this[int index]
        {
            get
            {
                ValidateIndex(index);
                return GetNodeAt(index).Value;
            }
            set
            {
                ValidateIndex(index);
                Insert(index, value);
                RemoveAt(index);
            }
        }

        /// <summary>
        /// Return a readonly collection of the current object.
        /// </summary>
        /// <returns>A readonly collection of the current object.</returns>
        public ReadOnlyCollection<string> ReadOnly()
        {
            return new ReadOnlyCollection<string>(this);
        }

        /// <summary>
        /// Adds the given string to the end of the list.
        /// </summary>
        /// <param name="item">A non null object of type String.</param>
        public void Add(string item)
        {
            Insert(Count, item);
        }

        /// <summary>
        /// Clears the contents of the list.
        /// </summary>
        public void Clear()
        {
            lines.Clear();
        }

        /// <summary>
        /// Returns true if the specified element is in the list.
        /// Equality is determined by calling item.Equals().
        /// </summary>
        /// <param name="item">An item of type string.</param>
        /// <returns>true if item is contained in the list, otherwise false.</returns>
        public bool Contains(string item)
        {
            return lines.Contains(item);
        }

        /// <summary>
        /// Copies the list into an array, which must be of a compatible array type.
        /// </summary>
        /// <param name="array">An array of size TextLines.Count - arrayIndex</param>
        /// <param name="arrayIndex">Start index of the list from which to start copying into array.</param>
        public void CopyTo(string[] array, int arrayIndex)
        {
            lines.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator for this list.
        /// </summary>
        public IEnumerator<string> GetEnumerator()
        {
            return lines.GetEnumerator();
        }

        /// <summary>
        /// Returns the first occurrence of a given value in a range of this list.
        /// </summary>
        /// <param name="item">Item to be searched.</param>
        /// <returns>The index if there is a match of the item in the list, otherwise -1.</returns>
        public int IndexOf(string item)
        {
            var node = lines.First;
            int index = 0;
            while (node != null)
            {
                if (node.Value.Equals(item))
                {
                    return index;
                }

                node = node.Next;
                index++;
            }

            return -1;
        }

        /// <summary>
        /// Inserts an element into this list at a given index.
        /// </summary>
        public void Insert(int index, string item)
        {
            ThrowIfNull(item, nameof(item));
            LinkedListNode<string> itemInserted;
            if (Count == 0 && index == 0)
            {
                itemInserted = lines.AddFirst(item);
            }
            else if (Count == index)
            {
                itemInserted = lines.AddLast(item);
            }
            else
            {
                ValidateIndex(index);
                itemInserted = lines.AddBefore(GetNodeAt(index), item);
            }

            SetLastAccessed(index, itemInserted);
            Count++;
        }

        /// <summary>
        /// Remove an item from the list.
        /// </summary>
        /// <returns>true if removal is successful, otherwise false.</returns>
        public bool Remove(string item)
        {
            var itemIndex = IndexOf(item);
            if (itemIndex == -1)
            {
                return false;
            }

            RemoveAt(itemIndex);
            return true;
        }

        /// <summary>
        /// Removes the element at the given index.
        /// </summary>
        public void RemoveAt(int index)
        {
            ValidateIndex(index);
            var node = GetNodeAt(index);
            if (node.Next != null)
            {
                SetLastAccessed(index, node.Next);
            }
            else if (node.Previous != null)
            {
                SetLastAccessed(index - 1, node.Previous);
            }
            else
            {
                InvalidateLastAccessed();
            }

            lines.Remove(node);
            Count--;
        }

        /// <summary>
        /// Returns an enumerator over the elements in the list.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return lines.GetEnumerator();
        }

        /// <summary>
        /// Returns the string representation of the object.
        /// </summary>
        public override string ToString()
        {
            return string.Join(Environment.NewLine, lines);
        }

        private void ValidateIndex(int index)
        {
            if (index >= Count || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        private void SetLastAccessed(int index, LinkedListNode<string> node)
        {
            lastAccessedIndex = index;
            lastAccessedNode = node;
        }

        private void InvalidateLastAccessed()
        {
            lastAccessedIndex = -1;
            lastAccessedNode = null;
        }

        private bool IsLastAccessedValid()
        {
            return lastAccessedIndex != -1;
        }

        private LinkedListNode<string> GetNodeAt(int index)
        {
            LinkedListNode<string> nodeAtIndex;
            if (index == 0)
            {
                nodeAtIndex = lines.First;
            }
            else if (index == Count - 1)
            {
                nodeAtIndex = lines.Last;
            }
            else
            {
                int searchDirection;
                int refIndex;
                GetClosestReference(index, out nodeAtIndex, out refIndex, out searchDirection);
                while (nodeAtIndex != null)
                {
                    if (refIndex == index)
                    {
                        break;
                    }

                    refIndex += searchDirection;
                    if (searchDirection > 0)
                    {
                        nodeAtIndex = nodeAtIndex.Next;
                    }
                    else
                    {
                        nodeAtIndex = nodeAtIndex.Previous;
                    }
                }

                if (nodeAtIndex == null)
                {
                    throw new InvalidOperationException();
                }
            }

            SetLastAccessed(index, nodeAtIndex);
            return nodeAtIndex;
        }

        private void GetClosestReference(
            int index,
            out LinkedListNode<string> refNode,
            out int refIndex,
            out int searchDirection)
        {
            var delta = index - lastAccessedIndex;
            var deltaAbs = Math.Abs(delta);

            // lastAccessedIndex is closer to index than that to 0
            if (IsLastAccessedValid() && deltaAbs < index)
            {
                // lastAccessedIndex is closer to index than to (Count - 1)
                if (deltaAbs < (Count - 1 - index))
                {
                    refNode = lastAccessedNode;
                    refIndex = lastAccessedIndex;
                    searchDirection = Math.Sign(delta);
                }
                else
                {
                    refNode = lines.Last;
                    refIndex = Count - 1;
                    searchDirection = -1;
                }
            }
            else
            {
                refNode = lines.First;
                refIndex = 0;
                searchDirection = 1;
            }
        }

        private static void ThrowIfNull<T>(T param, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
