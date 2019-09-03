using DevZest.Data.Primitives;
using System;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="DateTime"/> column.
    /// </summary>
    public sealed class _DateTime : Column<DateTime?>, IColumn<DbReader, DateTime?>
    {
        private sealed class CastToStringExpression : CastExpression<DateTime?, String>
        {
            public CastToStringExpression(Column<DateTime?> x)
                : base(x)
            {
            }

            protected override String Cast(DateTime? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString("O", CultureInfo.InvariantCulture) : null;
            }
        }

        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        public override bool AreEqual(DateTime? x, DateTime? y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected sealed override Column<DateTime?> CreateParam(DateTime? value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected internal sealed override Column<DateTime?> CreateConst(DateTime? value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(DateTime? value)
        {
            return JsonValue.DateTime(value);
        }

        /// <inheritdoc/>
        protected internal override DateTime? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new DateTime?(DateTime.Parse(value.Text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        }

        /// <summary>Gets the value of this column from <see cref="DbReader"/>'s current row.</summary>
        /// <param name="reader">The <see cref="DbReader"/> object.</param>
        /// <returns>The value of this column from <see cref="DbReader"/>'s current row.</returns>
        public DateTime? this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private DateTime? GetValue(DbReader reader)
        {
            return reader.GetDateTime(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(DateTime? value)
        {
            return !value.HasValue;
        }

        /// <summary>Creates a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <param name="sourceColumn">The value which will be passed to <see cref="DbParamExpression.SourceColumn"/>.</param>
        /// <returns>The column of parameter expression.</returns>
        public static _DateTime Param(DateTime? x, _DateTime sourceColumn = null)
        {
            return new ParamExpression<DateTime?>(x, sourceColumn).MakeColumn<_DateTime>();
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="x">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        public static _DateTime Const(DateTime? x)
        {
            return new ConstantExpression<DateTime?>(x).MakeColumn<_DateTime>();
        }

        /// <summary>Implicitly converts the supplied value to a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <returns>The column of parameter expression.</returns>
        public static implicit operator _DateTime(DateTime? x)
        {
            return Param(x);
        }

        private sealed class FromStringCast : CastExpression<String, DateTime?>
        {
            public FromStringCast(Column<string> x)
                : base(x)
            {
            }

            protected override DateTime? Cast(String value)
            {
                if (value == null)
                    return null;
                return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_DateTime" />.</summary>
        /// <returns>A <see cref="_DateTime" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _DateTime(_String x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromStringCast(x).MakeColumn<_DateTime>();
        }

        private sealed class LessThanExpression : BinaryExpression<DateTime?, bool?>
        {
            public LessThanExpression(Column<DateTime?> x, Column<DateTime?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(DateTime? x, DateTime? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) < 0;
            }
        }

        /// <summary>Compares the two <see cref="_DateTime" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_DateTime" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_DateTime" /> object. </param>
        /// <param name="y">A <see cref="_DateTime" /> object. </param>
        public static _Boolean operator <(_DateTime x, _DateTime y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<DateTime?, bool?>
        {
            public LessThanOrEqualExpression(Column<DateTime?> x, Column<DateTime?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(DateTime? x, DateTime? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) <= 0;
            }
        }

        /// <summary>Compares the two <see cref="_DateTime" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_DateTime" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_DateTime" /> object. </param>
        /// <param name="y">A <see cref="_DateTime" /> object. </param>
        public static _Boolean operator <=(_DateTime x, _DateTime y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<DateTime?, bool?>
        {
            public GreaterThanExpression(Column<DateTime?> x, Column<DateTime?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(DateTime? x, DateTime? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) > 0;
            }
        }

        /// <summary>Compares the two <see cref="_DateTime" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_DateTime" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_DateTime" /> object. </param>
        /// <param name="y">A <see cref="_DateTime" /> object. </param>
        public static _Boolean operator >(_DateTime x, _DateTime y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<DateTime?, bool?>
        {
            public GreaterThanOrEqualExpression(Column<DateTime?> x, Column<DateTime?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(DateTime? x, DateTime? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) >= 0;
            }
        }

        /// <summary>Compares the two <see cref="_DateTime" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_DateTime" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_DateTime" /> object. </param>
        /// <param name="y">A <see cref="_DateTime" /> object. </param>
        public static _Boolean operator >=(_DateTime x, _DateTime y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<DateTime?, bool?>
        {
            public EqualExpression(Column<DateTime?> x, Column<DateTime?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(DateTime? x, DateTime? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault() == y.GetValueOrDefault();
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_DateTime" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_DateTime" /> object. </param>
        /// <param name="y">A <see cref="_DateTime" /> object. </param>
        public static _Boolean operator ==(_DateTime x, _DateTime y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<DateTime?, bool?>
        {
            public NotEqualExpression(Column<DateTime?> x, Column<DateTime?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(DateTime? x, DateTime? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault() != y.GetValueOrDefault();
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_DateTime" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_DateTime" /> object. </param>
        /// <param name="y">A <see cref="_DateTime" /> object. </param>
        public static _Boolean operator !=(_DateTime x, _DateTime y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
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

        private class UtcNowFunction : ScalarFunctionExpression<DateTime?>
        {
            public override DateTime? this[DataRow dataRow]
            {
                get { return DateTime.UtcNow; }
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.UtcNow; }
            }
        }

        public static _DateTime UtcNow()
        {
            return new UtcNowFunction().MakeColumn<_DateTime>();
        }

        private sealed class NowFunction : ScalarFunctionExpression<DateTime?>
        {
            public override DateTime? this[DataRow dataRow]
            {
                get { return DateTime.Now; }
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.Now; }
            }
        }

        public static _DateTime Now()
        {
            return new NowFunction().MakeColumn<_DateTime>();
        }
    }
}
