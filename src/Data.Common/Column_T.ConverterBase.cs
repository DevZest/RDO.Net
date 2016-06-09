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

            internal sealed override void WritePropertiesJson(StringBuilder stringBuilder, object obj)
            {
                var column = (TColumn)obj;
                if (column.IsExpression)
                    stringBuilder.WritePair(ColumnJsonParser.EXPRESSION, sb => WriteExpression(sb, column.Expression));
                else
                    stringBuilder.WriteNameStringPair(ColumnJsonParser.NAME, column.Name);
            }

            private void WriteExpression(StringBuilder stringBuilder, ColumnExpression<T> expression)
            {
                var converter = ColumnConverter.Get(expression);
                converter.WriteJson(stringBuilder, expression);
            }

            internal sealed override Column ParseJson(Model model, ColumnJsonParser parser)
            {
                return parser.ParseColumn(model);
            }
        }
    }
}
