using DevZest.Data.Primitives;
using System;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Decimal"/> column.
    /// </summary>
    public sealed class _Decimal : Column<Decimal?>, IColumn<DbReader, Decimal?>
    {
        private sealed class CastToStringExpression : CastExpression<Decimal?, String>
        {
            public CastToStringExpression(Column<Decimal?> x)
                : base(x)
            {
            }

            protected override String Cast(Decimal? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : null;
            }
        }

        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        /// <summary>Converts the supplied <see cref="_Decimal" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        public static explicit operator _String(_Decimal x)
        {
            x.VerifyNotNull(nameof(x));
            return x.CastToString();
        }

        public override bool AreEqual(decimal? x, decimal? y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected sealed override Column<decimal?> CreateParam(decimal? value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected internal sealed override Column<decimal?> CreateConst(decimal? value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(Decimal? value)
        {
            return JsonValue.Number(value);
        }

        /// <inheritdoc/>
        protected internal override Decimal? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Decimal?(Convert.ToDecimal(value.Text));
        }

        /// <inheritdoc cref="P:DevZest.Data._Binary.Item(DevZest.Data.DbReader)"/>
        public Decimal? this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private Decimal? GetValue(DbReader reader)
        {
            return reader.GetDecimal(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(decimal? value)
        {
            return !value.HasValue;
        }

        /// <inheritdoc cref="_Binary.Param(Binary, _Binary)"/>
        public static _Decimal Param(Decimal? x, _Decimal sourceColumn = null)
        {
            return new ParamExpression<Decimal?>(x, sourceColumn).MakeColumn<_Decimal>();
        }

        /// <inheritdoc cref="_Binary.Const(Binary)"/>
        public static _Decimal Const(Decimal? x)
        {
            return new ConstantExpression<Decimal?>(x).MakeColumn<_Decimal>();
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        public static implicit operator _Decimal(Decimal? x)
        {
            return Param(x);
        }

        private sealed class NegateExpression : UnaryExpression<Decimal?>
        {
            public NegateExpression(Column<Decimal?> x)
                : base(x)
            {
            }

            protected override Decimal? EvalCore(Decimal? x)
            {
                return -x;
            }

            protected override DbUnaryExpressionKind ExpressionKind
            {
                get { return DbUnaryExpressionKind.Negate; }
            }
        }

        /// <summary>Negates the <see cref="_Decimal" /> operand.</summary>
        /// <returns>A <see cref="_Decimal" /> expression that contains the negated result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object.</param>
        public static _Decimal operator -(_Decimal x)
        {
            x.VerifyNotNull(nameof(x));
            return new NegateExpression(x).MakeColumn<_Decimal>();
        }

        private sealed class AddExpression : BinaryExpression<Decimal?>
        {
            public AddExpression(Column<Decimal?> x, Column<Decimal?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Add; }
            }

            protected override Decimal? EvalCore(Decimal? x, Decimal? y)
            {
                return x + y;
            }
        }

        /// <summary>Computes the sum of the two specified <see cref="_Decimal" /> objects.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        /// <param name="y">A <see cref="_Decimal" /> object. </param>
        public static _Decimal operator +(_Decimal x, _Decimal y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new AddExpression(x, y).MakeColumn<_Decimal>();
        }

        private sealed class SubstractExpression : BinaryExpression<Decimal?>
        {
            public SubstractExpression(Column<Decimal?> x, Column<Decimal?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Substract; }
            }

            protected override Decimal? EvalCore(Decimal? x, Decimal? y)
            {
                return x - y;
            }
        }

        /// <summary>Substracts the two specified <see cref="_Decimal" /> objects.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        /// <param name="y">A <see cref="_Decimal" /> object. </param>
        public static _Decimal operator -(_Decimal x, _Decimal y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new SubstractExpression(x, y).MakeColumn<_Decimal>();
        }

        private sealed class MultiplyExpression : BinaryExpression<Decimal?>
        {
            public MultiplyExpression(Column<Decimal?> x, Column<Decimal?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Multiply; }
            }

            protected override Decimal? EvalCore(Decimal? x, Decimal? y)
            {
                return x * y;
            }
        }

        /// <summary>Multiplies the two specified <see cref="_Decimal" /> objects.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        /// <param name="y">A <see cref="_Decimal" /> object. </param>
        public static _Decimal operator *(_Decimal x, _Decimal y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new MultiplyExpression(x, y).MakeColumn<_Decimal>();
        }

        private sealed class DivideExpression : BinaryExpression<Decimal?>
        {
            public DivideExpression(Column<Decimal?> x, Column<Decimal?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Divide; }
            }

            protected override Decimal? EvalCore(Decimal? x, Decimal? y)
            {
                return x / y;
            }
        }

        /// <summary>Divides the two specified <see cref="_Decimal" /> objects.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        /// <param name="y">A <see cref="_Decimal" /> object. </param>
        public static _Decimal operator /(_Decimal x, _Decimal y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new DivideExpression(x, y).MakeColumn<_Decimal>();
        }

        private sealed class ModuloExpression : BinaryExpression<Decimal?>
        {
            public ModuloExpression(Column<Decimal?> x, Column<Decimal?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Modulo; }
            }

            protected override Decimal? EvalCore(Decimal? x, Decimal? y)
            {
                return x % y;
            }
        }

        /// <summary>Computes the remainder after dividing the first <see cref="_Decimal" /> parameter by the second.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        /// <param name="y">A <see cref="_Decimal" /> object. </param>
        public static _Decimal operator %(_Decimal x, _Decimal y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new ModuloExpression(x, y).MakeColumn<_Decimal>();
        }

        private sealed class LessThanExpression : BinaryExpression<Decimal?, bool?>
        {
            public LessThanExpression(Column<Decimal?> x, Column<Decimal?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(Decimal? x, Decimal? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() < y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Decimal" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        /// <param name="y">A <see cref="_Decimal" /> object. </param>
        public static _Boolean operator <(_Decimal x, _Decimal y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<Decimal?, bool?>
        {
            public LessThanOrEqualExpression(Column<Decimal?> x, Column<Decimal?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(Decimal? x, Decimal? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() <= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Decimal" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        /// <param name="y">A <see cref="_Decimal" /> object. </param>
        public static _Boolean operator <=(_Decimal x, _Decimal y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<Decimal?, bool?>
        {
            public GreaterThanExpression(Column<Decimal?> x, Column<Decimal?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(Decimal? x, Decimal? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() > y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Decimal" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        /// <param name="y">A <see cref="_Decimal" /> object. </param>
        public static _Boolean operator >(_Decimal x, _Decimal y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Decimal?, bool?>
        {
            public GreaterThanOrEqualExpression(Column<Decimal?> x, Column<Decimal?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(Decimal? x, Decimal? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() >= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Decimal" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        /// <param name="y">A <see cref="_Decimal" /> object. </param>
        public static _Boolean operator >=(_Decimal x, _Decimal y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<Decimal?, bool?>
        {
            public EqualExpression(Column<Decimal?> x, Column<Decimal?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(Decimal? x, Decimal? y)
            {
                return x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Decimal" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        /// <param name="y">A <see cref="_Decimal" /> object. </param>
        public static _Boolean operator ==(_Decimal x, _Decimal y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<Decimal?, bool?>
        {
            public NotEqualExpression(Column<Decimal?> x, Column<Decimal?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(Decimal? x, Decimal? y)
            {
                return !x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Decimal" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        /// <param name="y">A <see cref="_Decimal" /> object. </param>
        public static _Boolean operator !=(_Decimal x, _Decimal y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class FromByteCast : CastExpression<byte?, Decimal?>
        {
            public FromByteCast(Column<Byte?> x)
                : base(x)
            {
            }

            protected override Decimal? Cast(byte? value)
            {
                return value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Byte" /> to <see cref="_Decimal" />.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        public static implicit operator _Decimal(_Byte x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromByteCast(x).MakeColumn<_Decimal>();
        }

        private sealed class FromInt16Cast : CastExpression<Int16?, Decimal?>
        {
            public FromInt16Cast(Column<Int16?> x)
                : base(x)
            {
            }

            protected override Decimal? Cast(Int16? value)
            {
                return value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int16" /> to <see cref="_Decimal" />.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        public static implicit operator _Decimal(_Int16 x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromInt16Cast(x).MakeColumn<_Decimal>();
        }

        private sealed class FromInt64Cast : CastExpression<Int64?, Decimal?>
        {
            public FromInt64Cast(Column<Int64?> x)
                : base(x)
            {
            }

            protected override Decimal? Cast(Int64? value)
            {
                return (Decimal?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int64" /> to <see cref="_Decimal" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        public static implicit operator _Decimal(_Int64 x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromInt64Cast(x).MakeColumn<_Decimal>();
        }

        private sealed class FromInt32Cast : CastExpression<Int32?, Decimal?>
        {
            public FromInt32Cast(Column<Int32?> x)
                : base(x)
            {
            }

            protected override Decimal? Cast(Int32? value)
            {
                return (Decimal?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int32" /> to <see cref="_Decimal" />.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        public static implicit operator _Decimal(_Int32 x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromInt32Cast(x).MakeColumn<_Decimal>();
        }

        private sealed class FromDoubleCast : CastExpression<Double?, Decimal?>
        {
            public FromDoubleCast(Column<Double?> x)
                : base(x)
            {
            }

            protected override Decimal? Cast(Double? value)
            {
                return (Decimal?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Double" /> to <see cref="_Decimal" />.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        public static implicit operator _Decimal(_Double x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromDoubleCast(x).MakeColumn<_Decimal>();
        }

        private sealed class FromSingleCast : CastExpression<Single?, Decimal?>
        {
            public FromSingleCast(Column<Single?> x)
                : base(x)
            {
            }

            protected override Decimal? Cast(Single? value)
            {
                return (Decimal?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Single" /> to <see cref="_Decimal" />.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        public static implicit operator _Decimal(_Single x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromSingleCast(x).MakeColumn<_Decimal>();
        }

        private sealed class FromStringCast : CastExpression<String, Decimal?>
        {
            public FromStringCast(Column<string> x)
                : base(x)
            {
            }

            protected override Decimal? Cast(String value)
            {
                if (value == null)
                    return null;
                return Decimal.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_Decimal" />.</summary>
        /// <returns>A <see cref="_Decimal" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _Decimal(_String x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromStringCast(x).MakeColumn<_Decimal>();
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
