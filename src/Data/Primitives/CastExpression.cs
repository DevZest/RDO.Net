using System;

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
            operand.VerifyNotNull(nameof(operand));
            Operand = operand;
        }

        /// <inheritdoc />
        protected sealed override IColumns GetBaseColumns()
        {
            return Operand.BaseColumns;
        }

        /// <summary>Gets the operand to be casted.</summary>
        public Column<TSource> Operand { get; private set; }

        /// <summary>Casts the provided value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The casted result.</returns>
        protected abstract TTarget Cast(TSource value);

        /// <inheritdoc/>
        public sealed override TTarget this[DataRow dataRow]
        {
            get { return Cast(Operand[dataRow]); }
        }

        /// <inheritdoc/>
        protected sealed override IModels GetScalarSourceModels()
        {
            return Operand.ParentModel;
        }

        /// <inheritdoc/>
        protected sealed override IModels GetAggregateBaseModels()
        {
            return Operand.AggregateSourceModels;
        }

        /// <inheritdoc/>
        public sealed override DbExpression GetDbExpression()
        {
            return new DbCastExpression(Operand.DbExpression, Operand, this.Owner);
        }

        /// <inheritdoc />
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
