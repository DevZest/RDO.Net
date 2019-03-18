using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.AspNetCore
{
    public class DataSetValidatorProvider : IModelValidatorProvider
    {
        internal sealed class DataSetValidator : IModelValidator
        {
            public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
            {
                var prefix = context.ModelMetadata.Name;
                var dataSet = (DataSet)context.Model;
                var isScalar = context.ModelMetadata.IsScalar();

                if (isScalar)
                    return ValidateScalar(prefix, dataSet);
                else
                    return ValidateCollection(prefix, dataSet);
            }

            private static IEnumerable<ModelValidationResult> ValidateScalar(string prefix, DataSet dataSet)
            {
                return dataSet.Count > 0 ? Validate(prefix, dataSet[0]) : Enumerable.Empty<ModelValidationResult>();
            }

            private static IEnumerable<ModelValidationResult> ValidateCollection(string prefix, DataSet dataSet)
            {
                for (int index = 0; index < dataSet.Count; index++)
                {
                    var dataRowPrefix = ModelNames.CreateIndexModelName(prefix, index);
                    foreach (var result in Validate(dataRowPrefix, dataSet[index]))
                        yield return result;
                }
            }

            private static IEnumerable<ModelValidationResult> Validate(string prefix, DataRow dataRow)
            {
                var errors = dataRow.Validate();
                foreach (var error in errors)
                    yield return ToModelValidationResult(prefix, error);

                IReadOnlyList<DataSet> childDataSets = dataRow.ChildDataSets;
                foreach (var childDataSet in childDataSets)
                {
                    var childDataSetPrefix = ModelNames.CreatePropertyModelName(prefix, childDataSet.Model.GetName());
                    foreach (var result in ValidateCollection(childDataSetPrefix, childDataSet))
                        yield return result;
                }
            }

            private static ModelValidationResult ToModelValidationResult(string prefix, DataValidationError error)
            {
                var columns = error.Source;
                var columnName = columns.Count > 0 ? string.Empty : columns.Single().Name;
                var memberName = ModelNames.CreatePropertyModelName(prefix, columnName);
                return new ModelValidationResult(memberName, error.Message);
            }
        }

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
    }
}
