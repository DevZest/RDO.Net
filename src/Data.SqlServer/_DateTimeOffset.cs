using DevZest.Data.Primitives;
using System;
using System.Globalization;

namespace DevZest.Data.SqlServer
{
    /// <summary>
    /// Represents a nullable <see cref="DateTimeOffset"/> column.
    /// </summary>
    public sealed class _DateTimeOffset : Column<DateTimeOffset?>, IColumn<SqlReader, DateTimeOffset?>
    {
        protected override bool AreEqual(DateTimeOffset? x, DateTimeOffset? y)
        {
            return x == y;
        }

        protected override Column<DateTimeOffset?> CreateParam(DateTimeOffset? value)
        {
            return Param(value, this);
        }

        protected override Column<DateTimeOffset?> CreateConst(DateTimeOffset? value)
        {
            return Const(value);
        }

        protected override JsonValue SerializeValue(DateTimeOffset? value)
        {
            if (!value.HasValue)
                return JsonValue.Null;

            var x = value.GetValueOrDefault();
            return JsonValue.FastString(x.ToString("o"));
        }

        protected override DateTimeOffset? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new DateTimeOffset?(DateTimeOffset.Parse(value.Text));
        }

        public DateTimeOffset? this[SqlReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private DateTimeOffset? GetValue(SqlReader reader)
        {
            return reader.GetDateTimeOffset(Ordinal);
        }

        void IColumn<SqlReader>.Read(SqlReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <exclude />
        protected override bool IsNull(DateTimeOffset? value)
        {
            return !value.HasValue;
        }

        public static _DateTimeOffset Param(DateTimeOffset? x, _DateTimeOffset sourceColumn = null)
        {
            return new ParamExpression<DateTimeOffset?>(x, sourceColumn).MakeColumn<_DateTimeOffset>();
        }

        public static _DateTimeOffset Const(DateTimeOffset? x)
        {
            return new ConstantExpression<DateTimeOffset?>(x).MakeColumn<_DateTimeOffset>();
        }

        /// <summary>Converts the supplied nullable DateTime to <see cref="_DateTime" /> expression.</summary>
        /// <returns>A new <see cref="_DateTime" /> expression from the provided value.</returns>
        /// <param name="x">A nullable DateTime value.</param>
        public static implicit operator _DateTimeOffset(DateTimeOffset? x)
        {
            return Param(x);
        }

        private sealed class DbStringCast : CastExpression<String, DateTimeOffset?>
        {
            public DbStringCast(_String x)
                : base(x)
            {
            }

            protected override DateTimeOffset? Cast(String value)
            {
                if (value == null)
                    return null;
                return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_DateTimeOffset" />.</summary>
        /// <returns>A <see cref="_DateTimeOffset" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _DateTimeOffset(_String x)
        {
            Check.NotNull(x, "x");
            return new DbStringCast(x).MakeColumn<_DateTimeOffset>();
        }

        private sealed class LessThanExpression : BinaryExpression<DateTimeOffset?, bool?>
        {
            public LessThanExpression(_DateTimeOffset x, _DateTimeOffset y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(DateTimeOffset? x, DateTimeOffset? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) < 0;
            }
        }

        /// <summary>Compares the two <see cref="_DateTimeOffset" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_DateTimeOffset" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_DateTimeOffset" /> object. </param>
        /// <param name="y">A <see cref="_DateTimeOffset" /> object. </param>
        public static _Boolean operator <(_DateTimeOffset x, _DateTimeOffset y)
        {
            Check.NotNull(x, "x");
            Check.NotNull(y, "y");
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<DateTimeOffset?, bool?>
        {
            public LessThanOrEqualExpression(_DateTimeOffset x, _DateTimeOffset y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(DateTimeOffset? x, DateTimeOffset? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) <= 0;
            }
        }

        /// <summary>Compares the two <see cref="_DateTimeOffset" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_Boolean" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_DateTimeOffset" /> object. </param>
        /// <param name="y">A <see cref="_DateTimeOffset" /> object. </param>
        public static _Boolean operator <=(_DateTimeOffset x, _DateTimeOffset y)
        {
            Check.NotNull(x, "x");
            Check.NotNull(y, "y");
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<DateTimeOffset?, bool?>
        {
            public GreaterThanExpression(_DateTimeOffset x, _DateTimeOffset y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(DateTimeOffset? x, DateTimeOffset? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) > 0;
            }
        }

        /// <summary>Compares the two <see cref="_DateTimeOffset" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_Boolean" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_DateTime" /> object. </param>
        /// <param name="y">A <see cref="_DateTime" /> object. </param>
        public static _Boolean operator >(_DateTimeOffset x, _DateTimeOffset y)
        {
            Check.NotNull(x, "x");
            Check.NotNull(y, "y");
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<DateTimeOffset?, bool?>
        {
            public GreaterThanOrEqualExpression(_DateTimeOffset x, _DateTimeOffset y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(DateTimeOffset? x, DateTimeOffset? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) >= 0;
            }
        }

        /// <summary>Compares the two <see cref="_DateTimeOffset" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_Boolean" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_DateTimeOffset" /> object. </param>
        /// <param name="y">A <see cref="_DateTimeOffset" /> object. </param>
        public static _Boolean operator >=(_DateTimeOffset x, _DateTimeOffset y)
        {
            Check.NotNull(x, "x");
            Check.NotNull(y, "y");
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<DateTimeOffset?, bool?>
        {
            public EqualExpression(_DateTimeOffset x, _DateTimeOffset y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(DateTimeOffset? x, DateTimeOffset? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault() == y.GetValueOrDefault();
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_DateTimeOffset" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_DateTimeOffset" /> object. </param>
        /// <param name="y">A <see cref="_DateTimeOffset" /> object. </param>
        public static _Boolean operator ==(_DateTimeOffset x, _DateTimeOffset y)
        {
            Check.NotNull(x, "x");
            Check.NotNull(y, "y");

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<DateTimeOffset?, bool?>
        {
            public NotEqualExpression(_DateTimeOffset x, _DateTimeOffset y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(DateTimeOffset? x, DateTimeOffset? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault() != y.GetValueOrDefault();
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_DateTimeOffset" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_DateTimeOffset" /> object. </param>
        /// <param name="y">A <see cref="_DateTimeOffset" /> object. </param>
        public static _Boolean operator !=(_DateTimeOffset x, _DateTimeOffset y)
        {
            Check.NotNull(x, "x");
            Check.NotNull(y, "y");

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
    }
}
