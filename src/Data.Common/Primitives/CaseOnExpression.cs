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
    [ExpressionConverterGenerics(typeof(CaseOnExpression<,>.Converter), Id = "CaseOnExpression")]
    public sealed class CaseOnExpression<TOn, TResult> : ColumnExpression<TResult>
    {
        private sealed class Converter : ExpressionConverter
        {
            private const string On = "On";
            private const string WHEN = "When";
            private const string THEN = "Then";
            private const string ELSE = "Else";

            internal sealed override void WriteJson(JsonWriter jsonWriter, ColumnExpression expression)
            {
                var caseOnExpression = (CaseOnExpression<TOn, TResult>)expression;
                jsonWriter.WriteNameColumnPair(On, caseOnExpression._on).WriteComma()
                    .WriteNameColumnsPair(WHEN, caseOnExpression._when).WriteComma()
                    .WriteNameColumnsPair(THEN, caseOnExpression._then).WriteComma()
                    .WriteNameColumnPair(ELSE, caseOnExpression._else);
            }

            internal override ColumnExpression ParseJson(JsonParser jsonParser, Model model)
            {
                var on = jsonParser.ParseNameColumnPair<Column<TOn>>(On, model);
                jsonParser.ExpectComma();
                var when = jsonParser.ParseNameColumnsPair<Column<TOn>>(WHEN, model);
                jsonParser.ExpectComma();
                var then = jsonParser.ParseNameColumnsPair<Column<TResult>>(THEN, model);
                jsonParser.ExpectComma();
                if (when.Count == 0 || when.Count != then.Count)
                    throw new FormatException(Strings.Case_WhenThenNotMatch);
                var elseExpr = (Column<TResult>)jsonParser.ParseNameColumnPair<Column<TResult>>(ELSE, model);
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

        protected sealed override IColumnSet GetBaseColumns()
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
        protected override IModelSet GetScalarSourceModels()
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
        protected override IModelSet GetAggregateBaseModels()
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

        protected internal override Type[] ArgColumnTypes
        {
            get { return new Type[] { _on.GetType(), _else.GetType() }; }
        }
    }
}
