﻿using DevZest.Data.Primitives;
using System;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Single"/> column.
    /// </summary>
    public sealed class _Single : Column<Single?>, IColumn<DbReader, Single?>
    {
        private sealed class CastToStringExpression : CastExpression<Single?, String>
        {
            public CastToStringExpression(Column<Single?> x)
                : base(x)
            {
            }

            protected override String Cast(Single? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : null;
            }
        }

        /// <inheritdoc/>
        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        /// <summary>Converts the supplied <see cref="_Single" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        public static explicit operator _String(_Single x)
        {
            x.VerifyNotNull(nameof(x));
            return x.CastToString();
        }

        /// <inheritdoc/>
        public override bool AreEqual(float? x, float? y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected override Column<float?> CreateParam(float? value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected internal override Column<float?> CreateConst(float? value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(Single? value)
        {
            return JsonValue.Number(value);
        }

        /// <inheritdoc/>
        protected internal override Single? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Single?(Convert.ToSingle(value.Text));
        }

        /// <summary>Gets the value of this column from <see cref="DbReader"/>'s current row.</summary>
        /// <param name="reader">The <see cref="DbReader"/> object.</param>
        /// <returns>The value of this column from <see cref="DbReader"/>'s current row.</returns>
        public Single? this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private Single? GetValue(DbReader reader)
        {
            return reader.GetSingle(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(float? value)
        {
            return !value.HasValue;
        }

        /// <summary>Creates a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <param name="sourceColumn">The value which will be passed to <see cref="DbParamExpression.SourceColumn"/>.</param>
        /// <returns>The column of parameter expression.</returns>
        public static _Single Param(Single? x, _Single sourceColumn = null)
        {
            return new ParamExpression<Single?>(x, sourceColumn).MakeColumn<_Single>();
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="x">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        public static _Single Const(Single? x)
        {
            return new ConstantExpression<Single?>(x).MakeColumn<_Single>();
        }

        /// <summary>Implicitly converts the supplied value to a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <returns>The column of parameter expression.</returns>
        public static implicit operator _Single(Single? x)
        {
            return Param(x);
        }

        private sealed class NegateExpression : UnaryExpression<Single?>
        {
            public NegateExpression(Column<Single?> x)
                : base(x)
            {
            }

            protected override Single? EvalCore(Single? x)
            {
                return -x;
            }

            protected override DbUnaryExpressionKind ExpressionKind
            {
                get { return DbUnaryExpressionKind.Negate; }
            }
        }

        /// <summary>Negates the <see cref="_Single" /> operand.</summary>
        /// <returns>A <see cref="_Single" /> expression that contains the negated result.</returns>
        /// <param name="x">A <see cref="_Single" /> object.</param>
        public static _Single operator -(_Single x)
        {
            x.VerifyNotNull(nameof(x));
            return new NegateExpression(x).MakeColumn<_Single>();
        }

        private sealed class AddExpression : BinaryExpression<Single?>
        {
            public AddExpression(Column<Single?> x, Column<Single?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Add; }
            }

            protected override Single? EvalCore(Single? x, Single? y)
            {
                return x + y;
            }
        }

        /// <summary>Computes the sum of the two specified <see cref="_Single" /> objects.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        /// <param name="y">A <see cref="_Single" /> object. </param>
        public static _Single operator +(_Single x, _Single y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new AddExpression(x, y).MakeColumn<_Single>();
        }

        private sealed class SubstractExpression : BinaryExpression<Single?>
        {
            public SubstractExpression(Column<Single?> x, Column<Single?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Substract; }
            }

            protected override Single? EvalCore(Single? x, Single? y)
            {
                return x - y;
            }
        }

        /// <summary>Substracts the two specified <see cref="_Single" /> objects.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        /// <param name="y">A <see cref="_Single" /> object. </param>
        public static _Single operator -(_Single x, _Single y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new SubstractExpression(x, y).MakeColumn<_Single>();
        }

        private sealed class MultiplyExpression : BinaryExpression<Single?>
        {
            public MultiplyExpression(Column<Single?> x, Column<Single?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Multiply; }
            }

            protected override Single? EvalCore(Single? x, Single? y)
            {
                return x * y;
            }
        }

        /// <summary>Multiplies the two specified <see cref="_Single" /> objects.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        /// <param name="y">A <see cref="_Single" /> object. </param>
        public static _Single operator *(_Single x, _Single y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new MultiplyExpression(x, y).MakeColumn<_Single>();
        }

        private sealed class DivideExpression : BinaryExpression<Single?>
        {
            public DivideExpression(Column<Single?> x, Column<Single?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Divide; }
            }

            protected override Single? EvalCore(Single? x, Single? y)
            {
                return x / y;
            }
        }

        /// <summary>Divides the two specified <see cref="_Single" /> objects.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        /// <param name="y">A <see cref="_Single" /> object. </param>
        public static _Single operator /(_Single x, _Single y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new DivideExpression(x, y).MakeColumn<_Single>();
        }

        private sealed class ModuloExpression : BinaryExpression<Single?>
        {
            public ModuloExpression(Column<Single?> x, Column<Single?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Modulo; }
            }

            protected override Single? EvalCore(Single? x, Single? y)
            {
                return x % y;
            }
        }

        /// <summary>Computes the remainder after dividing the first <see cref="_Single" /> parameter by the second.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        /// <param name="y">A <see cref="_Single" /> object. </param>
        public static _Single operator %(_Single x, _Single y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new ModuloExpression(x, y).MakeColumn<_Single>();
        }

        private sealed class LessThanExpression : BinaryExpression<Single?, bool?>
        {
            public LessThanExpression(Column<Single?> x, Column<Single?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(Single? x, Single? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() < y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Single" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        /// <param name="y">A <see cref="_Single" /> object. </param>
        public static _Boolean operator <(_Single x, _Single y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<Single?, bool?>
        {
            public LessThanOrEqualExpression(Column<Single?> x, Column<Single?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(Single? x, Single? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() <= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Single" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        /// <param name="y">A <see cref="_Single" /> object. </param>
        public static _Boolean operator <=(_Single x, _Single y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<Single?, bool?>
        {
            public GreaterThanExpression(Column<Single?> x, Column<Single?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(Single? x, Single? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() > y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Single" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        /// <param name="y">A <see cref="_Single" /> object. </param>
        public static _Boolean operator >(_Single x, _Single y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Single?, bool?>
        {
            public GreaterThanOrEqualExpression(Column<Single?> x, Column<Single?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(Single? x, Single? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() >= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Single" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        /// <param name="y">A <see cref="_Single" /> object. </param>
        public static _Boolean operator >=(_Single x, _Single y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<Single?, bool?>
        {
            public EqualExpression(Column<Single?> x, Column<Single?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(Single? x, Single? y)
            {
                return x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Single" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        /// <param name="y">A <see cref="_Single" /> object. </param>
        public static _Boolean operator ==(_Single x, _Single y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<Single?, bool?>
        {
            public NotEqualExpression(Column<Single?> x, Column<Single?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(Single? x, Single? y)
            {
                return !x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Single" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        /// <param name="y">A <see cref="_Single" /> object. </param>
        public static _Boolean operator !=(_Single x, _Single y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class FromBooleanCast : CastExpression<bool?, Single?>
        {
            public FromBooleanCast(Column<bool?> x)
                : base(x)
            {
            }

            protected override Single? Cast(bool? value)
            {
                if (value.HasValue)
                    return value.GetValueOrDefault() ? 1 : 0;
                return null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Boolean" /> to <see cref="_Single" />.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Boolean" /> object. </param>
        public static explicit operator _Single(_Boolean x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromBooleanCast(x).MakeColumn<_Single>();
        }

        private sealed class FromByteCast : CastExpression<byte?, Single?>
        {
            public FromByteCast(Column<Byte?> x)
                : base(x)
            {
            }

            protected override Single? Cast(byte? value)
            {
                return value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Byte" /> to <see cref="_Single" />.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        public static explicit operator _Single(_Byte x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromByteCast(x).MakeColumn<_Single>();
        }

        private sealed class FromInt16Cast : CastExpression<Int16?, Single?>
        {
            public FromInt16Cast(Column<Int16?> x)
                : base(x)
            {
            }

            protected override Single? Cast(Int16? value)
            {
                return value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int16" /> to <see cref="_Single" />.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        public static explicit operator _Single(_Int16 x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromInt16Cast(x).MakeColumn<_Single>();
        }

        private sealed class FromInt64Cast : CastExpression<Int64?, Single?>
        {
            public FromInt64Cast(Column<Int64?> x)
                : base(x)
            {
            }

            protected override Single? Cast(Int64? value)
            {
                return (Single?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int64" /> to <see cref="_Single" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        public static explicit operator _Single(_Int64 x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromInt64Cast(x).MakeColumn<_Single>();
        }

        private sealed class FromInt32Cast : CastExpression<Int32?, Single?>
        {
            public FromInt32Cast(Column<Int32?> x)
                : base(x)
            {
            }

            protected override Single? Cast(Int32? value)
            {
                return (Single?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int32" /> to <see cref="_Single" />.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        public static explicit operator _Single(_Int32 x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromInt32Cast(x).MakeColumn<_Single>();
        }

        private sealed class FromDecimalCast : CastExpression<Decimal?, Single?>
        {
            public FromDecimalCast(Column<Decimal?> x)
                : base(x)
            {
            }

            protected override Single? Cast(Decimal? value)
            {
                return (Single?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Single" /> to <see cref="_Single" />.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        public static explicit operator _Single(_Decimal x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromDecimalCast(x).MakeColumn<_Single>();
        }

        private sealed class FromDoubleCast : CastExpression<Double?, Single?>
        {
            public FromDoubleCast(Column<Double?> x)
                : base(x)
            {
            }

            protected override Single? Cast(Double? value)
            {
                return (Single?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Double" /> to <see cref="_Single" />.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        public static explicit operator _Single(_Double x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromDoubleCast(x).MakeColumn<_Single>();
        }

        private sealed class FromStringCast : CastExpression<String, Single?>
        {
            public FromStringCast(Column<String> x)
                : base(x)
            {
            }

            protected override Single? Cast(String value)
            {
                if (value == null)
                    return null;
                return Single.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_Single" />.</summary>
        /// <returns>A <see cref="_Single" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _Single(_String x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromStringCast(x).MakeColumn<_Single>();
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
