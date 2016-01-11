using System;
using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    /// <summary>Base class for column type cast expression.</summary>
    /// <typeparam name="TSource">Data type of source column.</typeparam>
    /// <typeparam name="TTarget">Data type of target column.</typeparam>
    public abstract class CastExpression<TSource, TTarget> : ColumnExpression<TTarget>
    {
        /// <summary>Initialize a new instance of <see cref="CastExpression{TSource, TTarget}"/> object.</summary>
        /// <param name="operand">The operand to be casted.</param>
        protected CastExpression(Column<TSource> operand)
        {
            Check.NotNull(operand, nameof(operand));
            Operand = operand;
        }

        /// <summary>Gets the operand to be casted.</summary>
        public Column<TSource> Operand { get; private set; }

        /// <summary>Casts the provided value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The casted result.</returns>
        protected abstract TTarget Cast(TSource value);

        /// <inheritdoc/>
        protected internal sealed override TTarget this[DataRow dataRow]
        {
            get { return Cast(Operand[dataRow]); }
        }

        /// <inheritdoc/>
        protected internal sealed override TTarget Eval()
        {
            return Cast(Operand.Eval());
        }

        /// <inheritdoc/>
        protected sealed override IModelSet GetParentModelSet()
        {
            return Operand.ParentModel;
        }

        /// <inheritdoc/>
        protected sealed override IModelSet GetAggregateModelSet()
        {
            return Operand.AggregateModelSet;
        }

        /// <inheritdoc/>
        public sealed override DbExpression GetDbExpression()
        {
            return new DbCastExpression(Operand.DbExpression, typeof(TSource), this.Owner);
        }

        /// <inheritdoc/>
        internal sealed override Column GetParallelColumn(Model model)
        {
            var expr = (CastExpression<TSource, TTarget>)this.MemberwiseClone();
            expr.Operand = (Column<TSource>)Operand.GetParrallel(model);
            return expr.MakeColumn(Owner.GetType());
        }
    }
}
