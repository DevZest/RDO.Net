using System;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.SqlServer
{
    public sealed class SqlBinaryAttribute : ColumnAttribute
    {
        public SqlBinaryAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            ((Column<Binary>)column).AsBinary(Size);
        }
    }

    public sealed class SqlBinaryMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<Binary>)column).AsBinaryMax();
        }
    }

    public sealed class SqlCharAttribute : ColumnAttribute
    {
        public SqlCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsChar(Size);
        }
    }

    public sealed class SqlCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsCharMax();
        }
    }

    public sealed class SqlDateAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<DateTime?>)column).AsDate();
        }
    }

    public sealed class SqlDateTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<DateTime?>)column).AsDateTime();
        }
    }

    public sealed class SqlDateTime2Attribute : ColumnAttribute
    {
        public SqlDateTime2Attribute(byte precision)
        {
            Precision = precision;
        }

        public byte Precision { get; private set; }

        protected override void Wireup(Column column)
        {
            ((Column<DateTime?>)column).AsDateTime2(Precision);
        }
    }

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
            ((Column<Decimal?>)column).AsDecimal(Precision, Scale);
        }
    }

    public sealed class SqlMoneyAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<Decimal?>)column).AsMoney();
        }
    }

    public sealed class SqlNCharAttribute : ColumnAttribute
    {
        public SqlNCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsNChar(Size);
        }
    }

    public sealed class SqlNCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsNCharMax();
        }
    }

    public sealed class SqlNVarCharAttribute : ColumnAttribute
    {
        public SqlNVarCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsNVarChar(Size);
        }
    }

    public sealed class SqlNVarCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsNVarCharMax();
        }
    }

    public sealed class SqlSmallDateTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<DateTime?>)column).AsSmallDateTime();
        }
    }

    public sealed class SqlSmallMoneyAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<Decimal?>)column).AsSmallMoney();
        }
    }

    public sealed class SqlTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<DateTime?>)column).AsTime();
        }
    }

    public sealed class SqlTimeStampAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<Binary>)column).AsTimestamp();
        }
    }

    public sealed class SqlVarBinaryAttribute : ColumnAttribute
    {
        public SqlVarBinaryAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            ((Column<Binary>)column).AsVarBinary(Size);
        }
    }

    public sealed class SqlVarBinaryMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<Binary>)column).AsVarBinaryMax();
        }
    }

    public sealed class SqlVarCharAttribute : ColumnAttribute
    {
        public SqlVarCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsVarChar(Size);
        }
    }

    public sealed class SqlVarCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsVarCharMax();
        }
    }
}
