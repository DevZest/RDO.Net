using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Text;

namespace DevZest.Data
{
    partial class Column<T>
    {
        protected abstract class ConverterBase<TColumn> : ColumnConverter<TColumn>
            where TColumn : Column<T>, new()
        {
            public sealed override Type DataType
            {
                get { return typeof(T); }
            }

            internal sealed override void WriteJsonContent(object obj, StringBuilder stringBuilder)
            {
                BuildJson((TColumn)obj, stringBuilder);
            }

            private void BuildJson(TColumn column, StringBuilder stringBuilder)
            {
                if (column.IsExpression)
                {
                    JsonHelper.WriteObjectName(stringBuilder, ColumnJsonParser.EXPRESSION);
                    var expression = column.Expression;
                    var converter = ColumnConverter.Get(expression);
                    converter.WriteJson(expression, stringBuilder);
                }
                else
                {
                    JsonHelper.WriteObjectName(stringBuilder, ColumnJsonParser.NAME);
                    JsonValue.String(column.Name).Write(stringBuilder);
                }
            }

            internal sealed override Column ParseJson(Model model, ColumnJsonParser parser)
            {
                return parser.Parse(model);
            }
        }
    }
}
