using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DevZest.Data.Primitives
{
    internal sealed class AutoColumnSelector : IAutoColumnSelector
    {
        public AutoColumnSelector(IEnumerable<Column> columns1, IEnumerable<Column> columns2)
        {
            foreach (var column in columns1)
                Add(column);
            foreach (var column in columns2)
                Add(column);
        }

        private Dictionary<ColumnId, IColumns> _byModelId = new Dictionary<ColumnId, IColumns>();
        private Dictionary<ColumnId, IColumns> _byOriginalId = new Dictionary<ColumnId, IColumns>();

        private void Add(Column column)
        {
            Debug.Assert(column != null);
            Add(_byModelId, column, c => c.Id);
            Add(_byOriginalId, column, c => c.OriginalId);
        }

        private static void Add(Dictionary<ColumnId, IColumns> dictionary, Column column, Func<Column, ColumnId> columnIdGetter)
        {
            var columnId = columnIdGetter(column);
            IColumns columns;
            if (dictionary.TryGetValue(columnId, out columns))
                dictionary[columnId] = columns.Add(column);
            else
                dictionary.Add(columnId, column);

        }

        public Column Select(Column column)
        {
            return Select(_byModelId, column, c => c.Id) ?? Select(_byOriginalId, column, c => c.OriginalId);
        }

        private static Column Select(Dictionary<ColumnId, IColumns> dictionary, Column column, Func<Column, ColumnId> columnIdGetter)
        {
            var columnId = columnIdGetter(column);

            IColumns columns;
            if (dictionary.TryGetValue(columnId, out columns))
            {
                if (columns.Count == 1)
                    return columns.Single();
            }
            return null;
        }

        public IAutoColumnSelector Merge(IEnumerable<Column> columns)
        {
            foreach (var column in columns)
                Add(column);

            return this;
        }
    }
}
