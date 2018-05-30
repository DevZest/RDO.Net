using System;
using System.Collections.Generic;

namespace DevZest.Data
{
    public interface IDataValues
    {
        DataRow DataRow { get; }
        IReadOnlyList<Column> Columns { get; }
    }

    public static class DataValues
    {
        public static IDataValues Create(params Column[] columns)
        {
            columns.VerifyNotEmpty(nameof(columns));

            for (int i = 0; i < columns.Length; i++)
            {
                var column = columns[i];
                if (column == null || column.ParentModel != null || column.ScalarSourceModels.Count > 0 || column.AggregateSourceModels.Count > 0)
                    throw new ArgumentException(DiagnosticMessages.DataValues_InvalidColumn, string.Format("{0}[{1}]", nameof(columns), i));
            }

            return new ColumnDataValues(columns);
        }

        private sealed class ColumnDataValues : IDataValues
        {
            public ColumnDataValues(params Column[] columns)
            {
                _columns = columns;
            }

            private readonly IReadOnlyList<Column> _columns;
            public IReadOnlyList<Column> Columns { get { return _columns; } }

            public DataRow DataRow { get { return null; } }
        }

        public static int? GetValueHashCode(this IDataValues dataValues)
        {
            var dataRow = dataValues.DataRow;
            var columns = dataValues.Columns;
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
    }

}
