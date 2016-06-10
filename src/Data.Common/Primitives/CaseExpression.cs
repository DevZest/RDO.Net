using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents a CASE WHEN..ELSE... expression.</summary>
    /// <typeparam name="TResult">The data type of the result.</typeparam>
    [GenericExpressionConverter(typeof(CaseExpression<>.Converter<>))]
    public sealed class CaseExpression<TResult> : ColumnExpression<TResult>
    {
        private const string WHEN = "When";
        private const string THEN = "Then";
        private const string ELSE = "Else";

        private sealed class Converter<TColumn> : GenericExpressionConverter<TColumn>
            where TColumn : Column<TResult>, new()
        {
            protected sealed override void WritePropertiesCore(StringBuilder stringBuilder, ColumnExpression<TResult> expression)
            {
                var caseExpression = (CaseExpression<TResult>)expression;
                stringBuilder.WriteNameColumnsPair(WHEN, caseExpression._when).WriteComma()
                    .WriteNameColumnsPair(THEN, caseExpression._then).WriteComma()
                    .WriteNameColumnPair(ELSE, caseExpression._else);
            }

            internal override Column ParseJson(Model model, ColumnJsonParser parser)
            {
                var when = parser.ParseNameColumnsPair<_Boolean>(WHEN, model);
                var then = parser.ParseNameColumnsPair<Column<TResult>>(THEN, model);
                var elseExpr = (Column<TResult>)parser.ParseNameColumnPair<Column<TResult>>(ELSE, model);
                return new CaseExpression<TResult>(when, then, elseExpr).MakeColumn(elseExpr.GetType());
            }
        }

        internal CaseExpression()
        {
            _when = new List<_Boolean>();
            _then = new List<Column<TResult>>();
        }

        private CaseExpression(List<_Boolean> when, List<Column<TResult>> then, Column<TResult> elseExpr)
        {
            _when = when;
            _then = then;
            _else = elseExpr;
        }

        private List<_Boolean> _when;
        private List<Column<TResult>> _then;
        private Column<TResult> _else;

        /// <summary>Builds the WHEN...THEN... expression.</summary>
        /// <typeparam name="TColumn">The column type of the result.</typeparam>
        /// <param name="value">Value of the Else... expression.</param>
        /// <returns>The result column expression.</returns>
        public CaseExpression<TResult> WhenThen(_Boolean when, Column<TResult> then)
        {
            Check.NotNull(when, nameof(when));
            Check.NotNull(then, nameof(then));
            _when.Add(when);
            _then.Add(then);
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
