using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Guid"/> column.
    /// </summary>
    [ColumnConverter(typeof(Converter))]
    public sealed class _Guid : Column<Guid?>, IColumn<DbReader, Guid?>
    {
        private sealed class Converter : ConverterBase<_Guid>
        {
        }

        [ExpressionConverterNonGenerics(typeof(CastToStringExpression.Converter), Id = "_Guid.CastToString")]
        private sealed class CastToStringExpression : CastExpression<Guid?, String>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<Guid?, string> MakeExpression(Column<Guid?> operand)
                {
                    return new CastToStringExpression(operand);
                }
            }

            public CastToStringExpression(Column<Guid?> x)
                : base(x)
            {
            }

            protected override String Cast(Guid? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString() : null;
            }
        }

        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        /// <summary>Converts the supplied <see cref="_Guid" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Guid" /> object. </param>
        public static explicit operator _String(_Guid x)
        {
            Check.NotNull(x, nameof(x));
            return x.CastToString();
        }

        protected override bool AreEqual(Guid? x, Guid? y)
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

        /// <inheritdoc cref="P:DevZest.Data._Binary.Item(DevZest.Data.DbReader)"/>
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

        /// <inheritdoc cref="_Binary.Param(Binary, _Binary)"/>
        public static _Guid Param(Guid? x, _Guid sourceColumn = null)
        {
            return new ParamExpression<Guid?>(x, sourceColumn).MakeColumn<_Guid>();
        }

        /// <inheritdoc cref="_Binary.Const(Binary)"/>
        public static _Guid Const(Guid? x)
        {
            return new ConstantExpression<Guid?>(x).MakeColumn<_Guid>();
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        public static implicit operator _Guid(Guid? x)
        {
            return Param(x);
        }

        [ExpressionConverterNonGenerics(typeof(FromStringCast.Converter), Id = "_Guid.FromString")]
        private sealed class FromStringCast : CastExpression<String, Guid?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<string, Guid?> MakeExpression(Column<string> operand)
                {
                    return new FromStringCast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromStringCast(x).MakeColumn<_Guid>();
        }

        [ExpressionConverterNonGenerics(typeof(LessThanExpression.Converter), Id = "_Guid.LessThan")]
        private sealed class LessThanExpression : BinaryExpression<Guid?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<Guid?, bool?> MakeExpression(Column<Guid?> left, Column<Guid?> right)
                {
                    return new LessThanExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(LessThanOrEqualExpression.Converter), Id = "_Guid.LessThanOrEqual")]
        private sealed class LessThanOrEqualExpression : BinaryExpression<Guid?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<Guid?, bool?> MakeExpression(Column<Guid?> left, Column<Guid?> right)
                {
                    return new LessThanOrEqualExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(GreaterThanExpression.Converter), Id = "_Guid.GreaterThan")]
        private sealed class GreaterThanExpression : BinaryExpression<Guid?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<Guid?, bool?> MakeExpression(Column<Guid?> left, Column<Guid?> right)
                {
                    return new GreaterThanExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(GreaterThanOrEqualExpression.Converter), Id = "_Guid.GreaterThanOrEqual")]
        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Guid?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<Guid?, bool?> MakeExpression(Column<Guid?> left, Column<Guid?> right)
                {
                    return new GreaterThanOrEqualExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(EqualExpression.Converter), Id = "_Guid.Equal")]
        private sealed class EqualExpression : BinaryExpression<Guid?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<Guid?, bool?> MakeExpression(Column<Guid?> left, Column<Guid?> right)
                {
                    return new EqualExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(NotEqualExpression.Converter), Id = "_Guid.NotEqual")]
        private sealed class NotEqualExpression : BinaryExpression<Guid?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<Guid?, bool?> MakeExpression(Column<Guid?> left, Column<Guid?> right)
                {
                    return new NotEqualExpression(left, right);
                }
            }

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
