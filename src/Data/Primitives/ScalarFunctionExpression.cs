namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents scalar function expression.
    /// </summary>
    /// <typeparam name="T">Data type of the column.</typeparam>
    public abstract class ScalarFunctionExpression<T> : FunctionExpression<T>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ScalarFunctionExpression{T}"/> class.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        protected ScalarFunctionExpression(params Column[] parameters)
            : base(parameters)
        {
        }

        /// <inheritdoc/>
        protected override IModels GetScalarSourceModels()
        {
            if (Parameters == null)
                return Models.Empty;

            var result = Models.Empty;
            foreach (var column in Parameters)
            {
                if (column == null)
                    continue;

                result = result.Union(column.ScalarSourceModels);
            }

            return result.Seal();
        }

        /// <inheritdoc/>
        protected override IModels GetAggregateBaseModels()
        {
            if (Parameters == null)
                return Models.Empty;

            var result = Models.Empty;
            foreach (var column in Parameters)
            {
                if (column == null)
                    continue;

                result = result.Union(column.AggregateSourceModels);
            }

            return result.Seal();
        }
    }
}
