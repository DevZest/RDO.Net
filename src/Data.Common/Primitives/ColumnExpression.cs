using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public abstract class ColumnExpression
    {
        internal abstract void SetOwner(Column column);

        public abstract IColumnSet BaseColumns { get; }
    }

    /// <summary>
    /// Represents the expression of <see cref="Column{T}"/>.
    /// </summary>
    /// <typeparam name="T">Data type of the expression.</typeparam>
    public abstract class ColumnExpression<T> : ColumnExpression
    {
        protected ColumnExpression()
        {
            ExpressionConverter.EnsureInitialized(this);
        }

        protected internal abstract T this[DataRow dataRow] { get; }

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

        private IModelSet _parentModelSet;
        /// <summary>
        /// Gets the set of parent models involved in this expression.
        /// </summary>
        public IModelSet ParentModelSet
        {
            get { return _parentModelSet ?? (_parentModelSet = GetParentModelSet().Seal()); }
        }

        /// <summary>
        /// Gets the set of parent models involved in this expression.
        /// </summary>
        /// <returns>The set of parent models involved in this expression.</returns>
        protected abstract IModelSet GetParentModelSet();

        private IModelSet _aggregateModelSet;
        /// <summary>
        /// Gets the set of aggregated models involved in this expression.
        /// </summary>
        public IModelSet AggregateModelSet
        {
            get { return _aggregateModelSet ?? (_aggregateModelSet = GetAggregateModelSet().Seal()); }
        }

        /// <summary>
        /// Gets the set of aggregated models involved in this expression.
        /// </summary>
        /// <returns>The set of aggregated models involved in this expression.</returns>
        protected abstract IModelSet GetAggregateModelSet();

        /// <summary>
        /// Gets the <see cref="DbExpression"/> object which can be used for database command generation.
        /// </summary>
        /// <returns>The <see cref="DbExpression"/> object.</returns>
        public abstract DbExpression GetDbExpression();

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

        protected internal virtual Type[] ArgColumnTypes
        {
            get { return Array<Type>.Empty; }
        }

        internal sealed override void SetOwner(Column column)
        {
            Owner = (Column<T>)column;
        }
    }
}
