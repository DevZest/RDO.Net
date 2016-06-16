using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents a CASE ON...WHEN...THEN...ELSE... expression.</summary>
    /// <typeparam name="TOn">Data type of ON... and WHEN... expression.</typeparam>
    /// <typeparam name="TResult">Data type of result.</typeparam>
    [ExpressionConverterGenerics(typeof(CaseOnExpression<,>.Converter), TypeId = "CaseOnExpression")]
    public sealed class CaseOnExpression<TOn, TResult> : ColumnExpression<TResult>
    {
        private sealed class Converter : ExpressionConverter
        {
            private const string On = "On";
            private const string WHEN = "When";
            private const string THEN = "Then";
            private const string ELSE = "Else";

            internal sealed override void WriteJson(StringBuilder stringBuilder, ColumnExpression expression)
            {
                var caseOnExpression = (CaseOnExpression<TOn, TResult>)expression;
                stringBuilder.WriteNameColumnPair(On, caseOnExpression._on).WriteComma()
                    .WriteNameColumnsPair(WHEN, caseOnExpression._when).WriteComma()
                    .WriteNameColumnsPair(THEN, caseOnExpression._then).WriteComma()
                    .WriteNameColumnPair(ELSE, caseOnExpression._else);
            }

            internal override ColumnExpression ParseJson(Model model, ColumnJsonParser parser)
            {
                var on = parser.ParseNameColumnPair<Column<TOn>>(On, model);
                parser.ExpectComma();
                var when = parser.ParseNameColumnsPair<Column<TOn>>(WHEN, model);
                parser.ExpectComma();
                var then = parser.ParseNameColumnsPair<Column<TResult>>(THEN, model);
                parser.ExpectComma();
                if (when.Count == 0 || when.Count != then.Count)
                    throw new FormatException(Strings.Case_WhenThenNotMatch);
                var elseExpr = (Column<TResult>)parser.ParseNameColumnPair<Column<TResult>>(ELSE, model);
                return new CaseOnExpression<TOn, TResult>(on, when, then, elseExpr);
            }
        }

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

        /// <inheritdoc/>
        protected internal sealed override TResult this[DataRow dataRow]
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

        /// <inheritdoc/>
        protected internal sealed override TResult Eval()
        {
            var onValue = _on.Eval();
            for (int i = 0; i < _when.Count; i++)
            {
                var whenValue = _when[i].Eval();
                if (EqualityComparer<TOn>.Default.Equals(onValue, whenValue))
                    return _then[i].Eval();
            }

            return _else.Eval();
        }

        public CaseOnWhen<TOn, TResult> When(Column<TOn> when)
        {
            Check.NotNull(when, nameof(when));
            return new CaseOnWhen<TOn, TResult>(this, when);
        }

        internal CaseOnExpression<TOn, TResult> WhenThen(Column<TOn> when, Column<TResult> then)
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
            var expr = (CaseOnExpression<TOn, TResult>)this.MemberwiseClone();
            expr._on = _on.GetCounterpart(model);
            expr._when = _when.GetCounterpart(model);
            expr._then = _then.GetCounterpart(model);
            if (_else != null)
                expr._else = _else.GetCounterpart(model);
            return GetCounterpart(expr);
        }

        protected internal override Type[] ArgColumnTypes
        {
            get { return new Type[] { _on.GetType(), _else.GetType() }; }
        }
    }
}
