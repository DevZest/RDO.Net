using DevZest.Data.Primitives;
using System;
using System.Globalization;

namespace DevZest.Data.SqlServer
{
    /// <summary>
    /// Represents a nullable <see cref="TimeSpan"/> column.
    /// </summary>
    public sealed class _TimeSpan : Column<TimeSpan?>, IColumn<SqlReader, TimeSpan?>
    {
        protected override Column<TimeSpan?> CreateParam(TimeSpan? value)
        {
            return Param(value, this);
        }

        protected override Column<TimeSpan?> CreateConst(TimeSpan? value)
        {
            return Const(value);
        }

        protected override JsonValue SerializeValue(TimeSpan? value)
        {
            if (!value.HasValue)
                return JsonValue.Null;

            var x = value.GetValueOrDefault();
            return JsonValue.FastString(x.ToString("c"));
        }

        protected override TimeSpan? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new TimeSpan?(TimeSpan.Parse(value.Text));
        }

        public TimeSpan? this[SqlReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private TimeSpan? GetValue(SqlReader reader)
        {
            return reader.GetTimeSpan(Ordinal);
        }

        void IColumn<SqlReader>.Read(SqlReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <exclude />
        protected override bool IsNull(TimeSpan? value)
        {
            return !value.HasValue;
        }

        public static _TimeSpan Param(TimeSpan? x, _TimeSpan sourceColumn = null)
        {
            return new ParamExpression<TimeSpan?>(x, sourceColumn).MakeColumn<_TimeSpan>();
        }

        public static _TimeSpan Const(TimeSpan? x)
        {
            return new ConstantExpression<TimeSpan?>(x).MakeColumn<_TimeSpan>();
        }

        /// <summary>Converts the supplied nullable DateTime to <see cref="_DateTime" /> expression.</summary>
        /// <returns>A new <see cref="_DateTime" /> expression from the provided value.</returns>
        /// <param name="x">A nullable DateTime value.</param>
        public static implicit operator _TimeSpan(TimeSpan? x)
        {
            return Param(x);
        }

        private sealed class DbStringCast : CastExpression<String, TimeSpan?>
        {
            public DbStringCast(_String x)
                : base(x)
            {
            }

            protected override TimeSpan? Cast(String value)
            {
                if (value == null)
                    return null;
                return TimeSpan.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_TimeSpan" />.</summary>
        /// <returns>A <see cref="_TimeSpan" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _TimeSpan(_String x)
        {
            Check.NotNull(x, "x");
            return new DbStringCast(x).MakeColumn<_TimeSpan>();
        }

        private sealed class LessThanExpression : BinaryExpression<TimeSpan?, bool?>
        {
            public LessThanExpression(_TimeSpan x, _TimeSpan y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(TimeSpan? x, TimeSpan? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) < 0;
            }
        }

        /// <summary>Compares the two <see cref="_TimeSpan" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_TimeSpan" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_TimeSpan" /> object. </param>
        /// <param name="y">A <see cref="_TimeSpan" /> object. </param>
        public static _Boolean operator <(_TimeSpan x, _TimeSpan y)
        {
            Check.NotNull(x, "x");
            Check.NotNull(y, "y");
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<TimeSpan?, bool?>
        {
            public LessThanOrEqualExpression(_TimeSpan x, _TimeSpan y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(TimeSpan? x, TimeSpan? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) <= 0;
            }
        }

        /// <summary>Compares the two <see cref="_TimeSpan" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_Boolean" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_TimeSpan" /> object. </param>
        /// <param name="y">A <see cref="_TimeSpan" /> object. </param>
        public static _Boolean operator <=(_TimeSpan x, _TimeSpan y)
        {
            Check.NotNull(x, "x");
            Check.NotNull(y, "y");
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<TimeSpan?, bool?>
        {
            public GreaterThanExpression(_TimeSpan x, _TimeSpan y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(TimeSpan? x, TimeSpan? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) > 0;
            }
        }

        /// <summary>Compares the two <see cref="_TimeSpan" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_Boolean" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_DateTime" /> object. </param>
        /// <param name="y">A <see cref="_DateTime" /> object. </param>
        public static _Boolean operator >(_TimeSpan x, _TimeSpan y)
        {
            Check.NotNull(x, "x");
            Check.NotNull(y, "y");
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<TimeSpan?, bool?>
        {
            public GreaterThanOrEqualExpression(_TimeSpan x, _TimeSpan y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(TimeSpan? x, TimeSpan? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) >= 0;
            }
        }

        /// <summary>Compares the two <see cref="_TimeSpan" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_Boolean" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_TimeSpan" /> object. </param>
        /// <param name="y">A <see cref="_TimeSpan" /> object. </param>
        public static _Boolean operator >=(_TimeSpan x, _TimeSpan y)
        {
            Check.NotNull(x, "x");
            Check.NotNull(y, "y");
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<TimeSpan?, bool?>
        {
            public EqualExpression(_TimeSpan x, _TimeSpan y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(TimeSpan? x, TimeSpan? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault() == y.GetValueOrDefault();
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_TimeSpan" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_TimeSpan" /> object. </param>
        /// <param name="y">A <see cref="_TimeSpan" /> object. </param>
        public static _Boolean operator ==(_TimeSpan x, _TimeSpan y)
        {
            Check.NotNull(x, "x");
            Check.NotNull(y, "y");

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<TimeSpan?, bool?>
        {
            public NotEqualExpression(_TimeSpan x, _TimeSpan y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(TimeSpan? x, TimeSpan? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault() != y.GetValueOrDefault();
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_TimeSpan" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_TimeSpan" /> object. </param>
        /// <param name="y">A <see cref="_TimeSpan" /> object. </param>
        public static _Boolean operator !=(_TimeSpan x, _TimeSpan y)
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
