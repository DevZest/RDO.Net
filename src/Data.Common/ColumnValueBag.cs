using DevZest.Data.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DevZest.Data
{
    public sealed class ColumnValueBag : IReadOnlyDictionary<Column, object>
    {
        private Dictionary<Column, object> _columnValues = new Dictionary<Column, object>();

        public object this[Column key]
        {
            get
            {
                Check.NotNull(key, nameof(key));
                return _columnValues[key];
            }
            set
            {
                Check.NotNull(key, nameof(key));
                if (value == null)
                {
                    if (!CanAssignNull(key.DataType))
                        throw new ArgumentException(Strings.ColumnValueBag_NotAssignableFromNull, nameof(value));
                }
                else
                {
                    var columnDataType = key.DataType;
                    var valueDataType = value.GetType();
                    if (!columnDataType.IsAssignableFrom(valueDataType))
                        throw new ArgumentException(Strings.ColumnValueBag_NotAssignableFromValue(columnDataType.ToString(), valueDataType.ToString()), nameof(value));
                }
                _columnValues[key] = value;
            }
        }

        private static bool CanAssignNull(Type type)
        {
            return type.GetTypeInfo().IsValueType ? type.IsNullable() : true;
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

        public void AutoSelect(PrimaryKey key, DataRow dataRow)
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

        public void AutoSelect(ModelExtender extender, DataRow dataRow, bool ignoreExpression = true)
        {
            Check.NotNull(extender, nameof(extender));
            Check.NotNull(dataRow, nameof(dataRow));

            var valueColumns = dataRow.Model.Columns;
            foreach (var keyColumn in extender.Columns)
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

        public ColumnValueBag Clone()
        {
            var result = new ColumnValueBag();
            foreach (var keyValuePair in _columnValues)
                result._columnValues.Add(keyValuePair.Key, keyValuePair.Value);
            return result;
        }

        public void ResetValues()
        {
            foreach (var keyValuePair in _columnValues.ToArray())
            {
                var column = keyValuePair.Key;
                _columnValues[column] = column.GetDefaultValue();
            }
        }
    }
}
