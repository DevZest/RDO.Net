using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Data.Common;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Int16"/> column.
    /// </summary>
    [ColumnConverter(typeof(Converter))]
    public sealed class _Int16 : Column<Int16?>, IColumn<DbReader, Int16?>
    {
        private sealed class Converter : ConverterBase<_Int16>
        {
        }

        protected override bool AreEqual(short? x, short? y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected override Column<short?> CreateParam(short? value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected internal override Column<short?> CreateConst(short? value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(Int16? value)
        {
            return JsonValue.Number(value);
        }

        /// <inheritdoc/>
        protected internal override Int16? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Int16?(Convert.ToInt16(value.Text));
        }

        /// <inheritdoc cref="P:DevZest.Data._Binary.Item(DevZest.Data.DbReader)"/>
        public Int16? this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private Int16? GetValue(DbReader reader)
        {
            return reader.GetInt16(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(Int16? value)
        {
            return !value.HasValue;
        }

        /// <inheritdoc cref="_Binary.Param(Binary, _Binary)"/>
        public static _Int16 Param(Int16? x, _Int16 sourceColumn = null)
        {
            return new ParamExpression<Int16?>(x, sourceColumn).MakeColumn<_Int16>();
        }

        /// <inheritdoc cref="_Binary.Const(Binary)"/>
        public static _Int16 Const(Int16? x)
        {
            return new ConstantExpression<Int16?>(x).MakeColumn<_Int16>();
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        public static implicit operator _Int16(Int16? x)
        {
            return Param(x);
        }

        private sealed class NegateExpression : UnaryExpression<Int16?>
        {
            public NegateExpression(_Int16 x)
                : base(x)
            {
            }

            protected override Int16? EvalCore(Int16? x)
            {
                return (Int16?)(-x);
            }

            protected override DbUnaryExpressionKind ExpressionKind
            {
                get { return DbUnaryExpressionKind.Negate; }
            }
        }

        /// <summary>Negates the <see cref="_Int16" /> operand.</summary>
        /// <returns>A <see cref="_Int16" /> expression that contains the negated result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object.</param>
        public static _Int16 operator -(_Int16 x)
        {
            Check.NotNull(x, nameof(x));
            return new NegateExpression(x).MakeColumn<_Int16>();
        }

        private sealed class OnesComplementExpression : UnaryExpression<Int16?>
        {
            public OnesComplementExpression(_Int16 x)
                : base(x)
            {
            }

            protected override Int16? EvalCore(Int16? x)
            {
                return (Int16?)(~x);
            }

            protected override DbUnaryExpressionKind ExpressionKind
            {
                get { return DbUnaryExpressionKind.OnesComplement; }
            }
        }

        /// <summary>Performs a bitwise one's complement operation on the specified <see cref="_Int16" />.</summary>
        /// <returns>A <see cref="_Int16" /> expression that contains the results of the one's complement operation.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        public static _Int16 operator ~(_Int16 x)
        {
            Check.NotNull(x, nameof(x));
            return new OnesComplementExpression(x).MakeColumn<_Int16>();
        }

        private sealed class AddExpression : BinaryExpression<Int16?>
        {
            public AddExpression(_Int16 x, _Int16 y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Add; }
            }

            protected override Int16? EvalCore(Int16? x, Int16? y)
            {
                return (Int16?)(x + y);
            }
        }

        /// <summary>Computes the sum of the two specified <see cref="_Int16" /> objects.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <param name="y">A <see cref="_Int16" /> object. </param>
        public static _Int16 operator +(_Int16 x, _Int16 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new AddExpression(x, y).MakeColumn<_Int16>();
        }

        private sealed class SubstractExpression : BinaryExpression<Int16?>
        {
            public SubstractExpression(_Int16 x, _Int16 y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Substract; }
            }

            protected override Int16? EvalCore(Int16? x, Int16? y)
            {
                return (Int16?)(x - y);
            }
        }

        /// <summary>Substracts the two specified <see cref="_Int16" /> objects.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <param name="y">A <see cref="_Int16" /> object. </param>
        public static _Int16 operator -(_Int16 x, _Int16 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new SubstractExpression(x, y).MakeColumn<_Int16>();
        }

        private sealed class MultiplyExpression : BinaryExpression<Int16?>
        {
            public MultiplyExpression(_Int16 x, _Int16 y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Multiply; }
            }

            protected override Int16? EvalCore(Int16? x, Int16? y)
            {
                return (Int16?)(x * y);
            }
        }

        /// <summary>Multiplies the two specified <see cref="_Int16" /> objects.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <param name="y">A <see cref="_Int16" /> object. </param>
        public static _Int16 operator *(_Int16 x, _Int16 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new MultiplyExpression(x, y).MakeColumn<_Int16>();
        }

        private sealed class DivideExpression : BinaryExpression<Int16?>
        {
            public DivideExpression(_Int16 x, _Int16 y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Divide; }
            }

            protected override Int16? EvalCore(Int16? x, Int16? y)
            {
                return (Int16?)(x / y);
            }
        }

        /// <summary>Divides the two specified <see cref="_Int16" /> objects.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <param name="y">A <see cref="_Int16" /> object. </param>
        public static _Int16 operator /(_Int16 x, _Int16 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new DivideExpression(x, y).MakeColumn<_Int16>();
        }

        private sealed class ModuloExpression : BinaryExpression<Int16?>
        {
            public ModuloExpression(_Int16 x, _Int16 y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Modulo; }
            }

            protected override Int16? EvalCore(Int16? x, Int16? y)
            {
                return (Int16?)(x % y);
            }
        }

        /// <summary>Computes the remainder after dividing the first <see cref="_Int16" /> parameter by the second.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <param name="y">A <see cref="_Int16" /> object. </param>
        public static _Int16 operator %(_Int16 x, _Int16 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new ModuloExpression(x, y).MakeColumn<_Int16>();
        }

        private sealed class BitwiseAndExpression : BinaryExpression<Int16?>
        {
            public BitwiseAndExpression(_Int16 x, _Int16 y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseAnd; }
            }

            protected override Int16? EvalCore(Int16? x, Int16? y)
            {
                return (Int16?)(x & y);
            }
        }

        /// <summary>Computes the bitwise AND of its <see cref="_Int16" /> operands.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <param name="y">A <see cref="_Int16" /> object. </param>
        public static _Int16 operator &(_Int16 x, _Int16 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseAndExpression(x, y).MakeColumn<_Int16>();
        }

        private sealed class BitwiseOrExpression : BinaryExpression<Int16?>
        {
            public BitwiseOrExpression(_Int16 x, _Int16 y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseOr; }
            }

            protected override Int16? EvalCore(Int16? x, Int16? y)
            {
                return (Int16?)(x | y);
            }
        }

        /// <summary>Computes the bitwise OR of its <see cref="_Int16" /> operands.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <param name="y">A <see cref="_Int16" /> object. </param>
        public static _Int16 operator |(_Int16 x, _Int16 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseOrExpression(x, y).MakeColumn<_Int16>();
        }

        private sealed class BitwiseXorExpression : BinaryExpression<Int16?>
        {
            public BitwiseXorExpression(_Int16 x, _Int16 y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseXor; }
            }

            protected override Int16? EvalCore(Int16? x, Int16? y)
            {
                return (Int16?)(x ^ y);
            }
        }

        /// <summary>Computes the bitwise exclusive-OR of its <see cref="_Int16" /> operands.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <param name="y">A <see cref="_Int16" /> object. </param>
        public static _Int16 operator ^(_Int16 x, _Int16 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseXorExpression(x, y).MakeColumn<_Int16>();
        }

        private sealed class LessThanExpression : BinaryExpression<Int16?, bool?>
        {
            public LessThanExpression(_Int16 x, _Int16 y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(Int16? x, Int16? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() < y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Int16" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <param name="y">A <see cref="_Int16" /> object. </param>
        public static _Boolean operator <(_Int16 x, _Int16 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<Int16?, bool?>
        {
            public LessThanOrEqualExpression(_Int16 x, _Int16 y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(Int16? x, Int16? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() <= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Int16" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <param name="y">A <see cref="_Int16" /> object. </param>
        public static _Boolean operator <=(_Int16 x, _Int16 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<Int16?, bool?>
        {
            public GreaterThanExpression(_Int16 x, _Int16 y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(Int16? x, Int16? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() > y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Int16" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <param name="y">A <see cref="_Int16" /> object. </param>
        public static _Boolean operator >(_Int16 x, _Int16 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Int16?, bool?>
        {
            public GreaterThanOrEqualExpression(_Int16 x, _Int16 y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(Int16? x, Int16? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() >= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Int16" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <param name="y">A <see cref="_Int16" /> object. </param>
        public static _Boolean operator >=(_Int16 x, _Int16 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<Int16?, bool?>
        {
            public EqualExpression(Column<Int16?> x, Column<Int16?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(Int16? x, Int16? y)
            {
                return x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Int16" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <param name="y">A <see cref="_Int16" /> object. </param>
        public static _Boolean operator ==(_Int16 x, _Int16 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<Int16?, bool?>
        {
            public NotEqualExpression(Column<Int16?> x, Column<Int16?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(Int16? x, Int16? y)
            {
                return !x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Int16" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <param name="y">A <see cref="_Int16" /> object. </param>
        public static _Boolean operator !=(_Int16 x, _Int16 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class DbBooleanCast : CastExpression<bool?, Int16?>
        {
            public DbBooleanCast(_Boolean x)
                : base(x)
            {
            }

            protected override Int16? Cast(bool? value)
            {
                if (value.HasValue)
                    return (Int16?)(value.GetValueOrDefault() ? 1 : 0);
                return null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Boolean" /> to <see cref="_Int16" />.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Boolean" /> object. </param>
        public static explicit operator _Int16(_Boolean x)
        {
            Check.NotNull(x, nameof(x));
            return new DbBooleanCast(x).MakeColumn<_Int16>();
        }

        private sealed class DbByteCast : CastExpression<byte?, Int16?>
        {
            public DbByteCast(_Byte x)
                : base(x)
            {
            }

            protected override Int16? Cast(byte? value)
            {
                return value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Byte" /> to <see cref="_Int16" />.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        public static implicit operator _Int16(_Byte x)
        {
            Check.NotNull(x, nameof(x));
            return new DbByteCast(x).MakeColumn<_Int16>();
        }

        private sealed class DbInt32Cast : CastExpression<Int32?, Int16?>
        {
            public DbInt32Cast(_Int32 x)
                : base(x)
            {
            }

            protected override Int16? Cast(Int32? value)
            {
                return (Int16?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int32" /> to <see cref="_Int16" />.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        public static explicit operator _Int16(_Int32 x)
        {
            Check.NotNull(x, nameof(x));
            return new DbInt32Cast(x).MakeColumn<_Int16>();
        }

        private sealed class DbInt64Cast : CastExpression<Int64?, Int16?>
        {
            public DbInt64Cast(_Int64 x)
                : base(x)
            {
            }

            protected override Int16? Cast(Int64? value)
            {
                return (Int16?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int64" /> to <see cref="_Int16" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        public static explicit operator _Int16(_Int64 x)
        {
            Check.NotNull(x, nameof(x));
            return new DbInt64Cast(x).MakeColumn<_Int16>();
        }

        private sealed class DbDecimalCast : CastExpression<Decimal?, Int16?>
        {
            public DbDecimalCast(_Decimal x)
                : base(x)
            {
            }

            protected override Int16? Cast(Decimal? value)
            {
                return (Int16?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Decimal" /> to <see cref="_Int16" />.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        public static explicit operator _Int16(_Decimal x)
        {
            Check.NotNull(x, nameof(x));
            return new DbDecimalCast(x).MakeColumn<_Int16>();
        }

        private sealed class DbDoubleCast : CastExpression<Double?, Int16?>
        {
            public DbDoubleCast(_Double x)
                : base(x)
            {
            }

            protected override Int16? Cast(Double? value)
            {
                return (Int16?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Double" /> to <see cref="_Int16" />.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        public static explicit operator _Int16(_Double x)
        {
            Check.NotNull(x, nameof(x));
            return new DbDoubleCast(x).MakeColumn<_Int16>();
        }

        private sealed class DbSingleCast : CastExpression<Single?, Int16?>
        {
            public DbSingleCast(_Single x)
                : base(x)
            {
            }

            protected override Int16? Cast(Single? value)
            {
                return (Int16?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Single" /> to <see cref="_Int16" />.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        public static explicit operator _Int16(_Single x)
        {
            Check.NotNull(x, nameof(x));
            return new DbSingleCast(x).MakeColumn<_Int16>();
        }

        private sealed class DbStringCast : CastExpression<String, Int16?>
        {
            public DbStringCast(_String x)
                : base(x)
            {
            }

            protected override Int16? Cast(String value)
            {
                if (value == null)
                    return null;
                return Int16.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_Int16" />.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _Int16(_String x)
        {
            Check.NotNull(x, nameof(x));
            return new DbStringCast(x).MakeColumn<_Int16>();
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
