using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Char"/> column.
    /// </summary>
    public sealed class _Char : Column<Char?>, IColumn<DbReader, Char?>
    {
        private sealed class CastToStringExpression : CastExpression<Char?, String>
        {
            public CastToStringExpression(Column<Char?> x)
                : base(x)
            {
            }

            protected override String Cast(Char? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString() : null;
            }
        }

        /// <inheritdoc/>
        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        /// <summary>Converts the supplied <see cref="_Char" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Char" /> object. </param>
        public static explicit operator _String(_Char x)
        {
            x.VerifyNotNull(nameof(x));
            return x.CastToString();
        }

        /// <inheritdoc/>
        protected sealed override Column<char?> CreateParam(char? value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        public override bool AreEqual(char? x, char? y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected internal sealed override Column<char?> CreateConst(char? value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(Char? value)
        {
            return JsonValue.Char(value);
        }

        /// <inheritdoc/>
        protected internal override Char? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Char?(Convert.ToChar(value.Text));
        }

        /// <summary>Gets the value of this column from <see cref="DbReader"/>'s current row.</summary>
        /// <param name="reader">The <see cref="DbReader"/> object.</param>
        /// <returns>The value of this column from <see cref="DbReader"/>'s current row.</returns>
        public Char? this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private Char? GetValue(DbReader reader)
        {
            return reader.GetChar(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(Char? value)
        {
            return !value.HasValue;
        }

        /// <summary>Creates a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <param name="sourceColumn">The value which will be passed to <see cref="DbParamExpression.SourceColumn"/>.</param>
        /// <returns>The column of parameter expression.</returns>
        public static _Char Param(Char? x, _Char sourceColumn = null)
        {
            return new ParamExpression<Char?>(x, sourceColumn).MakeColumn<_Char>();
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="x">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        public static _Char Const(char? x)
        {
            return new ConstantExpression<Char?>(x).MakeColumn<_Char>();
        }

        /// <summary>Implicitly converts the supplied value to a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <returns>The column of parameter expression.</returns>
        public static implicit operator _Char(Char? x)
        {
            return Param(x);
        }

        private sealed class FromStringCast : CastExpression<String, Char?>
        {
            public FromStringCast(Column<string> x)
                : base(x)
            {
            }

            protected override Char? Cast(String value)
            {
                if (value == null)
                    return null;
                return Char.Parse(value);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_Char" />.</summary>
        /// <returns>A <see cref="_Char" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _Char(_String x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromStringCast(x).MakeColumn<_Char>();
        }

        private sealed class LessThanExpression : BinaryExpression<Char?, bool?>
        {
            public LessThanExpression(Column<char?> x, Column<char?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(Char? x, Char? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) < 0;
            }
        }

        /// <summary>Compares the two <see cref="_Char" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_Char" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Char" /> object. </param>
        /// <param name="y">A <see cref="_Char" /> object. </param>
        public static _Boolean operator <(_Char x, _Char y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<Char?, bool?>
        {
            public LessThanOrEqualExpression(Column<char?> x, Column<char?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(Char? x, Char? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) <= 0;
            }
        }

        /// <summary>Compares the two <see cref="_Char" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_Char" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Char" /> object. </param>
        /// <param name="y">A <see cref="_Char" /> object. </param>
        public static _Boolean operator <=(_Char x, _Char y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<Char?, bool?>
        {
            public GreaterThanExpression(Column<char?> x, Column<char?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(Char? x, Char? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) > 0;
            }
        }

        /// <summary>Compares the two <see cref="_Char" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_Char" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Char" /> object. </param>
        /// <param name="y">A <see cref="_Char" /> object. </param>
        public static _Boolean operator >(_Char x, _Char y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Char?, bool?>
        {
            public GreaterThanOrEqualExpression(Column<char?> x, Column<char?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(Char? x, Char? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) >= 0;
            }
        }

        /// <summary>Compares the two <see cref="_Char" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_Char" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Char" /> object. </param>
        /// <param name="y">A <see cref="_Char" /> object. </param>
        public static _Boolean operator >=(_Char x, _Char y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<Char?, bool?>
        {
            public EqualExpression(Column<char?> x, Column<char?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(Char? x, Char? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault() == y.GetValueOrDefault();
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Char" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Char" /> object. </param>
        /// <param name="y">A <see cref="_Char" /> object. </param>
        public static _Boolean operator ==(_Char x, _Char y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<Char?, bool?>
        {
            public NotEqualExpression(Column<char?> x, Column<char?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(Char? x, Char? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault() != y.GetValueOrDefault();
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Char" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Char" /> object. </param>
        /// <param name="y">A <see cref="_Char" /> object. </param>
        public static _Boolean operator !=(_Char x, _Char y)
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
