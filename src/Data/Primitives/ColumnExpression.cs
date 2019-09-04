using System;
using System.Diagnostics;
using System.Threading;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represent column expression.
    /// </summary>
    public abstract class ColumnExpression
    {
        internal abstract void SetOwner(Column column);

        /// <summary>
        /// Gets the owner which owns this column expression.
        /// </summary>
        /// <returns>The owner column.</returns>
        public abstract Column GetOwner();

        private IColumns _baseColumns;
        /// <summary>
        /// Gets the base columns to make up this expression.
        /// </summary>
        public IColumns BaseColumns
        {
            get { return LazyInitializer.EnsureInitialized(ref _baseColumns, () => GetBaseColumns().Seal()); }
        }

        /// <summary>
        /// Gets base columns to make up this expression.
        /// </summary>
        /// <returns>The columns set to make up this expression.</returns>
        protected abstract IColumns GetBaseColumns();

        private IModels _scalarSourceModels;
        /// <summary>
        /// Gets the set of parent models involved in this expression.
        /// </summary>
        public IModels ScalarSourceModels
        {
            get { return LazyInitializer.EnsureInitialized(ref _scalarSourceModels, () => GetScalarSourceModels().Seal()); }
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
            get { return LazyInitializer.EnsureInitialized(ref _aggregateSourceModels, () => GetAggregateBaseModels().Seal()); }
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

        /// <summary>
        /// Translates this expression for specified model.
        /// </summary>
        /// <param name="model">The specified model.</param>
        /// <returns>The translated expression.</returns>
        protected internal abstract ColumnExpression PerformTranslateTo(Model model);
    }

    /// <summary>
    /// Represents the expression of <see cref="Column{T}"/>.
    /// </summary>
    /// <typeparam name="T">Data type of the expression.</typeparam>
    public abstract class ColumnExpression<T> : ColumnExpression
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ColumnExpression{T}"/> class.
        /// </summary>
        protected ColumnExpression()
        {
        }

        /// <summary>
        /// Gets the value for specified DataRow.
        /// </summary>
        /// <param name="dataRow">The specified DataRow.</param>
        /// <returns>The value for specified DataRow.</returns>
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
                throw new InvalidOperationException(DiagnosticMessages.ColumnExpression_AlreadyAttached);

            var result = new TColumn();
            Owner = result;
            return result;
        }

        internal sealed override void SetOwner(Column column)
        {
            Owner = (Column<T>)column;
        }

        /// <inheritdoc />
        public sealed override Column GetOwner()
        {
            return Owner;
        }
    }
}
