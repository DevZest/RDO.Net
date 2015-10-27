using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Data.Common;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a string column.
    /// </summary>
    public sealed class _String : Column<String>, IColumn<DbReader, String>
    {
        /// <inheritdoc/>
        protected override Column<string> CreateParam(string value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected internal override Column<string> CreateConst(string value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(String value)
        {
            return JsonValue.String(value);
        }

        /// <inheritdoc/>
        protected internal override String DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : value.Text;
        }

        /// <inheritdoc cref="P:DevZest.Data._Binary.Item(DevZest.Data.DbReader)"/>
        public String this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private String GetValue(DbReader reader)
        {
            return reader.GetString(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        protected internal override bool IsNull(string value)
        {
            return value == null;
        }

        /// <inheritdoc cref="_Binary.Param(Binary, _Binary)"/>
        public static _String Param(String x, _String sourceColumn = null)
        {
            return new ParamExpression<String>(x, sourceColumn).MakeColumn<_String>();
        }

        /// <inheritdoc cref="_Binary.Const(Binary)"/>
        public static _String Const(String x)
        {
            return new ConstantExpression<String>(x).MakeColumn<_String>();
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        public static implicit operator _String(String x)
        {
            return Param(x);
        }

        private sealed class AddExpression : BinaryExpression<String>
        {
            public AddExpression(_String x, _String y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Add; }
            }

            protected override String EvalCore(String x, String y)
            {
                if (x != null && y != null)
                    return x + y;
                else
                    return null;
            }
        }

        /// <summary>Computes the sum of the two specified <see cref="_String" /> objects.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _String operator +(_String x, _String y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new AddExpression(x, y).MakeColumn<_String>();
        }

        private sealed class LessThanExpression : BinaryExpression<String, bool?>
        {
            public LessThanExpression(_String x, _String y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) < 0;
            }
        }

        /// <summary>Compares the two <see cref="_String" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator <(_String x, _String y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<String, bool?>
        {
            public LessThanOrEqualExpression(_String x, _String y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) <= 0;
            }
        }

        /// <summary>Compares the two <see cref="_String" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator <=(_String x, _String y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<String, bool?>
        {
            public GreaterThanExpression(_String x, _String y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) > 0;
            }
        }

        /// <summary>Compares the two <see cref="_String" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator >(_String x, _String y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<String, bool?>
        {
            public GreaterThanOrEqualExpression(_String x, _String y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) >= 0;
            }
        }

        /// <summary>Compares the two <see cref="_String" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator >=(_String x, _String y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<String, bool?>
        {
            public EqualExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) == 0;
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_String" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator ==(_String x, _String y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<String, bool?>
        {
            public NotEqualExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) != 0;
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_String" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator !=(_String x, _String y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class DbBooleanCast : CastExpression<Boolean?, String>
        {
            public DbBooleanCast(_Boolean x)
                : base(x)
            {
            }

            protected override String Cast(Boolean? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString() : null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Boolean" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Boolean" /> object. </param>
        public static explicit operator _String(_Boolean x)
        {
            Check.NotNull(x, nameof(x));
            return new DbBooleanCast(x).MakeColumn<_String>();
        }

        private sealed class DbByteCast : CastExpression<Byte?, String>
        {
            public DbByteCast(_Byte x)
                : base(x)
            {
            }

            protected override String Cast(Byte? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Byte" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        public static explicit operator _String(_Byte x)
        {
            Check.NotNull(x, nameof(x));
            return new DbByteCast(x).MakeColumn<_String>();
        }

        private sealed class DbInt16Cast : CastExpression<Int16?, String>
        {
            public DbInt16Cast(_Int16 x)
                : base(x)
            {
            }

            protected override String Cast(Int16? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int16" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        public static explicit operator _String(_Int16 x)
        {
            Check.NotNull(x, nameof(x));
            return new DbInt16Cast(x).MakeColumn<_String>();
        }

        private sealed class DbInt32Cast : CastExpression<Int32?, String>
        {
            public DbInt32Cast(_Int32 x)
                : base(x)
            {
            }

            protected override String Cast(Int32? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int32" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        public static explicit operator _String(_Int32 x)
        {
            Check.NotNull(x, nameof(x));
            return new DbInt32Cast(x).MakeColumn<_String>();
        }

        private sealed class DbInt64Cast : CastExpression<Int64?, String>
        {
            public DbInt64Cast(_Int64 x)
                : base(x)
            {
            }

            protected override String Cast(Int64? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int64" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        public static explicit operator _String(_Int64 x)
        {
            Check.NotNull(x, nameof(x));
            return new DbInt64Cast(x).MakeColumn<_String>();
        }

        private sealed class DbDecimalCast : CastExpression<Decimal?, String>
        {
            public DbDecimalCast(_Decimal x)
                : base(x)
            {
            }

            protected override String Cast(Decimal? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Decimal" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        public static explicit operator _String(_Decimal x)
        {
            Check.NotNull(x, nameof(x));
            return new DbDecimalCast(x).MakeColumn<_String>();
        }

        private sealed class DbDoubleCast : CastExpression<Double?, String>
        {
            public DbDoubleCast(_Double x)
                : base(x)
            {
            }

            protected override String Cast(Double? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Double" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        public static explicit operator _String(_Double x)
        {
            Check.NotNull(x, nameof(x));
            return new DbDoubleCast(x).MakeColumn<_String>();
        }

        private sealed class DbSingleCast : CastExpression<Single?, String>
        {
            public DbSingleCast(_Single x)
                : base(x)
            {
            }

            protected override String Cast(Single? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Single" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        public static explicit operator _String(_Single x)
        {
            Check.NotNull(x, nameof(x));
            return new DbSingleCast(x).MakeColumn<_String>();
        }

        private sealed class DbCharCast : CastExpression<Char?, String>
        {
            public DbCharCast(_Char x)
                : base(x)
            {
            }

            protected override String Cast(Char? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString() : null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Char" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Char" /> object. </param>
        public static explicit operator _String(_Char x)
        {
            Check.NotNull(x, nameof(x));
            return new DbCharCast(x).MakeColumn<_String>();
        }

        private sealed class DbDateTimeCast : CastExpression<DateTime?, String>
        {
            public DbDateTimeCast(_DateTime x)
                : base(x)
            {
            }

            protected override String Cast(DateTime? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString("O", CultureInfo.InvariantCulture) : null;
            }
        }

        /// <summary>Converts the supplied <see cref="_DateTime" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_DateTime" /> object. </param>
        public static explicit operator _String(_DateTime x)
        {
            Check.NotNull(x, nameof(x));
            return new DbDateTimeCast(x).MakeColumn<_String>();
        }

        private sealed class DbGuidCast : CastExpression<Guid?, String>
        {
            public DbGuidCast(_Guid x)
                : base(x)
            {
            }

            protected override String Cast(Guid? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString() : null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Guid" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        public static explicit operator _String(_Guid x)
        {
            Check.NotNull(x, nameof(x));
            return new DbGuidCast(x).MakeColumn<_String>();
        }

        /// <exclude />
        public override bool Equals(object obj)
        {
            // override to eliminate compile warning
            return base.Equals(obj);
        }

        /// <exclude />
        public override int GetHashCode()
        {
            // override to eliminate compile warning
            return base.GetHashCode();
        }
    }
}
