using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents a CASE WHEN..ELSE... expression.</summary>
    /// <typeparam name="TResult">The data type of the result.</typeparam>
    public sealed class CaseExpression<TResult> : ColumnExpression<TResult>
    {
        internal CaseExpression()
        {
        }

        private List<Column<bool?>> _when = new List<Column<bool?>>();

        private List<Column<TResult>> _then = new List<Column<TResult>>();

        private Column<TResult> _else;

        /// <inheritdoc/>
        protected internal sealed override TResult this[DataRow dataRow]
        {
            get
            {
                for (int i = 0; i < _when.Count; i++)
                {
                    var whenValue = _when[i][dataRow];
                    if (whenValue == true)
                        return _then[i][dataRow];
                }

                return _else[dataRow];
            }
        }

        /// <inheritdoc/>
        protected internal sealed override TResult Eval()
        {
            for (int i = 0; i < _when.Count; i++)
            {
                var whenValue = _when[i].Eval();
                if (whenValue == true)
                    return _then[i].Eval();
            }

            return _else.Eval();
        }

        /// <summary>Builds the WHEN...THEN... expression.</summary>
        /// <param name="condition">Condition of the WHEN... expression.</param>
        /// <param name="value">value of the THEN... expression.</param>
        /// <returns>This <see cref="CaseExpression{TResult}"/> for fluent build.</returns>
        public CaseExpression<TResult> Then(_Boolean condition, Column<TResult> value)
        {
            Check.NotNull(condition, nameof(condition));
            Check.NotNull(value, nameof(value));

            if (_else != null)
                throw new InvalidOperationException(Strings.Case_WhenAfterElse);

            _when.Add(condition);
            _then.Add(value);
            return this;
        }

        /// <summary>Builds the ELSE... expression.</summary>
        /// <typeparam name="TColumn">The column type of the result.</typeparam>
        /// <param name="value">Value of the Else... expression.</param>
        /// <returns>The result column expression.</returns>
        public TColumn Else<TColumn>(TColumn value)
            where TColumn : Column<TResult>, new()
        {
            _else = value;
            return this.MakeColumn<TColumn>();
        }

        /// <inheritdoc/>
        public override DbExpression GetDbExpression()
        {
            return new DbCaseExpression(null,
                _when.Select(x => x.DbExpression),
                _then.Select(x => x.DbExpression),
                _else.DbExpression);
        }

        /// <inheritdoc/>
        protected override IModelSet GetParentModelSet()
        {
            var result = ModelSet.Empty;
            for (int i = 0; i < _when.Count; i++)
            {
                result.Union(_when[i].ParentModelSet);
                result.Union(_then[i].ParentModelSet);
            }
            result.Union(_else.ParentModelSet);
            return result;
        }

        /// <inheritdoc/>
        protected override IModelSet GetAggregateModelSet()
        {
            var result = ModelSet.Empty;
            for (int i = 0; i < _when.Count; i++)
            {
                result.Union(_when[i].AggregateModelSet);
                result.Union(_then[i].AggregateModelSet);
            }
            result.Union(_else.AggregateModelSet);
            return result;
        }

        internal sealed override Column<TResult> GetCounterpart(Model model)
        {
            var expr = (CaseExpression<TResult>)this.MemberwiseClone();
            expr._when = _when.GetCounterpart(model);
            expr._then = _then.GetCounterpart(model);
            if (_else != null)
                expr._else = _else.GetCounterpart(model);

            return GetCounterpart(expr);
        }
    }
}
