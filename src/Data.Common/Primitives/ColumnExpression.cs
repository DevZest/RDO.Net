using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public abstract class ColumnExpression
    {
        internal abstract void SetOwner(Column column);

        private IColumns _baseColumns;
        public IColumns BaseColumns
        {
            get { return _baseColumns ?? (_baseColumns = GetBaseColumns().Seal()); }
        }

        protected abstract IColumns GetBaseColumns();

        private IModels _scalarSourceModels;
        /// <summary>
        /// Gets the set of parent models involved in this expression.
        /// </summary>
        public IModels ScalarSourceModels
        {
            get { return _scalarSourceModels ?? (_scalarSourceModels = GetScalarSourceModels().Seal()); }
        }

        /// <summary>
        /// Gets the set of parent models involved in this expression.
        /// </summary>
        /// <returns>The set of parent models involved in this expression.</returns>
        protected abstract IModels GetScalarSourceModels();

        private IModels _aggregateSourceModels;
        /// <summary>
        /// Gets the set of aggregated models involved in this expression.
        /// </summary>
        public IModels AggregateSourceModels
        {
            get { return _aggregateSourceModels ?? (_aggregateSourceModels = GetAggregateBaseModels().Seal()); }
        }

        /// <summary>
        /// Gets the set of aggregated models involved in this expression.
        /// </summary>
        /// <returns>The set of aggregated models involved in this expression.</returns>
        protected abstract IModels GetAggregateBaseModels();

        /// <summary>
        /// Gets the <see cref="DbExpression"/> object which can be used for database command generation.
        /// </summary>
        /// <returns>The <see cref="DbExpression"/> object.</returns>
        public abstract DbExpression GetDbExpression();

        protected internal abstract ColumnExpression PerformTranslateTo(Model model);
    }

    /// <summary>
    /// Represents the expression of <see cref="Column{T}"/>.
    /// </summary>
    /// <typeparam name="T">Data type of the expression.</typeparam>
    public abstract class ColumnExpression<T> : ColumnExpression
    {
        protected ColumnExpression()
        {
        }

        public abstract T this[DataRow dataRow] { get; }

        private Column<T> _owner;
        /// <summary>
        /// Gets the <see cref="Column{T}" /> object which owns this expression.
        /// </summary>
        public Column<T> Owner
        {
            get { return _owner; }
            private set
            {
                Debug.Assert(_owner == null && value.Expression == null);
                _owner = value;
                value.Expression = this;
            }
        }

        /// <summary>
        /// Makes a new <see cref="Column{T}"/> object which contains this expression.
        /// </summary>
        /// <typeparam name="TColumn">The actual type of the column.</typeparam>
        /// <returns>The new <see cref="Column{T}"/> object which contains this expression.</returns>
        public TColumn MakeColumn<TColumn>()
            where TColumn : Column<T>, new()
        {
            if (Owner != null)
                throw new InvalidOperationException(Strings.ColumnExpression_AlreadyAttached);

            var result = new TColumn();
            Owner = result;
            return result;
        }

        internal sealed override void SetOwner(Column column)
        {
            Owner = (Column<T>)column;
        }
    }
}
