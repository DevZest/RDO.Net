using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Char"/> column.
    /// </summary>
    [ColumnConverter(typeof(Converter))]    
    public sealed class _Char : Column<Char?>, IColumn<DbReader, Char?>
    {
        private sealed class Converter : ConverterBase<_Char>
        {
        }

        [ExpressionConverterNonGenerics(typeof(CastToStringExpression.Converter), Id = "_Char.CastToString")]
        private sealed class CastToStringExpression : CastExpression<Char?, String>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<char?, string> MakeExpression(Column<char?> operand)
                {
                    return new CastToStringExpression(operand);
                }
            }

            public CastToStringExpression(Column<Char?> x)
                : base(x)
            {
            }

            protected override String Cast(Char? value)
            {
                return value.HasValue ? value.GetValueOrDefault().ToString() : null;
            }
        }

        public override _String CastToString()
        {
            return new CastToStringExpression(this).MakeColumn<_String>();
        }

        /// <summary>Converts the supplied <see cref="_Char" /> to <see cref="_String" />.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_Char" /> object. </param>
        public static explicit operator _String(_Char x)
        {
            Check.NotNull(x, nameof(x));
            return x.CastToString();
        }

        /// <inheritdoc/>
        protected sealed override Column<char?> CreateParam(char? value)
        {
            return Param(value, this);
        }

        protected override bool AreEqual(char? x, char? y)
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

        /// <inheritdoc cref="P:DevZest.Data._Binary.Item(DevZest.Data.DbReader)"/>
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

        /// <inheritdoc cref="_Binary.Param(Binary, _Binary)"/>
        public static _Char Param(Char? x, _Char sourceColumn = null)
        {
            return new ParamExpression<Char?>(x, sourceColumn).MakeColumn<_Char>();
        }

        /// <inheritdoc cref="_Binary.Const(Binary)"/>
        public static _Char Const(char? x)
        {
            return new ConstantExpression<Char?>(x).MakeColumn<_Char>();
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        public static implicit operator _Char(Char? x)
        {
            return Param(x);
        }

        [ExpressionConverterNonGenerics(typeof(FromStringCast.Converter), Id = "_Char.FromString")]
        private sealed class FromStringCast : CastExpression<String, Char?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<string, char?> MakeExpression(Column<string> operand)
                {
                    return new FromStringCast(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromStringCast(x).MakeColumn<_Char>();
        }

        [ExpressionConverterNonGenerics(typeof(LessThanExpression.Converter), Id = "_Char.LessThan")]
        private sealed class LessThanExpression : BinaryExpression<Char?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<char?, bool?> MakeExpression(Column<char?> left, Column<char?> right)
                {
                    return new LessThanExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(LessThanOrEqualExpression.Converter), Id = "_Char.LessThanOrEqual")]
        private sealed class LessThanOrEqualExpression : BinaryExpression<Char?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<char?, bool?> MakeExpression(Column<char?> left, Column<char?> right)
                {
                    return new LessThanOrEqualExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(GreaterThanExpression.Converter), Id = "_Char.GreaterThan")]
        private sealed class GreaterThanExpression : BinaryExpression<Char?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<char?, bool?> MakeExpression(Column<char?> left, Column<char?> right)
                {
                    return new GreaterThanExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(GreaterThanOrEqualExpression.Converter), Id = "_Char.GreaterThanOrEqual")]
        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Char?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<char?, bool?> MakeExpression(Column<char?> left, Column<char?> right)
                {
                    return new GreaterThanOrEqualExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(EqualExpression.Converter), Id = "_Char.Equal")]
        private sealed class EqualExpression : BinaryExpression<Char?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<char?, bool?> MakeExpression(Column<char?> left, Column<char?> right)
                {
                    return new EqualExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(NotEqualExpression.Converter), Id = "_Char.NotEqual")]
        private sealed class NotEqualExpression : BinaryExpression<Char?, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<char?, bool?> MakeExpression(Column<char?> left, Column<char?> right)
                {
                    return new NotEqualExpression(left, right);
                }
            }

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
