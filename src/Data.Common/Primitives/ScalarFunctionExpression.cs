using System;

namespace DevZest.Data.Primitives
{
    public abstract class ScalarFunctionExpression<T> : FunctionExpression<T>
    {
        protected ScalarFunctionExpression(params Column[] parameters)
            : base(parameters)
        {
        }

        protected override IModelSet GetParentModelSet()
        {
            if (Parameters == null)
                return ModelSet.Empty;

            var result = ModelSet.Empty;
            foreach (var column in Parameters)
            {
                if (column == null)
                    continue;

                result = result.Union(column.ParentModelSet);
            }

            return result;
        }

        protected override IModelSet GetAggregateModelSet()
        {
            if (Parameters == null)
                return ModelSet.Empty;

            var result = ModelSet.Empty;
            foreach (var column in Parameters)
            {
                if (column == null)
                    continue;

                result = result.Union(column.AggregateModelSet);
            }

            return result;
        }
    }
}
