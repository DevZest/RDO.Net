using System;

namespace DevZest.Data.Primitives
{
    public abstract class ScalarFunctionExpression<T> : FunctionExpression<T>
    {
        protected ScalarFunctionExpression(params Column[] parameters)
            : base(parameters)
        {
        }

        protected override IModelSet GetScalarBaseModels()
        {
            if (Parameters == null)
                return ModelSet.Empty;

            var result = ModelSet.Empty;
            foreach (var column in Parameters)
            {
                if (column == null)
                    continue;

                result = result.Union(column.ScalarBaseModels);
            }

            return result.Seal();
        }

        protected override IModelSet GetAggregateBaseModels()
        {
            if (Parameters == null)
                return ModelSet.Empty;

            var result = ModelSet.Empty;
            foreach (var column in Parameters)
            {
                if (column == null)
                    continue;

                result = result.Union(column.AggregateBaseModels);
            }

            return result.Seal();
        }
    }
}
