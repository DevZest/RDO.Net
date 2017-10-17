using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public sealed class _ByteEnum<T> : EnumColumn<T, byte?>
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

        public static _ByteEnum<T> Param(T? x, _ByteEnum<T> sourceColumn = null)
        {
            return new ParamExpression<T?>(x, sourceColumn).MakeColumn<_ByteEnum<T>>();
        }

        public static _ByteEnum<T> Const(T? x)
        {
            return new ConstantExpression<T?>(x).MakeColumn<_ByteEnum<T>>();
        }

        public static implicit operator _ByteEnum<T>(T? x)
        {
            return Param(x);
        }

        public override byte? ConvertToDbValue(T? value)
        {
            return PerformConvert(value);
        }

        private static byte? PerformConvert(T? value)
        {
            if (!value.HasValue)
                return null;
            return value.Value.ToByte(null);
        }

        public override T? ConvertToEnum(byte? dbValue)
        {
            return PerformConvert(dbValue);
        }

        private static T? PerformConvert(byte? value)
        {
            if (!value.HasValue)
                return null;
            else
                return (T)Enum.ToObject(typeof(T), value.GetValueOrDefault());
        }

        protected override JsonValue SerializeDbValue(byte? value)
        {
            return JsonValue.Number(value);
        }

        protected override byte? DeserializeDbValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Byte?(Convert.ToByte(value.Text));
        }

        protected override byte? Read(DbReader reader)
        {
            return reader.GetByte(Ordinal);
        }

        public static explicit operator _String(_ByteEnum<T> x)
        {
            Check.NotNull(x, nameof(x));
            return x.CastToString();
        }

        private sealed class ToByteCast : CastExpression<T?, Byte?>
        {
            public ToByteCast(Column<T?> x)
                : base(x)
            {
            }

            protected override Byte? Cast(T? value)
            {
                return PerformConvert(value);
            }
        }

        public static explicit operator _Byte(_ByteEnum<T> x)
        {
            Check.NotNull(x, nameof(x));
            return new ToByteCast(x).MakeColumn<_Byte>();
        }

        private sealed class FromByteCast : CastExpression<Byte?, T?>
        {
            public FromByteCast(Column<Byte?> x)
                : base(x)
            {
            }

            protected override T? Cast(Byte? value)
            {
                return PerformConvert(value);
            }
        }

        public static explicit operator _ByteEnum<T>(_Byte x)
        {
            Check.NotNull(x, nameof(x));
            return new FromByteCast(x).MakeColumn<_ByteEnum<T>>();
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
                return PerformConvert((byte?)(PerformConvert(x) & PerformConvert(y)));
            }
        }

        public static _ByteEnum<T> operator &(_ByteEnum<T> x, _ByteEnum<T> y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseAndExpression(x, y).MakeColumn<_ByteEnum<T>>();
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
                return PerformConvert((byte?)(PerformConvert(x) | PerformConvert(y)));
            }
        }

        public static _ByteEnum<T> operator |(_ByteEnum<T> x, _ByteEnum<T> y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new BitwiseOrExpression(x, y).MakeColumn<_ByteEnum<T>>();
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

        public static _Boolean operator ==(_ByteEnum<T> x, _ByteEnum<T> y)
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

        public static _Boolean operator !=(_ByteEnum<T> x, _ByteEnum<T> y)
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
