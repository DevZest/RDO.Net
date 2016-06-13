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

        private sealed class FromStringCast : CastExpression<String, Char?>
        {
            public FromStringCast(_String x)
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

        private sealed class LessThanExpression : BinaryExpression<Char?, bool?>
        {
            public LessThanExpression(_Char x, _Char y)
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

        private sealed class LessThanOrEqualExpression : BinaryExpression<Char?, bool?>
        {
            public LessThanOrEqualExpression(_Char x, _Char y)
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

        private sealed class GreaterThanExpression : BinaryExpression<Char?, bool?>
        {
            public GreaterThanExpression(_Char x, _Char y)
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

        private sealed class GreaterThanOrEqualExpression : BinaryExpression<Char?, bool?>
        {
            public GreaterThanOrEqualExpression(_Char x, _Char y)
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

        [ExpressionConverterNonGenerics(typeof(EqualExpression.Converter), TypeId = "_Char.Equal")]
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

        private sealed class NotEqualExpression : BinaryExpression<Char?, bool?>
        {
            public NotEqualExpression(_Char x, _Char y)
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
