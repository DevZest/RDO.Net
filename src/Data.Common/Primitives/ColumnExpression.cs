using DevZest.Data.Utilities;
using System;
using System.Text;

namespace DevZest.Data.Primitives
{
    /// <summary>
    /// Represents the expression of <see cref="Column{T}"/>.
    /// </summary>
    /// <typeparam name="T">Data type of the expression.</typeparam>
    public abstract class ColumnExpression<T>
    {
        protected abstract class ExpressionConverter<TColumn> : ColumnConverter<TColumn>
            where TColumn : Column, new()
        {
            public sealed override Type DataType
            {
                get { return typeof(T); }
            }
        }

        protected abstract class GenericExpressionConverter<TColumn> : ExpressionConverter<TColumn>
            where TColumn : Column, new()
        {
            internal sealed override void WriteJsonContent(object obj, StringBuilder stringBuilder)
            {
                JsonHelper.WriteObjectName(stringBuilder, ColumnJsonParser.TYPE_ARG_ID);
                stringBuilder.Append(ColumnConverter.GetTypeId<TColumn>());
                stringBuilder.Append(',');
                WriteJsonContent(stringBuilder, (ColumnExpression<T>)obj);
            }

            protected abstract void WriteJsonContent(StringBuilder stringBuilder, ColumnExpression<T> expression);
        }

        protected internal abstract T this[DataRow dataRow] { get; }

        protected internal abstract T Eval();

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
            if (Owner != null)
                throw new InvalidOperationException(Strings.ColumnExpression_AlreadyAttached);

            var result = new TColumn();
            result.Expression = this;
            Owner = result;
            return result;
        }

        internal abstract Column<T> GetCounterpart(Model model);

        internal Column<T> GetCounterpart(ColumnExpression<T> expr)
        {
            Column<T> result = (Column<T>)Activator.CreateInstance(Owner.GetType());
            result.Expression = expr;
            return result;
        }
    }
}
