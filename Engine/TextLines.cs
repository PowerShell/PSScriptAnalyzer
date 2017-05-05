using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    internal class TextLines : IList<string>
    {
        private LinkedList<string> lines;
        private int lastAccessedIndex;
        private LinkedListNode<string> lastAccessedNode;

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
                int count;
                GetClosestReference(index, out nodeAtIndex, out count, out searchDirection);
                while (nodeAtIndex != null)
                {
                    if (count == index)
                    {
                        break;
                    }

                    count += searchDirection;
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

        public int Count { get; private set; }

        public bool IsReadOnly => false;

        public TextLines()
        {
            lines = new LinkedList<string>();
            Count = 0;
            InvalidateLastAccessed();
        }

        public TextLines(IEnumerable<string> inputLines) : this()
        {
            if (inputLines == null)
            {
                throw new ArgumentNullException(nameof(inputLines));
            }

            if (inputLines.Any(line => line == null))
            {
                // todo localize
                throw new ArgumentException("Line element cannot be null.");
            }

            lines = new LinkedList<string>(inputLines);
            Count = lines.Count;
        }

        public void Add(string item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Insert(Count - 1, item);
        }

        public void Clear()
        {
            lines.Clear();
        }

        public bool Contains(string item)
        {
            return lines.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            lines.CopyTo(array, arrayIndex);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return lines.GetEnumerator();
        }

        public int IndexOf(string item)
        {
            var llNode = lines.First;
            int count = 0;
            while (llNode != null)
            {
                if (llNode.Value.Equals(item))
                {
                    return count;
                }

                llNode = llNode.Next;
                count++;
            }

            return -1;
        }

        public void Insert(int index, string item)
        {
            ValidateIndex(index);
            SetLastAccessed(index, lines.AddBefore(GetNodeAt(index), item));
            Count++;
        }

        public bool Remove(string item)
        {
            if (lines.Remove(item))
            {
                Count--;
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return lines.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, lines);
        }
    }
}
