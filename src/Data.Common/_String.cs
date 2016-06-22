using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Data.Common;
using System.Globalization;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a string column.
    /// </summary>
    [ColumnConverter(typeof(Converter))]
    public sealed class _String : Column<String>, IColumn<DbReader, String>
    {
        private sealed class Converter : ConverterBase<_String>
        {
        }

        protected override bool AreEqual(string x, string y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        protected override Column<string> CreateParam(string value)
        {
            return Param(value, this);
        }

        /// <inheritdoc/>
        protected internal override Column<string> CreateConst(string value)
        {
            return Const(value);
        }

        /// <inheritdoc/>
        protected internal override JsonValue SerializeValue(String value)
        {
            return JsonValue.String(value);
        }

        /// <inheritdoc/>
        protected internal override String DeserializeValue(JsonValue value)
        {
            return value.Type == JsonValueType.Null ? null : value.Text;
        }

        /// <inheritdoc cref="P:DevZest.Data._Binary.Item(DevZest.Data.DbReader)"/>
        public String this[DbReader reader]
        {
            get
            {
                VerifyDbReader(reader);
                return GetValue(reader);
            }
        }

        private String GetValue(DbReader reader)
        {
            return reader.GetString(Ordinal);
        }

        void IColumn<DbReader>.Read(DbReader reader, DataRow dataRow)
        {
            this[dataRow] = GetValue(reader);
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        protected internal override bool IsNull(string value)
        {
            return value == null;
        }

        /// <inheritdoc cref="_Binary.Param(Binary, _Binary)"/>
        public static _String Param(String x, _String sourceColumn = null)
        {
            return new ParamExpression<String>(x, sourceColumn).MakeColumn<_String>();
        }

        /// <inheritdoc cref="_Binary.Const(Binary)"/>
        public static _String Const(String x)
        {
            return new ConstantExpression<String>(x).MakeColumn<_String>();
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        public static implicit operator _String(String x)
        {
            return Param(x);
        }

        [ExpressionConverterNonGenerics(typeof(AddExpression.Converter), Id = "_String.Add")]
        private sealed class AddExpression : BinaryExpression<String>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<string, string> MakeExpression(Column<string> left, Column<string> right)
                {
                    return new AddExpression(left, right);
                }
            }

            public AddExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Add; }
            }

            protected override String EvalCore(String x, String y)
            {
                if (x != null && y != null)
                    return x + y;
                else
                    return null;
            }
        }

        /// <summary>Computes the sum of the two specified <see cref="_String" /> objects.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _String operator +(_String x, _String y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new AddExpression(x, y).MakeColumn<_String>();
        }

        [ExpressionConverterNonGenerics(typeof(LessThanExpression.Converter), Id = "_String.LessThan")]
        private sealed class LessThanExpression : BinaryExpression<String, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<string, bool?> MakeExpression(Column<string> left, Column<string> right)
                {
                    return new LessThanExpression(left, right);
                }
            }

            public LessThanExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThan; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) < 0;
            }
        }

        /// <summary>Compares the two <see cref="_String" /> parameters to determine whether the first is less than the second.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator <(_String x, _String y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(LessThanOrEqualExpression.Converter), Id = "_String.LessThanOrEqual")]
        private sealed class LessThanOrEqualExpression : BinaryExpression<String, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<string, bool?> MakeExpression(Column<string> left, Column<string> right)
                {
                    return new LessThanOrEqualExpression(left, right);
                }
            }

            public LessThanOrEqualExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.LessThanOrEqual; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) <= 0;
            }
        }

        /// <summary>Compares the two <see cref="_String" /> parameters to determine whether the first is less than or equal the second.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator <=(_String x, _String y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new LessThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(GreaterThanExpression.Converter), Id = "_String.GreaterThan")]
        private sealed class GreaterThanExpression : BinaryExpression<String, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<string, bool?> MakeExpression(Column<string> left, Column<string> right)
                {
                    return new GreaterThanExpression(left, right);
                }
            }

            public GreaterThanExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThan; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) > 0;
            }
        }

        /// <summary>Compares the two <see cref="_String" /> parameters to determine whether the first is greater than the second.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator >(_String x, _String y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(GreaterThanOrEqualExpression.Converter), Id = "_String.GreaterThanOrEqual")]
        private sealed class GreaterThanOrEqualExpression : BinaryExpression<String, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<string, bool?> MakeExpression(Column<string> left, Column<string> right)
                {
                    return new GreaterThanOrEqualExpression(left, right);
                }
            }

            public GreaterThanOrEqualExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.GreaterThanOrEqual; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) >= 0;
            }
        }

        /// <summary>Compares the two <see cref="_String" /> parameters to determine whether the first is greater than or equal the second.</summary>
        /// <returns>A <see cref="_String" /> expression which contains the result.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator >=(_String x, _String y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new GreaterThanOrEqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(EqualExpression.Converter), Id = "_String.Equal")]
        private sealed class EqualExpression : BinaryExpression<String, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<string, bool?> MakeExpression(Column<string> left, Column<string> right)
                {
                    return new EqualExpression(left, right);
                }
            }

            public EqualExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.Equal; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) == 0;
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_String" /> parameters for equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator ==(_String x, _String y)
        {
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));

            return new EqualExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(NotEqualExpression.Converter), Id = "_String.NotEqual")]
        private sealed class NotEqualExpression : BinaryExpression<String, bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<string, bool?> MakeExpression(Column<string> left, Column<string> right)
                {
                    return new NotEqualExpression(left, right);
                }
            }

            public NotEqualExpression(Column<String> x, Column<String> y)
                : base(x, y)
            {
            }

            protected override BinaryExpressionKind Kind
            {
                get { return BinaryExpressionKind.NotEqual; }
            }

            protected override bool? EvalCore(String x, String y)
            {
                if (x == null || y == null)
                    return null;
                else
                    return String.Compare(x, y) != 0;
            }
        }

        /// <summary>Performs a logical comparison of the two <see cref="_String" /> parameters for non-equality.</summary>
        /// <returns>The result <see cref="_Boolean" /> expression.</returns>
        /// <param name="x">A <see cref="_String" /> object. </param>
        /// <param name="y">A <see cref="_String" /> object. </param>
        public static _Boolean operator !=(_String x, _String y)
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

        public override _String CastToString()
        {
            return this;
        }
    }
}
