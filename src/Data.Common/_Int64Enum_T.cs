using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public sealed class _Int64Enum<T> : EnumColumn<T, Int64?>
        where T : struct, IConvertible
    {
        protected override Column<T?> CreateParam(T? value)
        {
            return Param(value, this);
        }

        protected internal override Column<T?> CreateConst(T? value)
        {
            return Const(value);
        }

        public static _Int64Enum<T> Param(T? x, _Int64Enum<T> sourceColumn = null)
        {
            return new ParamExpression<T?>(x, sourceColumn).MakeColumn<_Int64Enum<T>>();
        }

        public static _Int64Enum<T> Const(T? x)
        {
            return new ConstantExpression<T?>(x).MakeColumn<_Int64Enum<T>>();
        }

        public static implicit operator _Int64Enum<T>(T? x)
        {
            return Param(x);
        }

        public override Int64? ConvertToDbValue(T? value)
        {
            return PerformConvert(value);
        }

        private static Int64? PerformConvert(T? value)
        {
            if (!value.HasValue)
                return null;
            return value.Value.ToInt64(null);
        }

        public override T? ConvertToEnum(Int64? dbValue)
        {
            return PerformConvert(dbValue);
        }

        private static T? PerformConvert(Int64? value)
        {
            if (!value.HasValue)
                return null;
            else
                return (T)Enum.ToObject(typeof(T), value.GetValueOrDefault());
        }

        protected override JsonValue SerializeDbValue(Int64? value)
        {
            return JsonValue.Number(value);
        }

        protected override Int64? DeserializeDbValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Int64?(Convert.ToByte(value.Text));
        }

        protected override Int64? Read(DbReader reader)
        {
            return reader.GetInt64(Ordinal);
        }

        public static explicit operator _String(_Int64Enum<T> x)
        {
            Check.NotNull(x, nameof(x));
            return x.CastToString();
        }

        private sealed class ToInt64Cast : CastExpression<T?, Int64?>
        {
            public ToInt64Cast(Column<T?> x)
                : base(x)
            {
            }

            protected override Int64? Cast(T? value)
            {
                return PerformConvert(value);
            }
        }

        public static explicit operator _Int64(_Int64Enum<T> x)
        {
            Check.NotNull(x, nameof(x));
            return new ToInt64Cast(x).MakeColumn<_Int64>();
        }

        private sealed class FromInt64Cast : CastExpression<Int64?, T?>
        {
            public FromInt64Cast(Column<Int64?> x)
                : base(x)
            {
            }

            protected override T? Cast(Int64? value)
            {
                return PerformConvert(value);
            }
        }

        public static explicit operator _Int64Enum<T>(_Int64 x)
        {
            Check.NotNull(x, nameof(x));
            return new FromInt64Cast(x).MakeColumn<_Int64Enum<T>>();
        }

        private sealed class BitwiseAndExpression : BinaryExpression<T?>
        {
            public BitwiseAndExpression(Column<T?> x, Column<T?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseAnd; }
            }

            protected override T? EvalCore(T? x, T? y)
            {
                return PerformConvert((Int64?)(PerformConvert(x) & PerformConvert(y)));
            }
        }

        public static _Int64Enum<T> operator &(_Int64Enum<T> x, _Int64Enum<T> y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseAndExpression(x, y).MakeColumn<_Int64Enum<T>>();
        }

        private sealed class BitwiseOrExpression : BinaryExpression<T?>
        {
            public BitwiseOrExpression(Column<T?> x, Column<T?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.BitwiseOr; }
            }

            protected override T? EvalCore(T? x, T? y)
            {
                return PerformConvert((Int64?)(PerformConvert(x) | PerformConvert(y)));
            }
        }

        public static _Int64Enum<T> operator |(_Int64Enum<T> x, _Int64Enum<T> y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseOrExpression(x, y).MakeColumn<_Int64Enum<T>>();
        }

        private sealed class EqualExpression : BinaryExpression<T?, bool?>
        {
            public EqualExpression(Column<T?> x, Column<T?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(T? x, T? y)
            {
                return x.EqualsTo(y);
            }
        }

        public static _Boolean operator ==(_Int64Enum<T> x, _Int64Enum<T> y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<T?, bool?>
        {
            public NotEqualExpression(Column<T?> x, Column<T?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(T? x, T? y)
            {
                return !x.EqualsTo(y);
            }
        }

        public static _Boolean operator !=(_Int64Enum<T> x, _Int64Enum<T> y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

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
