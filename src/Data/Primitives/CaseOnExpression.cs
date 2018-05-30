using DevZest.Data.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents a CASE ON...WHEN...THEN...ELSE... expression.</summary>
    /// <typeparam name="TOn">Data type of ON... and WHEN... expression.</typeparam>
    /// <typeparam name="TResult">Data type of result.</typeparam>
    public sealed class CaseOnExpression<TOn, TResult> : ColumnExpression<TResult>
    {
        internal CaseOnExpression(Column<TOn> on)
        {
            _on = on;
            _when = new List<Column<TOn>>();
            _then = new List<Column<TResult>>();
        }

        private CaseOnExpression(Column<TOn> on, List<Column<TOn>> when, List<Column<TResult>> then, Column<TResult> elseExpr)
        {
            _on = on;
            _when = when;
            _then = then;
            _else = elseExpr;
        }

        private Column<TOn> _on;
        private List<Column<TOn>> _when;
        private List<Column<TResult>> _then;
        private Column<TResult> _else;

        protected sealed override IColumns GetBaseColumns()
        {
            var result = _on.BaseColumns;
            for (int i = 0; i < _when.Count; i++)
                result = result.Union(_when[i].BaseColumns);
            for (int i = 0; i < _then.Count; i++)
                result = result.Union(_then[i].BaseColumns);
            result = result.Union(_else.BaseColumns);
            return result.Seal();
        }

        /// <inheritdoc/>
        public sealed override TResult this[DataRow dataRow]
        {
            get
            {
                var onValue = _on[dataRow];
                for (int i = 0; i < _when.Count; i++)
                {
                    var whenValue = _when[i][dataRow];
                    if (EqualityComparer<TOn>.Default.Equals(onValue, whenValue))
                        return _then[i][dataRow];
                }

                return _else[dataRow];
            }
        }

        public CaseOnWhen<TOn, TResult> When(Column<TOn> when)
        {
            when.VerifyNotNull(nameof(when));
            return new CaseOnWhen<TOn, TResult>(this, when);
        }

        internal CaseOnExpression<TOn, TResult> WhenThen(Column<TOn> when, Column<TResult> then)
        {
            when.VerifyNotNull(nameof(when));
            then.VerifyNotNull(nameof(then));

            _when.Add(when);
            _then.Add(then);
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
        protected override IModels GetScalarSourceModels()
        {
            var result = _on.ScalarSourceModels;
            for (int i = 0; i < _when.Count; i++)
            {
                result = result.Union(_when[i].ScalarSourceModels);
                result = result.Union(_then[i].ScalarSourceModels);
            }
            result = result.Union(_else.ScalarSourceModels);
            return result.Seal();
        }

        /// <inheritdoc/>
        protected override IModels GetAggregateBaseModels()
        {
            var result = _on.AggregateSourceModels;
            for (int i = 0; i < _when.Count; i++)
            {
                result = result.Union(_when[i].AggregateSourceModels);
                result = result.Union(_then[i].AggregateSourceModels);
            }
            result = result.Union(_else.AggregateSourceModels);
            return result.Seal();
        }

        protected internal override ColumnExpression PerformTranslateTo(Model model)
        {
            var on = _on.TranslateTo(model);
            var when = _when.TranslateToColumns(model);
            var then = _then.TranslateToColumns(model);
            var elseExpr = _else.TranslateTo(model);
            if (on != _on || when != _when || then != _then || elseExpr != _else)
                return new CaseOnExpression<TOn, TResult>(on, when, then, elseExpr);
            else
                return this;
        }
    }
}
