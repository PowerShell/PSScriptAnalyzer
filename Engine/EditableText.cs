using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// A class to represent text to which `TextEdit`s can be applied.
    /// </summary>
    public class EditableText
    {
        private TextLines lines { get; set; }

        /// <summary>
        /// The text that is available for editing.
        /// </summary>
        public string Text { get { return String.Join(NewLine, lines); } }

        /// <summary>
        /// The lines in the Text.
        /// </summary>
        public string[] Lines { get { return lines.ToArray(); } }

        /// <summary>
        /// The new line character in the Text.
        /// </summary>
        public string NewLine { get; private set; }

        /// <summary>
        /// Construct an EditableText type object.
        /// </summary>
        /// <param name="text"></param>
        public EditableText(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            string[] lines;
            NewLine = GetNewLineCharacters(text, out lines);
            this.lines = new TextLines(lines);
        }

        /// <summary>
        /// Apply edits defined by a TextEdit object to Text.
        /// </summary>
        /// <param name="textEdit">A TextEdit object that encapsulates the text and the range that need to be replaced.</param>
        /// <returns>An editable object which contains the supplied edit.</returns>
        public EditableText ApplyEdit(TextEdit textEdit)
        {
            ValidateTextEdit(textEdit);

            var editLines = textEdit.Lines;

            // Get the first fragment of the first line
            string firstLineFragment =
                lines[textEdit.StartLineNumber - 1]
                    .Substring(0, textEdit.StartColumnNumber - 1);

            // Get the last fragment of the last line
            string endLine = lines[textEdit.EndLineNumber - 1];
            string lastLineFragment =
                endLine.Substring(
                    textEdit.EndColumnNumber - 1,
                    lines[textEdit.EndLineNumber - 1].Length - textEdit.EndColumnNumber + 1);

            // Remove the old lines
            for (int i = 0; i <= textEdit.EndLineNumber - textEdit.StartLineNumber; i++)
            {
                lines.RemoveAt(textEdit.StartLineNumber - 1);
            }

            // Build and insert the new lines
            int currentLineNumber = textEdit.StartLineNumber;
            for (int changeIndex = 0; changeIndex < editLines.Length; changeIndex++)
            {
                // Since we split the lines above using \n, make sure to
                // trim the ending \r's off as well.
                string finalLine = editLines[changeIndex].TrimEnd('\r');

                // Should we add first or last line fragments?
                if (changeIndex == 0)
                {
                    // Append the first line fragment
                    finalLine = firstLineFragment + finalLine;
                }
                if (changeIndex == editLines.Length - 1)
                {
                    // Append the last line fragment
                    finalLine = finalLine + lastLineFragment;
                }

                lines.Insert(currentLineNumber - 1, finalLine);
                currentLineNumber++;
            }

            return new EditableText(String.Join(NewLine, lines));
        }

        // TODO Add a method that takes multiple edits, checks if they are unique and applies them.

        public override string ToString()
        {
            return Text;
        }

        private void ValidateTextEdit(TextEdit textEdit)
        {
            if (textEdit == null)
            {
                throw new NullReferenceException(nameof(textEdit));
            }

            ValidateTextEditExtent(textEdit);
        }

        private void ValidateTextEditExtent(TextEdit textEdit)
        {
            if (textEdit.StartLineNumber > Lines.Length
                || textEdit.EndLineNumber > Lines.Length
                || textEdit.StartColumnNumber > Lines[textEdit.StartLineNumber - 1].Length
                || textEdit.EndColumnNumber > Lines[textEdit.EndLineNumber - 1].Length + 1)
            {
                // TODO Localize
                throw new ArgumentException("TextEdit extent not completely contained in EditableText.");
            }
        }

        private int GetOffset(int lineNumber, int columnNumber)
        {
            if (lineNumber < 1)
            {
                throw new ArgumentException("Line number must be greater than 0.", nameof(lineNumber));
            }

            if (columnNumber < 1)
            {
                throw new ArgumentException("Column number must be greater than 0.", nameof(lineNumber));
            }

            var zeroBasedLineNumber = lineNumber - 1;
            var zeroBasedColumnNumber = columnNumber - 1;
            var offset = 0;
            for (var k = 0; k < zeroBasedLineNumber; k++)
            {
                offset += Lines[k].Length + NewLine.Length;
            }

            return offset + zeroBasedColumnNumber;
        }

        private static string GetNewLineCharacters(string text, out string[] lines)
        {
            int numNewLineChars = GetNumNewLineCharacters(text, out lines);
            if (lines.Length == 1)
            {
                return Environment.NewLine;
            }

            return text.Substring(lines[0].Length, numNewLineChars);
        }

        private static int GetNumNewLineCharacters(string text, out string[] lines)
        {
            lines = text.GetLines().ToArray();
            if (lines.Length == 1)
            {
                return Environment.NewLine.Length;
            }

            var charsInLines = lines.Sum(line => line.Length);
            var numCharDiff = text.Length - charsInLines;
            int remainder = numCharDiff % (lines.Length - 1);
            if (remainder != 0)
            {
                // TODO localize
                throw new ArgumentException("Cannot determine line endings as the text probably contain mixed line endings.", nameof(text));
            }

            return numCharDiff / (lines.Length - 1);
        }

        private class TextLines : IList<String>
        {
            private LinkedList<String> lines;
            private int lastAccessedIndex;
            private LinkedListNode<String> lastAccessedNode;

            private void ValidateIndex(int index)
            {
                if (index >= Count || index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }

            private void SetLastAccessed(int index, LinkedListNode<String> node)
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

            private LinkedListNode<String> GetNodeAt(int index)
            {
                if (index == 0)
                {
                    return lines.First;
                }

                if (index == Count - 1)
                {
                    return lines.Last;
                }

                LinkedListNode<string> node;
                int searchDirection;
                int count;
                GetClosestReference(index, out node, out count, out searchDirection);
                while (node != null)
                {
                    if (count == index)
                    {
                        SetLastAccessed(index, node);
                        return node;
                    }

                    count += searchDirection;
                    if (searchDirection > 0)
                    {
                        node = node.Next;
                    }
                    else
                    {
                        node = node.Previous;
                    }
                }

                throw new InvalidOperationException();
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
                lines = new LinkedList<String>();
                Count = 0;
                InvalidateLastAccessed();
            }

            public TextLines(IEnumerable<String> inputLines) : this()
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

                lines = new LinkedList<String>(inputLines);
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
                return String.Join(Environment.NewLine, lines);
            }
        }
    }
}
