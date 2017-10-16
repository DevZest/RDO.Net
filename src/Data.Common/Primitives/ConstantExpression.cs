using System;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents a constant value expression.</summary>
    /// <typeparam name="T">The data type.</typeparam>
    public sealed class ConstantExpression<T> : ValueExpression<T>
    {
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
