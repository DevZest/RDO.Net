using DevZest.Data.Utilities;
using System;
using System.Text;

namespace DevZest.Data.Primitives
{
    public abstract class ValueExpression<T> : ColumnExpression<T>
    {
        private const string VALUE = nameof(Value);

        protected abstract class ConverterBase<TColumn> : AbstractConverter
            where TColumn : Column<T>, new()
        {
            private static readonly TColumn s_column = new TColumn();

            protected sealed override void WritePropertiesCore(StringBuilder stringBuilder, ColumnExpression<T> expression)
            {
                var valueExpression = (ValueExpression<T>)expression;
                stringBuilder.WriteNameValuePair(VALUE, s_column.SerializeValue(valueExpression.Value));
            }

            internal sealed override ColumnExpression ParseJson(Model model, ColumnJsonParser parser)
            {
                var value = parser.ParseNameValuePair(VALUE, s_column);
                return MakeExpression(value);
            }

            protected abstract ValueExpression<T> MakeExpression(T value);
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

        protected internal sealed override T Eval()
        {
            return Value;
        }

        protected sealed override IModelSet GetParentModelSet()
        {
            return ModelSet.Empty;
        }

        protected sealed override IModelSet GetAggregateModelSet()
        {
            return ModelSet.Empty;
        }

        internal sealed override Type[] ArgColumnTypes
        {
            get { return new Type[] { Owner.GetType(), Owner.GetType() }; }
        }
    }
}
