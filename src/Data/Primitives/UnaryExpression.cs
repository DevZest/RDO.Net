using System;

namespace DevZest.Data.Primitives
{
    /// <summary>Column expression with single operand.</summary>
    /// <typeparam name="T">Data type of the column.</typeparam>
    public abstract class UnaryExpression<T> : ColumnExpression<T>
    {
        /// <summary>Initializes a new instance of <see cref="UnaryExpression{T}"/> object.</summary>
        /// <param name="operand">The operand.</param>
        protected UnaryExpression(Column<T> operand)
        {
            operand.VerifyNotNull(nameof(operand));
            Operand = operand;
        }

        /// <summary>Gets the operand of this expression.</summary>
        public Column<T> Operand { get; private set; }

        protected sealed override IColumns GetBaseColumns()
        {
            return Operand.BaseColumns;
        }

        /// <inheritdoc/>
        protected override IModels GetScalarSourceModels()
        {
            return Operand.ParentModel;
        }

        /// <inheritdoc/>
        protected override IModels GetAggregateBaseModels()
        {
            return Operand.AggregateSourceModels;
        }

        /// <summary>Gets the kind of the unary expression.</summary>
        protected abstract DbUnaryExpressionKind ExpressionKind { get; }

        /// <inheritdoc/>
        public override DbExpression GetDbExpression()
        {
            return new DbUnaryExpression(ExpressionKind, Operand.DbExpression);
        }

        /// <inheritdoc/>
        public sealed override T this[DataRow dataRow]
        {
            get { return EvalCore(Operand[dataRow]); }
        }

        /// <summary>Evaluates against the given value.</summary>
        /// <param name="x">The given value.</param>
        /// <returns>The result.</returns>
        protected abstract T EvalCore(T x);

        protected internal sealed override ColumnExpression PerformTranslateTo(Model model)
        {
            var operand = Operand.TranslateTo(model);
            if (operand == Operand)
                return this;
            else
                return (ColumnExpression)Activator.CreateInstance(GetType(), operand);
        }
    }
}
