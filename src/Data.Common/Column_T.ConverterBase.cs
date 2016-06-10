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
                    stringBuilder.WriteObjectName(ColumnJsonParser.EXPRESSION).WriteExpression(column.Expression);
                else
                    stringBuilder.WriteNameStringPair(ColumnJsonParser.NAME, column.Name);
            }

            internal sealed override Column ParseJson(Model model, ColumnJsonParser parser)
            {
                return parser.ParseColumn(model);
            }
        }
    }
}
