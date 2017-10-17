using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public sealed class _Int16Enum<T> : EnumColumn<T, Int16?>
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

        public static _Int16Enum<T> Param(T? x, _Int16Enum<T> sourceColumn = null)
        {
            return new ParamExpression<T?>(x, sourceColumn).MakeColumn<_Int16Enum<T>>();
        }

        public static _Int16Enum<T> Const(T? x)
        {
            return new ConstantExpression<T?>(x).MakeColumn<_Int16Enum<T>>();
        }

        public static implicit operator _Int16Enum<T>(T? x)
        {
            return Param(x);
        }

        public override Int16? ConvertToDbValue(T? value)
        {
            return PerformConvert(value);
        }

        private static Int16? PerformConvert(T? value)
        {
            if (!value.HasValue)
                return null;
            return value.Value.ToInt16(null);
        }

        public override T? ConvertToEnum(Int16? dbValue)
        {
            return PerformConvert(dbValue);
        }

        private static T? PerformConvert(Int16? value)
        {
            if (!value.HasValue)
                return null;
            else
                return (T)Enum.ToObject(typeof(T), value.GetValueOrDefault());
        }

        protected override JsonValue SerializeDbValue(Int16? value)
        {
            return JsonValue.Number(value);
        }

        protected override Int16? DeserializeDbValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Int16?(Convert.ToByte(value.Text));
        }

        protected override Int16? Read(DbReader reader)
        {
            return reader.GetInt16(Ordinal);
        }

        public static explicit operator _String(_Int16Enum<T> x)
        {
            Check.NotNull(x, nameof(x));
            return x.CastToString();
        }

        private sealed class ToInt16Cast : CastExpression<T?, Int16?>
        {
            public ToInt16Cast(Column<T?> x)
                : base(x)
            {
            }

            protected override Int16? Cast(T? value)
            {
                return PerformConvert(value);
            }
        }

        public static explicit operator _Int16(_Int16Enum<T> x)
        {
            Check.NotNull(x, nameof(x));
            return new ToInt16Cast(x).MakeColumn<_Int16>();
        }

        private sealed class FromInt16Cast : CastExpression<Int16?, T?>
        {
            public FromInt16Cast(Column<Int16?> x)
                : base(x)
            {
            }

            protected override T? Cast(Int16? value)
            {
                return PerformConvert(value);
            }
        }

        public static explicit operator _Int16Enum<T>(_Int16 x)
        {
            Check.NotNull(x, nameof(x));
            return new FromInt16Cast(x).MakeColumn<_Int16Enum<T>>();
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
                return PerformConvert((Int16?)(PerformConvert(x) & PerformConvert(y)));
            }
        }

        public static _Int16Enum<T> operator &(_Int16Enum<T> x, _Int16Enum<T> y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseAndExpression(x, y).MakeColumn<_Int16Enum<T>>();
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
                return PerformConvert((Int16?)(PerformConvert(x) | PerformConvert(y)));
            }
        }

        public static _Int16Enum<T> operator |(_Int16Enum<T> x, _Int16Enum<T> y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseOrExpression(x, y).MakeColumn<_Int16Enum<T>>();
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

        public static _Boolean operator ==(_Int16Enum<T> x, _Int16Enum<T> y)
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

        public static _Boolean operator !=(_Int16Enum<T> x, _Int16Enum<T> y)
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
