using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents a CASE ON...WHEN...THEN...ELSE... expression.</summary>
    /// <typeparam name="TWhen">Data type of ON... and WHEN... expression.</typeparam>
    /// <typeparam name="TResult">Data type of result.</typeparam>
    public sealed class CaseOnExpression<TWhen, TResult> : ColumnExpression<TResult>
    {
        internal CaseOnExpression(Column<TWhen> on)
        {
            _on = on;
        }

        private Column<TWhen> _on;

        private List<Column<TWhen>> _when = new List<Column<TWhen>>();

        private List<Column<TResult>> _then = new List<Column<TResult>>();

        private Column<TResult> _else;

        /// <inheritdoc/>
        protected internal sealed override TResult this[DataRow dataRow]
        {
            get
            {
                var onValue = _on[dataRow];
                for (int i = 0; i < _when.Count; i++)
                {
                    var whenValue = _when[i][dataRow];
                    if (EqualityComparer<TWhen>.Default.Equals(onValue, whenValue))
                        return _then[i][dataRow];
                }

                return _else[dataRow];
            }
        }

        /// <inheritdoc/>
        protected internal sealed override TResult Eval()
        {
            var onValue = _on.Eval();
            for (int i = 0; i < _when.Count; i++)
            {
                var whenValue = _when[i].Eval();
                if (EqualityComparer<TWhen>.Default.Equals(onValue, whenValue))
                    return _then[i].Eval();
            }

            return _else.Eval();
        }

        /// <summary>Builds the WHEN...THEN... expression.</summary>
        /// <param name="condition">The WHEN... expression.</param>
        /// <param name="value">The THEN... expression.</param>
        /// <returns>This <see cref="CaseOnExpression{TWhen, TResult}"/> object for fluent build.</returns>
        public CaseOnExpression<TWhen, TResult> When(Column<TWhen> condition, Column<TResult> value)
        {
            Check.NotNull(condition, nameof(condition));
            Check.NotNull(value, nameof(value));

            if (_else != null)
                throw new InvalidOperationException(Strings.Case_WhenAfterElse);

            _when.Add(condition);
            _then.Add(value);
            return this;
        }

        /// <summary>Builds ELSE... expression.</summary>
        /// <typeparam name="TColumn">Type of result column.</typeparam>
        /// <param name="value">The ELSE... expression.</param>
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
            return new DbCaseExpression(_on.DbExpression,
                _when.Select(x => x.DbExpression),
                _then.Select(x => x.DbExpression),
                _else.DbExpression);
        }

        /// <inheritdoc/>
        protected override IModelSet GetParentModelSet()
        {
            var result = _on.ParentModelSet;
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
            var result = _on.AggregateModelSet;
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
            var expr = (CaseOnExpression<TWhen, TResult>)this.MemberwiseClone();
            expr._on = _on.GetCounterpart(model);
            expr._when = _when.GetCounterpart(model);
            expr._then = _then.GetCounterpart(model);
            if (_else != null)
                expr._else = _else.GetCounterpart(model);
            return GetCounterpart(expr);
        }
    }
}
