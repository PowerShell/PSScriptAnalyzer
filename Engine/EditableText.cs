// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// A class to represent text to which `TextEdit`s can be applied.
    /// </summary>
    public class EditableText
    {
        private TextLines lines { get; set; }

        /// <summary>
        /// Return the number of lines in the text.
        /// </summary>
        public int LineCount => lines.Count;

        /// <summary>
        /// The text that is available for editing.
        /// </summary>
        public string Text { get { return String.Join(NewLine, lines); } }

        /// <summary>
        /// The lines in the Text.
        /// </summary>
        public ReadOnlyCollection<string> Lines => lines.ReadOnly();

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
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, Strings.EditableTextInvalidLineEnding),
                    nameof(text));
            }

            return numCharDiff / (lines.Length - 1);
        }
    }
}
