using System;
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
        public IScriptExtent Extent { get; private set; }
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
            Extent = GetExtent();
            NewLine = GetNewLineCharacters();
        }

        public EditableText ApplyEdit(TextEdit textEdit)
        {
            if (textEdit == null)
            {
                throw new NullReferenceException(nameof(textEdit));
            }

            if (!Extent.Contains(textEdit.ScriptExtent))
            {
                throw new ArgumentException("TextEdit is not strictly contained in text.");
            }

            var stringBuilder = new StringBuilder(Text.Substring(
                0,
                GetOffset(textEdit.ScriptExtent.StartScriptPosition)));
            stringBuilder.Append(textEdit.NewText);
            stringBuilder.Append(Text.Substring(GetOffset(textEdit.ScriptExtent.EndScriptPosition)));
            return new EditableText(stringBuilder.ToString());
        }

        // TODO Add a method that takes multiple edits, checks if they are unique and applies them.

        public override string ToString()
        {
            return Text;
        }

        private int GetOffset(IScriptPosition scriptPosition)
        {
            return GetOffset(scriptPosition.LineNumber, scriptPosition.ColumnNumber);
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
