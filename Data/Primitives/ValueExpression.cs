using System;

namespace DevZest.Data.Primitives
{
    public abstract class ValueExpression<T> : ColumnExpression<T>
    {
        protected ValueExpression(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }

        public sealed override T Eval(DataRow dataRow)
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
    }
}
