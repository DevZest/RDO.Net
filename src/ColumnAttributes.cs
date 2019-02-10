using System;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.MySql
{
    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class MySqlBinaryAttribute : ColumnAttribute
    {
        public MySqlBinaryAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsMySqlBinary(Size);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class MySqlCharAttribute : ColumnAttribute
    {
        public MySqlCharAttribute(int size, string charSetName = null, string collationName = null)
        {
            Size = size;
            CharSetName = charSetName;
            CollationName = collationName;
        }

        public int Size { get; }

        public string CharSetName { get; }

        public string CollationName { get; }

        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlChar(Size, CharSetName, CollationName);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class MySqlDateAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsMySqlDate();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class MySqlDateTimeAttribute : ColumnAttribute
    {
        public MySqlDateTimeAttribute(int precision = 0)
        {
            ColumnExtensions.VerifyTimePrecision(precision, nameof(precision));
            Precision = precision;
        }

        public int Precision { get; }

        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsMySqlDateTime(Precision);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Decimal) })]
    public sealed class MySqlDecimalAttribute : ColumnAttribute
    {
        public MySqlDecimalAttribute(byte precision, byte scale)
        {
            Precision = precision;
            Scale = scale;
        }

        public byte Precision { get; private set; }

        public byte Scale { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsMySqlDecimal(Precision, Scale);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Decimal) })]
    public sealed class MySqlMoneyAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Decimal decimalColumn)
                decimalColumn.AsMySqlMoney();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class MySqlTimeAttribute : ColumnAttribute
    {
        public MySqlTimeAttribute(int precision = 0)
        {
            ColumnExtensions.VerifyTimePrecision(precision, nameof(precision));
            Precision = precision;
        }

        public int Precision { get; }

        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsMySqlTime(Precision);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_DateTime) })]
    public sealed class MySqlTimeStampAttribute : ColumnAttribute
    {
        public MySqlTimeStampAttribute(int precision = 0)
        {
            ColumnExtensions.VerifyTimePrecision(precision, nameof(precision));
            Precision = precision;
        }

        public int Precision { get; }

        protected override void Wireup(Column column)
        {
            if (column is _DateTime dateTime)
                dateTime.AsMySqlTimestamp(Precision);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class MySqlVarBinaryAttribute : ColumnAttribute
    {
        public MySqlVarBinaryAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            if (column is _Binary binary)
                binary.AsMySqlVarBinary(Size);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class MySqlVarCharAttribute : ColumnAttribute
    {
        public MySqlVarCharAttribute(int size, string charSetName = null, string collationName = null)
        {
            Size = size;
            CharSetName = charSetName;
            CollcationName = collationName;
        }

        public int Size { get; }

        public string CharSetName { get; }

        public string CollcationName { get; }

        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlVarChar(Size, CharSetName, CollcationName);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class MySqlTinyTextAttribute : ColumnAttribute
    {
        public MySqlTinyTextAttribute(string charSetName = null, string collationName = null)
        {
            CharSetName = charSetName;
            CollcationName = collationName;
        }

        public string CharSetName { get; }

        public string CollcationName { get; }

        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlTinyText(CharSetName, CollcationName);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class MySqlTextAttribute : ColumnAttribute
    {
        public MySqlTextAttribute(string charSetName = null, string collationName = null)
        {
            CharSetName = charSetName;
            CollcationName = collationName;
        }

        public string CharSetName { get; }

        public string CollcationName { get; }

        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlText(CharSetName, CollcationName);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class MySqlMediumTextAttribute : ColumnAttribute
    {
        public MySqlMediumTextAttribute(string charSetName = null, string collationName = null)
        {
            CharSetName = charSetName;
            CollcationName = collationName;
        }

        public string CharSetName { get; }

        public string CollcationName { get; }

        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlMediumText(CharSetName, CollcationName);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_String) })]
    public sealed class MySqlLongTextAttribute : ColumnAttribute
    {
        public MySqlLongTextAttribute(string charSetName = null, string collationName = null)
        {
            CharSetName = charSetName;
            CollcationName = collationName;
        }

        public string CharSetName { get; }

        public string CollcationName { get; }

        protected override void Wireup(Column column)
        {
            if (column is _String stringColumn)
                stringColumn.AsMySqlLongText(CharSetName, CollcationName);
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class MySqlTinyBlobAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Binary binaryColumn)
                binaryColumn.AsMySqlTinyBlob();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class MySqlBlobAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Binary binaryColumn)
                binaryColumn.AsMySqlBlob();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class MySqlMediumBlobAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Binary binaryColumn)
                binaryColumn.AsMySqlMediumBlob();
        }
    }

    [ModelDesignerSpec(addonTypes: new Type[] { typeof(MySqlType) }, validOnTypes: new Type[] { typeof(_Binary) })]
    public sealed class MySqlLongBlobAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            if (column is _Binary binaryColumn)
                binaryColumn.AsMySqlLongBlob();
        }
    }
}
