// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Class to represent range in text. Range is represented as a pair of positions, [Start, End).
    /// </summary>
    public class Range
    {
        /// <summary>
        /// Constructs a Range object to represent a range of text.
        /// </summary>
        /// <param name="start">The start position of the text.</param>
        /// <param name="end">The end position of the text, such that range is [start, end).</param>
        public Range(Position start, Position end)
        {
            Start = new Position(start);
            End = new Position(end);
            ValidatePositions();
        }

        /// <summary>
        /// Constructs a Range object to represent a range of text.
        /// </summary>
        /// <param name="startLineNumber">1-based line number on which the text starts.</param>
        /// <param name="startColumnNumber">1-based offset on start line at which the text starts. This includes the first character of the text.</param>
        /// <param name="endLineNumber">1-based line number on which the text ends.</param>
        /// <param name="endColumnNumber">1-based offset on end line at which the text ends. This offset value is 1 more than the offset of the last character of the text. </param>
        public Range(int startLineNumber, int startColumnNumber, int endLineNumber, int endColumnNumber)
            : this(
                new Position(startLineNumber, startColumnNumber),
                new Position(endLineNumber, endColumnNumber))
        {
            ValidatePositions();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="range">A Range object</param>
        public Range(Range range)
        {
            if (range == null)
            {
                throw new ArgumentNullException(nameof(range));
            }

            Start = new Position(range.Start);
            End = new Position(range.End);
        }

        /// <summary>
        /// Start position of the range.
        /// </summary>
        public Position Start { get; }

        /// <summary>
        /// End position of the range.
        ///
        /// This position does not contain the last character of the range, but instead is the position
        /// right after the last character in the range.
        /// </summary>
        public Position End { get; }

        /// <summary>
        /// Normalize a range with respect to the input position.
        /// </summary>
        /// <param name="refPosition">Reference position.</param>
        /// <param name="range">Range to be normalized.</param>
        /// <returns>Range object with normalized positions.</returns>
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

        /// <summary>
        /// Returns a new range object with shifted positions.
        /// </summary>
        public Range Shift(int lineDelta, int columnDelta)
        {
            var newStart = Start.Shift(lineDelta, columnDelta);
            var newEnd = End.Shift(lineDelta, Start.Line == End.Line ? columnDelta : 0);
            return new Range(newStart, newEnd);
        }

        private void ValidatePositions()
        {
            if (Start > End)
            {
                throw new ArgumentException(String.Format(
                    CultureInfo.CurrentCulture,
                    Strings.RangeStartPosGreaterThanEndPos));
            }
        }
    }
}
