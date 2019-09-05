using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents function call in column expression.
    /// </summary>
    /// <typeparam name="T">Data type of the expression.</typeparam>
    public abstract class FunctionExpression<T> : ColumnExpression<T>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FunctionExpression{T}"/> class.
        /// </summary>
        /// <param name="parameters">The parameters of the function call.</param>
        protected FunctionExpression(params Column[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                Parameters = null;
            Parameters = new ReadOnlyCollection<Column>(parameters);
        }

        /// <summary>
        /// Gets the parameters of the function call.
        /// </summary>
        public ReadOnlyCollection<Column> Parameters;

        /// <inheritdoc/>
        protected sealed override IColumns GetBaseColumns()
        {
            var result = Columns.Empty;
            if (Parameters == null)
                return result;

            for (int i = 0; i < Parameters.Count; i++)
                result = result.Union(Parameters[i].BaseColumns);

            return result.Seal();
        }

        /// <summary>
        /// Gets the function key which identifies this function.
        /// </summary>
        protected abstract FunctionKey FunctionKey { get; }

        private DbExpression _dbExpression;
        /// <inheritdoc/>
        public sealed override DbExpression GetDbExpression()
        {
            return LazyInitializer.EnsureInitialized(ref _dbExpression, () => CreateDbExpression());
        }

        private DbExpression CreateDbExpression()
        {
            if (Parameters == null)
                return new DbFunctionExpression(typeof(T), FunctionKey);

            var paramList = new DbExpression[Parameters.Count];
            for (int i = 0; i < Parameters.Count; i++)
            {
                var parameter = Parameters[i];
                paramList[i] = parameter?.DbExpression;
            }

            return new DbFunctionExpression(typeof(T), FunctionKey, paramList);
        }

        /// <inheritdoc/>
        protected sealed internal override ColumnExpression PerformTranslateTo(Model model)
        {
            var parameters = Parameters.TranslateToParams(model);
            if (parameters == null)
                return this;
            else
                return (ColumnExpression)Activator.CreateInstance(GetType(), parameters);
        }
    }
}
