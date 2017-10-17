using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    public sealed class _CharEnum<T> : EnumColumn<T, char?>
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

        public static _CharEnum<T> Param(T? x, _CharEnum<T> sourceColumn = null)
        {
            return new ParamExpression<T?>(x, sourceColumn).MakeColumn<_CharEnum<T>>();
        }

        public static _CharEnum<T> Const(T? x)
        {
            return new ConstantExpression<T?>(x).MakeColumn<_CharEnum<T>>();
        }

        public static implicit operator _CharEnum<T>(T? x)
        {
            return Param(x);
        }

        public override char? ConvertToDbValue(T? value)
        {
            return PerformConvert(value);
        }

        private static char? PerformConvert(T? value)
        {
            if (!value.HasValue)
                return null;
            return value.Value.ToChar(null);
        }

        public override T? ConvertToEnum(char? dbValue)
        {
            return PerformConvert(dbValue);
        }

        private static T? PerformConvert(char? value)
        {
            if (!value.HasValue)
                return null;
            else
                return (T)Enum.ToObject(typeof(T), value.GetValueOrDefault());
        }

        protected override JsonValue SerializeDbValue(char? value)
        {
            return JsonValue.Char(value);
        }

        protected override char? DeserializeDbValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new char?(Convert.ToChar(value.Text));
        }

        protected override char? Read(DbReader reader)
        {
            return reader.GetChar(Ordinal);
        }

        public static explicit operator _String(_CharEnum<T> x)
        {
            Check.NotNull(x, nameof(x));
            return x.CastToString();
        }

        private sealed class ToCharCast : CastExpression<T?, Char?>
        {
            public ToCharCast(Column<T?> x)
                : base(x)
            {
            }

            protected override Char? Cast(T? value)
            {
                return PerformConvert(value);
            }
        }

        public static explicit operator _Char(_CharEnum<T> x)
        {
            Check.NotNull(x, nameof(x));
            return new ToCharCast(x).MakeColumn<_Char>();
        }

        private sealed class FromCharCast : CastExpression<Char?, T?>
        {
            public FromCharCast(Column<Char?> x)
                : base(x)
            {
            }

            protected override T? Cast(Char? value)
            {
                return PerformConvert(value);
            }
        }

        public static explicit operator _CharEnum<T>(_Char x)
        {
            Check.NotNull(x, nameof(x));
            return new FromCharCast(x).MakeColumn<_CharEnum<T>>();
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

        public static _Boolean operator ==(_CharEnum<T> x, _CharEnum<T> y)
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

        public static _Boolean operator !=(_CharEnum<T> x, _CharEnum<T> y)
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
