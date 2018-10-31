using System;
using DevZest.Data.Annotations.Primitives;

namespace DevZest.Data.SqlServer
{
    public sealed class AsBinaryAttribute : ColumnAttribute
    {
        public AsBinaryAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            ((Column<Binary>)column).AsBinary(Size);
        }
    }

    public sealed class AsBinaryMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<Binary>)column).AsBinaryMax();
        }
    }

    public sealed class AsCharAttribute : ColumnAttribute
    {
        public AsCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsChar(Size);
        }
    }

    public sealed class AsCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsCharMax();
        }
    }

    public sealed class AsDateAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<DateTime?>)column).AsDate();
        }
    }

    public sealed class AsDateTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<DateTime?>)column).AsDateTime();
        }
    }

    public sealed class AsDateTime2Attribute : ColumnAttribute
    {
        public AsDateTime2Attribute(byte precision)
        {
            Precision = precision;
        }

        public byte Precision { get; private set; }

        protected override void Wireup(Column column)
        {
            ((Column<DateTime?>)column).AsDateTime2(Precision);
        }
    }

    public sealed class AsDecimalAttribute : ColumnAttribute
    {
        public AsDecimalAttribute(byte precision, byte scale)
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

    public sealed class AsMoneyAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<Decimal?>)column).AsMoney();
        }
    }

    public sealed class AsNCharAttribute : ColumnAttribute
    {
        public AsNCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsNChar(Size);
        }
    }

    public sealed class AsNCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsNCharMax();
        }
    }

    public sealed class AsNVarCharAttribute : ColumnAttribute
    {
        public AsNVarCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsNVarChar(Size);
        }
    }

    public sealed class AsNVarCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsNVarCharMax();
        }
    }

    public sealed class AsSmallDateTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<DateTime?>)column).AsSmallDateTime();
        }
    }

    public sealed class AsSmallMoneyAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<Decimal?>)column).AsSmallMoney();
        }
    }

    public sealed class AsTimeAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<DateTime?>)column).AsTime();
        }
    }

    public sealed class AsTimeStampAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<Binary>)column).AsTimestamp();
        }
    }

    public sealed class AsVarBinaryAttribute : ColumnAttribute
    {
        public AsVarBinaryAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            ((Column<Binary>)column).AsVarBinary(Size);
        }
    }

    public sealed class AsVarBinaryMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<Binary>)column).AsVarBinaryMax();
        }
    }

    public sealed class AsVarCharAttribute : ColumnAttribute
    {
        public AsVarCharAttribute(int size)
        {
            Size = size;
        }

        public int Size { get; private set; }

        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsVarChar(Size);
        }
    }

    public sealed class AsVarCharMaxAttribute : ColumnAttribute
    {
        protected override void Wireup(Column column)
        {
            ((Column<string>)column).AsVarCharMax();
        }
    }
}
