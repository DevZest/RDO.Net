﻿using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Int32"/> Column.
    /// </summary>
    public sealed class _Int32 : Column<Int32?>, IColumn<DbReader, Int32?>
    {
        private sealed class CastToStringExpression : CastExpression<Int32?, String>
        {
            public CastToStringExpression(Column<Int32?> x)
                : base(x)
            {
            }

            protected override String Cast(Int32? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : null;
            }
        }

        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        /// <summary>Converts the supplied <see cref="_Int32" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        public static explicit operator _String(_Int32 x)
        {
            Check.NotNull(x, nameof(x));
            return x.CastToString();
        }

        public override bool AreEqual(int? x, int? y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected override Column<int?> CreateParam(int? value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected internal override Column<int?> CreateConst(int? value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(Int32? value)
        {
            return JsonValue.Number(value);
        }

        /// <inheritdoc/>
        protected internal override Int32? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Int32?(Convert.ToInt32(value.Text));
        }

        /// <inheritdoc cref="P:DevZest.Data._Binary.Item(DevZest.Data.DbReader)"/>
        public Int32? this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private Int32? GetValue(DbReader reader)
        {
            return reader.GetInt32(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(Int32? value)
        {
            return !value.HasValue;
        }

        /// <inheritdoc cref="_Binary.Param(Binary, _Binary)"/>
        public static _Int32 Param(Int32? x, _Int32 sourceColumn = null)
        {
            return new ParamExpression<Int32?>(x, sourceColumn).MakeColumn<_Int32>();
        }

        /// <inheritdoc cref="_Binary.Const(Binary)"/>
        public static _Int32 Const(Int32? x)
        {
            return new ConstantExpression<Int32?>(x).MakeColumn<_Int32>();
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        public static implicit operator _Int32(Int32? x)
        {
            return Param(x);
        }

        private sealed class NegateExpression : UnaryExpression<Int32?>
        {
            public NegateExpression(Column<Int32?> x)
                : base(x)
            {
            }

            protected override Int32? EvalCore(Int32? x)
            {
                return -x;
            }

            protected override DbUnaryExpressionKind ExpressionKind
            {
                get { return DbUnaryExpressionKind.Negate; }
            }
        }

        /// <summary>Negates the <see cref="_Int32" /> operand.</summary>
        /// <returns>A <see cref="_Int32" /> expression that contains the negated result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object.</param>
        public static _Int32 operator -(_Int32 x)
        {
            Check.NotNull(x, nameof(x));
            return new NegateExpression(x).MakeColumn<_Int32>();
        }

        private sealed class OnesComplementExpression : UnaryExpression<Int32?>
        {
            public OnesComplementExpression(Column<Int32?> x)
                : base(x)
            {
            }

            protected override Int32? EvalCore(Int32? x)
            {
                return ~x;
            }

            protected override DbUnaryExpressionKind ExpressionKind
            {
                get { return DbUnaryExpressionKind.OnesComplement; }
            }
        }

        /// <summary>Performs a bitwise one's complement operation on the specified <see cref="_Int32" />.</summary>
        /// <returns>A <see cref="_Int32" /> expression that contains the results of the one's complement operation.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        public static _Int32 operator ~(_Int32 x)
        {
            Check.NotNull(x, nameof(x));
            return new OnesComplementExpression(x).MakeColumn<_Int32>();
        }

        private sealed class AddExpression : BinaryExpression<Int32?>
        {
            public AddExpression(Column<Int32?> x, Column<Int32?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Add; }
            }

            protected override Int32? EvalCore(Int32? x, Int32? y)
            {
                return x + y;
            }
        }

        /// <summary>Computes the sum of the two specified <see cref="_Int32" /> objects.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        /// <param name="y">A <see cref="_Int32" /> object. </param>
        public static _Int32 operator +(_Int32 x, _Int32 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new AddExpression(x, y).MakeColumn<_Int32>();
        }

        private sealed class SubstractExpression : BinaryExpression<Int32?>
        {
            public SubstractExpression(Column<Int32?> x, Column<Int32?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Substract; }
            }

            protected override Int32? EvalCore(Int32? x, Int32? y)
            {
                return x - y;
            }
        }

        /// <summary>Substracts the two specified <see cref="_Int32" /> objects.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        /// <param name="y">A <see cref="_Int32" /> object. </param>
        public static _Int32 operator -(_Int32 x, _Int32 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new SubstractExpression(x, y).MakeColumn<_Int32>();
        }

        private sealed class MultiplyExpression : BinaryExpression<Int32?>
        {
            public MultiplyExpression(Column<Int32?> x, Column<Int32?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Multiply; }
            }

            protected override Int32? EvalCore(Int32? x, Int32? y)
            {
                return x * y;
            }
        }

        /// <summary>Multiplies the two specified <see cref="_Int32" /> objects.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        /// <param name="y">A <see cref="_Int32" /> object. </param>
        public static _Int32 operator *(_Int32 x, _Int32 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new MultiplyExpression(x, y).MakeColumn<_Int32>();
        }

        private sealed class DivideExpression : BinaryExpression<Int32?>
        {
            public DivideExpression(Column<int?> x, Column<int?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Divide; }
            }

            protected override Int32? EvalCore(Int32? x, Int32? y)
            {
                return x / y;
            }
        }

        /// <summary>Divides the two specified <see cref="_Int32" /> objects.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        /// <param name="y">A <see cref="_Int32" /> object. </param>
        public static _Int32 operator /(_Int32 x, _Int32 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new DivideExpression(x, y).MakeColumn<_Int32>();
        }

        private sealed class ModuloExpression : BinaryExpression<Int32?>
        {
            public ModuloExpression(Column<Int32?> x, Column<Int32?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Modulo; }
            }

            protected override Int32? EvalCore(Int32? x, Int32? y)
            {
                return x % y;
            }
        }

        /// <summary>Computes the remainder after dividing the first <see cref="_Int32" /> parameter by the second.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        /// <param name="y">A <see cref="_Int32" /> object. </param>
        public static _Int32 operator %(_Int32 x, _Int32 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new ModuloExpression(x, y).MakeColumn<_Int32>();
        }

        private sealed class BitwiseAndExpression : BinaryExpression<Int32?>
        {
            public BitwiseAndExpression(Column<Int32?> x, Column<Int32?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseAnd; }
            }

            protected override Int32? EvalCore(Int32? x, Int32? y)
            {
                return x & y;
            }
        }

        /// <summary>Computes the bitwise AND of its <see cref="_Int32" /> operands.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        /// <param name="y">A <see cref="_Int32" /> object. </param>
        public static _Int32 operator &(_Int32 x, _Int32 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseAndExpression(x, y).MakeColumn<_Int32>();
        }

        private sealed class BitwiseOrExpression : BinaryExpression<Int32?>
        {
            public BitwiseOrExpression(Column<Int32?> x, Column<Int32?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseOr; }
            }

            protected override Int32? EvalCore(Int32? x, Int32? y)
            {
                return x | y;
            }
        }

        /// <summary>Computes the bitwise OR of its <see cref="_Int32" /> operands.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        /// <param name="y">A <see cref="_Int32" /> object. </param>
        public static _Int32 operator |(_Int32 x, _Int32 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseOrExpression(x, y).MakeColumn<_Int32>();
        }

        private sealed class BitwiseXorExpression : BinaryExpression<Int32?>
        {
            public BitwiseXorExpression(Column<Int32?> x, Column<Int32?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseXor; }
            }

            protected override Int32? EvalCore(Int32? x, Int32? y)
            {
                return x ^ y;
            }
        }

        /// <summary>Computes the bitwise exclusive-OR of its <see cref="_Int32" /> operands.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        /// <param name="y">A <see cref="_Int32" /> object. </param>
        public static _Int32 operator ^(_Int32 x, _Int32 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseXorExpression(x, y).MakeColumn<_Int32>();
        }

        private sealed class LessThanExpression : BinaryExpression<Int32?, bool?>
        {
            public LessThanExpression(Column<Int32?> x, Column<Int32?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(Int32? x, Int32? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() < y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Int32" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        /// <param name="y">A <see cref="_Int32" /> object. </param>
        public static _Boolean operator <(_Int32 x, _Int32 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<Int32?, bool?>
        {
            public LessThanOrEqualExpression(Column<Int32?> x, Column<Int32?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(Int32? x, Int32? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() <= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Int32" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        /// <param name="y">A <see cref="_Int32" /> object. </param>
        public static _Boolean operator <=(_Int32 x, _Int32 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<Int32?, bool?>
        {
            public GreaterThanExpression(Column<Int32?> x, Column<Int32?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(Int32? x, Int32? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() > y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Int32" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        /// <param name="y">A <see cref="_Int32" /> object. </param>
        public static _Boolean operator >(_Int32 x, _Int32 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Int32?, bool?>
        {
            public GreaterThanOrEqualExpression(Column<Int32?> x, Column<Int32?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(Int32? x, Int32? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() >= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Int32" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        /// <param name="y">A <see cref="_Int32" /> object. </param>
        public static _Boolean operator >=(_Int32 x, _Int32 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<Int32?, bool?>
        {
            public EqualExpression(Column<Int32?> x, Column<Int32?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(Int32? x, Int32? y)
            {
                return x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Int32" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        /// <param name="y">A <see cref="_Int32" /> object. </param>
        public static _Boolean operator ==(_Int32 x, _Int32 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<Int32?, bool?>
        {
            public NotEqualExpression(Column<Int32?> x, Column<Int32?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(Int32? x, Int32? y)
            {
                return !x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Int32" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        /// <param name="y">A <see cref="_Int32" /> object. </param>
        public static _Boolean operator !=(_Int32 x, _Int32 y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
        }
        
        private sealed class FromBooleanCast : CastExpression<bool?, Int32?>
        {
            public FromBooleanCast(Column<bool?> x)
                : base(x)
            {
            }

            protected override Int32? Cast(bool? value)
            {
                if (value.HasValue)
                    return value.GetValueOrDefault() ? 1 : 0;
                return null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Boolean" /> to <see cref="_Int32" />.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Boolean" /> object. </param>
        public static explicit operator _Int32(_Boolean x)
        {
            Check.NotNull(x, nameof(x));
            return new FromBooleanCast(x).MakeColumn<_Int32>();
        }

        private sealed class FromByteCast : CastExpression<byte?, Int32?>
        {
            public FromByteCast(Column<Byte?> x)
                : base(x)
            {
            }

            protected override Int32? Cast(byte? value)
            {
                return value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Byte" /> to <see cref="_Int32" />.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        public static implicit operator _Int32(_Byte x)
        {
            Check.NotNull(x, nameof(x));
            return new FromByteCast(x).MakeColumn<_Int32>();
        }

        private sealed class FromInt16Cast : CastExpression<Int16?, Int32?>
        {
            public FromInt16Cast(Column<Int16?> x)
                : base(x)
            {
            }

            protected override Int32? Cast(Int16? value)
            {
                return value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int16" /> to <see cref="_Int32" />.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        public static implicit operator _Int32(_Int16 x)
        {
            Check.NotNull(x, nameof(x));
            return new FromInt16Cast(x).MakeColumn<_Int32>();
        }

        private sealed class FromInt64Cast : CastExpression<Int64?, Int32?>
        {
            public FromInt64Cast(Column<Int64?> x)
                : base(x)
            {
            }

            protected override Int32? Cast(Int64? value)
            {
                return (Int32?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int64" /> to <see cref="_Int32" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        public static explicit operator _Int32(_Int64 x)
        {
            Check.NotNull(x, nameof(x));
            return new FromInt64Cast(x).MakeColumn<_Int32>();
        }

        private sealed class FromDecimalCast : CastExpression<Decimal?, Int32?>
        {
            public FromDecimalCast(Column<Decimal?> x)
                : base(x)
            {
            }

            protected override Int32? Cast(Decimal? value)
            {
                return (Int32?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Decimal" /> to <see cref="_Int32" />.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        public static explicit operator _Int32(_Decimal x)
        {
            Check.NotNull(x, nameof(x));
            return new FromDecimalCast(x).MakeColumn<_Int32>();
        }

        private sealed class FromDoubleCast : CastExpression<Double?, Int32?>
        {
            public FromDoubleCast(Column<Double?> x)
                : base(x)
            {
            }

            protected override Int32? Cast(Double? value)
            {
                return (Int32?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Double" /> to <see cref="_Int32" />.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        public static explicit operator _Int32(_Double x)
        {
            Check.NotNull(x, nameof(x));
            return new FromDoubleCast(x).MakeColumn<_Int32>();
        }

        private sealed class FromSingleCast : CastExpression<Single?, Int32?>
        {
            public FromSingleCast(Column<Single?> x)
                : base(x)
            {
            }

            protected override Int32? Cast(Single? value)
            {
                return (Int32?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Single" /> to <see cref="_Int32" />.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        public static explicit operator _Int32(_Single x)
        {
            Check.NotNull(x, nameof(x));
            return new FromSingleCast(x).MakeColumn<_Int32>();
        }

        private sealed class FromStringCast : CastExpression<String, Int32?>
        {
            public FromStringCast(Column<String> x)
                : base(x)
            {
            }

            protected override Int32? Cast(String value)
            {
                if (value == null)
                    return null;
                return Int32.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_Int32" />.</summary>
        /// <returns>A <see cref="_Int32" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _Int32(_String x)
        {
            Check.NotNull(x, nameof(x));
            return new FromStringCast(x).MakeColumn<_Int32>();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            // override to eliminate compile warning
            return base.Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // override to eliminate compile warning
            return base.GetHashCode();
        }

        /// <summary>Declares this column as identity column.</summary>
        /// <param name="seed">The seed of the identity.</param>
        /// <param name="increment">The increment of the identity.</param>
        /// <returns>The <see cref="Data.Identity"/> object.</returns>
        /// <inheritdoc cref="ModelMember.VerifyDesignMode" select="exception"/>
        public Identity Identity(int seed, int increment)
        {
            return Identity(seed, increment, false);
        }

        internal Identity Identity(int seed, int increment, bool isTempTable)
        {
            VerifyDesignMode();

            if (increment == 0)
                throw new ArgumentException(Strings.Model_InvalidIdentityIncrement, nameof(increment));

            var identity = new Identity(this, seed, increment, isTempTable);
            AddOrUpdateExtension(identity);
            var model = ParentModel;
            if (model.ContainsExtension(((IExtension)identity).Key))
                throw new InvalidOperationException(Strings.Model_MultipleIdentityColumn);
            model.AddExtension(identity);
            return identity;
        }

        bool _isIdentityInitialized;
        Identity _identity;
        int _currentIdentityValue;

        public override int? DefaultValue
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
    }
}
