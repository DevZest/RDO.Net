using System;

namespace DevZest.Data.Primitives
{
    public abstract class ScalarFunctionExpression<T> : FunctionExpression<T>
    {
        protected ScalarFunctionExpression(params Column[] parameters)
            : base(parameters)
        {
        }

        protected override IModelSet GetScalarSourceModels()
        {
            if (Parameters == null)
                return ModelSet.Empty;

            var result = ModelSet.Empty;
            foreach (var column in Parameters)
            {
                if (column == null)
                    continue;

                result = result.Union(column.ScalarSourceModels);
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

                result = result.Union(column.AggregateSourceModels);
            }

            return result.Seal();
        }
    }
}
