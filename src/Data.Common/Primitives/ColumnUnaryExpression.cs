using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Primitives
{
    /// <summary>Column expression with single operand.</summary>
    /// <typeparam name="T">Data type of the column.</typeparam>
    public abstract class ColumnUnaryExpression<T> : ColumnExpression<T>
    {
        /// <summary>Initializes a new instance of <see cref="ColumnUnaryExpression{T}"/> object.</summary>
        /// <param name="operand">The operand.</param>
        protected ColumnUnaryExpression(Column<T> operand)
        {
            Check.NotNull(operand, nameof(operand));
            Operand = operand;
        }

        /// <summary>Gets the operand of this expression.</summary>
        public Column<T> Operand { get; private set; }

        /// <inheritdoc/>
        protected override IModelSet GetParentModelSet()
        {
            return Operand.ParentModel;
        }

        /// <inheritdoc/>
        protected override IModelSet GetAggregateModelSet()
        {
            return Operand.AggregateModelSet;
        }

        /// <summary>Gets the kind of the unary expression.</summary>
        protected abstract DbUnaryExpressionKind ExpressionKind { get; }

        /// <inheritdoc/>
        public override DbExpression GetDbExpression()
        {
            return new DbUnaryExpression(ExpressionKind, Operand.DbExpression);
        }

        /// <inheritdoc/>
        protected internal sealed override T this[DataRow dataRow]
        {
            get { return EvalCore(Operand[dataRow]); }
        }

        /// <inheritdoc/>
        protected internal sealed override T Eval()
        {
            return EvalCore(Operand.Eval());
        }

        /// <summary>Evaluates against the given value.</summary>
        /// <param name="x">The given value.</param>
        /// <returns>The result.</returns>
        protected abstract T EvalCore(T x);

        internal sealed override Column<T> GetCounterpart(Model model)
        {
            var expr = (ColumnUnaryExpression<T>)this.MemberwiseClone();
            expr.Operand = Operand.GetCounterpart(model);
            return GetCounterpart(expr);
        }
    }
}
