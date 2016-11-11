using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    public static class ColumnSet
    {
        private class EmptyColumnSet : IColumnSet
        {
            public bool Contains(Column column)
            {
                return false;
            }

            public int Count
            {
                get { return 0; }
            }

            public Column this[int index]
            {
                get { throw new ArgumentOutOfRangeException(nameof(index)); }
            }

            public IEnumerator<Column> GetEnumerator()
            {
                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield break;
            }
        }

        private class ListColumnSet : List<Column>, IColumnSet
        {
            public ListColumnSet(IList<Column> columns)
                : base(columns)
            {
            }
        }

        public static readonly IColumnSet Empty = new EmptyColumnSet();

        public static IColumnSet New(params Column[] columns)
        {
            return New(columns, false);
        }

        private static IColumnSet New(IList<Column> columns, bool isNormalized)
        {
            if (columns == null || columns.Count == 0)
                return Empty;

            if (columns.Count == 1)
            {
                var column = columns[0];
                return column == null ? Empty : column;
            }

            if (isNormalized)
                return new ListColumnSet(columns);

            return New(Normalize(columns), true);
        }

        private static IList<Column> Normalize(IList<Column> columns)
        {
            Debug.Assert(columns.Count > 1);

            List<Column> result = null;
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                if (column == null)
                {
                    result = CopyFrom(columns, i);
                    continue;
                }

                if (result != null)
                {
                    if (!result.Contains(column))
                        result.Add(column);
                    continue;
                }

                for (int j = 0; j < i; j++)
                {
                    if (column == columns[j])
                    {
                        result = CopyFrom(columns, i);
                        continue;
                    }
                }
            }

            return result == null ? columns : result;
        }

        private static List<Column> CopyFrom(IList<Column> columns, int count)
        {
            var result = new List<Column>();
            for (int i = 0; i < count; i++)
                result.Add(columns[i]);

            return result;
        }

        public static IColumnSet Merge(this IColumnSet columnSet, IColumnSet value)
        {
            Check.NotNull(columnSet, nameof(columnSet));
            Check.NotNull(value, nameof(value));

            var count1 = columnSet.Count;
            if (count1 == 0)
                return value;
            var count2 = value.Count;
            if (count2 == 0)
                return columnSet;

            return count1 >= count2 ? DoMerge(columnSet, value) : DoMerge(value, columnSet);
        }

        private static IColumnSet DoMerge(IColumnSet x, IColumnSet y)
        {
            Debug.Assert(x.Count >= y.Count);

            for (int i = 0; i < y.Count; i++)
            {
                if (!x.Contains(y[i]))
                    return DoMerge(x, y, i);
            }
            return x;
        }

        private static IColumnSet DoMerge(IColumnSet x, IColumnSet y, int startYIndex)
        {
            var result = new List<Column>(x);
            result.Add(y[startYIndex]);
            for (int i = startYIndex + 1; i < y.Count; i++)
            {
                if (!result.Contains(y[i]))
                    result.Add(y[i]);
            }
            return new ListColumnSet(result);
        }
    }
}
