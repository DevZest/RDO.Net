using DevZest.Data.Utilities;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents the expression of <see cref="Column{T}"/>.
    /// </summary>
    /// <typeparam name="T">Data type of the expression.</typeparam>
    public abstract class ColumnExpression<T>
    {
        /// <summary>
        /// Evaluates the expression against provided <see cref="DataRow"/> object.
        /// </summary>
        /// <param name="dataRow">The <see cref="DataRow"/> object.</param>
        /// <returns>The evaluated result.</returns>
        public abstract T Eval(DataRow dataRow);

        /// <summary>
        /// Gets the <see cref="Column{T}" /> object which owns this expression.
        /// </summary>
        public Column<T> Owner { get; internal set; }

        private IModelSet _parentModelSet;
        /// <summary>
        /// Gets the set of parent models involved in this expression.
        /// </summary>
        public IModelSet ParentModelSet
        {
            get { return _parentModelSet ?? (_parentModelSet = GetParentModelSet()); }
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
            get { return _aggregateModelSet ?? (_aggregateModelSet = GetAggregateModelSet()); }
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
            var result = new TColumn();
            result.Expression = this;
            return result;
        }

        internal abstract Column GetParallelColumn(Model model);

        private static ConcurrentDictionary<Type, Func<Type, Column>> s_makeColumnInvokers =
            new ConcurrentDictionary<Type, Func<Type, Column>>();

        internal Column MakeColumn(Type columnType)
        {
            var invoker = s_makeColumnInvokers.GetOrAdd(columnType, BuildMakeColumnInvoker(columnType));
            return invoker(columnType);
        }

        private static Column _MakeColumn<TColumn>(ColumnExpression<T> expr)
            where TColumn : Column<T>, new()
        {
            return expr.MakeColumn<TColumn>();
        }

        private static Func<Type, Column> BuildMakeColumnInvoker(Type columnType)
        {
            var methodInfo = typeof(ColumnExpression<T>).GetStaticMethodInfo(nameof(_MakeColumn));
            methodInfo = methodInfo.MakeGenericMethod(columnType);
            var param = Expression.Parameter(typeof(Type), methodInfo.GetParameters()[0].Name);
            var call = Expression.Call(methodInfo, param);
            return Expression.Lambda<Func<Type, Column>>(call, param).Compile();
        }
    }
}
