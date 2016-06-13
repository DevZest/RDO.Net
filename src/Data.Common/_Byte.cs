using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Byte"/> column.
    /// </summary>
    [ColumnConverter(typeof(Converter))]
    public sealed class _Byte : Column<Byte?>, IColumn<DbReader, Byte?>
    {
        private sealed class Converter : ConverterBase<_Byte>
        {
        }

        /// <inheritdoc/>
        protected sealed override Column<byte?> CreateParam(byte? value)
        {
            return Param(value, this);
        }

        protected override bool AreEqual(byte? x, byte? y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected internal sealed override Column<byte?> CreateConst(byte? value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(Byte? value)
        {
            return JsonValue.Number(value);
        }

        /// <inheritdoc/>
        protected internal override Byte? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Byte?(Convert.ToByte(value.Text));
        }

        /// <inheritdoc cref="P:DevZest.Data._Binary.Item(DevZest.Data.DbReader)"/>
        public Byte? this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private Byte? GetValue(DbReader reader)
        {
            return reader.GetByte(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(byte? value)
        {
            return !value.HasValue;
        }

        /// <inheritdoc cref="_Binary.Param(Binary, _Binary)"/>
        public static _Byte Param(byte? x, _Byte sourceColumn = null)
        {
            return new ParamExpression<byte?>(x, sourceColumn).MakeColumn<_Byte>();
        }

        /// <inheritdoc cref="_Binary.Const(Binary)"/>
        public static _Byte Const(byte? x)
        {
            return new ConstantExpression<byte?>(x).MakeColumn<_Byte>();
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        public static implicit operator _Byte(byte? x)
        {
            return Param(x);
        }

        private sealed class OnesComplementExpression : UnaryExpression<Byte?>
        {
            public OnesComplementExpression(_Byte x)
                : base(x)
            {
            }

            protected override Byte? EvalCore(Byte? x)
            {
                return (Byte?)(~x);
            }

            protected override DbUnaryExpressionKind ExpressionKind
            {
                get { return DbUnaryExpressionKind.OnesComplement; }
            }
        }

        /// <summary>Performs a bitwise one's complement operation on the specified <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Byte" /> expression that contains the results of the one's complement operation.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        public static _Byte operator ~(_Byte x)
        {
            Check.NotNull(x, nameof(x));
            return new OnesComplementExpression(x).MakeColumn<_Byte>();
        }

        [ExpressionConverterNonGenerics(typeof(AddExpression.Converter), TypeId = "_Byte.Add")]
        private sealed class AddExpression : BinaryExpression<Byte?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<byte?, byte?> MakeExpression(Column<byte?> left, Column<byte?> right)
                {
                    return new AddExpression(left, right);
                }
            }

            public AddExpression(Column<Byte?> x, Column<Byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Add; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x + y);
            }
        }

        /// <summary>Computes the sum of the two specified <see cref="_Byte" /> objects.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator +(_Byte x, _Byte y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new AddExpression(x, y).MakeColumn<_Byte>();
        }

        private sealed class SubstractExpression : BinaryExpression<Byte?>
        {
            public SubstractExpression(_Byte x, _Byte y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Substract; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x - y);
            }
        }

        /// <summary>Substracts the two specified <see cref="_Byte" /> objects.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator -(_Byte x, _Byte y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new SubstractExpression(x, y).MakeColumn<_Byte>();
        }

        private sealed class MultiplyExpression : BinaryExpression<Byte?>
        {
            public MultiplyExpression(_Byte x, _Byte y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Multiply; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x * y);
            }
        }

        /// <summary>Multiplies the two specified <see cref="_Byte" /> objects.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator *(_Byte x, _Byte y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new MultiplyExpression(x, y).MakeColumn<_Byte>();
        }

        [ExpressionConverterNonGenerics(typeof(DivideExpression.Converter), TypeId = "_Byte.Divide")]
        private sealed class DivideExpression : BinaryExpression<Byte?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<byte?, byte?> MakeExpression(Column<byte?> left, Column<byte?> right)
                {
                    return new DivideExpression(left, right);
                }
            }

            public DivideExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Divide; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x / y);
            }
        }

        /// <summary>Divides the two specified <see cref="_Byte" /> objects.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator /(_Byte x, _Byte y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new DivideExpression(x, y).MakeColumn<_Byte>();
        }

        private sealed class ModuloExpression : BinaryExpression<Byte?>
        {
            public ModuloExpression(_Byte x, _Byte y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Modulo; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x % y);
            }
        }

        /// <summary>Computes the remainder after dividing the first <see cref="_Byte" /> parameter by the second.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator %(_Byte x, _Byte y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new ModuloExpression(x, y).MakeColumn<_Byte>();
        }

        [ExpressionConverterNonGenerics(typeof(BitwiseAndExpression.Converter), TypeId = "_Byte.BitwiseAnd")]
        private sealed class BitwiseAndExpression : BinaryExpression<Byte?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<byte?, byte?> MakeExpression(Column<byte?> left, Column<byte?> right)
                {
                    return new BitwiseAndExpression(left, right);
                }
            }

            public BitwiseAndExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseAnd; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x & y);
            }
        }

        /// <summary>Computes the bitwise AND of its <see cref="_Byte" /> operands.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator &(_Byte x, _Byte y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseAndExpression(x, y).MakeColumn<_Byte>();
        }

        [ExpressionConverterNonGenerics(typeof(BitwiseOrExpression.Converter), TypeId = "_Byte.BitwiseOr")]
        private sealed class BitwiseOrExpression : BinaryExpression<Byte?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<byte?, byte?> MakeExpression(Column<byte?> left, Column<byte?> right)
                {
                    return new BitwiseOrExpression(left, right);
                }
            }

            public BitwiseOrExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseOr; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x | y);
            }
        }

        /// <summary>Computes the bitwise OR of its <see cref="_Byte" /> operands.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator |(_Byte x, _Byte y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseOrExpression(x, y).MakeColumn<_Byte>();
        }

        [ExpressionConverterNonGenerics(typeof(BitwiseXorExpression.Converter), TypeId = "_Byte.BitwiseXor")]
        private sealed class BitwiseXorExpression : BinaryExpression<Byte?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<byte?, byte?> MakeExpression(Column<byte?> left, Column<byte?> right)
                {
                    return new BitwiseXorExpression(left, right);
                }
            }

            public BitwiseXorExpression(Column<byte?> x, Column<byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseXor; }
            }

            protected override Byte? EvalCore(Byte? x, Byte? y)
            {
                return (Byte?)(x ^ y);
            }
        }

        /// <summary>Computes the bitwise exclusive-OR of its <see cref="_Byte" /> operands.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Byte operator ^(_Byte x, _Byte y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseXorExpression(x, y).MakeColumn<_Byte>();
        }

        private sealed class LessThanExpression : BinaryExpression<Byte?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<Byte?, bool?> MakeExpression(Column<byte?> left, Column<byte?> right)
                {
                    return new LessThanExpression((_Byte)left, (_Byte)right);
                }
            }

            public LessThanExpression(_Byte x, _Byte y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(Byte? x, Byte? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() < y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Byte" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Boolean operator <(_Byte x, _Byte y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<Byte?, bool?>
        {
            public LessThanOrEqualExpression(_Byte x, _Byte y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(Byte? x, Byte? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() <= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Byte" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Boolean operator <=(_Byte x, _Byte y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<Byte?, bool?>
        {
            public GreaterThanExpression(_Byte x, _Byte y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(Byte? x, Byte? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() > y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Byte" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Boolean operator >(_Byte x, _Byte y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Byte?, bool?>
        {
            public GreaterThanOrEqualExpression(_Byte x, _Byte y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(Byte? x, Byte? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() >= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Byte" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Boolean operator >=(_Byte x, _Byte y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<Byte?, bool?>
        {
            public EqualExpression(Column<Byte?> x, Column<Byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(Byte? x, Byte? y)
            {
                return x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Byte" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Boolean operator ==(_Byte x, _Byte y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<Byte?, bool?>
        {
            public NotEqualExpression(Column<Byte?> x, Column<Byte?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(Byte? x, Byte? y)
            {
                return !x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Byte" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <param name="y">A <see cref="_Byte" /> object. </param>
        public static _Boolean operator !=(_Byte x, _Byte y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class FromBooleanCast : CastExpression<bool?, Byte?>
        {
            public FromBooleanCast(_Boolean x)
                : base(x)
            {
            }

            protected override Byte? Cast(bool? value)
            {
                if (value.HasValue)
                    return (Byte?)(value.GetValueOrDefault() ? 1 : 0);
                return null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Boolean" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Boolean" /> object. </param>
        public static explicit operator _Byte(_Boolean x)
        {
            Check.NotNull(x, nameof(x));
            return new FromBooleanCast(x).MakeColumn<_Byte>();
        }

        private sealed class FromInt16Cast : CastExpression<Int16?, Byte?>
        {
            public FromInt16Cast(_Int16 x)
                : base(x)
            {
            }

            protected override Byte? Cast(Int16? value)
            {
                return (Byte?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int16" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        public static explicit operator _Byte(_Int16 x)
        {
            Check.NotNull(x, nameof(x));
            return new FromInt16Cast(x).MakeColumn<_Byte>();
        }

        private sealed class FromInt32Cast : CastExpression<Int32?, Byte?>
        {
            public FromInt32Cast(_Int32 x)
                : base(x)
            {
            }

            protected override Byte? Cast(Int32? value)
            {
                return (Byte?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int32" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        public static explicit operator _Byte(_Int32 x)
        {
            Check.NotNull(x, nameof(x));
            return new FromInt32Cast(x).MakeColumn<_Byte>();
        }

        private sealed class FromInt64Cast : CastExpression<Int64?, Byte?>
        {
            public FromInt64Cast(_Int64 x)
                : base(x)
            {
            }

            protected override Byte? Cast(Int64? value)
            {
                return (Byte?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int64" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        public static explicit operator _Byte(_Int64 x)
        {
            Check.NotNull(x, nameof(x));
            return new FromInt64Cast(x).MakeColumn<_Byte>();
        }

        private sealed class FromDecimalCast : CastExpression<Decimal?, Byte?>
        {
            public FromDecimalCast(_Decimal x)
                : base(x)
            {
            }

            protected override Byte? Cast(Decimal? value)
            {
                return (Byte?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Decimal" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Decimal" /> object. </param>
        public static explicit operator _Byte(_Decimal x)
        {
            Check.NotNull(x, nameof(x));
            return new FromDecimalCast(x).MakeColumn<_Byte>();
        }

        private sealed class FromDoubleCast : CastExpression<Double?, Byte?>
        {
            public FromDoubleCast(_Double x)
                : base(x)
            {
            }

            protected override Byte? Cast(Double? value)
            {
                return (Byte?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Double" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        public static explicit operator _Byte(_Double x)
        {
            Check.NotNull(x, nameof(x));
            return new FromDoubleCast(x).MakeColumn<_Byte>();
        }

        private sealed class FromSingleCast : CastExpression<Single?, Byte?>
        {
            public FromSingleCast(_Single x)
                : base(x)
            {
            }

            protected override Byte? Cast(Single? value)
            {
                return (Byte?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Single" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        public static explicit operator _Byte(_Single x)
        {
            Check.NotNull(x, nameof(x));
            return new FromSingleCast(x).MakeColumn<_Byte>();
        }

        private sealed class FromStringCast : CastExpression<String, Byte?>
        {
            public FromStringCast(_String x)
                : base(x)
            {
            }

            protected override Byte? Cast(String value)
            {
                if (value == null)
                    return null;
                return Byte.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_Byte" />.</summary>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _Byte(_String x)
        {
            Check.NotNull(x, nameof(x));
            return new FromStringCast(x).MakeColumn<_Byte>();
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
