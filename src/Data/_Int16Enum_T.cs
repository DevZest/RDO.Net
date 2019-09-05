using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Represents enum column which is stored as Int16 in database.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    public sealed class _Int16Enum<T> : EnumColumn<T, Int16?>
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
        public static _Int16Enum<T> Param(T? x, _Int16Enum<T> sourceColumn = null)
        {
            return new ParamExpression<T?>(x, sourceColumn).MakeColumn<_Int16Enum<T>>();
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="x">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        public static _Int16Enum<T> Const(T? x)
        {
            return new ConstantExpression<T?>(x).MakeColumn<_Int16Enum<T>>();
        }

        /// <summary>Implicitly converts the supplied value to a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <returns>The column of parameter expression.</returns>
        public static implicit operator _Int16Enum<T>(T? x)
        {
            return Param(x);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override JsonValue SerializeDbValue(Int16? value)
        {
            return JsonValue.Number(value);
        }

        /// <inheritdoc/>
        protected override Int16? DeserializeDbValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Int16?(Convert.ToInt16(value.Text));
        }

        /// <inheritdoc/>
        protected override Int16? Read(DbReader reader)
        {
            return reader.GetInt16(Ordinal);
        }

        /// <summary>Converts the supplied <see cref="_Int16Enum{T}" /> to <see cref="_String" />.</summary>
        /// <param name="x">A <see cref="_Int16Enum{T}" /> object.</param>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        public static explicit operator _String(_Int16Enum<T> x)
        {
            x.VerifyNotNull(nameof(x));
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

        /// <summary>Converts the supplied <see cref="_Int16Enum{T}" /> to <see cref="_Int16" />.</summary>
        /// <param name="x">A <see cref="_Int16Enum{T}" /> object.</param>
        /// <returns>A <see cref="_Int16" /> expression which contains the result.</returns>
        public static explicit operator _Int16(_Int16Enum<T> x)
        {
            x.VerifyNotNull(nameof(x));
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

        /// <summary>Converts the supplied <see cref="_Int16" /> to <see cref="_Int16Enum{T}" />.</summary>
        /// <param name="x">A <see cref="_Int16" /> object. </param>
        /// <returns>A <see cref="_Int16Enum{T}" /> expression which contains the result.</returns>
        public static explicit operator _Int16Enum<T>(_Int16 x)
        {
            x.VerifyNotNull(nameof(x));
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

        /// <summary>Computes the bitwise AND of its <see cref="_Int16Enum{T}" /> operands.</summary>
        /// <param name="x">A <see cref="_Int16Enum{T}" /> object. </param>
        /// <param name="y">A <see cref="_Int16Enum{T}" /> object. </param>
        /// <returns>A <see cref="_Int16Enum{T}" /> expression which contains the result.</returns>
        public static _Int16Enum<T> operator &(_Int16Enum<T> x, _Int16Enum<T> y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
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

        /// <summary>Computes the bitwise OR of its <see cref="_Int16Enum{T}" /> operands.</summary>
        /// <param name="x">A <see cref="_Int16Enum{T}" /> object. </param>
        /// <param name="y">A <see cref="_Int16Enum{T}" /> object. </param>
        /// <returns>A <see cref="_Int16Enum{T}" /> expression which contains the result.</returns>
        public static _Int16Enum<T> operator |(_Int16Enum<T> x, _Int16Enum<T> y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
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

        /// <summary>Performs a logical comparison of the two <see cref="_Int16Enum{T}" /> parameters for equality.</summary>
        /// <param name="x">A <see cref="_Int16Enum{T}" /> object.</param>
        /// <param name="y">A <see cref="_Int16Enum{T}" /> object.</param>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        public static _Boolean operator ==(_Int16Enum<T> x, _Int16Enum<T> y)
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

        /// <summary>Performs a logical comparison of the two <see cref="_Int16Enum{T}" /> parameters for non-equality.</summary>
        /// <param name="x">A <see cref="_Int16Enum{T}" /> object.</param>
        /// <param name="y">A <see cref="_Int16Enum{T}" /> object.</param>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        public static _Boolean operator !=(_Int16Enum<T> x, _Int16Enum<T> y)
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
