using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Data.Common;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Single"/> column.
    /// </summary>
    [ColumnConverter(typeof(Converter))]
    public sealed class _Single : Column<Single?>, IColumn<DbReader, Single?>
    {
        private sealed class Converter : ConverterBase<_Single>
        {
        }

        [ExpressionConverterNonGenerics(typeof(CastToStringExpression.Converter), Id = "_Single.CastToString")]
        private sealed class CastToStringExpression : CastExpression<Single?, String>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<float?, string> MakeExpression(Column<float?> operand)
                {
                    return new CastToStringExpression(operand);
                }
            }

            public CastToStringExpression(Column<Single?> x)
                : base(x)
            {
            }

            protected override String Cast(Single? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : null;
            }
        }

        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        /// <summary>Converts the supplied <see cref="_Single" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        public static explicit operator _String(_Single x)
        {
            Check.NotNull(x, nameof(x));
            return x.CastToString();
        }

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

        /// <inheritdoc cref="P:DevZest.Data._Binary.Item(DevZest.Data.DbReader)"/>
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

        /// <inheritdoc cref="_Binary.Param(Binary, _Binary)"/>
        public static _Single Param(Single? x, _Single sourceColumn = null)
        {
            return new ParamExpression<Single?>(x, sourceColumn).MakeColumn<_Single>();
        }

        /// <inheritdoc cref="_Binary.Const(Binary)"/>
        public static _Single Const(Single? x)
        {
            return new ConstantExpression<Single?>(x).MakeColumn<_Single>();
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        public static implicit operator _Single(Single? x)
        {
            return Param(x);
        }

        [ExpressionConverterNonGenerics(typeof(NegateExpression.Converter), Id = "_Single.Negate")]
        private sealed class NegateExpression : UnaryExpression<Single?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override UnaryExpression<float?> MakeExpression(Column<float?> operand)
                {
                    return new NegateExpression(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new NegateExpression(x).MakeColumn<_Single>();
        }

        [ExpressionConverterNonGenerics(typeof(AddExpression.Converter), Id = "_Single.Add")]
        private sealed class AddExpression : BinaryExpression<Single?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<float?, float?> MakeExpression(Column<float?> left, Column<float?> right)
                {
                    return new AddExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new AddExpression(x, y).MakeColumn<_Single>();
        }

        [ExpressionConverterNonGenerics(typeof(SubstractExpression.Converter), Id = "_Single.Substract")]
        private sealed class SubstractExpression : BinaryExpression<Single?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<float?, float?> MakeExpression(Column<float?> left, Column<float?> right)
                {
                    return new SubstractExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new SubstractExpression(x, y).MakeColumn<_Single>();
        }

        [ExpressionConverterNonGenerics(typeof(MultiplyExpression.Converter), Id = "_Single.Multiply")]
        private sealed class MultiplyExpression : BinaryExpression<Single?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<float?, float?> MakeExpression(Column<float?> left, Column<float?> right)
                {
                    return new MultiplyExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new MultiplyExpression(x, y).MakeColumn<_Single>();
        }

        [ExpressionConverterNonGenerics(typeof(DivideExpression.Converter), Id = "_Single.Divide")]
        private sealed class DivideExpression : BinaryExpression<Single?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<float?, float?> MakeExpression(Column<float?> left, Column<float?> right)
                {
                    return new DivideExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new DivideExpression(x, y).MakeColumn<_Single>();
        }

        [ExpressionConverterNonGenerics(typeof(ModuloExpression.Converter), Id = "_Single.Modulo")]
        private sealed class ModuloExpression : BinaryExpression<Single?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<float?, float?> MakeExpression(Column<float?> left, Column<float?> right)
                {
                    return new ModuloExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new ModuloExpression(x, y).MakeColumn<_Single>();
        }

        [ExpressionConverterNonGenerics(typeof(LessThanExpression.Converter), Id = "_Single.LessThan")]
        private sealed class LessThanExpression : BinaryExpression<Single?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<float?, bool?> MakeExpression(Column<float?> left, Column<float?> right)
                {
                    return new LessThanExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(LessThanOrEqualExpression.Converter), Id = "_Single.LessThanOrEqual")]
        private sealed class LessThanOrEqualExpression : BinaryExpression<Single?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<float?, bool?> MakeExpression(Column<float?> left, Column<float?> right)
                {
                    return new LessThanOrEqualExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(GreaterThanExpression.Converter), Id = "_Single.GreaterThan")]
        private sealed class GreaterThanExpression : BinaryExpression<Single?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<float?, bool?> MakeExpression(Column<float?> left, Column<float?> right)
                {
                    return new GreaterThanExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(GreaterThanOrEqualExpression.Converter), Id = "_Single.GreaterThanOrEqual")]
        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Single?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<float?, bool?> MakeExpression(Column<float?> left, Column<float?> right)
                {
                    return new GreaterThanOrEqualExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(EqualExpression.Converter), Id = "_Single.Equal")]
        private sealed class EqualExpression : BinaryExpression<Single?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<float?, bool?> MakeExpression(Column<float?> left, Column<float?> right)
                {
                    return new EqualExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(NotEqualExpression.Converter), Id = "_Single.NotEqual")]
        private sealed class NotEqualExpression : BinaryExpression<Single?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<float?, bool?> MakeExpression(Column<float?> left, Column<float?> right)
                {
                    return new NotEqualExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(FromBooleanCast.Converter), Id = "_Single.FromBoolean")]
        private sealed class FromBooleanCast : CastExpression<bool?, Single?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<bool?, float?> MakeExpression(Column<bool?> operand)
                {
                    return new FromBooleanCast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromBooleanCast(x).MakeColumn<_Single>();
        }

        [ExpressionConverterNonGenerics(typeof(FromByteCast.Converter), Id = "_Single.FromByte")]
        private sealed class FromByteCast : CastExpression<byte?, Single?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<byte?, float?> MakeExpression(Column<byte?> operand)
                {
                    return new FromByteCast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromByteCast(x).MakeColumn<_Single>();
        }

        [ExpressionConverterNonGenerics(typeof(FromInt16Cast.Converter), Id = "_Single.FromInt16")]
        private sealed class FromInt16Cast : CastExpression<Int16?, Single?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<short?, float?> MakeExpression(Column<short?> operand)
                {
                    return new FromInt16Cast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromInt16Cast(x).MakeColumn<_Single>();
        }

        [ExpressionConverterNonGenerics(typeof(FromInt64Cast.Converter), Id = "_Single.FromInt64")]
        private sealed class FromInt64Cast : CastExpression<Int64?, Single?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<long?, float?> MakeExpression(Column<long?> operand)
                {
                    return new FromInt64Cast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromInt64Cast(x).MakeColumn<_Single>();
        }

        [ExpressionConverterNonGenerics(typeof(FromInt32Cast.Converter), Id = "_Single.FromInt32")]
        private sealed class FromInt32Cast : CastExpression<Int32?, Single?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<int?, float?> MakeExpression(Column<int?> operand)
                {
                    return new FromInt32Cast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromInt32Cast(x).MakeColumn<_Single>();
        }

        [ExpressionConverterNonGenerics(typeof(FromDecimalCast.Converter), Id = "_Single.FromDecimal")]
        private sealed class FromDecimalCast : CastExpression<Decimal?, Single?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<decimal?, float?> MakeExpression(Column<decimal?> operand)
                {
                    return new FromDecimalCast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromDecimalCast(x).MakeColumn<_Single>();
        }

        [ExpressionConverterNonGenerics(typeof(FromDoubleCast.Converter), Id = "_Single.FromDouble")]
        private sealed class FromDoubleCast : CastExpression<Double?, Single?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<double?, float?> MakeExpression(Column<double?> operand)
                {
                    return new FromDoubleCast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromDoubleCast(x).MakeColumn<_Single>();
        }

        [ExpressionConverterNonGenerics(typeof(FromStringCast.Converter), Id = "_Single.FromString")]
        private sealed class FromStringCast : CastExpression<String, Single?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<string, float?> MakeExpression(Column<string> operand)
                {
                    return new FromStringCast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
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
