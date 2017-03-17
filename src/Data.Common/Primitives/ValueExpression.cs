using DevZest.Data.Utilities;
using System;
using System.Text;

namespace DevZest.Data.Primitives
{
    public abstract class ValueExpression<T> : ColumnExpression<T>
    {
        private const string VALUE = nameof(Value);

        internal abstract class ConverterBase<TColumn> : ExpressionConverter
            where TColumn : Column<T>, new()
        {
            private static readonly TColumn s_column = new TColumn();

            internal override void WriteJson(JsonWriter jsonWriter, ColumnExpression expression)
            {
                var valueExpression = (ValueExpression<T>)expression;
                jsonWriter.WriteNameValuePair(VALUE, s_column.SerializeValue(valueExpression.Value));
            }

            internal sealed override ColumnExpression ParseJson(JsonParser parser, Model model)
            {
                var value = parser.ParseNameValuePair(VALUE, s_column);
                return ParseJson(parser, model, value);
            }

            internal abstract ValueExpression<T> ParseJson(JsonParser jsonParser, Model model, T value);
        }

        protected ValueExpression(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }

        protected internal sealed override T this[DataRow dataRow]
        {
            get { return Value; }
        }

        protected sealed override IColumnSet GetBaseColumns()
        {
            return ColumnSet.Empty;
        }

        protected sealed override IModelSet GetScalarSourceModels()
        {
            return ModelSet.Empty;
        }

        protected sealed override IModelSet GetAggregateBaseModels()
        {
            return ModelSet.Empty;
        }

        protected internal sealed override Type[] ArgColumnTypes
        {
            get { return new Type[] { Owner.GetType(), Owner.GetType() }; }
        }
    }
}
