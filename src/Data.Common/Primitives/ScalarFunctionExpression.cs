using System;

namespace DevZest.Data.Primitives
{
    public abstract class ScalarFunctionExpression<T> : FunctionExpression<T>
    {
        protected ScalarFunctionExpression(params Column[] parameters)
            : base(parameters)
        {
        }

        protected override IModels GetScalarSourceModels()
        {
            if (Parameters == null)
                return Models.Empty;

            var result = Models.Empty;
            foreach (var column in Parameters)
            {
                if (column == null)
                    continue;

                result = result.Union(column.ScalarSourceModels);
            }

            return result.Seal();
        }

        protected override IModels GetAggregateBaseModels()
        {
            if (Parameters == null)
                return Models.Empty;

            var result = Models.Empty;
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
