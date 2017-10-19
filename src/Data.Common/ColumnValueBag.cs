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

        public bool DesignMode { get; private set; } = true;

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

        public void Seal()
        {
            DesignMode = false;
        }

        public bool TryGetValue(Column key, out object value)
        {
            return _columnValues.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _columnValues.GetEnumerator();
        }

        private void VerifyDesignMode()
        {
            if (!DesignMode)
                throw new InvalidOperationException(Strings.VerifyDesignMode);
        }

        public void Add<T>(Column<T> column, T value)
        {
            VerifyDesignMode();
            Check.NotNull(column, nameof(column));
            _columnValues.Add(column, value);
        }

        public void Add(Column column, DataRow dataRow)
        {
            VerifyDesignMode();
            Check.NotNull(column, nameof(column));
            Check.NotNull(dataRow, nameof(dataRow));
            _columnValues.Add(column, column.GetValue(dataRow));
        }

        public void AutoSelect(KeyBase key, DataRow dataRow)
        {
            VerifyDesignMode();
            Check.NotNull(key, nameof(key));
            Check.NotNull(dataRow, nameof(dataRow));

            var valueColumns = dataRow.Model.Columns;
            foreach (var columnSort in key)
            {
                var keyColumn = columnSort.Column;
                var valueColumn = valueColumns.AutoSelect(keyColumn);
                if (valueColumn != null)
                    _columnValues.Add(keyColumn, valueColumn.GetValue(dataRow));
            }
        }

        public void AutoSelect(ModelExtension extension, DataRow dataRow, bool ignoreExpression = true)
        {
            VerifyDesignMode();
            Check.NotNull(extension, nameof(extension));
            Check.NotNull(dataRow, nameof(dataRow));

            var valueColumns = dataRow.Model.Columns;
            foreach (var keyColumn in extension.Columns)
            {
                if (keyColumn.IsExpression && ignoreExpression)
                    continue;
                var valueColumn = valueColumns.AutoSelect(keyColumn);
                if (valueColumn != null)
                    _columnValues.Add(keyColumn, valueColumn.GetValue(dataRow));
            }
        }

        public T GetValue<T>(Column<T> column)
        {
            Check.NotNull(column, nameof(column));
            return (T)_columnValues[column];
        }
    }
}
