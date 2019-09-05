using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Represents enum column which is stored as char in database.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    public sealed class _CharEnum<T> : EnumColumn<T, char?>
        where T : struct, IConvertible
    {
        /// <inheritdoc/>
        protected override Column<T?> CreateParam(T? value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected internal override Column<T?> CreateConst(T? value)
        {
            return Const(value);
        }

        /// <summary>Creates a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <param name="sourceColumn">The value which will be passed to <see cref="DbParamExpression.SourceColumn"/>.</param>
        /// <returns>The column of parameter expression.</returns>
        public static _CharEnum<T> Param(T? x, _CharEnum<T> sourceColumn = null)
        {
            return new ParamExpression<T?>(x, sourceColumn).MakeColumn<_CharEnum<T>>();
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="x">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        public static _CharEnum<T> Const(T? x)
        {
            return new ConstantExpression<T?>(x).MakeColumn<_CharEnum<T>>();
        }

        /// <summary>Implicitly converts the supplied value to a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <returns>The column of parameter expression.</returns>
        public static implicit operator _CharEnum<T>(T? x)
        {
            return Param(x);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override JsonValue SerializeDbValue(char? value)
        {
            return JsonValue.Char(value);
        }

        /// <inheritdoc/>
        protected override char? DeserializeDbValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new char?(Convert.ToChar(value.Text));
        }

        /// <inheritdoc/>
        protected override char? Read(DbReader reader)
        {
            return reader.GetChar(Ordinal);
        }

        /// <summary>Converts the supplied <see cref="_CharEnum{T}" /> to <see cref="_String" />.</summary>
        /// <param name="x">A <see cref="_CharEnum{T}" /> object.</param>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        public static explicit operator _String(_CharEnum<T> x)
        {
            x.VerifyNotNull(nameof(x));
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

        /// <summary>Converts the supplied <see cref="_CharEnum{T}" /> to <see cref="_Char" />.</summary>
        /// <param name="x">A <see cref="_CharEnum{T}" /> object.</param>
        /// <returns>A <see cref="_Char" /> expression which contains the result.</returns>
        public static explicit operator _Char(_CharEnum<T> x)
        {
            x.VerifyNotNull(nameof(x));
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

        /// <summary>Converts the supplied <see cref="_Char" /> to <see cref="_CharEnum{T}" />.</summary>
        /// <param name="x">A <see cref="_Char" /> object. </param>
        /// <returns>A <see cref="_CharEnum{T}" /> expression which contains the result.</returns>
        public static explicit operator _CharEnum<T>(_Char x)
        {
            x.VerifyNotNull(nameof(x));
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

        /// <summary>Performs a logical comparison of the two <see cref="_CharEnum{T}" /> parameters for equality.</summary>
        /// <param name="x">A <see cref="_CharEnum{T}" /> object.</param>
        /// <param name="y">A <see cref="_CharEnum{T}" /> object.</param>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        public static _Boolean operator ==(_CharEnum<T> x, _CharEnum<T> y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

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

        /// <summary>Performs a logical comparison of the two <see cref="_CharEnum{T}" /> parameters for non-equality.</summary>
        /// <param name="x">A <see cref="_CharEnum{T}" /> object.</param>
        /// <param name="y">A <see cref="_CharEnum{T}" /> object.</param>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        public static _Boolean operator !=(_CharEnum<T> x, _CharEnum<T> y)
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
    }
}
