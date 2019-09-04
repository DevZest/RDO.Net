using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Retrieves a read-only, forward-only stream of data from a DataSet.
    /// </summary>
    /// <remarks>You can use this class as data source for bulk copy.</remarks>
    public sealed class DataSetReader : DbDataReader
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DataSetReader"/>.
        /// </summary>
        /// <param name="dataSet">The contained DataSet.</param>
        public DataSetReader(DataSet dataSet)
        {
            _dataSet = dataSet;
        }

        private readonly DataSet _dataSet;
        private bool _isOpen = true;
        private int _currentIndex = -1;

        private Model Model
        {
            get { return _dataSet.Model; }
        }

        private ColumnCollection Columns
        {
            get { return Model.Columns; }
        }

        private DataRow CurrentRow
        {
            get { return _dataSet[_currentIndex]; }
        }

        private object GetValue(Column column)
        {
            return column.GetValue(CurrentRow);
        }

        private void ValidateOpen()
        {
            if (!_isOpen)
                throw new InvalidOperationException(DiagnosticMessages.DataSetReader_Closed);
        }

        /// <inheritdoc />
        public override int Depth
        {
            get
            {
                ValidateOpen();
                return 0;
            }
        }

        /// <inheritdoc />
        public override bool IsClosed
        {
            get { return !_isOpen; }
        }

        /// <inheritdoc />
        public override void Close()
        {
            _isOpen = false;
        }

        /// <inheritdoc />
        public override int RecordsAffected
        {
            get { return 0; }
        }

        /// <inheritdoc />
        public override bool HasRows
        {
            get
            {
                ValidateOpen();
                return _dataSet.Count > 0;
            }
        }

        /// <inheritdoc />
        public override int FieldCount
        {
            get
            {
                ValidateOpen();
                return Columns.Count;
            }
        }

        /// <inheritdoc />
        public override bool Read()
        {
            ValidateOpen();
            if (_currentIndex >= _dataSet.Count - 1)
                return false;
            _currentIndex++;
            return true;
        }

        /// <inheritdoc />
        public override object this[int ordinal]
        {
            get
            {
                ValidateOpen();
                return GetValue(ordinal);
            }
        }

        /// <inheritdoc />
        public override object this[string name]
        {
            get
            {
                ValidateOpen();
                return GetValue(Columns[name]);
            }
        }

        /// <inheritdoc />
        public override DataTable GetSchemaTable()
        {
            ValidateOpen();
            return GetSchemaTableFromDataSet(_dataSet);
        }

        /// <inheritdoc />
        public override bool NextResult()
        {
            return false;
        }

        /// <inheritdoc />
        public override Type GetFieldType(int ordinal)
        {
            ValidateOpen();
            return GetFieldType(Columns[ordinal]);
        }

        private static Type GetFieldType(Column column)
        {
            var result = column.DataType;
            return (result.IsGenericType && result.GetGenericTypeDefinition() == typeof(Nullable<>)) ? result.GenericTypeArguments[0] : result;
        }

        /// <inheritdoc />
        public override Type GetProviderSpecificFieldType(int ordinal)
        {
            ValidateOpen();
            return GetFieldType(ordinal);
        }

        /// <inheritdoc />
        public override object GetValue(int ordinal)
        {
            ValidateOpen();
            return GetValue(Columns[ordinal]);
        }

        /// <inheritdoc />
        public override int GetValues(object[] values)
        {
            ValidateOpen();
            values.VerifyNotNull(nameof(values));
            var result = Math.Min(Columns.Count, values.Length);

            for (int i = 0; i < result; i++)
                values[i] = GetValue(i);
            return result;
        }

        /// <inheritdoc />
        public override int GetProviderSpecificValues(object[] values)
        {
            ValidateOpen();
            return GetValues(values);
        }

        private T GetValue<T>(int ordinal)
            where T : struct
        {
            ValidateOpen();
            var column = Columns[ordinal];
            if (column is Column<T?> nullableColumn)
                return nullableColumn[CurrentRow].Value;
            else if (column is Column<T> nonNullableColumn)
                return nonNullableColumn[CurrentRow];
            else
                return (T)GetValue(column);
        }

        /// <inheritdoc />
        public override bool GetBoolean(int ordinal)
        {
            ValidateOpen();
            return GetValue<bool>(ordinal);
        }

        /// <inheritdoc />
        public override byte GetByte(int ordinal)
        {
            ValidateOpen();
            return GetValue<byte>(ordinal);
        }

        /// <inheritdoc />
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferIndex, int length)
        {
            ValidateOpen();
            int dataIndex = (int)dataOffset;
            if (dataIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(dataOffset));
            if (length <= 0)
                return 0;

            var array = (byte[])GetValue(ordinal);
            if (buffer == null)
                return array.Length;

            int result = Math.Min(array.Length - dataIndex, length);
            if (result <= 0)
                return 0;

            Array.Copy(array, dataIndex, buffer, bufferIndex, result);
            return result;
        }

        /// <inheritdoc />
        public override char GetChar(int ordinal)
        {
            ValidateOpen();
            return GetValue<char>(ordinal);
        }

        /// <inheritdoc />
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            ValidateOpen();
            int dataIndex = (int)dataOffset;
            if (dataIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(dataOffset));
            if (length <= 0)
                return 0;

            var array = (char[])GetValue(ordinal);
            if (buffer == null)
                return array.Length;

            int result = Math.Min(array.Length - dataIndex, length);
            if (result <= 0)
                return 0;

            Array.Copy(array, dataIndex, buffer, bufferOffset, result);
            return result;
        }

        /// <inheritdoc />
        public override string GetDataTypeName(int ordinal)
        {
            ValidateOpen();
            return GetFieldType(ordinal).Name;
        }

        /// <inheritdoc />
        public override DateTime GetDateTime(int ordinal)
        {
            ValidateOpen();
            return GetValue<DateTime>(ordinal);
        }

        /// <inheritdoc />
        public override decimal GetDecimal(int ordinal)
        {
            ValidateOpen();
            return GetValue<decimal>(ordinal);
        }

        /// <inheritdoc />
        public override double GetDouble(int ordinal)
        {
            ValidateOpen();
            return GetValue<double>(ordinal);
        }

        /// <inheritdoc />
        public override float GetFloat(int ordinal)
        {
            ValidateOpen();
            return GetValue<float>(ordinal);
        }

        /// <inheritdoc />
        public override Guid GetGuid(int ordinal)
        {
            ValidateOpen();
            return GetValue<Guid>(ordinal);
        }

        /// <inheritdoc />
        public override short GetInt16(int ordinal)
        {
            ValidateOpen();
            return GetValue<short>(ordinal);
        }

        /// <inheritdoc />
        public override int GetInt32(int ordinal)
        {
            ValidateOpen();
            return GetValue<int>(ordinal);
        }

        /// <inheritdoc />
        public override long GetInt64(int ordinal)
        {
            ValidateOpen();
            return GetValue<long>(ordinal);
        }

        /// <inheritdoc />
        public override string GetName(int ordinal)
        {
            ValidateOpen();
            return Columns[ordinal].Name;
        }

        /// <inheritdoc />
        public override int GetOrdinal(string name)
        {
            ValidateOpen();
            var column = Columns[name];
            if (column == null)
                throw new ArgumentOutOfRangeException(nameof(name));
            return column.Ordinal;
        }

        /// <inheritdoc />
        public override string GetString(int ordinal)
        {
            ValidateOpen();
            return (string)GetValue(ordinal);
        }

        /// <inheritdoc />
        public override bool IsDBNull(int ordinal)
        {
            ValidateOpen();
            return Columns[ordinal].IsNull(CurrentRow);
        }

        /// <inheritdoc />
        public override IEnumerator GetEnumerator()
        {
            ValidateOpen();
            return new DbEnumerator(this);
        }

        private static DataTable GetSchemaTableFromDataSet(DataSet dataSet)
        {
            Debug.Assert(dataSet != null);

            DataTable dataTable = new DataTable("SchemaTable");
            dataTable.Locale = CultureInfo.InvariantCulture;
            DataColumn columnName = new DataColumn(SchemaTableColumn.ColumnName, typeof(string));
            DataColumn columnOrdinal = new DataColumn(SchemaTableColumn.ColumnOrdinal, typeof(int));
            DataColumn columnSize = new DataColumn(SchemaTableColumn.ColumnSize, typeof(int));
            DataColumn numericPrecision = new DataColumn(SchemaTableColumn.NumericPrecision, typeof(short));
            DataColumn numericScale = new DataColumn(SchemaTableColumn.NumericScale, typeof(short));
            DataColumn dataType = new DataColumn(SchemaTableColumn.DataType, typeof(Type));
            DataColumn providerType = new DataColumn(SchemaTableColumn.ProviderType, typeof(int));
            DataColumn isLong = new DataColumn(SchemaTableColumn.IsLong, typeof(bool));
            DataColumn allowDbNull = new DataColumn(SchemaTableColumn.AllowDBNull, typeof(bool));
            DataColumn isReadOnly = new DataColumn(SchemaTableOptionalColumn.IsReadOnly, typeof(bool));
            DataColumn isUnique = new DataColumn(SchemaTableColumn.IsUnique, typeof(bool));
            DataColumn isKey = new DataColumn(SchemaTableColumn.IsKey, typeof(bool));
            DataColumn isAutoIncrement = new DataColumn(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool));
            DataColumn baseSchemaName = new DataColumn(SchemaTableColumn.BaseSchemaName, typeof(string));
            DataColumn baseTableName = new DataColumn(SchemaTableColumn.BaseTableName, typeof(string));
            DataColumn baseColumnName = new DataColumn(SchemaTableColumn.BaseColumnName, typeof(string));
            DataColumn autoIncrementSeed = new DataColumn(SchemaTableOptionalColumn.AutoIncrementSeed, typeof(long));
            DataColumn autoIncrementStep = new DataColumn(SchemaTableOptionalColumn.AutoIncrementStep, typeof(long));
            DataColumn defaultValue = new DataColumn(SchemaTableOptionalColumn.DefaultValue, typeof(object));
            columnSize.DefaultValue = -1;
            isLong.DefaultValue = false;
            isReadOnly.DefaultValue = false;
            isKey.DefaultValue = false;
            isAutoIncrement.DefaultValue = false;
            autoIncrementSeed.DefaultValue = 0;
            autoIncrementStep.DefaultValue = 1;
            dataTable.Columns.Add(columnName);
            dataTable.Columns.Add(columnOrdinal);
            dataTable.Columns.Add(columnSize);
            dataTable.Columns.Add(numericPrecision);
            dataTable.Columns.Add(numericScale);
            dataTable.Columns.Add(dataType);
            dataTable.Columns.Add(providerType);
            dataTable.Columns.Add(isLong);
            dataTable.Columns.Add(allowDbNull);
            dataTable.Columns.Add(isReadOnly);
            dataTable.Columns.Add(isUnique);
            dataTable.Columns.Add(isKey);
            dataTable.Columns.Add(isAutoIncrement);
            dataTable.Columns.Add(baseSchemaName);
            dataTable.Columns.Add(baseTableName);
            dataTable.Columns.Add(baseColumnName);
            dataTable.Columns.Add(autoIncrementSeed);
            dataTable.Columns.Add(autoIncrementStep);
            dataTable.Columns.Add(defaultValue);

            var model = dataSet.Model;
            foreach (var column in model.Columns)
            {
                var dataRow = dataTable.NewRow();
                dataRow[columnName] = column.Name;
                dataRow[columnOrdinal] = column.Ordinal;
                dataRow[dataType] = GetFieldType(column);
                dataRow[allowDbNull] = column.IsNullable;
                dataRow[isReadOnly] = column.IsExpression || column.IsIdentity;
                dataRow[isUnique] = column.IsUnique;
                if (column.IsIdentity)
                {
                    dataRow[isAutoIncrement] = true;
                    var identity = column.GetIdentity(false);
                    Debug.Assert(identity != null);
                    dataRow[autoIncrementSeed] = identity.Seed;
                    dataRow[autoIncrementStep] = identity.Increment;
                }
                var columnDefaultValue = column.GetDefaultValue();
                if (columnDefaultValue != null)
                    dataRow[defaultValue] = columnDefaultValue;
                dataRow[baseColumnName] = column.Name;
                dataTable.Rows.Add(dataRow);
            }

            IReadOnlyList<Column> primaryKey = model.PrimaryKey;
            if (primaryKey != null)
            {
                foreach (var column in primaryKey)
                    dataTable.Rows[column.Ordinal][isKey] = true;
            }
            dataTable.AcceptChanges();
            return dataTable;
        }
    }
}
