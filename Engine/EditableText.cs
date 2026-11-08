// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// A class to represent text to which `TextEdit`s can be applied.
    /// </summary>
    public class EditableText
    {
        private static readonly Regex LineEnding = new Regex(@"(\r\n|\n|\r)");
        private readonly TextLines lines;
        private readonly TextLines mixedLineEndings;

        /// <summary>
        /// Return the number of lines in the text.
        /// </summary>
        public int LineCount => lines.Count;

        /// <summary>
        /// The text that is available for editing.
        /// </summary>
        public string Text
        {
            get
            {
                if (mixedLineEndings == null)
                {
                    return string.Join(NewLine, lines);
                }

                var text = new StringBuilder();
                using (var endings = mixedLineEndings.GetEnumerator())
                {
                    foreach (string line in lines)
                    {
                        text.Append(line);
                        if (endings.MoveNext())
                        {
                            text.Append(endings.Current);
                        }
                    }
                }

                return text.ToString();
            }
        }

        /// <summary>
        /// The lines in the Text.
        /// </summary>
        public ReadOnlyCollection<string> Lines => lines.ReadOnly();

        /// <summary>
        /// The line ending used when inserting a new line.
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

            // The captured delimiters occupy the odd indexes between lines.
            string[] parts = LineEnding.Split(text);
            lines = new TextLines(parts.Where((part, index) => index % 2 == 0));
            NewLine = parts.Length > 1 ? parts[1] : Environment.NewLine;
            var endings = parts.Where((part, index) => index % 2 == 1);
            if (endings.Any(ending => ending != NewLine))
            {
                mixedLineEndings = new TextLines(endings);
            }
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
            int firstLineIndex = textEdit.StartLineNumber - 1;
            int linesToRemove = textEdit.EndLineNumber - textEdit.StartLineNumber + 1;
            for (int i = 0; i < linesToRemove; i++)
            {
                lines.RemoveAt(firstLineIndex);
            }

            // Keep the separator after the last replaced line.
            if (mixedLineEndings != null)
            {
                for (int i = 1; i < linesToRemove; i++)
                {
                    mixedLineEndings.RemoveAt(firstLineIndex);
                }

                if (editLines.Length == 0 && lines.Count > 0)
                {
                    // No replacement line remains to carry the separator.
                    mixedLineEndings.RemoveAt(Math.Min(firstLineIndex, lines.Count - 1));
                }
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
                if (changeIndex < editLines.Length - 1)
                {
                    mixedLineEndings?.Insert(currentLineNumber - 1, NewLine);
                }
                currentLineNumber++;
            }

            // returning self allows us to chain ApplyEdit calls.
            return this;
        }

        // TODO Add a method that takes multiple edits, checks if they are unique and applies them.

        /// <summary>
        /// Checks if the range falls within the bounds of the text.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public bool IsValidRange(Range range)
        {
            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            return range.Start.Line <= Lines.Count
                && range.End.Line <= Lines.Count
                && range.Start.Column <= Lines[range.Start.Line - 1].Length + 1
                && range.End.Column <= Lines[range.End.Line - 1].Length + 1;
        }

        /// <summary>
        /// Returns the text representation of the object.
        /// </summary>
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
            if (!IsValidRange(textEdit))
            {
                throw new ArgumentException(String.Format(
                    CultureInfo.CurrentCulture,
                    Strings.EditableTextRangeIsNotContained));
            }
        }
    }
}
