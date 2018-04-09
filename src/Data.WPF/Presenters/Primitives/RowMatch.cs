using System;
using System.Diagnostics;

namespace DevZest.Data.Presenters.Primitives
{
    internal struct RowMatch : IEquatable<RowMatch>
    {
        public RowMatch(RowPresenter rowPresenter, int valueHashCode)
        {
            Debug.Assert(rowPresenter != null);
            RowPresenter = rowPresenter;
            ValueHashCode = valueHashCode;
        }

        public readonly RowPresenter RowPresenter;
        public readonly int ValueHashCode;

        public override int GetHashCode()
        {
            return ValueHashCode;
        }

        public bool Equals(RowMatch other)
        {
            if (ValueHashCode != other.ValueHashCode)
                return false;

            if (RowPresenter == other.RowPresenter)
                return true;

            if (RowPresenter == null || other.RowPresenter == null)
                return false;

            var sourceRowMapper = RowPresenter.RowMapper;
            var targetRowMapper = other.RowPresenter.RowMapper;
            if (sourceRowMapper == targetRowMapper)
                return RowPresenter == other.RowPresenter;

            if (sourceRowMapper == null || targetRowMapper == null)
                return false;

            var sourceColumns = sourceRowMapper.RowMatchColumns;
            if (sourceColumns == null)
                return false;

            var targetColumns = targetRowMapper.RowMatchColumns;
            if (targetColumns == null)
                return false;

            if (sourceColumns.Count != targetColumns.Count)
                return false;

            for (int i = 0; i < sourceColumns.Count; i++)
            {
                if (!sourceColumns[i].Equals(RowPresenter.DataRow, targetColumns[i], other.RowPresenter.DataRow))
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
