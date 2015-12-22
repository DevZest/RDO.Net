using DevZest.Data.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    internal class ColumnSet : List<Column>, IColumnSet
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

        public static readonly IColumnSet Empty = new EmptyColumnSet();

        public static IColumnSet Create(params Column[] columns)
        {
            return Create(columns, false);
        }

        private static IColumnSet Create(IList<Column> columns, bool isNormalized)
        {
            if (columns == null || columns.Count == 0)
                return Empty;

            if (columns.Count == 1)
            {
                var column = columns[0];
                return column == null ? Empty : column;
            }

            if (isNormalized)
                return new ColumnSet(columns);

            return Create(Normalize(columns), true);
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

        private ColumnSet(IList<Column> columns)
            : base(columns)
        {
        }
    }
}
