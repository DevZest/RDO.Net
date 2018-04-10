using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data.Presenters.Primitives
{
    internal struct RowMatch : IEquatable<RowMatch>
    {
        public RowMatch(IDataValues dataValues, int valueHashCode)
        {
            Debug.Assert(dataValues != null);
            DataValues = dataValues;
            ValueHashCode = valueHashCode;
        }

        public readonly IDataValues DataValues;
        public readonly int ValueHashCode;

        public override int GetHashCode()
        {
            return ValueHashCode;
        }

        public bool Equals(RowMatch other)
        {
            if (ValueHashCode != other.ValueHashCode)
                return false;

            if (DataValues == other.DataValues)
                return true;

            if (DataValues == null || other.DataValues == null)
                return false;

            var sourceDataRow = DataValues.DataRow;
            var targetDataRow = other.DataValues.DataRow;
            var sourceColumns = DataValues.Columns;
            var targetColumns = other.DataValues.Columns;

            if (AreEqual(sourceColumns, targetColumns))
                return sourceDataRow == targetDataRow;

            if (sourceColumns.Count != targetColumns.Count)
                return false;

            for (int i = 0; i < sourceColumns.Count; i++)
            {
                if (!sourceColumns[i].Equals(DataValues.DataRow, targetColumns[i], other.DataValues.DataRow))
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
