using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data
{
    /// <summary>
    /// Represents a nullable <see cref="Boolean"/> column.
    /// </summary>
    [ColumnConverter(typeof(Converter))]
    public sealed class _Boolean : Column<bool?>, IColumn<DbReader, Boolean?>
    {
        private sealed class Converter : ConverterBase<_Boolean>
        {
        }

        /// <inheritdoc/>
        protected sealed override Column<bool?> CreateParam(bool? value)
        {
            return Param(value, this);
        }

        protected override bool AreEqual(bool? x, bool? y)
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
            throw new FormatException(Strings.BooleanColumn_CannotDeserialize);
        }

        /// <inheritdoc cref="P:DevZest.Data._Binary.Item(DevZest.Data.DbReader)"/>
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

        /// <inheritdoc cref="_Binary.Param(Binary, _Binary)"/>
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

        /// <inheritdoc cref="_Binary.Const(Binary)"/>
        public static _Boolean Const(bool? x)
        {
            return x.HasValue ? (x.GetValueOrDefault() ? True : False) : Null;
        }

        /// <inheritdoc cref="M:DevZest.Data._Binary.op_Implicit(DevZest.Data.Binary)~DevZest.Data._Binary"/>
        public static implicit operator _Boolean(bool? x)
        {
            return Param(x);
        }

        [ExpressionConverterNonGenerics(typeof(NotExpression.Converter), TypeId = "_Boolean.Not")]
        private sealed class NotExpression : UnaryExpression<bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected sealed override UnaryExpression<bool?> MakeExpression(Column<bool?> operand)
                {
                    return new NotExpression(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new NotExpression(x).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(AndExpression.Converter), TypeId = "_Boolean.And")]
        private sealed class AndExpression : BinaryExpression<bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<bool?, bool?> MakeExpression(Column<bool?> left, Column<bool?> right)
                {
                    return new AndExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new AndExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(OrExpression.Converter), TypeId = "_Boolean.Or")]
        private sealed class OrExpression : BinaryExpression<bool?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override BinaryExpression<bool?, bool?> MakeExpression(Column<bool?> left, Column<bool?> right)
                {
                    return new OrExpression(left, right);
                }
            }

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
            Check.NotNull(x, nameof(x));
            Check.NotNull(y, nameof(y));
            return new OrExpression(x, y).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(FromStringExpression.Converter), TypeId = "_Boolean.FromString")]
        private sealed class FromStringExpression : CastExpression<String, Boolean?>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<string, bool?> MakeExpression(Column<string> operand)
                {
                    return new FromStringExpression(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return new FromStringExpression(x).MakeColumn<_Boolean>();
        }

        [ExpressionConverterNonGenerics(typeof(CastToStringExpression.Converter), TypeId = "_Boolean.CastToString")]
        private sealed class CastToStringExpression : CastExpression<Boolean?, String>
        {
            private sealed class Converter : ConverterBase
            {
                protected override CastExpression<bool?, string> MakeExpression(Column<bool?> operand)
                {
                    return new CastToStringExpression(operand);
                }
            }

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
            Check.NotNull(x, nameof(x));
            return x.CastToString();
        }
    }
}
