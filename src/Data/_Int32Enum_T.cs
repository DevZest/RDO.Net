using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public sealed class _Int32Enum<T> : EnumColumn<T, Int32?>
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

        public static _Int32Enum<T> Param(T? x, _Int32Enum<T> sourceColumn = null)
        {
            return new ParamExpression<T?>(x, sourceColumn).MakeColumn<_Int32Enum<T>>();
        }

        public static _Int32Enum<T> Const(T? x)
        {
            return new ConstantExpression<T?>(x).MakeColumn<_Int32Enum<T>>();
        }

        public static implicit operator _Int32Enum<T>(T? x)
        {
            return Param(x);
        }

        public override Int32? ConvertToDbValue(T? value)
        {
            return PerformConvert(value);
        }

        private static Int32? PerformConvert(T? value)
        {
            if (!value.HasValue)
                return null;
            return value.Value.ToInt32(null);
        }

        public override T? ConvertToEnum(Int32? dbValue)
        {
            return PerformConvert(dbValue);
        }

        private static T? PerformConvert(Int32? value)
        {
            if (!value.HasValue)
                return null;
            else
                return (T)Enum.ToObject(typeof(T), value.GetValueOrDefault());
        }

        protected override JsonValue SerializeDbValue(Int32? value)
        {
            return JsonValue.Number(value);
        }

        protected override Int32? DeserializeDbValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Int32?(Convert.ToByte(value.Text));
        }

        protected override Int32? Read(DbReader reader)
        {
            return reader.GetInt32(Ordinal);
        }

        public static explicit operator _String(_Int32Enum<T> x)
        {
            Check.NotNull(x, nameof(x));
            return x.CastToString();
        }

        private sealed class ToInt32Cast : CastExpression<T?, Int32?>
        {
            public ToInt32Cast(Column<T?> x)
                : base(x)
            {
            }

            protected override Int32? Cast(T? value)
            {
                return PerformConvert(value);
            }
        }

        public static explicit operator _Int32(_Int32Enum<T> x)
        {
            Check.NotNull(x, nameof(x));
            return new ToInt32Cast(x).MakeColumn<_Int32>();
        }

        private sealed class FromInt32Cast : CastExpression<Int32?, T?>
        {
            public FromInt32Cast(Column<Int32?> x)
                : base(x)
            {
            }

            protected override T? Cast(Int32? value)
            {
                return PerformConvert(value);
            }
        }

        public static explicit operator _Int32Enum<T>(_Int32 x)
        {
            Check.NotNull(x, nameof(x));
            return new FromInt32Cast(x).MakeColumn<_Int32Enum<T>>();
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
                return PerformConvert((Int32?)(PerformConvert(x) & PerformConvert(y)));
            }
        }

        public static _Int32Enum<T> operator &(_Int32Enum<T> x, _Int32Enum<T> y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseAndExpression(x, y).MakeColumn<_Int32Enum<T>>();
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
                return PerformConvert((Int32?)(PerformConvert(x) | PerformConvert(y)));
            }
        }

        public static _Int32Enum<T> operator |(_Int32Enum<T> x, _Int32Enum<T> y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseOrExpression(x, y).MakeColumn<_Int32Enum<T>>();
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

        public static _Boolean operator ==(_Int32Enum<T> x, _Int32Enum<T> y)
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

        public static _Boolean operator !=(_Int32Enum<T> x, _Int32Enum<T> y)
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
