namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents expression which contains a value.
    /// </summary>
    /// <typeparam name="T">Data type of the value.</typeparam>
    public abstract class ValueExpression<T> : ColumnExpression<T>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ValueExpression{T}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        protected ValueExpression(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the value of this expression.
        /// </summary>
        public T Value { get; private set; }

        /// <inheritdoc/>
        public sealed override T this[DataRow dataRow]
        {
            get { return Value; }
        }

        /// <inheritdoc/>
        protected sealed override IColumns GetBaseColumns()
        {
            return Columns.Empty;
        }

        /// <inheritdoc/>
        protected sealed override IModels GetScalarSourceModels()
        {
            return Models.Empty;
        }

        /// <inheritdoc/>
        protected sealed override IModels GetAggregateBaseModels()
        {
            return Models.Empty;
        }

        /// <inheritdoc/>
        protected internal override ColumnExpression PerformTranslateTo(Model model)
        {
            return this;
        }
    }
}
