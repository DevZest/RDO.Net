using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.Linq;

namespace DevZest.Data.AspNetCore
{
    internal sealed class DataSetValidator : IModelValidator
    {
        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            var modelState = context.ActionContext.ModelState;
            var modelName = context.ModelMetadata.Name ?? string.Empty;
            ResetValidationState(modelState, modelName);

            var dataSet = (DataSet)context.Model;
            var isScalar = context.ModelMetadata.IsScalar();

            if (isScalar)
                return ValidateScalar(dataSet);
            else
                return ValidateCollection(string.Empty, dataSet);
        }

        private static void ResetValidationState(ModelStateDictionary modelState, string key)
        {
            var entries = modelState.FindKeysWithPrefix(key);
            foreach (var item in entries)
            {
                if (item.Value.ValidationState == ModelValidationState.Unvalidated)
                    item.Value.ValidationState = ModelValidationState.Valid;
            }
        }

        private static IEnumerable<ModelValidationResult> ValidateScalar(DataSet dataSet)
        {
            return dataSet.Count > 0 ? Validate(string.Empty, dataSet[0]) : Enumerable.Empty<ModelValidationResult>();
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
            for (int i = 0; i < errors.Count; i++)
            {
                var error = errors[i];
                foreach (var result in ToModelValidationResults(prefix, error))
                    yield return result;
            }

            IReadOnlyList<DataSet> childDataSets = dataRow.ChildDataSets;
            foreach (var childDataSet in childDataSets)
            {
                var childDataSetPrefix = ModelNames.CreatePropertyModelName(prefix, childDataSet.Model.GetName());
                foreach (var result in ValidateCollection(childDataSetPrefix, childDataSet))
                    yield return result;
            }
        }

        private static IEnumerable<ModelValidationResult> ToModelValidationResults(string prefix, DataValidationError error)
        {
            var columns = error.Source;

            if (columns == null || columns.Count == 0)
            {
                yield return new ModelValidationResult(prefix, error.Message);
                yield break;
            }

            foreach (var column in columns)
            {
                var columnName = column.Name;
                var memberName = ModelNames.CreatePropertyModelName(prefix, columnName);
                yield return new ModelValidationResult(memberName, error.Message);
            }
        }
    }
}
