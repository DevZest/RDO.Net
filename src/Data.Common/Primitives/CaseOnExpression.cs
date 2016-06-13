using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents a CASE ON...WHEN...THEN...ELSE... expression.</summary>
    /// <typeparam name="TWhen">Data type of ON... and WHEN... expression.</typeparam>
    /// <typeparam name="TResult">Data type of result.</typeparam>
    public sealed class CaseOnExpression<TWhen, TResult> : ColumnExpression<TResult>
    {
        private sealed class Converter : AbstractConverter
        {
            private const string On = "On";
            private const string WHEN = "When";
            private const string THEN = "Then";
            private const string ELSE = "Else";

            protected sealed override void WritePropertiesCore(StringBuilder stringBuilder, ColumnExpression<TResult> expression)
            {
                var caseOnExpression = (CaseOnExpression<TWhen, TResult>)expression;
                stringBuilder.WriteNameColumnPair(On, caseOnExpression._on).WriteComma()
                    .WriteNameColumnsPair(WHEN, caseOnExpression._when).WriteComma()
                    .WriteNameColumnsPair(THEN, caseOnExpression._then).WriteComma()
                    .WriteNameColumnPair(ELSE, caseOnExpression._else);
            }

            internal override ColumnExpression ParseJson(Model model, ColumnJsonParser parser)
            {
                var on = parser.ParseNameColumnPair<Column<TWhen>>(On, model);
                var when = parser.ParseNameColumnsPair<Column<TWhen>>(WHEN, model);
                var then = parser.ParseNameColumnsPair<Column<TResult>>(THEN, model);
                var elseExpr = (Column<TResult>)parser.ParseNameColumnPair<Column<TResult>>(ELSE, model);
                return new CaseOnExpression<TWhen, TResult>(on, when, then, elseExpr);
            }
        }

        internal CaseOnExpression(Column<TWhen> on)
        {
            _on = on;
            _when = new List<Column<TWhen>>();
            _then = new List<Column<TResult>>();
        }

        private CaseOnExpression(Column<TWhen> on, List<Column<TWhen>> when, List<Column<TResult>> then, Column<TResult> elseExpr)
        {
            _on = on;
            _when = when;
            _then = then;
            _else = elseExpr;
        }

        private Column<TWhen> _on;
        private List<Column<TWhen>> _when;
        private List<Column<TResult>> _then;
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

        public CaseOnWhen<TWhen, TResult> When(Column<TWhen> when)
        {
            Check.NotNull(when, nameof(when));
            return new CaseOnWhen<TWhen, TResult>(this, when);
        }

        internal CaseOnExpression<TWhen, TResult> WhenThen(Column<TWhen> when, Column<TResult> then)
        {
            Check.NotNull(when, nameof(when));
            Check.NotNull(then, nameof(then));

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
