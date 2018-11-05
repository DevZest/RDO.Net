using System;
using DevZest.Data.Annotations.Primitives;
using DevZest.Data.SqlServer.Addons;

namespace DevZest.Data.SqlServer
{
    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
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

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlBinaryMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlBinaryMax();
        }
    }

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
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

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlCharMax();
        }
    }

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlDateAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlDate();
        }
    }

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlDateTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlDateTime();
        }
    }

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
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

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Decimal) })]
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

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Decimal) })]
    public sealed class SqlMoneyAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsSqlMoney();
        }
    }

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
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

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlNCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlNCharMax();
        }
    }

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
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

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlNVarCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlNVarCharMax();
        }
    }

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlSmallDateTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlSmallDateTime();
        }
    }

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Decimal) })]
    public sealed class SqlSmallMoneyAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsSqlSmallMoney();
        }
    }

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class SqlTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsSqlTime();
        }
    }

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlTimeStampAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlTimestamp();
        }
    }

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
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

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class SqlVarBinaryMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsSqlVarBinaryMax();
        }
    }

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
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

    [ModelMemberAttributeSpec(addonTypes: new Type[] { typeof(SqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class SqlVarCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsSqlVarCharMax();
        }
    }
}
