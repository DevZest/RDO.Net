using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Double"/> column.
    /// </summary>
    public sealed class _Double : Column<Double?>, IColumn<DbReader, Double?>
    {
        /// <inheritdoc/>
        protected override Column<double?> CreateParam(double? value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected internal sealed override Column<double?> CreateConst(double? value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(Double? value)
        {
            return JsonValue.Number(value);
        }

        /// <inheritdoc/>
        protected internal override Double? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Double?(Convert.ToDouble(value.Text));
        }

        /// <inheritdoc cref="P:DevZest.Data._Binary.Item(DevZest.Data.DbReader)"/>
        public Double? this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private Double? GetValue(DbReader reader)
        {
            return reader.GetDouble(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(double? value)
        {
            return !value.HasValue;
        }

        /// <inheritdoc cref="_Binary.Param(Binary, _Binary)"/>
        public static _Double Param(Double? x, _Double sourceColumn = null)
        {
            return new ParamExpression<Double?>(x, sourceColumn).MakeColumn<_Double>();
        }

        /// <inheritdoc cref="_Binary.Const(Binary)"/>
        public static _Double Const(Double? x)
        {
            return new ConstantExpression<Double?>(x).MakeColumn<_Double>();
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        public static implicit operator _Double(Double? x)
        {
            return Param(x);
        }

        private sealed class NegateExpression : ColumnUnaryExpression<Double?>
        {
            public NegateExpression(_Double x)
                : base(x)
            {
            }

            protected override Double? EvalCore(Double? x)
            {
                return -x;
            }

            protected override DbUnaryExpressionKind ExpressionKind
            {
                get { return DbUnaryExpressionKind.Negate; }
            }
        }

        /// <summary>Negates the <see cref="_Double" /> operand.</summary>
        /// <returns>A <see cref="_Double" /> expression that contains the negated result.</returns>
        /// <param name="x">A <see cref="_Double" /> object.</param>
        public static _Double operator -(_Double x)
        {
            Check.NotNull(x, nameof(x));
            return new NegateExpression(x).MakeColumn<_Double>();
        }

        private sealed class AddExpression : BinaryExpression<Double?>
        {
            public AddExpression(_Double x, _Double y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Add; }
            }

            protected override Double? EvalCore(Double? x, Double? y)
            {
                return x + y;
            }
        }

        /// <summary>Computes the sum of the two specified <see cref="_Double" /> objects.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        /// <param name="y">A <see cref="_Double" /> object. </param>
        public static _Double operator +(_Double x, _Double y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new AddExpression(x, y).MakeColumn<_Double>();
        }

        private sealed class SubstractExpression : BinaryExpression<Double?>
        {
            public SubstractExpression(_Double x, _Double y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Substract; }
            }

            protected override Double? EvalCore(Double? x, Double? y)
            {
                return x - y;
            }
        }

        /// <summary>Substracts the two specified <see cref="_Double" /> objects.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        /// <param name="y">A <see cref="_Double" /> object. </param>
        public static _Double operator -(_Double x, _Double y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new SubstractExpression(x, y).MakeColumn<_Double>();
        }

        private sealed class MultiplyExpression : BinaryExpression<Double?>
        {
            public MultiplyExpression(_Double x, _Double y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Multiply; }
            }

            protected override Double? EvalCore(Double? x, Double? y)
            {
                return x * y;
            }
        }

        /// <summary>Multiplies the two specified <see cref="_Double" /> objects.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        /// <param name="y">A <see cref="_Double" /> object. </param>
        public static _Double operator *(_Double x, _Double y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new MultiplyExpression(x, y).MakeColumn<_Double>();
        }

        private sealed class DivideExpression : BinaryExpression<Double?>
        {
            public DivideExpression(_Double x, _Double y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Divide; }
            }

            protected override Double? EvalCore(Double? x, Double? y)
            {
                return x / y;
            }
        }

        /// <summary>Divides the two specified <see cref="_Double" /> objects.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        /// <param name="y">A <see cref="_Double" /> object. </param>
        public static _Double operator /(_Double x, _Double y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new DivideExpression(x, y).MakeColumn<_Double>();
        }

        private sealed class ModuloExpression : BinaryExpression<Double?>
        {
            public ModuloExpression(_Double x, _Double y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Modulo; }
            }

            protected override Double? EvalCore(Double? x, Double? y)
            {
                return x % y;
            }
        }

        /// <summary>Computes the remainder after dividing the first <see cref="_Double" /> parameter by the second.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        /// <param name="y">A <see cref="_Double" /> object. </param>
        public static _Double operator %(_Double x, _Double y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new ModuloExpression(x, y).MakeColumn<_Double>();
        }

        private sealed class LessThanExpression : BinaryExpression<Double?, bool?>
        {
            public LessThanExpression(_Double x, _Double y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(Double? x, Double? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() < y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Double" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        /// <param name="y">A <see cref="_Double" /> object. </param>
        public static _Boolean operator <(_Double x, _Double y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<Double?, bool?>
        {
            public LessThanOrEqualExpression(_Double x, _Double y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(Double? x, Double? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() <= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Double" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        /// <param name="y">A <see cref="_Double" /> object. </param>
        public static _Boolean operator <=(_Double x, _Double y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<Double?, bool?>
        {
            public GreaterThanExpression(_Double x, _Double y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(Double? x, Double? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() > y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Double" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        /// <param name="y">A <see cref="_Double" /> object. </param>
        public static _Boolean operator >(_Double x, _Double y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Double?, bool?>
        {
            public GreaterThanOrEqualExpression(_Double x, _Double y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(Double? x, Double? y)
            {
                if (x.HasValue && y.HasValue)
                    return x.GetValueOrDefault() >= y.GetValueOrDefault();
                return null;
            }
        }

        /// <summary>Compares the two <see cref="_Double" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        /// <param name="y">A <see cref="_Double" /> object. </param>
        public static _Boolean operator >=(_Double x, _Double y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<Double?, bool?>
        {
            public EqualExpression(Column<Double?> x, Column<Double?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(Double? x, Double? y)
            {
                return x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Double" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        /// <param name="y">A <see cref="_Double" /> object. </param>
        public static _Boolean operator ==(_Double x, _Double y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<Double?, bool?>
        {
            public NotEqualExpression(Column<Double?> x, Column<Double?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(Double? x, Double? y)
            {
                return !x.EqualsTo(y);
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Double" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        /// <param name="y">A <see cref="_Double" /> object. </param>
        public static _Boolean operator !=(_Double x, _Double y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class DbBooleanCast : CastExpression<bool?, Double?>
        {
            public DbBooleanCast(_Boolean x)
                : base(x)
            {
            }

            protected override Double? Cast(bool? value)
            {
                if (value.HasValue)
                    return value.GetValueOrDefault() ? 1 : 0;
                return null;
            }
        }

        /// <summary>Converts the supplied <see cref="_Boolean" /> to <see cref="_Double" />.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Boolean" /> object. </param>
        public static explicit operator _Double(_Boolean x)
        {
            Check.NotNull(x, nameof(x));
            return new DbBooleanCast(x).MakeColumn<_Double>();
        }

        private sealed class DbByteCast : CastExpression<byte?, Double?>
        {
            public DbByteCast(_Byte x)
                : base(x)
            {
            }

            protected override Double? Cast(byte? value)
            {
                return value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Byte" /> to <see cref="_Double" />.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        public static explicit operator _Double(_Byte x)
        {
            Check.NotNull(x, nameof(x));
            return new DbByteCast(x).MakeColumn<_Double>();
        }

        private sealed class DbInt16Cast : CastExpression<Int16?, Double?>
        {
            public DbInt16Cast(_Int16 x)
                : base(x)
            {
            }

            protected override Double? Cast(Int16? value)
            {
                return value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int16" /> to <see cref="_Double" />.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        public static explicit operator _Double(_Int16 x)
        {
            Check.NotNull(x, nameof(x));
            return new DbInt16Cast(x).MakeColumn<_Double>();
        }

        private sealed class DbInt64Cast : CastExpression<Int64?, Double?>
        {
            public DbInt64Cast(_Int64 x)
                : base(x)
            {
            }

            protected override Double? Cast(Int64? value)
            {
                return (Double?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int64" /> to <see cref="_Double" />.</summary>
        /// <returns>A <see cref="_Int64" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int64" /> object. </param>
        public static explicit operator _Double(_Int64 x)
        {
            Check.NotNull(x, nameof(x));
            return new DbInt64Cast(x).MakeColumn<_Double>();
        }

        private sealed class DbInt32Cast : CastExpression<Int32?, Double?>
        {
            public DbInt32Cast(_Int32 x)
                : base(x)
            {
            }

            protected override Double? Cast(Int32? value)
            {
                return (Double?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Int32" /> to <see cref="_Double" />.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Int32" /> object. </param>
        public static explicit operator _Double(_Int32 x)
        {
            Check.NotNull(x, nameof(x));
            return new DbInt32Cast(x).MakeColumn<_Double>();
        }

        private sealed class DbDecimalCast : CastExpression<Decimal?, Double?>
        {
            public DbDecimalCast(_Decimal x)
                : base(x)
            {
            }

            protected override Double? Cast(Decimal? value)
            {
                return (Double?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Double" /> to <see cref="_Double" />.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Double" /> object. </param>
        public static explicit operator _Double(_Decimal x)
        {
            Check.NotNull(x, nameof(x));
            return new DbDecimalCast(x).MakeColumn<_Double>();
        }

        private sealed class DbSingleCast : CastExpression<Single?, Double?>
        {
            public DbSingleCast(_Single x)
                : base(x)
            {
            }

            protected override Double? Cast(Single? value)
            {
                return (Double?)value;
            }
        }

        /// <summary>Converts the supplied <see cref="_Single" /> to <see cref="_Double" />.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Single" /> object. </param>
        public static explicit operator _Double(_Single x)
        {
            Check.NotNull(x, nameof(x));
            return new DbSingleCast(x).MakeColumn<_Double>();
        }

        private sealed class DbStringCast : CastExpression<String, Double?>
        {
            public DbStringCast(_String x)
                : base(x)
            {
            }

            protected override Double? Cast(String value)
            {
                if (value == null)
                    return null;
                return Double.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_Double" />.</summary>
        /// <returns>A <see cref="_Double" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _Double(_String x)
        {
            Check.NotNull(x, nameof(x));
            return new DbStringCast(x).MakeColumn<_Double>();
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
