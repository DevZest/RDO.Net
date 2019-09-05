using System;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.MySql
{
    /// <summary>
    /// Specifies MySQL BINARY(n) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class MySqlBinaryAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MySqlBinaryAttribute"/>.
        /// </summary>
        /// <param name="size">The size of the data type.</param>
        public MySqlBinaryAttribute(int size)
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
                binary.AsMySqlBinary(Size);
        }
    }

    /// <summary>
    /// Specifies MySQL CHAR(n) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class MySqlCharAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MySqlCharAttribute"/>.
        /// </summary>
        /// <param name="size">The size of the data type.</param>
        /// <param name="charSetName">The char set name of the data type.</param>
        /// <param name="collationName">The collation name of the data type.</param>
        public MySqlCharAttribute(int size, string charSetName = null, string collationName = null)
        {
            Size = size;
            CharSetName = charSetName;
            CollationName = collationName;
        }

        /// <summary>
        /// Gets the size of the data type.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets the char set name of the data type.
        /// </summary>
        public string CharSetName { get; }

        /// <summary>
        /// Gets the collation name of the data type.
        /// </summary>
        public string CollationName { get; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlChar(Size, CharSetName, CollationName);
        }
    }

    /// <summary>
    /// Specifies MySQL DATE data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class MySqlDateAttribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        /// <summary>
        /// Gets the logical data type.
        /// </summary>
        public LogicalDataType LogicalDataType => LogicalDataType.Date;

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsMySqlDate();
        }
    }

    /// <summary>
    /// Specifies MySQL DATETIME data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class MySqlDateTimeAttribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MySqlDateTimeAttribute"/>.
        /// </summary>
        /// <param name="precision">The precision of the data type.</param>
        public MySqlDateTimeAttribute(int precision = 0)
        {
            ColumnExtensions.VerifyTimePrecision(precision, nameof(precision));
            Precision = precision;
        }

        /// <summary>
        /// Gets the logical data type.
        /// </summary>
        public LogicalDataType LogicalDataType => LogicalDataType.DateTime;

        /// <summary>
        /// Gets the precision of the data type.
        /// </summary>
        public int Precision { get; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsMySqlDateTime(Precision);
        }
    }

    /// <summary>
    /// Specifies MySQL DECIMAL data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Decimal) })]
    public sealed class MySqlDecimalAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MySqlDecimalAttribute"/>.
        /// </summary>
        /// <param name="precision">The precision of the data type.</param>
        /// <param name="scale">The scale of the data type.</param>
        public MySqlDecimalAttribute(byte precision, byte scale)
        {
            Precision = precision;
            Scale = scale;
        }

        /// <summary>
        /// Gets the precision of the data type.
        /// </summary>
        public byte Precision { get; private set; }

        /// <summary>
        /// Gets the scale of the data tyep.
        /// </summary>
        public byte Scale { get; private set; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsMySqlDecimal(Precision, Scale);
        }
    }

    /// <summary>
    /// Specifies MySQL DECIMAL(19,4) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_Decimal) })]
    public sealed class MySqlMoneyAttribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        /// <summary>
        /// Gets the logical data type.
        /// </summary>
        public LogicalDataType LogicalDataType => LogicalDataType.Currency;

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsMySqlMoney();
        }
    }

    /// <summary>
    /// Specifies MySQL TIME data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class MySqlTimeAttribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        /// <summary>
        /// Gets the logical data type.
        /// </summary>
        public LogicalDataType LogicalDataType => LogicalDataType.Time;

        /// <summary>
        /// Initializes a new instance of <see cref="MySqlTimeAttribute"/>.
        /// </summary>
        /// <param name="precision">The precision of the data type.</param>
        public MySqlTimeAttribute(int precision = 0)
        {
            ColumnExtensions.VerifyTimePrecision(precision, nameof(precision));
            Precision = precision;
        }

        /// <summary>
        /// Gets the precision of the data type.
        /// </summary>
        public int Precision { get; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsMySqlTime(Precision);
        }
    }

    /// <summary>
    /// Specifies MySQL TIMESTAMP data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class MySqlTimeStampAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MySqlTimeStampAttribute"/>.
        /// </summary>
        /// <param name="precision">The precision of the data type.</param>
        public MySqlTimeStampAttribute(int precision = 0)
        {
            ColumnExtensions.VerifyTimePrecision(precision, nameof(precision));
            Precision = precision;
        }

        /// <summary>
        /// Gets the precision of the data type.
        /// </summary>
        public int Precision { get; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsMySqlTimestamp(Precision);
        }
    }

    /// <summary>
    /// Specifies MySQL VARBINARY(n) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class MySqlVarBinaryAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initiallizes a new instance of <see cref="MySqlVarBinaryAttribute"/>.
        /// </summary>
        /// <param name="size"></param>
        public MySqlVarBinaryAttribute(int size)
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
                binary.AsMySqlVarBinary(Size);
        }
    }

    /// <summary>
    /// Specifies MySQL VARCHAR(n) data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class MySqlVarCharAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MySqlVarCharAttribute"/>.
        /// </summary>
        /// <param name="size">The size of the data type.</param>
        /// <param name="charSetName">The char set name of the data type.</param>
        /// <param name="collationName">The collation name of the data type.</param>
        public MySqlVarCharAttribute(int size, string charSetName = null, string collationName = null)
        {
            Size = size;
            CharSetName = charSetName;
            CollcationName = collationName;
        }

        /// <summary>
        /// Gets the size of the data type.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets the char set name of the data type.
        /// </summary>
        public string CharSetName { get; }

        /// <summary>
        /// Gets the collation name of the data type.
        /// </summary>
        public string CollcationName { get; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlVarChar(Size, CharSetName, CollcationName);
        }
    }

    /// <summary>
    /// Specifies MySQL TINYTEXT data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class MySqlTinyTextAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MySqlTinyTextAttribute"/>.
        /// </summary>
        /// <param name="charSetName">The char set name of the data type.</param>
        /// <param name="collationName">The collation name of the data type.</param>
        public MySqlTinyTextAttribute(string charSetName = null, string collationName = null)
        {
            CharSetName = charSetName;
            CollcationName = collationName;
        }

        /// <summary>
        /// Gets the char set name of the data type.
        /// </summary>
        public string CharSetName { get; }

        /// <summary>
        /// Gets the collation name of the data type.
        /// </summary>
        public string CollcationName { get; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlTinyText(CharSetName, CollcationName);
        }
    }

    /// <summary>
    /// Specifies MySQL TEXT data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class MySqlTextAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MySqlTextAttribute"/>.
        /// </summary>
        /// <param name="charSetName">The char set name of the data type.</param>
        /// <param name="collationName">The collation name of the data type.</param>
        public MySqlTextAttribute(string charSetName = null, string collationName = null)
        {
            CharSetName = charSetName;
            CollcationName = collationName;
        }

        /// <summary>
        /// Gets the char set name of the data type.
        /// </summary>
        public string CharSetName { get; }

        /// <summary>
        /// Gets the collation name of the data type.
        /// </summary>
        public string CollcationName { get; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlText(CharSetName, CollcationName);
        }
    }

    /// <summary>
    /// Specifies MySQL MEDIUMTEXT data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class MySqlMediumTextAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MySqlMediumTextAttribute"/>.
        /// </summary>
        /// <param name="charSetName">The char set name of the data type.</param>
        /// <param name="collationName">The collation name of the data type.</param>
        public MySqlMediumTextAttribute(string charSetName = null, string collationName = null)
        {
            CharSetName = charSetName;
            CollcationName = collationName;
        }
        
        /// <summary>
        /// Gets the char set name of the data type.
        /// </summary>
        public string CharSetName { get; }

        /// <summary>
        /// Gets the collation name of the data type.
        /// </summary>
        public string CollcationName { get; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlMediumText(CharSetName, CollcationName);
        }
    }

    /// <summary>
    /// Specifies MySQL LONGTEXT data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class MySqlLongTextAttribute : ColumnAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MySqlLongTextAttribute"/>.
        /// </summary>
        /// <param name="charSetName">The char set name of the data type.</param>
        /// <param name="collationName">The collation name of the data type.</param>
        public MySqlLongTextAttribute(string charSetName = null, string collationName = null)
        {
            CharSetName = charSetName;
            CollcationName = collationName;
        }

        /// <summary>
        /// Gets the char set name of the data type.
        /// </summary>
        public string CharSetName { get; }

        /// <summary>
        /// Gets the collation name of the data type.
        /// </summary>
        public string CollcationName { get; }

        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlLongText(CharSetName, CollcationName);
        }
    }

    /// <summary>
    /// Specifies MySQL TINYBLOB data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class MySqlTinyBlobAttribute : ColumnAttribute
    {
        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _Binary binaryColumn)
                binaryColumn.AsMySqlTinyBlob();
        }
    }

    /// <summary>
    /// Specifies MySQL BLOB data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class MySqlBlobAttribute : ColumnAttribute
    {
        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _Binary binaryColumn)
                binaryColumn.AsMySqlBlob();
        }
    }

    /// <summary>
    /// Specifies MySQL MEDIUMBLOB data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class MySqlMediumBlobAttribute : ColumnAttribute
    {
        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _Binary binaryColumn)
                binaryColumn.AsMySqlMediumBlob();
        }
    }

    /// <summary>
    /// Specifies MySQL LONGBLOB data type for column.
    /// </summary>
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class MySqlLongBlobAttribute : ColumnAttribute
    {
        /// <inheritdoc/>
        protected override void Wireup(Column column)
        {
            if (column is _Binary binaryColumn)
                binaryColumn.AsMySqlLongBlob();
        }
    }
}
