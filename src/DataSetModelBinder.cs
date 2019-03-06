using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using DevZest.Data.Addons;
using DevZest.Data.Primitives;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Logging;

namespace DevZest.Data.AspNetCore
{
    public class DataSetModelBinder<T> : IModelBinder
        where T : class, IModelReference, new()
    {
        private readonly ILogger _logger;

        public DataSetModelBinder(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger<DataSetModelBinder<T>>();
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProvider = bindingContext.ValueProvider;
            var modelName = bindingContext.ModelName;

            _logger.AttemptingToBindDataSetModel<T>(bindingContext);

            if (!valueProvider.ContainsPrefix(modelName))
                _logger.FoundNoDataSetValueInRequest<T>(bindingContext);
            else
            {
                var dataSet = CreateDataSet(bindingContext);
                Bind(bindingContext.ModelState, valueProvider, modelName, dataSet);
                bindingContext.Result = ModelBindingResult.Success(dataSet);
            }

            _logger.DoneAttemptingToBindDataSetModel<T>(bindingContext);
            return Task.CompletedTask;
        }

        protected virtual DataSet<T> CreateDataSet(ModelBindingContext bindingContext)
        {
            return DataSet<T>.Create();
        }

        private void Bind(ModelStateDictionary modelState, IValueProvider valueProvider, string modelValueKey, DataSet dataSet)
        {
            modelState.SetModelValue(modelValueKey, dataSet, string.Empty);

            var _ = dataSet.Model;

            _.SuspendIdentity();

            for (int i = 0; ; i++)
            {
                var dataRowModelName = ModelNames.CreateIndexModelName(modelValueKey, i);
                if (!valueProvider.ContainsPrefix(dataRowModelName))
                    break;

                BindNewRow(modelState, valueProvider, dataRowModelName, dataSet);
            }

            _.ResumeIdentity();
        }

        private void BindNewRow(ModelStateDictionary modelState, IValueProvider valueProvider, string dataRowModelName, DataSet dataSet)
        {
            DataRow dataRow = dataSet.AddRow();

            modelState.SetModelValue(dataRowModelName, dataRow, string.Empty);

            dataRow.SuspendValueChangedNotification();

            var model = dataSet.Model;
            var columns = model.GetColumns();
            foreach (var column in columns)
                Bind(modelState, valueProvider, dataRowModelName, column, dataRow);

            IReadOnlyList<DataSet> childDataSets = dataRow.ChildDataSets;
            foreach (var childDataSet in childDataSets)
                Bind(modelState, valueProvider, ModelNames.CreatePropertyModelName(dataRowModelName, childDataSet.Model.GetName()), childDataSet);

            dataRow.ResumeValueChangedNotification();
        }

        private void Bind(ModelStateDictionary modelState, IValueProvider valueProvider, string dataRowModelName, Column column, DataRow dataRow)
        {
            var modelName = ModelNames.CreatePropertyModelName(dataRowModelName, column.Name);
            var valueProviderResult = valueProvider.GetValue(modelName);
            if (valueProviderResult == ValueProviderResult.None)
            {
                _logger.FoundNoColumnValueInRequest(dataRowModelName, column);
                return;
            }

            var columnValueConverter = column.GetAddon<IColumnValueConverter>();

            object value;
            try
            {
                value = columnValueConverter != null
                    ? columnValueConverter.ConvertToColumnValue(column, valueProviderResult, ConvertColumnValue)
                    : ConvertColumnValue(column, valueProviderResult);
            }
            catch (Exception exception)
            {
                AddConvertColumnValueError(modelState, modelName, valueProviderResult, column, exception);
                // Were able to find a converter for the type but conversion failed.
                return;
            }

            if (!VerifyNullColumnValue(column, value))
            {
                modelState.AddModelError(modelName, s_modelBindingMessageProvider.ValueMustNotBeNullAccessor(valueProviderResult.ToString()));
                return;
            }

            column.SetValue(dataRow, value);
            modelState.SetModelValue(modelName, value, valueProviderResult.ToString());
        }

        private void AddConvertColumnValueError(ModelStateDictionary modelState, string modelName, ValueProviderResult valueProviderResult, Column column, Exception exception)
        {
            var isFormatException = exception is FormatException;
            if (!isFormatException && exception.InnerException != null)
            {
                // TypeConverter throws System.Exception wrapping the FormatException,
                // so we capture the inner exception.
                exception = ExceptionDispatchInfo.Capture(exception.InnerException).SourceException;
            }

            var errorMessage = GetErrorMessage(column, exception);
            if (!string.IsNullOrEmpty(errorMessage))
                modelState.AddModelError(modelName, errorMessage);
            else
                modelState.TryAddModelException(modelName, exception);
        }

        private static ModelBindingMessageProvider s_modelBindingMessageProvider = new DefaultModelBindingMessageProvider();

        private static string GetErrorMessage(Column column, Exception exception)
        {
            if (exception is FormatException || exception is OverflowException)
                return s_modelBindingMessageProvider.UnknownValueIsInvalidAccessor(column.DisplayName);
            else
                return exception.Message;
        }

        private static object ConvertColumnValue(Column column, ValueProviderResult valueProviderResult)
        {
            var text = valueProviderResult.FirstValue;
            var dataType = column.DataType;
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var typeConverter = TypeDescriptor.GetConverter(column.DataType);
            return typeConverter.ConvertFromString(context: null, culture: valueProviderResult.Culture, text: text);
        }

        private static bool VerifyNullColumnValue(Column column, object value)
        {
            if (value != null)
                return true;

            var dataType = column.DataType;
            var isNullableValueType = Nullable.GetUnderlyingType(dataType) != null;
            var isReferenceType = !dataType.IsValueType;
            return isReferenceType || isNullableValueType;
        }
    }
}
