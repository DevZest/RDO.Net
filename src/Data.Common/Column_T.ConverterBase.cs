using DevZest.Data.Primitives;
using System;
using System.Text;

namespace DevZest.Data
{
    partial class Column<T>
    {
        protected abstract class ConverterBase<TColumn> : ColumnConverter
            where TColumn : Column<T>, new()
        {
            public sealed override Type ColumnType
            {
                get { return typeof(TColumn); }
            }

            public sealed override Type DataType
            {
                get { return typeof(T); }
            }

            internal sealed override Column MakeColumn(ColumnExpression expression)
            {
                var result = new TColumn();
                expression.SetOwner(result);
                return result;
            }

            internal sealed override void WriteExpressionJson(JsonWriter jsonWriter, Column column)
            {
                jsonWriter.WriteObjectName(JsonColumn.EXPRESSION).WriteExpression(((Column<T>)column).Expression);
            }
        }
    }
}
