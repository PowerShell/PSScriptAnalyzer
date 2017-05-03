using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    // TODO add documentation
    public class EditableText
    {
        public string Text { get; private set; }
        public string[] Lines { get; private set; }
        public string NewLine { get; private set; }
        public int NumNewLineChars { get { return NewLine.Length; } }

        public EditableText(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            this.Text = text;
            Lines = this.Text.GetLines().ToArray();
            NewLine = GetNewLineCharacters();
        }

        // TODO Use EditorServices implementation as it is very well tested.
        public EditableText ApplyEdit(TextEdit textEdit)
        {
            ValidateTextEdit(textEdit);
            var stringBuilder = new StringBuilder(Text.Substring(
                0,
                GetOffset(textEdit.StartLineNumber, textEdit.StartColumnNumber)));
            stringBuilder.Append(textEdit.Text);
            stringBuilder.Append(Text.Substring(GetOffset(textEdit.EndLineNumber, textEdit.EndColumnNumber)));
            return new EditableText(stringBuilder.ToString());
        }

        // TODO replace apply edit with an optimized version of this.
        public EditableText ApplyEdit1(TextEdit textEdit)
        {
            ValidateTextEdit(textEdit);

            // Break up the change lines
            var changeLines = textEdit.Lines;
            var Lines = new List<String>(this.Lines);

            // Get the first fragment of the first line
            string firstLineFragment =
                Lines[textEdit.StartLineNumber - 1]
                    .Substring(0, textEdit.StartColumnNumber - 1);

            // Get the last fragment of the last line
            string endLine = Lines[textEdit.EndLineNumber - 1];
            string lastLineFragment =
                endLine.Substring(
                    textEdit.EndColumnNumber - 1,
                    Lines[textEdit.EndLineNumber - 1].Length - textEdit.EndColumnNumber + 1);

            // Remove the old lines
            for (int i = 0; i <= textEdit.EndLineNumber - textEdit.StartLineNumber; i++)
            {
                Lines.RemoveAt(textEdit.StartLineNumber - 1);
            }

            // Build and insert the new lines
            int currentLineNumber = textEdit.StartLineNumber;
            for (int changeIndex = 0; changeIndex < changeLines.Length; changeIndex++)
            {
                // Since we split the lines above using \n, make sure to
                // trim the ending \r's off as well.
                string finalLine = changeLines[changeIndex].TrimEnd('\r');

                // Should we add first or last line fragments?
                if (changeIndex == 0)
                {
                    // Append the first line fragment
                    finalLine = firstLineFragment + finalLine;
                }
                if (changeIndex == changeLines.Length - 1)
                {
                    // Append the last line fragment
                    finalLine = finalLine + lastLineFragment;
                }

                Lines.Insert(currentLineNumber - 1, finalLine);
                currentLineNumber++;
            }

            return new EditableText(String.Join(NewLine, Lines));
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
                offset += Lines[k].Length + NumNewLineChars;
            }

            return offset + zeroBasedColumnNumber;
        }

        private string GetNewLineCharacters()
        {
            if (Lines.Length == 1)
            {
                return Environment.NewLine;
            }

            return Text.Substring(Lines[0].Length, GetNumNewLineCharacters());
        }

        // TODO No need to do all this. Just look at the first character at the end of first line.
        private int GetNumNewLineCharacters()
        {
            if (Lines.Length == 1)
            {
                return Environment.NewLine.Length;
            }

            var charsInLines = Lines.Sum(line => line.Length);
            var numCharDiff = Text.Length - charsInLines;
            int remainder = numCharDiff % (Lines.Length - 1);
            if (remainder != 0)
            {
                // TODO localize
                throw new ArgumentException("Cannot determine line endings as the text probably contain mixed line endings.", nameof(Text));
            }

            return numCharDiff / (Lines.Length - 1);
        }

        private IScriptExtent GetExtent()
        {
            return new ScriptExtent(new ScriptPosition(null, 1, 1, Lines[0]),
                new ScriptPosition(null, Lines.Length, Lines[Lines.Length - 1].Length + 1, Lines[Lines.Length - 1]));
        }
    }
}
