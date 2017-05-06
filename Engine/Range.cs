using System;
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    // TODO doc
    public class Range
    {
        public Position Start { get; }
        public Position End { get; }
        public Range(Position start, Position end)
        {
            if (start > end)
            {
                throw new ArgumentException(String.Format(
                    CultureInfo.CurrentCulture,
                    Strings.RangeStartPosGreaterThanEndPos));
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

        public static Range Normalize(Position refPosition, Range range)
        {
            if (refPosition == null)
            {
                throw new ArgumentNullException(nameof(refPosition));
            }

            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            if (refPosition > range.Start)
            {
                throw new ArgumentException(String.Format(
                    CultureInfo.CurrentCulture,
                    Strings.RangeRefPosShouldStartBeforeRangeStartPos));
            }

            return range.Shift(
                1 - refPosition.Line,
                range.Start.Line == refPosition.Line ? 1 - refPosition.Column : 0);
        }
    }
}
