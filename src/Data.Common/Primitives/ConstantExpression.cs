using System;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents a constant value expression.</summary>
    /// <typeparam name="T">The data type.</typeparam>
    [ExpressionConverterGenerics(typeof(ConstantExpression<>.Converter<>), Id = "ConstantExpression")]
    public sealed class ConstantExpression<T> : ValueExpression<T>
    {
        private sealed class Converter<TColumn> : ConverterBase<TColumn>
            where TColumn : Column<T>, new()
        {
            internal override ValueExpression<T> ParseJson(JsonParser jsonParser, Model model, T value)
            {
                return new ParamExpression<T>(value, null);
            }
        }

        /// <summary>Initializes a new instance of <see cref="ConstantExpression{T}"/> object.</summary>
        /// <param name="value">The constant value.</param>
        public ConstantExpression(T value)
            : base(value)
        {
        }

        /// <inheritdoc/>
        public override DbExpression GetDbExpression()
        {
            object exprValue;
            if (Owner.IsNull(Value))
                exprValue = null;
            else
                exprValue = Value;
            return new DbConstantExpression(Owner, exprValue);
        }
    }
}
