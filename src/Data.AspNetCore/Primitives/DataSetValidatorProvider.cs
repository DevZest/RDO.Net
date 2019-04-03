using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.AspNetCore.Primitives
{
    public class DataSetValidatorProvider : IMetadataBasedModelValidatorProvider
    {
        public void CreateValidators(ModelValidatorProviderContext context)
        {
            var modelMetadata = context.ModelMetadata;
            var modelType = modelMetadata.ModelType;
            if (modelType.IsDataSet())
            {
                if (!context.Results.Any(x => x.Validator.GetType() == typeof(DataSetValidator)))
                {
                    context.Results.Add(new ValidatorItem
                    {
                        Validator = new DataSetValidator(),
                        IsReusable = true
                    });
                }
            }
        }

        public bool HasValidators(Type modelType, IList<object> validatorMetadata)
        {
            return modelType.IsDataSet();
        }
    }
}
