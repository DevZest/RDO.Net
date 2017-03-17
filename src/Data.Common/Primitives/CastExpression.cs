using System;
using System.Text;
using DevZest.Data.Utilities;

namespace DevZest.Data.Primitives
{
    /// <summary>Base class for column type cast expression.</summary>
    /// <typeparam name="TSource">Data type of source column.</typeparam>
    /// <typeparam name="TTarget">Data type of target column.</typeparam>
    public abstract class CastExpression<TSource, TTarget> : ColumnExpression<TTarget>
    {
        protected abstract class ConverterBase : ExpressionConverter
        {
            private const string OPERAND = nameof(Operand);

            internal sealed override void WriteJson(JsonWriter jsonWriter, ColumnExpression expression)
            {
                jsonWriter.WriteNameColumnPair(OPERAND, ((CastExpression<TSource, TTarget>)expression).Operand);
            }

            internal sealed override ColumnExpression ParseJson(JsonParser jsonParser, Model model)
            {
                var operand = jsonParser.ParseNameColumnPair<Column<TSource>>(OPERAND, model);
                return MakeExpression(operand);
            }

            protected abstract CastExpression<TSource, TTarget> MakeExpression(Column<TSource> operand);
        }

        /// <summary>Initialize a new instance of <see cref="CastExpression{TSource, TTarget}"/> object.</summary>
        /// <param name="operand">The operand to be casted.</param>
        protected CastExpression(Column<TSource> operand)
        {
            Check.NotNull(operand, nameof(operand));
            Operand = operand;
        }

        protected sealed override IColumnSet GetBaseColumns()
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
        protected internal sealed override TTarget this[DataRow dataRow]
        {
            get { return Cast(Operand[dataRow]); }
        }

        /// <inheritdoc/>
        protected sealed override IModelSet GetScalarSourceModels()
        {
            return Operand.ParentModel;
        }

        /// <inheritdoc/>
        protected sealed override IModelSet GetAggregateBaseModels()
        {
            return Operand.AggregateSourceModels;
        }

        /// <inheritdoc/>
        public sealed override DbExpression GetDbExpression()
        {
            return new DbCastExpression(Operand.DbExpression, typeof(TSource), this.Owner);
        }
    }
}
