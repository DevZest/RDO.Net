using System;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.SqlServer
{
    /// <summary>
    /// Specifies SQL Server BINARY(n) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlBinaryAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SqlBinaryAttribute"/>.
        /// </summary>
        /// <param name="size">The size of the data type.</param>
        public SqlBinaryAttribute(int size)
        {
            Size = size;
        }

        /// <summary>
        /// Gets the size of the data type.
        /// </summary>
        public int Size { get; private set; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlBinary(Size);
        }
    }

    /// <summary>
    /// Specifies SQL Server BINARY(MAX) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlBinaryMaxAttribute : ColumnAttribute
    {
        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlBinaryMax();
        }
    }

    /// <summary>
    /// Specifies SQL Server CHAR(n) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlCharAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SqlCharAttribute"/>.
        /// </summary>
        /// <param name="size">The size of the data type.</param>
        public SqlCharAttribute(int size)
        {
            Size = size;
        }

        /// <summary>
        /// Gets the size of the data type.
        /// </summary>
        public int Size { get; private set; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlChar(Size);
        }
    }

    /// <summary>
    /// Specifies SQL Server CHAR(MAX) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlCharMaxAttribute : ColumnAttribute
    {
        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlCharMax();
        }
    }

    /// <summary>
    /// Specifies SQL Server DATE data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlDateAttribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        /// <summary>
        /// Gets the logical data type.
        /// </summary>
        public LogicalDataType LogicalDataType => LogicalDataType.Date;

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlDate();
        }
    }

    /// <summary>
    /// Specifies SQL Server DATETIME data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlDateTimeAttribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        /// <summary>
        /// Gets the logical data type.
        /// </summary>
        public LogicalDataType LogicalDataType => LogicalDataType.DateTime;

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlDateTime();
        }
    }

    /// <summary>
    /// Specifies SQL Server DATETIME2 data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlDateTime2Attribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SqlDateTime2Attribute"/>.
        /// </summary>
        /// <param name="precision">The precision of the data type.</param>
        public SqlDateTime2Attribute(byte precision)
        {
            Precision = precision;
        }

        /// <summary>
        /// Gets the logical data type.
        /// </summary>
        public LogicalDataType LogicalDataType => LogicalDataType.DateTime;

        /// <summary>
        /// Gets the precision of the data type.
        /// </summary>
        public byte Precision { get; private set; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlDateTime2(Precision);
        }
    }

    /// <summary>
    /// Specifies SQL Server DECIMAL data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Decimal) })]
    public sealed class SqlDecimalAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SqlDecimalAttribute"/>.
        /// </summary>
        /// <param name="precision">The precision of the data type.</param>
        /// <param name="scale">The scale of the data type.</param>
        public SqlDecimalAttribute(byte precision, byte scale)
        {
            Precision = precision;
            Scale = scale;
        }

        /// <summary>
        /// Gets the precision of the data type.
        /// </summary>
        public byte Precision { get; private set; }

        /// <summary>
        /// Gets the scale of the data type.
        /// </summary>
        public byte Scale { get; private set; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsSqlDecimal(Precision, Scale);
        }
    }

    /// <summary>
    /// Specifies SQL Server MONEY data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_Decimal) })]
    public sealed class SqlMoneyAttribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        /// <summary>
        /// Gets the logical data type.
        /// </summary>
        public LogicalDataType LogicalDataType => LogicalDataType.Currency;

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsSqlMoney();
        }
    }

    /// <summary>
    /// Specifies SQL Server NCHAR(n) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlNCharAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SqlNCharAttribute"/>.
        /// </summary>
        /// <param name="size">The size of the data type.</param>
        public SqlNCharAttribute(int size)
        {
            Size = size;
        }

        /// <summary>
        /// Gets the size of the data type.
        /// </summary>
        public int Size { get; private set; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlNChar(Size);
        }
    }

    /// <summary>
    /// Specifies SQL Server NCHAR(MAX) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlNCharMaxAttribute : ColumnAttribute
    {
        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlNCharMax();
        }
    }

    /// <summary>
    /// Specifies SQL Server NVARCHAR(n) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlNVarCharAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SqlNVarCharAttribute"/>.
        /// </summary>
        /// <param name="size">The size of the data type.</param>
        public SqlNVarCharAttribute(int size)
        {
            Size = size;
        }

        /// <summary>
        /// Gets the size of the data type.
        /// </summary>
        public int Size { get; private set; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlNVarChar(Size);
        }
    }

    /// <summary>
    /// Specifies SQL Server NVARCHAR(MAX) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlNVarCharMaxAttribute : ColumnAttribute
    {
        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlNVarCharMax();
        }
    }

    /// <summary>
    /// Specifies SQL Server SMALLDATETIME data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlSmallDateTimeAttribute : ColumnAttribute
    {
        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlSmallDateTime();
        }
    }

    /// <summary>
    /// Specifies SQL Server SMALLMONEY data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_Decimal) })]
    public sealed class SqlSmallMoneyAttribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        /// <summary>
        /// Gets the logical data type.
        /// </summary>
        public LogicalDataType LogicalDataType => LogicalDataType.Currency;

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsSqlSmallMoney();
        }
    }

    /// <summary>
    /// Specifies SQL Server TIME data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlTimeAttribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        /// <summary>
        /// Gets the logical data type.
        /// </summary>
        public LogicalDataType LogicalDataType => LogicalDataType.Time;

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlTime();
        }
    }

    /// <summary>
    /// Specifies SQL Server TIMESTAMP data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlTimeStampAttribute : ColumnAttribute
    {
        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlTimestamp();
        }
    }

    /// <summary>
    /// Specifies SQL Server VARBINARY(n) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlVarBinaryAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SqlVarBinaryAttribute"/>.
        /// </summary>
        /// <param name="size">The size of the data type.</param>
        public SqlVarBinaryAttribute(int size)
        {
            Size = size;
        }

        /// <summary>
        /// Gets the size of the data type.
        /// </summary>
        public int Size { get; private set; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlVarBinary(Size);
        }
    }

    /// <summary>
    /// Specifies SQL Server VARBINARY(MAX) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlVarBinaryMaxAttribute : ColumnAttribute
    {
        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlVarBinaryMax();
        }
    }

    /// <summary>
    /// Specifies SQL Server VARCHAR(n) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlVarCharAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SqlVarCharAttribute"/>.
        /// </summary>
        /// <param name="size">The size of the data type.</param>
        public SqlVarCharAttribute(int size)
        {
            Size = size;
        }

        /// <summary>
        /// Gets the size of the data type.
        /// </summary>
        public int Size { get; private set; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlVarChar(Size);
        }
    }

    /// <summary>
    /// Specifies SQL Server VARCHAR(MAX) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlVarCharMaxAttribute : ColumnAttribute
    {
        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlVarCharMax();
        }
    }
}
