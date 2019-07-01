// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    internal struct TextPosition
    {
        public TextPosition(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public int Line { get; }

        public int Column { get; }
    }

    internal struct TextRange
    {
        public TextRange(TextPosition start, TextPosition end)
        {
            Start = start;
            End = end;
        }

        public TextPosition Start { get; }

        public TextPosition End { get; }
    }

    internal class TextDocumentBuilder
    {
        private class CharBuffer
        {
            private char[] _charArray;

            private int _validLength;

            public CharBuffer()
            {
                _charArray = new char[128];
                _validLength = 0;
            }

            public void CopyFrom(string content, int startIndex, int length)
            {
                while (length > _charArray.Length)
                {
                    _charArray = new char[_charArray.Length * 2];
                }

                content.CopyTo(startIndex, _charArray, 0, length);
                _validLength = length;
            }

            public void CopyTo(StringBuilder buffer)
            {
                buffer.Append(_charArray, 0, _validLength);
            }
        }

        private readonly static char s_newlineStart = Environment.NewLine[0];

        private readonly CharBuffer _spanBuffer;

        private string _content;

        public TextDocumentBuilder(string content)
        {
            _content = content;
            _spanBuffer = new CharBuffer();
        }

        public override string ToString()
        {
            return _content;
        }

        public TextRange GetValidColumnIndexRange(TextRange textRange)
        {
            return new TextRange(
                new TextPosition(textRange.Start.Line - 1, Math.Min(1, textRange.Start.Column - 1)),
                new TextPosition(textRange.End.Line - 1, Math.Max(textRange.End.Column - 1, GetLastColumnLength())));
        }

        public bool IsValidRange(TextRange range)
        {
            return range.Start.Line <= range.End.Line
                && range.End.Line <= GetLineCount() + 1
                && range.Start.Column <= GetColumnLength(range.Start.Line) + 1
                && range.End.Column <= GetColumnLength(range.End.Line) + 1;
        }

        public void ApplyCorrections(IReadOnlyList<CorrectionExtent> corrections)
        {
            var newContent = new StringBuilder(_content.Length);
            var effectiveOldPosition = new TextPosition(0, 0);
            int currentIndex = 0;

            foreach (CorrectionExtent correction in corrections)
            {
                var correctionStartPosition = new TextPosition(correction.StartLineNumber - 1, correction.StartColumnNumber - 1);
                ReadNextSpan(ref currentIndex, effectiveOldPosition, correctionStartPosition);
                _spanBuffer.CopyTo(newContent);
                newContent.Append(correction.Text);
                currentIndex += correction.Text.Length;
                effectiveOldPosition = new TextPosition(correction.EndLineNumber - 1, correction.EndColumnNumber - 1);
            }
            ReadToEnd(currentIndex);
            _spanBuffer.CopyTo(newContent);
            _content = newContent.ToString();
        }

        private void ReadNextSpan(ref int index, TextPosition effectiveOldPosition, TextPosition correctionStartPosition)
        {
            int linesToRead = correctionStartPosition.Line - effectiveOldPosition.Line;
            int nextIndex = _content.IndexOf(Environment.NewLine, index, linesToRead) + correctionStartPosition.Column;
            _spanBuffer.CopyFrom(_content, index, nextIndex - index);
            index = nextIndex;
        }

        private void ReadToEnd(int currentIndex)
        {
            _spanBuffer.CopyFrom(_content, currentIndex, _content.Length - currentIndex);
        }

        private int GetColumnLength(int lineNumber)
        {
            int lineIndex = GetLineIndex(lineNumber);
            return _content.IndexOf(Environment.NewLine, lineIndex);
        }

        private int GetLastColumnLength()
        {
            return _content.Length - _content.LastIndexOf(Environment.NewLine);
        }

        private int GetLineCount()
        {
            int lineCount = 0;
            foreach (char c in _content)
            {
                if (c == '\n') { lineCount++; }
            }
            return lineCount;
        }

        private int GetLineIndex(int lineNumber)
        {
            return _content.IndexOf(Environment.NewLine, 0, lineNumber);
        }
    }
}