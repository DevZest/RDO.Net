﻿using DevZest.Data.Addons;
using DevZest.Data.Primitives;
using System;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Int64"/> column.
    /// </summary>
    public sealed class _Int64 : Column<Int64?>, IColumn<DbReader, Int64?>
    {
        private sealed class CastToStringExpression : CastExpression<Int64?, String>
        {
            public CastToStringExpression(Column<Int64?> x)
                : base(x)
            {
            }

            protected override String Cast(Int64? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : null;
            }
        }

        /// <inheritdoc/>
        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        /// <summary>Converts the supplied <see cref="_Int64" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        public static explicit operator _String(_Int64 x)
        {
            x.VerifyNotNull(nameof(x));
            return x.CastToString();
        }

        /// <inheritdoc/>
        public override bool AreEqual(long? x, long? y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected override Column<long?> CreateParam(long? value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected internal override Column<long?> CreateConst(long? value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(Int64? value)
        {
            return JsonValue.Number(value);
        }

        /// <inheritdoc/>
        protected internal override Int64? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Int64?(Convert.ToInt64(value.Text));
        }


        /// <summary>Gets the value of this column from <see cref="DbReader"/>'s current row.</summary>
        /// <param name="reader">The <see cref="DbReader"/> object.</param>
        /// <returns>The value of this column from <see cref="DbReader"/>'s current row.</returns>
        public Int64? this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private Int64? GetValue(DbReader reader)
        {
            return reader.GetInt64(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(Int64? value)
        {
            return !value.HasValue;
        }

        /// <summary>Creates a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <param name="sourceColumn">The value which will be passed to <see cref="DbParamExpression.SourceColumn"/>.</param>
        /// <returns>The column of parameter expression.</returns>
        public static _Int64 Param(Int64? x, _Int64 sourceColumn = null)
        {
            return new ParamExpression<Int64?>(x, sourceColumn).MakeColumn<_Int64>();
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="x">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        public static _Int64 Const(Int64? x)
        {
            return new ConstantExpression<Int64?>(x).MakeColumn<_Int64>();
        }

        /// <summary>Implicitly converts the supplied value to a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <returns>The column of parameter expression.</returns>
        public static implicit operator _Int64(Int64? x)
        {
            return Param(x);
        }

        private sealed class NegateExpression : UnaryExpression<Int64?>
        {
            public NegateExpression(Column<Int64?> x)
                : base(x)
            {
            }

            protected override Int64? EvalCore(Int64? x)
            {
                return -x;
            }

            protected override DbUnaryExpressionKind ExpressionKind
            {
                get { return DbUnaryExpressionKind.Negate; }
            }
        }

        /// <summary>Negates the <see cref="_Int64" /> operand.</summary>
        /// <returns>A <see cref="_Int64" /> expression that contains the negated result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object.</param>
        public static _Int64 operator -(_Int64 x)
        {
            x.VerifyNotNull(nameof(x));
            return new NegateExpression(x).MakeColumn<_Int64>();
        }

        private sealed class OnesComplementExpression : UnaryExpression<Int64?>
        {
            public OnesComplementExpression(Column<Int64?> x)
                : base(x)
            {
            }

            protected override Int64? EvalCore(Int64? x)
            {
                return ~x;
            }

            protected override DbUnaryExpressionKind ExpressionKind
            {
                get { return DbUnaryExpressionKind.OnesComplement; }
            }
        }

        /// <summary>Performs a bitwise one's complement operation on the specified <see cref="_Int64" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression that contains the results of the one's complement operation.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        public static _Int64 operator ~(_Int64 x)
        {
            x.VerifyNotNull(nameof(x));
            return new OnesComplementExpression(x).MakeColumn<_Int64>();
        }

        private sealed class AddExpression : BinaryExpression<Int64?>
        {
            public AddExpression(Column<Int64?> x, Column<Int64?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Add; }
            }

            protected override Int64? EvalCore(Int64? x, Int64? y)
            {
                return x + y;
            }
        }

        /// <summary>Computes the sum of the two specified <see cref="_Int64" /> objects.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        /// <param name="y">A <see cref="_Int64" /> object. </param>
        public static _Int64 operator +(_Int64 x, _Int64 y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new AddExpression(x, y).MakeColumn<_Int64>();
        }

        private sealed class SubstractExpression : BinaryExpression<Int64?>
        {
            public SubstractExpression(Column<Int64?> x, Column<Int64?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Substract; }
            }

            protected override Int64? EvalCore(Int64? x, Int64? y)
            {
                return x - y;
            }
        }

        /// <summary>Substracts the two specified <see cref="_Int64" /> objects.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        /// <param name="y">A <see cref="_Int64" /> object. </param>
        public static _Int64 operator -(_Int64 x, _Int64 y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new SubstractExpression(x, y).MakeColumn<_Int64>();
        }

        private sealed class MultiplyExpression : BinaryExpression<Int64?>
        {
            public MultiplyExpression(Column<Int64?> x, Column<Int64?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Multiply; }
            }

            protected override Int64? EvalCore(Int64? x, Int64? y)
            {
                return x * y;
            }
        }

        /// <summary>Multiplies the two specified <see cref="_Int64" /> objects.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        /// <param name="y">A <see cref="_Int64" /> object. </param>
        public static _Int64 operator *(_Int64 x, _Int64 y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new MultiplyExpression(x, y).MakeColumn<_Int64>();
        }

        private sealed class DivideExpression : BinaryExpression<Int64?>
        {
            public DivideExpression(Column<Int64?> x, Column<Int64?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Divide; }
            }

            protected override Int64? EvalCore(Int64? x, Int64? y)
            {
                return x / y;
            }
        }

        /// <summary>Divides the two specified <see cref="_Int64" /> objects.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        /// <param name="y">A <see cref="_Int64" /> object. </param>
        public static _Int64 operator /(_Int64 x, _Int64 y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new DivideExpression(x, y).MakeColumn<_Int64>();
        }

        private sealed class ModuloExpression : BinaryExpression<Int64?>
        {
            public ModuloExpression(Column<Int64?> x, Column<Int64?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Modulo; }
            }

            protected override Int64? EvalCore(Int64? x, Int64? y)
            {
                return x % y;
            }
        }

        /// <summary>Computes the remainder after dividing the first <see cref="_Int64" /> parameter by the second.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        /// <param name="y">A <see cref="_Int64" /> object. </param>
        public static _Int64 operator %(_Int64 x, _Int64 y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new ModuloExpression(x, y).MakeColumn<_Int64>();
        }

        private sealed class BitwiseAndExpression : BinaryExpression<Int64?>
        {
            public BitwiseAndExpression(Column<Int64?> x, Column<Int64?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseAnd; }
            }

            protected override Int64? EvalCore(Int64? x, Int64? y)
            {
                return x & y;
            }
        }

        /// <summary>Computes the bitwise AND of its <see cref="_Int64" /> operands.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        /// <param name="y">A <see cref="_Int64" /> object. </param>
        public static _Int64 operator &(_Int64 x, _Int64 y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new BitwiseAndExpression(x, y).MakeColumn<_Int64>();
        }

        private sealed class BitwiseOrExpression : BinaryExpression<Int64?>
        {
            public BitwiseOrExpression(Column<Int64?> x, Column<Int64?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseOr; }
            }

            protected override Int64? EvalCore(Int64? x, Int64? y)
            {
                return x | y;
            }
        }

        /// <summary>Computes the bitwise OR of its <see cref="_Int64" /> operands.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        /// <param name="y">A <see cref="_Int64" /> object. </param>
        public static _Int64 operator |(_Int64 x, _Int64 y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new BitwiseOrExpression(x, y).MakeColumn<_Int64>();
        }

        private sealed class BitwiseXorExpression : BinaryExpression<Int64?>
        {
            public BitwiseXorExpression(Column<Int64?> x, Column<Int64?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseXor; }
            }

            protected override Int64? EvalCore(Int64? x, Int64? y)
            {
                return x ^ y;
            }
        }

        /// <summary>Computes the bitwise exclusive-OR of its <see cref="_Int64" /> operands.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        /// <param name="y">A <see cref="_Int64" /> object. </param>
        public static _Int64 operator ^(_Int64 x, _Int64 y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new BitwiseXorExpression(x, y).MakeColumn<_Int64>();
        }

        private sealed class LessThanExpression : BinaryExpression<Int64?, bool?>
        {
            public LessThanExpression(Column<Int64?> x, Column<Int64?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(Int64? x, Int64? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() < y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Int64" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        /// <param name="y">A <see cref="_Int64" /> object. </param>
        public static _Boolean operator <(_Int64 x, _Int64 y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<Int64?, bool?>
        {
            public LessThanOrEqualExpression(Column<Int64?> x, Column<Int64?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(Int64? x, Int64? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() <= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Int64" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        /// <param name="y">A <see cref="_Int64" /> object. </param>
        public static _Boolean operator <=(_Int64 x, _Int64 y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<Int64?, bool?>
        {
            public GreaterThanExpression(Column<Int64?> x, Column<Int64?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(Int64? x, Int64? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() > y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Int64" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        /// <param name="y">A <see cref="_Int64" /> object. </param>
        public static _Boolean operator >(_Int64 x, _Int64 y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Int64?, bool?>
        {
            public GreaterThanOrEqualExpression(Column<Int64?> x, Column<Int64?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(Int64? x, Int64? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() >= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Int64" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        /// <param name="y">A <see cref="_Int64" /> object. </param>
        public static _Boolean operator >=(_Int64 x, _Int64 y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<Int64?, bool?>
        {
            public EqualExpression(Column<Int64?> x, Column<Int64?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(Int64? x, Int64? y)
            {
                return x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Int64" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        /// <param name="y">A <see cref="_Int64" /> object. </param>
        public static _Boolean operator ==(_Int64 x, _Int64 y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<Int64?, bool?>
        {
            public NotEqualExpression(Column<Int64?> x, Column<Int64?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(Int64? x, Int64? y)
            {
                return !x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Int64" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        /// <param name="y">A <see cref="_Int64" /> object. </param>
        public static _Boolean operator !=(_Int64 x, _Int64 y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class FromBooleanCast : CastExpression<bool?, Int64?>
        {
            public FromBooleanCast(Column<bool?> x)
                : base(x)
            {
            }

            protected override Int64? Cast(bool? value)
            {
                if (value.HasValue)
                    return value.GetValueOrDefault() ? 1 : 0;
                return null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Boolean" /> to <see cref="_Int64" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Boolean" /> object. </param>
        public static explicit operator _Int64(_Boolean x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromBooleanCast(x).MakeColumn<_Int64>();
        }

        private sealed class FromByteCast : CastExpression<byte?, Int64?>
        {
            public FromByteCast(Column<Byte?> x)
                : base(x)
            {
            }

            protected override Int64? Cast(byte? value)
            {
                return value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Byte" /> to <see cref="_Int64" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        public static implicit operator _Int64(_Byte x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromByteCast(x).MakeColumn<_Int64>();
        }

        private sealed class FromInt16Cast : CastExpression<Int16?, Int64?>
        {
            public FromInt16Cast(Column<Int16?> x)
                : base(x)
            {
            }

            protected override Int64? Cast(Int16? value)
            {
                return value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int16" /> to <see cref="_Int64" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        public static implicit operator _Int64(_Int16 x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromInt16Cast(x).MakeColumn<_Int64>();
        }

        private sealed class FromInt32Cast : CastExpression<Int32?, Int64?>
        {
            public FromInt32Cast(Column<Int32?> x)
                : base(x)
            {
            }

            protected override Int64? Cast(Int32? value)
            {
                return value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int32" /> to <see cref="_Int64" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        public static implicit operator _Int64(_Int32 x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromInt32Cast(x).MakeColumn<_Int64>();
        }

        private sealed class FromDecimalCast : CastExpression<Decimal?, Int64?>
        {
            public FromDecimalCast(Column<Decimal?> x)
                : base(x)
            {
            }

            protected override Int64? Cast(Decimal? value)
            {
                return (Int64?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Decimal" /> to <see cref="_Int64" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        public static explicit operator _Int64(_Decimal x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromDecimalCast(x).MakeColumn<_Int64>();
        }

        private sealed class FromDoubleCast : CastExpression<Double?, Int64?>
        {
            public FromDoubleCast(Column<Double?> x)
                : base(x)
            {
            }

            protected override Int64? Cast(Double? value)
            {
                return (Int64?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Double" /> to <see cref="_Int64" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        public static explicit operator _Int64(_Double x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromDoubleCast(x).MakeColumn<_Int64>();
        }

        private sealed class FromSingleCast : CastExpression<Single?, Int64?>
        {
            public FromSingleCast(Column<Single?> x)
                : base(x)
            {
            }

            protected override Int64? Cast(Single? value)
            {
                return (Int64?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Single" /> to <see cref="_Int64" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        public static explicit operator _Int64(_Single x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromSingleCast(x).MakeColumn<_Int64>();
        }

        private sealed class FromStringCast : CastExpression<String, Int64?>
        {
            public FromStringCast(Column<String> x)
                : base(x)
            {
            }

            protected override Int64? Cast(String value)
            {
                if (value == null)
                    return null;
                return Int64.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_Int64" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _Int64(_String x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromStringCast(x).MakeColumn<_Int64>();
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

        internal Identity SetIdentity(long seed, long increment)
        {
            VerifyDesignMode();

            var result = Identity.FromInt64Column(this, seed, increment);
            SetIdentity(result);
            return result;
        }

        bool _isIdentityInitialized;
        Identity _identity;
        long _currentIdentityValue;

        /// <inheritdoc/>
        public override long? DefaultValue
        {
            get
            {
                if (ParentModel.IsIdentitySuspended)
                    return base.DefaultValue;

                EnsureIdentityInitialized();
                if (_identity != null)
                    return _currentIdentityValue -= _identity.Increment;
                return base.DefaultValue;
            }
        }

        private void EnsureIdentityInitialized()
        {
            if (!_isIdentityInitialized)
            {
                var identity = this.GetIdentity(false);
                if (identity != null && object.ReferenceEquals(identity.Column, this))
                {
                    _identity = identity;
                    _currentIdentityValue = _identity.Seed;
                }
                _isIdentityInitialized = true;
            }
        }

        /// <inheritdoc/>
        public override bool IsIdentity
        {
            get
            {
                EnsureIdentityInitialized();
                return _identity != null;
            }
        }
    }
}
