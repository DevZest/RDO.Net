using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Linq;

namespace DevZest.Data.AspNetCore
{
    public class DataSetValidatorProvider : IModelValidatorProvider
    {
        public void CreateValidators(ModelValidatorProviderContext context)
        {
            var modelType = context.ModelMetadata.ModelType;
            if (modelType.IsGenericType &&
                modelType.GetGenericTypeDefinition() == typeof(DataSet<>) &&
                !context.Results.Any(x => x.Validator.GetType() == typeof(DataSetValidator)))
            {
                var validator = new DataSetValidator();
                context.Results.Add(new ValidatorItem
                {
                    Validator = validator,
                    IsReusable = true
                });
            }
        }
    }
}
