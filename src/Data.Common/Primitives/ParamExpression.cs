using System;
using System.Text;

namespace DevZest.Data.Primitives
{
    [ExpressionConverterGenerics(typeof(ParamExpression<>.Converter<>), Id = "ParamExpression")]
    public sealed class ParamExpression<T> : ValueExpression<T>
    {
        private const string SOURCE_COLUMN = nameof(SourceColumn);

        private sealed class Converter<TColumn> : ConverterBase<TColumn>
            where TColumn : Column<T>, new()
        {
            internal override void WriteJson(JsonWriter jsonWriter, ColumnExpression expression)
            {
                base.WriteJson(jsonWriter, expression);
                jsonWriter.WriteComma().WriteObjectName(SOURCE_COLUMN);
                var sourceColumn = ((ParamExpression<T>)expression).SourceColumn;
                if (sourceColumn == null)
                    jsonWriter.WriteValue(JsonValue.Null);
                else
                    jsonWriter.Write(sourceColumn);
            }

            internal override ValueExpression<T> ParseJson(JsonParser jsonParser, Model model, T value)
            {
                jsonParser.ExpectComma();
                var sourceColumn = jsonParser.ParseNameColumnPair<Column<T>>(SOURCE_COLUMN, model, true);
                return new ParamExpression<T>(value, sourceColumn);
            }
        }

        public ParamExpression(T value, Column<T> sourceColumn)
            : base(value)
        {
            SourceColumn = sourceColumn;
        }

        public sealed override IColumnSet ReferencedColumns
        {
            get { return ColumnSet.Empty; }
        }

        public Column<T> SourceColumn { get; private set; }

        public override DbExpression GetDbExpression()
        {
            object exprValue;
            if (Owner.IsNull(Value))
                exprValue = null;
            else
                exprValue = Value;
            return new DbParamExpression(Owner, SourceColumn, exprValue);
        }
    }
}
