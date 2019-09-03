using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Boolean"/> column.
    /// </summary>
    public sealed class _Boolean : Column<bool?>, IColumn<DbReader, Boolean?>
    {
        /// <inheritdoc/>
        protected sealed override Column<bool?> CreateParam(bool? value)
        {
            return Param(value, this);
        }

        public override bool AreEqual(bool? x, bool? y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected internal sealed override Column<bool?> CreateConst(bool? value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(bool? value)
        {
            return value.HasValue ? (value.GetValueOrDefault() ? JsonValue.True : JsonValue.False) : JsonValue.Null;
        }

        /// <inheritdoc/>
        protected internal override bool? DeserializeValue(JsonValue value)
        {
            if (value.Type == JsonValueType.Null)
                return null;
            else if (value.Type == JsonValueType.True)
                return true;
            else if (value.Type == JsonValueType.False)
                return false;
            throw new FormatException(DiagnosticMessages.BooleanColumn_CannotDeserialize);
        }

        /// <summary>Gets the value of this column from <see cref="DbReader"/>'s current row.</summary>
        /// <param name="reader">The <see cref="DbReader"/> object.</param>
        /// <returns>The value of this column from <see cref="DbReader"/>'s current row.</returns>
        public bool? this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private bool? GetValue(DbReader reader)
        {
            return reader.GetBoolean(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(bool? value)
        {
            return !value.HasValue;
        }

        /// <summary>Creates a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <param name="sourceColumn">The value which will be passed to <see cref="DbParamExpression.SourceColumn"/>.</param>
        /// <returns>The column of parameter expression.</returns>
        public static _Boolean Param(bool? x, _Boolean sourceColumn = null)
        {
            return new ParamExpression<bool?>(x, sourceColumn).MakeColumn<_Boolean>();
        }

        /// <summary>Column of <see langword="null"/> constant value.</summary>
        public static readonly _Boolean Null = new ConstantExpression<bool?>(null).MakeColumn<_Boolean>();
        /// <summary>Column of <see langword="true"/> constant value.</summary>
        public static readonly _Boolean True = new ConstantExpression<bool?>(true).MakeColumn<_Boolean>();
        /// <summary>Column of <see langword="false"/> constant value.</summary>
        public static readonly _Boolean False = new ConstantExpression<bool?>(false).MakeColumn<_Boolean>();

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="x">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        public static _Boolean Const(bool? x)
        {
            return x.HasValue ? (x.GetValueOrDefault() ? True : False) : Null;
        }

        /// <summary>Implicitly converts the supplied value to a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <returns>The column of parameter expression.</returns>
        public static implicit operator _Boolean(bool? x)
        {
            return Param(x);
        }

        private sealed class NotExpression : UnaryExpression<bool?>
        {
            public NotExpression(Column<bool?> operand)
                : base(operand)
            {
            }

            protected override DbUnaryExpressionKind ExpressionKind
            {
                get { return DbUnaryExpressionKind.Not; }
            }

            protected override bool? EvalCore(bool? x)
            {
                return !x;
            }
        }

        /// <summary>Performs a NOT operation on a <see cref="_Boolean" />.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">The original <see cref="_Boolean" /> on which the NOT operation will be performed. </param>
        public static _Boolean operator !(_Boolean x)
        {
            x.VerifyNotNull(nameof(x));
            return new NotExpression(x).MakeColumn<_Boolean>();
        }

        private sealed class AndExpression : BinaryExpression<bool?>
        {
            public AndExpression(Column<bool?> left, Column<bool?> right)
                : base(left, right)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.And; }
            }

            protected override bool? EvalCore(bool? x, bool? y)
            {
                return And(x, y);
            }
        }

        // 3 Valued Logic implimentation, may need to be moved into a service provider in the future.
        private static bool? And(bool? x, bool? y)
        {
            if (x == false || y == false)
                return false;
            if (x == true || y == true)
                return true;
            return null;
        }

        /// <summary>Computes the logical AND operation of two specified <see cref="_Boolean" /> objects.</summary>
        /// <returns>The result of the logical AND operation.</returns>
        /// <param name="x">A <see cref="_Boolean" /> object.</param>
        /// <param name="y">A <see cref="_Boolean" /> object.</param>
        public static _Boolean operator &(_Boolean x, _Boolean y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new AndExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class OrExpression : BinaryExpression<bool?>
        {
            public OrExpression(Column<bool?> left, Column<bool?> right)
                : base(left, right)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Or; }
            }

            protected override bool? EvalCore(bool? x, bool? y)
            {
                return Or(x, y);
            }
        }

        // 3 Valued Logic implimentation, may need to be moved into a service provider in the future.
        private static bool? Or(bool? x, bool? y)
        {
            if (x == true || y == true)
                return true;
            if (x == false && y == false)
                return false;
            return null;
        }

        /// <summary>Computes the logical OR operation of two specified <see cref="_Boolean" /> objects.</summary>
        /// <returns>The result of the logical OR operation.</returns>
        /// <param name="x">A <see cref="_Boolean" /> object.</param>
        /// <param name="y">A <see cref="_Boolean" /> object.</param>
        public static _Boolean operator |(_Boolean x, _Boolean y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new OrExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class FromStringExpression : CastExpression<String, Boolean?>
        {
            public FromStringExpression(Column<string> x)
                : base(x)
            {
            }

            protected override Boolean? Cast(String value)
            {
                if (value == null)
                    return null;
                return Boolean.Parse(value);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_Boolean" />.</summary>
        /// <returns>A <see cref="_Boolean" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _Boolean(_String x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromStringExpression(x).MakeColumn<_Boolean>();
        }

        private sealed class CastToStringExpression : CastExpression<Boolean?, String>
        {
            public CastToStringExpression(Column<Boolean?> x)
                : base(x)
            {
            }

            protected override String Cast(Boolean? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString() : null;
            }
        }

        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        /// <summary>Converts the supplied <see cref="_Boolean" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Boolean" /> object. </param>
        public static explicit operator _String(_Boolean x)
        {
            x.VerifyNotNull(nameof(x));
            return x.CastToString();
        }

        private sealed class EqualExpression : BinaryExpression<bool?, bool?>
        {
            public EqualExpression(Column<bool?> x, Column<bool?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(bool? x, bool? y)
            {
                return x.EqualsTo(y);
            }
        }

        public static _Boolean operator ==(_Boolean x, _Boolean y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<bool?, bool?>
        {
            public NotEqualExpression(Column<bool?> x, Column<bool?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(bool? x, bool? y)
            {
                return !x.EqualsTo(y);
            }
        }

        public static _Boolean operator !=(_Boolean x, _Boolean y)
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
