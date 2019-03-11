using System;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.SqlServer
{
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlBinaryAttribute : ColumnAttribute
    {
        public SqlBinaryAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlBinary(Size);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlBinaryMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlBinaryMax();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlCharAttribute : ColumnAttribute
    {
        public SqlCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlChar(Size);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlCharMax();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlDateAttribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        public LogicalDataType LogicalDataType => LogicalDataType.Date;

        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlDate();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlDateTimeAttribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        public LogicalDataType LogicalDataType => LogicalDataType.DateTime;

        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlDateTime();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlDateTime2Attribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        public SqlDateTime2Attribute(byte precision)
        {
            Precision = precision;
        }

        public LogicalDataType LogicalDataType => LogicalDataType.DateTime;

        public byte Precision { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlDateTime2(Precision);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Decimal) })]
    public sealed class SqlDecimalAttribute : ColumnAttribute
    {
        public SqlDecimalAttribute(byte precision, byte scale)
        {
            Precision = precision;
            Scale = scale;
        }

        public byte Precision { get; private set; }

        public byte Scale { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsSqlDecimal(Precision, Scale);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_Decimal) })]
    public sealed class SqlMoneyAttribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        public LogicalDataType LogicalDataType => LogicalDataType.Currency;

        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsSqlMoney();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlNCharAttribute : ColumnAttribute
    {
        public SqlNCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlNChar(Size);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlNCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlNCharMax();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlNVarCharAttribute : ColumnAttribute
    {
        public SqlNVarCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlNVarChar(Size);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlNVarCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlNVarCharMax();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlSmallDateTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlSmallDateTime();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_Decimal) })]
    public sealed class SqlSmallMoneyAttribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        public LogicalDataType LogicalDataType => LogicalDataType.Currency;

        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsSqlSmallMoney();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType), typeof(LogicalDataType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlTimeAttribute : ColumnAttribute, ILogicalDataTypeAttribute
    {
        public LogicalDataType LogicalDataType => LogicalDataType.Time;

        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlTime();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlTimeStampAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlTimestamp();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlVarBinaryAttribute : ColumnAttribute
    {
        public SqlVarBinaryAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlVarBinary(Size);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlVarBinaryMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlVarBinaryMax();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlVarCharAttribute : ColumnAttribute
    {
        public SqlVarCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlVarChar(Size);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlVarCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlVarCharMax();
        }
    }
}
