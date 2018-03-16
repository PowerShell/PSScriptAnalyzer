// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    /// <summary>
    /// Class to represent position in text.
    /// </summary>
    public class Position
    {
        /// <summary>
        /// Constructs a Position object.
        /// </summary>
        /// <param name="line">1-based line number.</param>
        /// <param name="column">1-based column number.</param>
        public Position(int line, int column)
        {
            if (line < 1)
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, Strings.PositionLineLessThanOne),
                    nameof(line));
            }

            if (column < 1)
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, Strings.PositionColumnLessThanOne),
                    nameof(column));
            }

            Line = line;
            Column = column;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="position">Object to be copied.</param>
        public Position(Position position)
        {
            if (position == null)
            {
                throw new ArgumentNullException(nameof(position));
            }

            Line = position.Line;
            Column = position.Column;
        }

        /// <summary>
        /// Line number of the position.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Column number of the position.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Shift the position by given line and column deltas.
        /// </summary>
        /// <param name="lineDelta">Number of lines to shift the position.</param>
        /// <param name="columnDelta">Number of columns to shift the position.</param>
        /// <returns>A new Position object with the shifted position.</returns>
        public Position Shift(int lineDelta, int columnDelta)
        {
            int newLine = Line;
            int newColumn = Column;

            newLine += lineDelta;
            if (newLine < 1)
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, Strings.PositionLineLessThanOne),
                    nameof(lineDelta));
            }

            newColumn += columnDelta;
            if (newColumn < 1)
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, Strings.PositionColumnLessThanOne),
                    nameof(columnDelta));
            }

            return new Position(newLine, newColumn);
        }

        /// <summary>
        /// Normalize position with respect to a reference position.
        /// </summary>
        /// <param name="refPos">Reference position.</param>
        /// <param name="pos">Position to be normalized.</param>
        /// <returns>A Position object with normalized position.</returns>
        public static Position Normalize(Position refPos, Position pos)
        {
            if (refPos == null)
            {
                throw new ArgumentNullException(nameof(refPos));
            }

            if (pos == null)
            {
                throw new ArgumentNullException(nameof(pos));
            }

            if (pos < refPos)
            {
                throw new ArgumentException(String.Format(
                    CultureInfo.CurrentCulture,
                    Strings.PositionRefPosLessThanInputPos));
            }

            if (pos.Line == refPos.Line)
            {
                return pos.Shift(0, pos.Column - refPos.Column + 1);
            }
            else
            {
                return pos.Shift(pos.Line - refPos.Line + 1, 0);
            }
        }

        /// <summary>
        /// Checks if two position objects are equal.
        /// </summary>
        public static bool operator ==(Position lhs, Position rhs)
        {
            if ((object)lhs == null)
            {
                return (object)rhs == null;
            }

            if ((object)rhs == null)
            {
                return false;
            }

            if (System.Object.ReferenceEquals(lhs, rhs))
            {
                return true;
            }

            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Checks if the position objects are not equal.
        /// </summary>
        public static bool operator !=(Position lhs, Position rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Checks if the left hand position comes before the right hand position.
        /// </summary>
        public static bool operator <(Position lhs, Position rhs)
        {
            if (lhs == null)
            {
                throw new ArgumentNullException(nameof(lhs));
            }

            if (rhs == null)
            {
                throw new ArgumentNullException(nameof(rhs));
            }

            return lhs.Line < rhs.Line || (lhs.Line == rhs.Line && lhs.Column < rhs.Column);
        }

        /// <summary>
        /// Checks if the left hand position comes before or is at the same position as that of the right hand position.
        /// </summary>
        public static bool operator <=(Position lhs, Position rhs)
        {
            return lhs == rhs || lhs < rhs;
        }

        /// <summary>
        /// Checks if the left hand position comes after the right hand position.
        /// </summary>
        public static bool operator >(Position lhs, Position rhs)
        {
            return !(lhs <= rhs);
        }

        /// <summary>
        /// Checks if the left hand position comes after or is at the same position as that of the right hand position.
        /// </summary>
        public static bool operator >=(Position lhs, Position rhs)
        {
            return !(lhs < rhs);
        }

        /// <summary>
        /// Checks of this object is equal the input object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Position p = obj as Position;
            if (p == null)
            {
                return false;
            }

            return Line == p.Line && Column == p.Column;
        }

        /// <summary>
        /// Returns the hash code of this object
        /// </summary>
        public override int GetHashCode()
        {
            return Line * Column;
        }
    }
}
