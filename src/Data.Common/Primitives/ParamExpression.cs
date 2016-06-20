using System;
using System.Text;

namespace DevZest.Data.Primitives
{
    [ExpressionConverterGenerics(typeof(ParamExpression<>.Converter<>), TypeId = "ParamExpression")]
    public sealed class ParamExpression<T> : ValueExpression<T>
    {
        private const string SOURCE_COLUMN = nameof(SourceColumn);

        private sealed class Converter<TColumn> : ConverterBase<TColumn>
            where TColumn : Column<T>, new()
        {
            internal override void WriteJson(StringBuilder stringBuilder, ColumnExpression expression)
            {
                base.WriteJson(stringBuilder, expression);
                stringBuilder.WriteComma().WriteObjectName(SOURCE_COLUMN);
                var sourceColumn = ((ParamExpression<T>)expression).SourceColumn;
                if (sourceColumn == null)
                    stringBuilder.WriteValue(JsonValue.Null);
                else
                    stringBuilder.WriteColumn(sourceColumn);
            }

            internal override ValueExpression<T> ParseJson(Model model, ColumnJsonParser parser, T value)
            {
                parser.ExpectComma();
                var sourceColumn = parser.ParseNameColumnPair<Column<T>>(SOURCE_COLUMN, model, true);
                return new ParamExpression<T>(value, sourceColumn);
            }
        }

        public ParamExpression(T value, Column<T> sourceColumn)
            : base(value)
        {
            SourceColumn = sourceColumn;
        }

        public Column<T> SourceColumn { get; private set; }

        public override DbExpression GetDbExpression()
        {
            return new DbParamExpression(Owner, SourceColumn, Value);
        }
    }
}
