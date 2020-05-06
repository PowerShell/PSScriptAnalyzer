
using System.Management.Automation.Language;

namespace Microsoft.PowerShell.ScriptAnalyzer
{
    public class ScriptPosition : IScriptPosition
    {
        public static ScriptPosition FromOffset(string scriptText, string scriptPath, int offset)
        {
            int currLine = 1;
            int i = 0;
            int lastLineOffset = -1;
            while (i < offset)
            {
                lastLineOffset = i;
                i = scriptText.IndexOf('\n', i);
                currLine++;
            }

            return new ScriptPosition(scriptText, scriptPath, scriptText.Substring(lastLineOffset, offset), offset, currLine, offset - lastLineOffset);
        }

        public static ScriptPosition FromPosition(string scriptText, string scriptPath, int line, int column)
        {
            int offset = 0;
            int currLine = 1;
            while (currLine < line)
            {
                offset = scriptText.IndexOf('\n', offset);
                currLine++;
            }

            string lineText = scriptText.Substring(offset, offset + column - 1);
            offset += column - 1;

            return new ScriptPosition(scriptText, scriptPath, lineText, offset, line, column);
        }

        private readonly string _scriptText;

        public ScriptPosition(string scriptText, string scriptPath, string line, int offset, int lineNumber, int columnNumber)
        {
            _scriptText = scriptText;
            File = scriptPath;
            Line = line;
            Offset = offset;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
        }

        public int ColumnNumber { get; }

        public string File { get; }

        public string Line { get; }

        public int LineNumber { get; }

        public int Offset { get; }

        public string GetFullScript() => _scriptText;
    }

    public class ScriptExtent : IScriptExtent
    {
        public static ScriptExtent FromOffsets(string scriptText, string scriptPath, int startOffset, int endOffset)
        {
            return new ScriptExtent(
                scriptText.Substring(startOffset, endOffset - startOffset),
                ScriptPosition.FromOffset(scriptText, scriptPath, startOffset),
                ScriptPosition.FromOffset(scriptText, scriptPath, endOffset));
        }

        public static ScriptExtent FromPositions(string scriptText, string scriptPath, int startLine, int startColumn, int endLine, int endColumn)
        {
            var startPosition = ScriptPosition.FromPosition(scriptText, scriptPath, startLine, startColumn);
            var endPosition = ScriptPosition.FromPosition(scriptText, scriptPath, endLine, endColumn);
            return new ScriptExtent(
                scriptText.Substring(startPosition.Offset, endPosition.Offset - startPosition.Offset),
                startPosition,
                endPosition);
        }

        public ScriptExtent(string text, IScriptPosition start, IScriptPosition end)
        {
            StartScriptPosition = start;
            EndScriptPosition = end;
            Text = text;
        }

        public int EndColumnNumber => EndScriptPosition.ColumnNumber;

        public int EndLineNumber => EndScriptPosition.LineNumber;

        public int EndOffset => EndScriptPosition.Offset;

        public IScriptPosition EndScriptPosition { get; }

        public string File => StartScriptPosition.File;

        public int StartColumnNumber => StartScriptPosition.ColumnNumber;

        public int StartLineNumber => StartScriptPosition.LineNumber;

        public int StartOffset => StartScriptPosition.Offset;

        public IScriptPosition StartScriptPosition { get; }

        public string Text { get; }
    }

}
