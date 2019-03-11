using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DevZest.Data.AspNetCore
{
    public class DataSetValidator : IModelValidator
    {
        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            var dataSet = (DataSet)context.Model;

            var results = dataSet.Validate();
            return results.Count == 0 ? Enumerable.Empty<ModelValidationResult>() : GetModelValidationResults(results);
        }

        private static IEnumerable<ModelValidationResult> GetModelValidationResults(IDataValidationResults results)
        {
            throw new System.NotImplementedException();
        }
    }
}
