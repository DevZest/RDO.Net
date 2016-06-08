using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Int32"/> Column.
    /// </summary>
    [ColumnConverter(typeof(Converter))]
    public sealed class _Int32 : Column<Int32?>, IColumn<DbReader, Int32?>
    {
        private sealed class Converter : ConverterBase<_Int32>
        {
        }

        protected override bool AreEqual(int? x, int? y)
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

        private sealed class NegateExpression : ColumnUnaryExpression<Int32?>
        {
            public NegateExpression(_Int32 x)
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

        private sealed class OnesComplementExpression : ColumnUnaryExpression<Int32?>
        {
            public OnesComplementExpression(_Int32 x)
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
            public AddExpression(_Int32 x, _Int32 y)
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
            public SubstractExpression(_Int32 x, _Int32 y)
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
            public MultiplyExpression(_Int32 x, _Int32 y)
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
            public DivideExpression(_Int32 x, _Int32 y)
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
            public ModuloExpression(_Int32 x, _Int32 y)
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
            public BitwiseAndExpression(_Int32 x, _Int32 y)
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
            public BitwiseOrExpression(_Int32 x, _Int32 y)
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
            public BitwiseXorExpression(_Int32 x, _Int32 y)
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
            public LessThanExpression(_Int32 x, _Int32 y)
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
            public LessThanOrEqualExpression(_Int32 x, _Int32 y)
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
            public GreaterThanExpression(_Int32 x, _Int32 y)
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
            public GreaterThanOrEqualExpression(_Int32 x, _Int32 y)
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

        private sealed class DbBooleanCast : CastExpression<bool?, Int32?>
        {
            public DbBooleanCast(_Boolean x)
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
            return new DbBooleanCast(x).MakeColumn<_Int32>();
        }

        private sealed class DbByteCast : CastExpression<byte?, Int32?>
        {
            public DbByteCast(_Byte x)
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
            return new DbByteCast(x).MakeColumn<_Int32>();
        }

        private sealed class DbInt16Cast : CastExpression<Int16?, Int32?>
        {
            public DbInt16Cast(_Int16 x)
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
            return new DbInt16Cast(x).MakeColumn<_Int32>();
        }

        private sealed class DbInt64Cast : CastExpression<Int64?, Int32?>
        {
            public DbInt64Cast(_Int64 x)
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
            return new DbInt64Cast(x).MakeColumn<_Int32>();
        }

        private sealed class DbDecimalCast : CastExpression<Decimal?, Int32?>
        {
            public DbDecimalCast(_Decimal x)
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
            return new DbDecimalCast(x).MakeColumn<_Int32>();
        }

        private sealed class DbDoubleCast : CastExpression<Double?, Int32?>
        {
            public DbDoubleCast(_Double x)
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
            return new DbDoubleCast(x).MakeColumn<_Int32>();
        }

        private sealed class DbSingleCast : CastExpression<Single?, Int32?>
        {
            public DbSingleCast(_Single x)
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
            return new DbSingleCast(x).MakeColumn<_Int32>();
        }

        private sealed class DbStringCast : CastExpression<String, Int32?>
        {
            public DbStringCast(_String x)
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
            return new DbStringCast(x).MakeColumn<_Int32>();
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
            AddOrUpdateInterceptor(identity);
            var model = ParentModel;
            if (model.ContainsInterceptor(identity.FullName))
                throw new InvalidOperationException(Strings.Model_MultipleIdentityColumn);
            model.AddInterceptor(identity);
            return identity;
        }

        bool _isIdentityInitialized;
        Identity _identity;
        int _currentIdentityValue;

        internal override int? GetDefaultValue(DataRow dataRow)
        {
            if (ParentModel.IsIdentityGenerationDisabled)
                return base.GetDefaultValue(dataRow);

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

            if (_identity != null)
                return _currentIdentityValue -= _identity.Increment;
            return base.GetDefaultValue(dataRow);
        }
    }
}
