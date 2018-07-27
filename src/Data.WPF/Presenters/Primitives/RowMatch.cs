using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters.Primitives
{
    internal struct RowMatch : IEquatable<RowMatch>
    {
        public RowMatch(IReadOnlyList<Column> columns, DataRow dataRow, int valueHashCode)
        {
            Debug.Assert(columns != null);
            Columns = columns;
            DataRow = dataRow;
            ValueHashCode = valueHashCode;
        }

        public static int? GetHashCode(IReadOnlyList<Column> columns, DataRow dataRow)
        {
            if (columns == null || columns.Count == 0)
                return null;

            unchecked
            {
                var hash = 2166136261;
                for (int i = 0; i < columns.Count; i++)
                {
                    hash = hash ^ (uint)columns[i].GetHashCode(dataRow);
                    hash = hash * 16777619;
                }
                return unchecked((int)hash);
            }
        }

        public readonly IReadOnlyList<Column> Columns;
        public readonly DataRow DataRow;
        public readonly int ValueHashCode;

        public override int GetHashCode()
        {
            return ValueHashCode;
        }

        public bool Equals(RowMatch other)
        {
            if (ValueHashCode != other.ValueHashCode)
                return false;

            if (AreEqual(Columns, other.Columns))
                return DataRow == other.DataRow;

            if (Columns.Count != other.Columns.Count)
                return false;

            for (int i = 0; i < Columns.Count; i++)
            {
                if (!Columns[i].Equals(DataRow, other.Columns[i], other.DataRow))
                    return false;
            }
            return true;
        }

        private static bool AreEqual(IReadOnlyList<Column> source, IReadOnlyList<Column> target)
        {
            if (source == null || target == null)
                return source == target;

            var sourceCount = source.Count;
            var targetCount = target.Count;
            if (sourceCount != targetCount)
                return false;

            for (int i = 0; i < sourceCount; i++)
            {
                if (source[i] != target[i])
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RowMatch))
                return false;
            return Equals((RowMatch)obj);
        }
    }
}
