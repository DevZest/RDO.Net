using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Guid"/> column.
    /// </summary>
    public sealed class _Guid : Column<Guid?>, IColumn<DbReader, Guid?>
    {
        private sealed class CastToStringExpression : CastExpression<Guid?, String>
        {
            public CastToStringExpression(Column<Guid?> x)
                : base(x)
            {
            }

            protected override String Cast(Guid? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString() : null;
            }
        }

        /// <inheritdoc/>
        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        /// <summary>Converts the supplied <see cref="_Guid" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        public static explicit operator _String(_Guid x)
        {
            x.VerifyNotNull(nameof(x));
            return x.CastToString();
        }

        /// <inheritdoc/>
        public override bool AreEqual(Guid? x, Guid? y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected sealed override Column<Guid?> CreateParam(Guid? value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected internal sealed override Column<Guid?> CreateConst(Guid? value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(Guid? value)
        {
            return JsonValue.Guid(value);
        }

        /// <inheritdoc/>
        protected internal override Guid? DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : new Guid?(new Guid(value.Text));
        }

        /// <summary>Gets the value of this column from <see cref="DbReader"/>'s current row.</summary>
        /// <param name="reader">The <see cref="DbReader"/> object.</param>
        /// <returns>The value of this column from <see cref="DbReader"/>'s current row.</returns>
        public Guid? this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private Guid? GetValue(DbReader reader)
        {
            return reader.GetGuid(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc/>
        protected internal override bool IsNull(Guid? value)
        {
            return !value.HasValue;
        }

        /// <summary>Creates a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <param name="sourceColumn">The value which will be passed to <see cref="DbParamExpression.SourceColumn"/>.</param>
        /// <returns>The column of parameter expression.</returns>
        public static _Guid Param(Guid? x, _Guid sourceColumn = null)
        {
            return new ParamExpression<Guid?>(x, sourceColumn).MakeColumn<_Guid>();
        }

        /// <summary>Creates a column of constant expression.</summary>
        /// <param name="x">The value of the constant expression.</param>
        /// <returns>The column of constant expression.</returns>
        public static _Guid Const(Guid? x)
        {
            return new ConstantExpression<Guid?>(x).MakeColumn<_Guid>();
        }

        /// <summary>Implicitly converts the supplied value to a column of parameter expression.</summary>
        /// <param name="x">The value of the parameter expression.</param>
        /// <returns>The column of parameter expression.</returns>
        public static implicit operator _Guid(Guid? x)
        {
            return Param(x);
        }

        private sealed class FromStringCast : CastExpression<String, Guid?>
        {
            public FromStringCast(Column<String> x)
                : base(x)
            {
            }

            protected override Guid? Cast(String value)
            {
                if (value == null)
                    return null;
                return Guid.Parse(value);
            }
        }

        /// <summary>Converts the supplied <see cref="_String" /> to <see cref="_Guid" />.</summary>
        /// <returns>A <see cref="_Guid" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        public static explicit operator _Guid(_String x)
        {
            x.VerifyNotNull(nameof(x));
            return new FromStringCast(x).MakeColumn<_Guid>();
        }

        private sealed class LessThanExpression : BinaryExpression<Guid?, bool?>
        {
            public LessThanExpression(Column<Guid?> x, Column<Guid?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(Guid? x, Guid? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) < 0;
            }
        }

        /// <summary>Compares the two <see cref="_Guid" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_Guid" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        /// <param name="y">A <see cref="_Guid" /> object. </param>
        public static _Boolean operator <(_Guid x, _Guid y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class LessThanOrEqualExpression : BinaryExpression<Guid?, bool?>
        {
            public LessThanOrEqualExpression(Column<Guid?> x, Column<Guid?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(Guid? x, Guid? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) <= 0;
            }
        }

        /// <summary>Compares the two <see cref="_Guid" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_Guid" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        /// <param name="y">A <see cref="_Guid" /> object. </param>
        public static _Boolean operator <=(_Guid x, _Guid y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanExpression : BinaryExpression<Guid?, bool?>
        {
            public GreaterThanExpression(Column<Guid?> x, Column<Guid?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(Guid? x, Guid? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) > 0;
            }
        }

        /// <summary>Compares the two <see cref="_Guid" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_Guid" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        /// <param name="y">A <see cref="_Guid" /> object. </param>
        public static _Boolean operator >(_Guid x, _Guid y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Guid?, bool?>
        {
            public GreaterThanOrEqualExpression(Column<Guid?> x, Column<Guid?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(Guid? x, Guid? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault().CompareTo(y.GetValueOrDefault()) >= 0;
            }
        }

        /// <summary>Compares the two <see cref="_Guid" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_Guid" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        /// <param name="y">A <see cref="_Guid" /> object. </param>
        public static _Boolean operator >=(_Guid x, _Guid y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class EqualExpression : BinaryExpression<Guid?, bool?>
        {
            public EqualExpression(Column<Guid?> x, Column<Guid?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(Guid? x, Guid? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault() == y.GetValueOrDefault();
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Guid" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        /// <param name="y">A <see cref="_Guid" /> object. </param>
        public static _Boolean operator ==(_Guid x, _Guid y)
        {
            x.VerifyNotNull(nameof(x));
            y.VerifyNotNull(nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        private sealed class NotEqualExpression : BinaryExpression<Guid?, bool?>
        {
            public NotEqualExpression(Column<Guid?> x, Column<Guid?> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(Guid? x, Guid? y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return x.GetValueOrDefault() != y.GetValueOrDefault();
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_Guid" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        /// <param name="y">A <see cref="_Guid" /> object. </param>
        public static _Boolean operator !=(_Guid x, _Guid y)
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

        private class NewGuidFunction : ScalarFunctionExpression<Guid?>
        {
            public override Guid? this[DataRow dataRow]
            {
                get { return Guid.NewGuid(); }
            }

            protected override FunctionKey FunctionKey
            {
                get { return FunctionKeys.NewGuid; }
            }
        }

        /// <summary>
        /// Gets a column that will return a new GUID value.
        /// </summary>
        /// <returns>The column.</returns>
        public static _Guid NewGuid()
        {
            return new NewGuidFunction().MakeColumn<_Guid>();
        }
    }
}
