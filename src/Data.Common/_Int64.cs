using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Int64"/> column.
    /// </summary>
    [ColumnConverter(typeof(Converter))]
    public sealed class _Int64 : Column<Int64?>, IColumn<DbReader, Int64?>
    {
        private sealed class Converter : ConverterBase<_Int64>
        {
        }

        protected override bool AreEqual(long? x, long? y)
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


        /// <inheritdoc cref="P:DevZest.Data._Binary.Item(DevZest.Data.DbReader)"/>
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

        /// <inheritdoc cref="_Binary.Param(Binary, _Binary)"/>
        public static _Int64 Param(Int64? x, _Int64 sourceColumn = null)
        {
            return new ParamExpression<Int64?>(x, sourceColumn).MakeColumn<_Int64>();
        }

        /// <inheritdoc cref="_Binary.Const(Binary)"/>
        public static _Int64 Const(Int64? x)
        {
            return new ConstantExpression<Int64?>(x).MakeColumn<_Int64>();
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        public static implicit operator _Int64(Int64? x)
        {
            return Param(x);
        }

        [ExpressionConverterNonGenerics(typeof(NegateExpression.Converter), TypeId = "_Int64.Negate")]
        private sealed class NegateExpression : UnaryExpression<Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override UnaryExpression<long?> MakeExpression(Column<long?> operand)
                {
                    return new NegateExpression(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new NegateExpression(x).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(OnesComplementExpression.Converter), TypeId = "_Int64.OnesComplement")]
        private sealed class OnesComplementExpression : UnaryExpression<Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override UnaryExpression<long?> MakeExpression(Column<long?> operand)
                {
                    return new OnesComplementExpression(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new OnesComplementExpression(x).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(AddExpression.Converter), TypeId = "_Int64.Add")]
        private sealed class AddExpression : BinaryExpression<Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<long?, long?> MakeExpression(Column<long?> left, Column<long?> right)
                {
                    return new AddExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new AddExpression(x, y).MakeColumn<_Int64>();
        }

        private sealed class SubstractExpression : BinaryExpression<Int64?>
        {
            public SubstractExpression(_Int64 x, _Int64 y)
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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new SubstractExpression(x, y).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(MultiplyExpression.Converter), TypeId = "_Int64.Multiply")]
        private sealed class MultiplyExpression : BinaryExpression<Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<long?, long?> MakeExpression(Column<long?> left, Column<long?> right)
                {
                    return new MultiplyExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new MultiplyExpression(x, y).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(DivideExpression.Converter), TypeId = "_Int64.Divide")]
        private sealed class DivideExpression : BinaryExpression<Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<long?, long?> MakeExpression(Column<long?> left, Column<long?> right)
                {
                    return new DivideExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new DivideExpression(x, y).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(ModuloExpression.Converter), TypeId = "_Int64.Modulo")]
        private sealed class ModuloExpression : BinaryExpression<Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<long?, long?> MakeExpression(Column<long?> left, Column<long?> right)
                {
                    return new ModuloExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new ModuloExpression(x, y).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(BitwiseAndExpression.Converter), TypeId = "_Int64.BitwiseAnd")]
        private sealed class BitwiseAndExpression : BinaryExpression<Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<long?, long?> MakeExpression(Column<long?> left, Column<long?> right)
                {
                    return new BitwiseAndExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseAndExpression(x, y).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(BitwiseOrExpression.Converter), TypeId = "_Int64.BitwiseOr")]
        private sealed class BitwiseOrExpression : BinaryExpression<Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<long?, long?> MakeExpression(Column<long?> left, Column<long?> right)
                {
                    return new BitwiseOrExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseOrExpression(x, y).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(BitwiseXorExpression.Converter), TypeId = "_Int64.BitwiseXor")]
        private sealed class BitwiseXorExpression : BinaryExpression<Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<long?, long?> MakeExpression(Column<long?> left, Column<long?> right)
                {
                    return new BitwiseXorExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseXorExpression(x, y).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(LessThanExpression.Converter), TypeId = "_Int64.LessThan")]
        private sealed class LessThanExpression : BinaryExpression<Int64?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<long?, bool?> MakeExpression(Column<long?> left, Column<long?> right)
                {
                    return new LessThanExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(LessThanOrEqualExpression.Converter), TypeId = "_Int64.LessThanOrEqual")]
        private sealed class LessThanOrEqualExpression : BinaryExpression<Int64?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<long?, bool?> MakeExpression(Column<long?> left, Column<long?> right)
                {
                    return new LessThanOrEqualExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(GreaterThanExpression.Converter), TypeId = "_Int64.GreaterThan")]
        private sealed class GreaterThanExpression : BinaryExpression<Int64?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<long?, bool?> MakeExpression(Column<long?> left, Column<long?> right)
                {
                    return new GreaterThanExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(GreaterThanOrEqualExpression.Converter), TypeId = "_Int64.GreaterThanOrEqual")]
        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Int64?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<long?, bool?> MakeExpression(Column<long?> left, Column<long?> right)
                {
                    return new GreaterThanOrEqualExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(EqualExpression.Converter), TypeId = "_Int64.Equal")]
        private sealed class EqualExpression : BinaryExpression<Int64?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<long?, bool?> MakeExpression(Column<long?> left, Column<long?> right)
                {
                    return new EqualExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(NotEqualExpression.Converter), TypeId = "_Int64.NotEqual")]
        private sealed class NotEqualExpression : BinaryExpression<Int64?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<long?, bool?> MakeExpression(Column<long?> left, Column<long?> right)
                {
                    return new NotEqualExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(FromBooleanCast.Converter), TypeId = "_Int64.FromBoolean")]
        private sealed class FromBooleanCast : CastExpression<bool?, Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<bool?, long?> MakeExpression(Column<bool?> operand)
                {
                    return new FromBooleanCast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromBooleanCast(x).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(FromByteCast.Converter), TypeId = "_Int64.FromByte")]
        private sealed class FromByteCast : CastExpression<byte?, Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<byte?, long?> MakeExpression(Column<byte?> operand)
                {
                    return new FromByteCast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromByteCast(x).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(FromInt16Cast.Converter), TypeId = "_Int64.FromInt16")]
        private sealed class FromInt16Cast : CastExpression<Int16?, Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<short?, long?> MakeExpression(Column<short?> operand)
                {
                    return new FromInt16Cast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromInt16Cast(x).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(FromInt32Cast.Converter), TypeId = "_Int64.FromInt32")]
        private sealed class FromInt32Cast : CastExpression<Int32?, Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<int?, long?> MakeExpression(Column<int?> operand)
                {
                    return new FromInt32Cast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromInt32Cast(x).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(FromDecimalCast.Converter), TypeId = "_Int64.FromDecimal")]
        private sealed class FromDecimalCast : CastExpression<Decimal?, Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<decimal?, long?> MakeExpression(Column<decimal?> operand)
                {
                    return new FromDecimalCast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromDecimalCast(x).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(FromDoubleCast.Converter), TypeId = "_Int64.FromDouble")]
        private sealed class FromDoubleCast : CastExpression<Double?, Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<double?, long?> MakeExpression(Column<double?> operand)
                {
                    return new FromDoubleCast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromDoubleCast(x).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(FromSingleCast.Converter), TypeId = "_Int64.FromSingle")]
        private sealed class FromSingleCast : CastExpression<Single?, Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<float?, long?> MakeExpression(Column<float?> operand)
                {
                    return new FromSingleCast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromSingleCast(x).MakeColumn<_Int64>();
        }

        [ExpressionConverterNonGenerics(typeof(FromStringCast.Converter), TypeId = "_Int64.FromString")]
        private sealed class FromStringCast : CastExpression<String, Int64?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<string, long?> MakeExpression(Column<string> operand)
                {
                    return new FromStringCast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
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
    }
}
