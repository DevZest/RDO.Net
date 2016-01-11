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
    }
}
