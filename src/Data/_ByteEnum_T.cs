using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Represents enum column which is stored as byte in database.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    public sealed class _ByteEnum<T> : EnumColumn<T, byte?>
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
        public static _ByteEnum<T> Param(T? x, _ByteEnum<T> sourceColumn = null)
        {
            return new ParamExpression<T?>(x, sourceColumn).MakeColumn<_ByteEnum<T>>();
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="x">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        public static _ByteEnum<T> Const(T? x)
        {
            return new ConstantExpression<T?>(x).MakeColumn<_ByteEnum<T>>();
        }

        /// <summary>Implicitly converts the supplied value to a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <returns>The column of parameter expression.</returns>
        public static implicit operator _ByteEnum<T>(T? x)
        {
            return Param(x);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override JsonValue SerializeDbValue(byte? value)
        {
            return JsonValue.Number(value);
        }

        /// <inheritdoc/>
        protected override byte? DeserializeDbValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Byte?(Convert.ToByte(value.Text));
        }

        /// <inheritdoc/>
        protected override byte? Read(DbReader reader)
        {
            return reader.GetByte(Ordinal);
        }

        /// <summary>Converts the supplied <see cref="_ByteEnum{T}" /> to <see cref="_String" />.</summary>
        /// <param name="x">A <see cref="_ByteEnum{T}" /> object.</param>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        public static explicit operator _String(_ByteEnum<T> x)
        {
            x.VerifyNotNull(nameof(x));
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

        /// <summary>Converts the supplied <see cref="_ByteEnum{T}" /> to <see cref="_Byte" />.</summary>
        /// <param name="x">A <see cref="_ByteEnum{T}" /> object.</param>
        /// <returns>A <see cref="_Byte" /> expression which contains the result.</returns>
        public static explicit operator _Byte(_ByteEnum<T> x)
        {
            x.VerifyNotNull(nameof(x));
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

        /// <summary>Converts the supplied <see cref="_Byte" /> to <see cref="_ByteEnum{T}" />.</summary>
        /// <param name="x">A <see cref="_Byte" /> object. </param>
        /// <returns>A <see cref="_ByteEnum{T}" /> expression which contains the result.</returns>
        public static explicit operator _ByteEnum<T>(_Byte x)
        {
            x.VerifyNotNull(nameof(x));
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

        /// <summary>Computes the bitwise AND of its <see cref="_ByteEnum{T}" /> operands.</summary>
        /// <param name="x">A <see cref="_ByteEnum{T}" /> object. </param>
        /// <param name="y">A <see cref="_ByteEnum{T}" /> object. </param>
        /// <returns>A <see cref="_ByteEnum{T}" /> expression which contains the result.</returns>
        public static _ByteEnum<T> operator &(_ByteEnum<T> x, _ByteEnum<T> y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
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

        /// <summary>Computes the bitwise OR of its <see cref="_ByteEnum{T}" /> operands.</summary>
        /// <param name="x">A <see cref="_ByteEnum{T}" /> object. </param>
        /// <param name="y">A <see cref="_ByteEnum{T}" /> object. </param>
        /// <returns>A <see cref="_ByteEnum{T}" /> expression which contains the result.</returns>
        public static _ByteEnum<T> operator |(_ByteEnum<T> x, _ByteEnum<T> y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
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

        /// <summary>Performs a logical comparison of the two <see cref="_ByteEnum{T}" /> parameters for equality.</summary>
        /// <param name="x">A <see cref="_ByteEnum{T}" /> object.</param>
        /// <param name="y">A <see cref="_ByteEnum{T}" /> object.</param>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        public static _Boolean operator ==(_ByteEnum<T> x, _ByteEnum<T> y)
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

        /// <summary>Performs a logical comparison of the two <see cref="_ByteEnum{T}" /> parameters for non-equality.</summary>
        /// <param name="x">A <see cref="_ByteEnum{T}" /> object.</param>
        /// <param name="y">A <see cref="_ByteEnum{T}" /> object.</param>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        public static _Boolean operator !=(_ByteEnum<T> x, _ByteEnum<T> y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new NotEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            // override to eliminate compile warning
            return base.Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            // override to eliminate compile warning
            return base.GetHashCode();
        }
    }
}
