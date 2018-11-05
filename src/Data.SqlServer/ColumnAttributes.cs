using System;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.SqlServer
{
    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_Binary))]
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

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_Binary))]
    public sealed class SqlBinaryMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlBinaryMax();
        }
    }

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_String))]
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

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_String))]
    public sealed class SqlCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlCharMax();
        }
    }

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_DateTime))]
    public sealed class SqlDateAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlDate();
        }
    }

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_DateTime))]
    public sealed class SqlDateTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlDateTime();
        }
    }

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_DateTime))]
    public sealed class SqlDateTime2Attribute : ColumnAttribute
    {
        public SqlDateTime2Attribute(byte precision)
        {
            Precision = precision;
        }

        public byte Precision { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlDateTime2(Precision);
        }
    }

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_Decimal))]
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

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_Decimal))]
    public sealed class SqlMoneyAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsSqlMoney();
        }
    }

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_String))]
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

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_String))]
    public sealed class SqlNCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlNCharMax();
        }
    }

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_String))]
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

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_String))]
    public sealed class SqlNVarCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlNVarCharMax();
        }
    }

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_DateTime))]
    public sealed class SqlSmallDateTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlSmallDateTime();
        }
    }

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_Decimal))]
    public sealed class SqlSmallMoneyAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsSqlSmallMoney();
        }
    }

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_DateTime))]
    public sealed class SqlTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlTime();
        }
    }

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_Binary))]
    public sealed class SqlTimeStampAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlTimestamp();
        }
    }

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_Binary))]
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

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_Binary))]
    public sealed class SqlVarBinaryMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlVarBinaryMax();
        }
    }

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_String))]
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

    [ModelMemberAttributeSpec(typeof(SqlType), true, typeof(_String))]
    public sealed class SqlVarCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlVarCharMax();
        }
    }
}
