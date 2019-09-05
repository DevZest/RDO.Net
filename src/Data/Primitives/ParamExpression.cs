namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents a parameter column expression.
    /// </summary>
    /// <typeparam name="T">Data type of the column.</typeparam>
    public sealed class ParamExpression<T> : ValueExpression<T>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ParamExpression{T}"/> class.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="sourceColumn">The source column.</param>
        public ParamExpression(T value, Column<T> sourceColumn)
            : base(value)
        {
            SourceColumn = sourceColumn;
        }

        /// <summary>
        /// Gets the source column.
        /// </summary>
        public Column<T> SourceColumn { get; private set; }

        /// <inheritdoc/>
        public override DbExpression GetDbExpression()
        {
            object exprValue;
            if (Owner.IsNull(Value))
                exprValue = null;
            else
                exprValue = Value;
            return new DbParamExpression(Owner, SourceColumn, exprValue);
        }
    }
}
