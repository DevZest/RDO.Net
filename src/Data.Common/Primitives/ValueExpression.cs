using DevZest.Data.Utilities;
using System;
using System.Text;

namespace DevZest.Data.Primitives
{
    public abstract class ValueExpression<T> : ColumnExpression<T>
    {
        protected ValueExpression(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }

        public sealed override T this[DataRow dataRow]
        {
            get { return Value; }
        }

        protected sealed override IColumns GetBaseColumns()
        {
            return Columns.Empty;
        }

        protected sealed override IModels GetScalarSourceModels()
        {
            return Models.Empty;
        }

        protected sealed override IModels GetAggregateBaseModels()
        {
            return Models.Empty;
        }

        protected internal sealed override Type[] ArgColumnTypes
        {
            get { return new Type[] { Owner.GetType(), Owner.GetType() }; }
        }
    }
}
