using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DevZest.Data
{
    public class ColumnValueBag : IReadOnlyDictionary<Column, object>
    {
        private Dictionary<Column, object> _columnValues = new Dictionary<Column, object>();

        public object this[Column key]
        {
            get { return _columnValues[key]; }
        }

        public int Count
        {
            get { return _columnValues.Count; }
        }

        public IEnumerable<Column> Keys
        {
            get { return _columnValues.Keys; }
        }

        public IEnumerable<object> Values
        {
            get { return _columnValues.Values; }
        }

        public bool ContainsKey(Column key)
        {
            return _columnValues.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<Column, object>> GetEnumerator()
        {
            return _columnValues.GetEnumerator();
        }

        public bool TryGetValue(Column key, out object value)
        {
            return _columnValues.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _columnValues.GetEnumerator();
        }

        public void SetValue<T>(Column<T> column, T value)
        {
            Check.NotNull(column, nameof(column));
            _columnValues[column] = value;
        }

        public void SetValue(Column column, DataRow dataRow)
        {
            Check.NotNull(column, nameof(column));
            Check.NotNull(dataRow, nameof(dataRow));
            _columnValues[column] = column.GetValue(dataRow);
        }

        public void AutoSelect(KeyBase key, DataRow dataRow)
        {
            Check.NotNull(key, nameof(key));
            Check.NotNull(dataRow, nameof(dataRow));

            var valueColumns = dataRow.Model.Columns;
            foreach (var columnSort in key)
            {
                var keyColumn = columnSort.Column;
                var valueColumn = valueColumns.AutoSelect(keyColumn);
                if (valueColumn != null)
                    _columnValues[keyColumn] = valueColumn.GetValue(dataRow);
            }
        }

        public void AutoSelect(ModelExtension extension, DataRow dataRow, bool ignoreExpression = true)
        {
            Check.NotNull(extension, nameof(extension));
            Check.NotNull(dataRow, nameof(dataRow));

            var valueColumns = dataRow.Model.Columns;
            foreach (var keyColumn in extension.Columns)
            {
                if (keyColumn.IsExpression && ignoreExpression)
                    continue;
                var valueColumn = valueColumns.AutoSelect(keyColumn);
                if (valueColumn != null)
                    _columnValues[keyColumn] = valueColumn.GetValue(dataRow);
            }
        }

        public void Remove(Column column)
        {
            _columnValues.Remove(column);
        }
        
        public void Clear()
        {
            _columnValues.Clear();
        }

        public T GetValue<T>(Column<T> column)
        {
            Check.NotNull(column, nameof(column));
            return (T)_columnValues[column];
        }
    }
}
