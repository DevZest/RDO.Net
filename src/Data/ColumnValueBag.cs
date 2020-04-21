using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DevZest.Data
{
    /// <summary>
    /// Stores column and data value as dictionary of key-value pairs.
    /// </summary>
    public sealed class ColumnValueBag : IReadOnlyDictionary<Column, object>
    {
        private Dictionary<Column, object> _columnValues = new Dictionary<Column, object>();

        /// <summary>
        /// Gets or sets the data value for specified column.
        /// </summary>
        /// <param name="key">The column as key.</param>
        /// <returns>The data value.</returns>
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

        /// <summary>
        /// Gets the count of this dictionary.
        /// </summary>
        public int Count
        {
            get { return _columnValues.Count; }
        }

        /// <summary>
        /// Gets the keys in this dictionary.
        /// </summary>
        public IEnumerable<Column> Keys
        {
            get { return _columnValues.Keys; }
        }

        /// <summary>
        /// Gets the values in this dictionary.
        /// </summary>
        public IEnumerable<object> Values
        {
            get { return _columnValues.Values; }
        }

        /// <summary>
        /// Determines whether specified column is contained in this dictionary.
        /// </summary>
        /// <param name="key">The specified column as key.</param>
        /// <returns><see langword="true" /> if specified column is contained in this dictionary, otherwise <see langword="false" />.</returns>
        public bool ContainsKey(Column key)
        {
            return _columnValues.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether all columns of specified <see cref="CandidateKey"/> are contained in this dictionary.
        /// </summary>
        /// <param name="key">The specified <see cref="CandidateKey"/>.</param>
        /// <returns><see langword="true" /> if all columns of specified <see cref="CandidateKey"/> are contained in this dictionary,
        /// otherwise <see langword="false" />.</returns>
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

        /// <summary>
        /// Gets the enumerator of this dictionary.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<Column, object>> GetEnumerator()
        {
            return _columnValues.GetEnumerator();
        }

        /// <summary>
        /// Gets the value that is associated with the specified column key.
        /// </summary>
        /// <param name="key">The column as key.</param>
        /// <param name="value">The data value.</param>
        /// <returns></returns>
        public bool TryGetValue(Column key, out object value)
        {
            return _columnValues.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _columnValues.GetEnumerator();
        }

        /// <summary>
        /// Sets the data value for the specified column key.
        /// </summary>
        /// <typeparam name="T">Data type of the column.</typeparam>
        /// <param name="column">The column as key.</param>
        /// <param name="value">The data value.</param>
        /// <remarks>If column key does not exist, the key-value pair will be added into this dictionary.</remarks>
        public void SetValue<T>(Column<T> column, T value)
        {
            column.VerifyNotNull(nameof(column));
            _columnValues[column] = value;
        }

        /// <summary>
        /// Sets the data value from specified <see cref="DataRow"/> for the specified column key.
        /// </summary>
        /// <param name="column">The column as key.</param>
        /// <param name="dataRow">The <see cref="DataRow"/>.</param>
        /// <remarks>If column key does not exist, the key-value pair will be added into this dictionary.</remarks>
        public void SetValue(Column column, DataRow dataRow)
        {
            column.VerifyNotNull(nameof(column));
            dataRow.VerifyNotNull(nameof(dataRow));
            _columnValues[column] = column.GetValue(dataRow);
        }

        /// <summary>
        /// Automatically select all <see cref="CandidateKey"/> columns and their data values for specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="key">The <see cref="CandidateKey"/>.</param>
        /// <param name="dataRow">The specified <see cref="DataRow"/>.</param>
        /// <returns>Number of columns selected.</returns>
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

        /// <summary>
        /// Automatically select all columns of <see cref="Projection"/> for specified <see cref="DataRow"/>.
        /// </summary>
        /// <param name="projection">The <see cref="Projection"/> object.</param>
        /// <param name="dataRow">The specified <see cref="DataRow"/>.</param>
        /// <param name="ignoreExpression">Specifies whether expression column should be excluded.</param>
        /// <returns></returns>
        public int AutoSelect(Projection projection, DataRow dataRow, bool ignoreExpression = true)
        {
            projection.VerifyNotNull(nameof(projection));
            dataRow.VerifyNotNull(nameof(dataRow));

            var result = 0;
            var keyColumns = projection.Columns;
            var valueColumns = GetValueColumns(projection, dataRow.Model);
            if (valueColumns == null)
                return result;
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

        private static ColumnCollection GetValueColumns(Projection projection, Model model)
        {
            return ResolveProjection(projection, model)?.Columns;
        }

        private static Model ResolveProjection(Model projection, Model model)
        {
            if (string.IsNullOrEmpty(projection.Namespace))
                return model;

            var resolvedParent = ResolveProjection(projection.ParentModel, model);
            return resolvedParent == null ? null : resolvedParent[projection.Name] as Projection;
        }

        /// <summary>
        /// Removes specified column from this dictionary.
        /// </summary>
        /// <param name="column">The <see cref="Column"/>.</param>
        public void Remove(Column column)
        {
            _columnValues.Remove(column);
        }
        
        /// <summary>
        /// Clears all items in this dictionary.
        /// </summary>
        public void Clear()
        {
            _columnValues.Clear();
        }

        /// <summary>
        /// Gets the data value for specified column.
        /// </summary>
        /// <typeparam name="T">Data type of column.</typeparam>
        /// <param name="column">The specified column.</param>
        /// <returns>The data value.</returns>
        public T GetValue<T>(Column<T> column)
        {
            column.VerifyNotNull(nameof(column));
            return (T)_columnValues[column];
        }

        /// <summary>
        /// Clones this object.
        /// </summary>
        /// <returns>The new object.</returns>
        public ColumnValueBag Clone()
        {
            var result = new ColumnValueBag();
            foreach (var keyValuePair in _columnValues)
                result._columnValues.Add(keyValuePair.Key, keyValuePair.Value);
            return result;
        }

        /// <summary>
        /// Resets all data values to default value of associated column.
        /// </summary>
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
