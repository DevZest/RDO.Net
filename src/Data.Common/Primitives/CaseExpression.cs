using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DevZest.Data.Primitives
{
    /// <summary>Represents a CASE WHEN..ELSE... expression.</summary>
    /// <typeparam name="TResult">The data type of the result.</typeparam>
    [ExpressionConverterGenerics(typeof(CaseExpression<>.Converter), Id = "CaseExpression")]
    public sealed class CaseExpression<TResult> : ColumnExpression<TResult>
    {
        private sealed class Converter : ExpressionConverter
        {
            private const string WHEN = "When";
            private const string THEN = "Then";
            private const string ELSE = "Else";

            internal sealed override void WriteJson(JsonWriter jsonWriter, ColumnExpression expression)
            {
                var caseExpression = (CaseExpression<TResult>)expression;
                jsonWriter.WriteNameColumnsPair(WHEN, caseExpression._when).WriteComma()
                    .WriteNameColumnsPair(THEN, caseExpression._then).WriteComma()
                    .WriteNameColumnPair(ELSE, caseExpression._else);
            }

            internal override ColumnExpression ParseJson(JsonParser jsonParser, Model model)
            {
                var when = jsonParser.ParseNameColumnsPair<_Boolean>(WHEN, model);
                jsonParser.ExpectComma();
                var then = jsonParser.ParseNameColumnsPair<Column<TResult>>(THEN, model);
                jsonParser.ExpectComma();
                if (when.Count == 0 || when.Count != then.Count)
                    throw new FormatException(Strings.Case_WhenThenNotMatch);
                var elseExpr = (Column<TResult>)jsonParser.ParseNameColumnPair<Column<TResult>>(ELSE, model);
                return new CaseExpression<TResult>(when, then, elseExpr);
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

        protected sealed override IColumnSet GetBaseColumns()
        {
            var result = ColumnSet.Empty;
            for (int i = 0; i < _when.Count; i++)
                result = result.Union(_when[i].BaseColumns);
            for (int i = 0; i < _then.Count; i++)
                result = result.Union(_then[i].BaseColumns);
            result = result.Union(_else.BaseColumns);
            return result.Seal();
        }

        public CaseWhen<TResult> When(_Boolean when)
        {
            Check.NotNull(when, nameof(when));
            return new CaseWhen<TResult>(this, when);
        }

        internal CaseExpression<TResult> WhenThen(_Boolean when, Column<TResult> then)
        {
            Debug.Assert(when != null);
            Debug.Assert(then != null);
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
                result = result.Union(_when[i].ParentModelSet);
                result = result.Union(_then[i].ParentModelSet);
            }
            result = result.Union(_else.ParentModelSet);
            return result.Seal();
        }

        /// <inheritdoc/>
        protected override IModelSet GetAggregateModelSet()
        {
            var result = ModelSet.Empty;
            for (int i = 0; i < _when.Count; i++)
            {
                result = result.Union(_when[i].AggregateModelSet);
                result = result.Union(_then[i].AggregateModelSet);
            }
            result = result.Union(_else.AggregateModelSet);
            return result;
        }

        protected internal override Type[] ArgColumnTypes
        {
            get { return new Type[] { Owner.GetType() }; }
        }
    }
}
