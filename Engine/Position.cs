using System;
using System.Globalization;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer
{
    public class Position
    {
        public int Line { get; }
        public int Column { get; }
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

        public Position(Position position)
        {
            if (position == null)
            {
                throw new ArgumentNullException(nameof(position));
            }

            Line = position.Line;
            Column = position.Column;
        }

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

        public static bool operator !=(Position lhs, Position rhs)
        {
            return !(lhs == rhs);
        }

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

        public static bool operator <=(Position lhs, Position rhs)
        {
            return lhs == rhs || lhs < rhs;
        }

        public static bool operator >(Position lhs, Position rhs)
        {
            return !(lhs <= rhs);
        }

        public static bool operator >=(Position lhs, Position rhs)
        {
            return !(lhs < rhs);
        }

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

        public override int GetHashCode()
        {
            return Line * Column;
        }
    }
}
