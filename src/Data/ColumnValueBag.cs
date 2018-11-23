using DevZest.Data.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
                key.VerifyNotNull(nameof(key));
                return _columnValues[key];
            }
            set
            {
                key.VerifyNotNull(nameof(key));
                if (value == null)
                {
                    if (!CanAssignNull(key.DataType))
                        throw new ArgumentException(DiagnosticMessages.ColumnValueBag_NotAssignableFromNull, nameof(value));
                }
                else
                {
                    var columnDataType = key.DataType;
                    var valueDataType = value.GetType();
                    if (!columnDataType.IsAssignableFrom(valueDataType))
                        throw new ArgumentException(DiagnosticMessages.ColumnValueBag_NotAssignableFromValue(columnDataType.ToString(), valueDataType.ToString()), nameof(value));
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

        public bool ContainsKey(CandidateKey key)
        {
            for (int i = 0; i < key.Count; i++)
            {
                var column = key[i].Column;
                if (!ContainsKey(column))
                    return false;
            }
            return true;
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
            column.VerifyNotNull(nameof(column));
            _columnValues[column] = value;
        }

        public void SetValue(Column column, DataRow dataRow)
        {
            column.VerifyNotNull(nameof(column));
            dataRow.VerifyNotNull(nameof(dataRow));
            _columnValues[column] = column.GetValue(dataRow);
        }

        public int AutoSelect(CandidateKey key, DataRow dataRow)
        {
            key.VerifyNotNull(nameof(key));
            dataRow.VerifyNotNull(nameof(dataRow));

            var valueKey = dataRow.Model.PrimaryKey;
            if (valueKey != null && valueKey.GetType() == key.GetType())
            {
                for (int i = 0; i < valueKey.Count; i++)
                    _columnValues[key[i].Column] = valueKey[i].Column.GetValue(dataRow);
                return valueKey.Count;
            }

            var result = 0;
            var valueColumns = dataRow.Model.Columns;
            foreach (var columnSort in key)
            {
                var keyColumn = columnSort.Column;
                var valueColumn = valueColumns.AutoSelect(keyColumn);
                if (valueColumn != null)
                {
                    _columnValues[keyColumn] = valueColumn.GetValue(dataRow);
                    result++;
                }
            }
            return result;
        }

        public int AutoSelect(Projection projection, DataRow dataRow, bool ignoreExpression = true)
        {
            projection.VerifyNotNull(nameof(projection));
            dataRow.VerifyNotNull(nameof(dataRow));

            var result = 0;
            var keyColumns = projection.Columns;
            var valueColumns = dataRow.Model.Columns;
            for (int i = 0; i < keyColumns.Count; i++)
            {
                var keyColumn = keyColumns[i];
                if (keyColumn.IsExpression && ignoreExpression)
                    continue;
                var valueColumn = valueColumns.AutoSelect(keyColumn);
                if (valueColumn != null)
                {
                    _columnValues[keyColumn] = valueColumn.GetValue(dataRow);
                    result++;
                }
            }

            return result;
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
            column.VerifyNotNull(nameof(column));
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
