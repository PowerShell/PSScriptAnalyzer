using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.Extensions
{
    public static class Extensions
    {
        public static bool Contains(this IScriptExtent extentOuter, IScriptExtent extentInner)
        {
            return extentOuter.StartLineNumber <= extentOuter.StartLineNumber
                && extentOuter.EndLineNumber >= extentInner.EndLineNumber
                && extentOuter.StartColumnNumber <= extentInner.StartColumnNumber
                && extentOuter.EndColumnNumber >= extentInner.EndColumnNumber;
        }

        // TODO Create an editable string or EditableText class which
        // - use some of these extension methods
        // - creates an immutable object.
        // - we can make this class private as we do not need to expose its apis
        // TODO make apply a TextEdit extenstion which takes an EditableString object
        public static string ApplyEdit(this string text, TextEdit textEdit)
        {
            if (textEdit == null)
            {
                throw new NullReferenceException(nameof(textEdit));
            }

            var lines = text.GetLines().ToArray();

            // Check if the text edits extent make sense
            var textExtent = text.Extent(lines);
            if (!textExtent.Contains(textEdit.ScriptExtent))
            {
                // TODO localize this
                throw new ArgumentException("TextEdit is not strictly contained in text.");
            }

            var stringBuilder = new StringBuilder(text.Substring(0, textEdit.ScriptExtent.StartScriptPosition.ToOffset(text)));
            stringBuilder.Append(textEdit.NewText);
            stringBuilder.Append(text.Substring(textEdit.ScriptExtent.EndScriptPosition.ToOffset(text)));
            return stringBuilder.ToString();
        }

        public static IEnumerable<string> GetLines(this string text)
        {
            var lines = new List<string>();
            using (var stringReader = new StringReader(text))
            {
                string line;
                line = stringReader.ReadLine();
                while (line != null)
                {
                    yield return line;
                    line = stringReader.ReadLine();
                }

            }
        }

        public static int ToOffset(this IScriptPosition scriptPosition, string text)
        {
            return text.GetOffset(scriptPosition.LineNumber, scriptPosition.ColumnNumber);
        }

        // This can be private member of the EditableText class
        public static int GetOffset(this string text, int lineNumber, int columnNumber)
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
            var lines = text.GetLines().ToArray();
            var numNewLineChars = text.GetNumNewLineCharacters(lines);
            var offset = 0;
            for (var k = 0; k < zeroBasedLineNumber; k++)
            {
                offset += lines[k].Length + numNewLineChars;
            }

            return offset + zeroBasedColumnNumber;
        }

        public static string GetNewLineCharacters(this string text)
        {
            return text.GetNewLineCharacters(text.GetLines().ToArray());
        }

        private static string GetNewLineCharacters(this string text, string[] lines)
        {
            if (lines.Length == 1)
            {
                return Environment.NewLine;
            }

            return text.Substring(lines[0].Length, text.GetNumNewLineCharacters(lines));
        }

        private static int GetNumNewLineCharacters(this string text, string[] lines)
        {
            if (lines.Length == 1)
            {
                return Environment.NewLine.Length;
            }

            var charsInLines = lines.Sum(line => line.Length);
            var numCharDiff = text.Length - charsInLines;
            int remainder = numCharDiff % (lines.Length - 1);
            if (remainder != 0)
            {
                throw new ArgumentException("Cannot determine line endings as the text might contain mixed line endings.", nameof(text));
            }

            return numCharDiff / (lines.Length - 1);
        }
        public static IScriptExtent GetExtent(this string text)
        {
            return text.Extent(text.GetLines().ToArray());
        }

        public static IScriptExtent Extent(this string text, string[] lines)
        {
            return new ScriptExtent(new ScriptPosition(null, 1, 1, lines[0]),
                new ScriptPosition(null, lines.Length, lines[lines.Length - 1].Length + 1, lines[lines.Length - 1]));
        }
    }
}
