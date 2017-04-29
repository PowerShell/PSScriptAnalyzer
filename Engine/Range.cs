using System;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    public class Range
    {
        public Position Start { get; }
        public Position End { get; }
        public Range(Position start, Position end)
        {
            if (start > end)
            {
                throw new ArgumentException("start position cannot be before end position.");
            }

            Start = new Position(start);
            End = new Position(end);
        }

        public Range(int startLineNumber, int startColumnNumber, int endLineNumber, int endColumnNumber)
            : this(
                new Position(startLineNumber, startColumnNumber),
                new Position(endLineNumber, endColumnNumber))
        {

        }

        public Range(Range range)
        {
            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            Start = new Position(range.Start);
            End = new Position(range.End);
        }

        public Range Shift(int lineDelta, int columnDelta)
        {
            var newStart = Start.Shift(lineDelta, columnDelta);
            var newEnd = End.Shift(lineDelta, Start.Line == End.Line ? columnDelta : 0);
            return new Range(newStart, newEnd);
        }

        public static Range Normalize(Range refRange, Range range)
        {
            if (refRange == null)
            {
                throw new ArgumentNullException(nameof(refRange));
            }

            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            if (refRange.Start > range.Start)
            {
                throw new ArgumentException("reference range should start before range");
            }

            return range.Shift(
                1 - refRange.Start.Line,
                range.Start.Line == refRange.Start.Line ? 1 - refRange.Start.Column : 0);
        }
    }
}
