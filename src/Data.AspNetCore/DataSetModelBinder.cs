using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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

            _logger.AttemptingToBindDataSetModel<T>(bindingContext);

            var result = bindingContext.Model as DataSet<T> ?? CreateDataSet(bindingContext);
            bindingContext.Model = result;

            var isScalar = bindingContext.ModelMetadata.IsScalar();

            Bind(bindingContext.ModelState, valueProvider, bindingContext.ModelName, result, isScalar);
            bindingContext.Result = ModelBindingResult.Success(result);

            _logger.DoneAttemptingToBindDataSetModel<T>(bindingContext);
            return Task.CompletedTask;
        }

        protected virtual DataSet<T> CreateDataSet(ModelBindingContext bindingContext)
        {
            return DataSet<T>.Create();
        }

        private void Bind(ModelStateDictionary modelState, IValueProvider valueProvider, string modelName, DataSet dataSet, bool isScalar)
        {
            modelState.SetModelValue(modelName, dataSet, string.Empty);

            if (!string.IsNullOrEmpty(modelName) && !valueProvider.ContainsPrefix(modelName))
                return;

            var _ = dataSet.Model;

            _.SuspendIdentity();

            if (isScalar)
                BindScalar(modelState, valueProvider, modelName, dataSet);
            else
                BindCollection(modelState, valueProvider, modelName, dataSet);

            _.ResumeIdentity();
        }

        private void BindScalar(ModelStateDictionary modelState, IValueProvider valueProvider, string modelName, DataSet dataSet)
        {
            if (dataSet.Count == 0)
                dataSet.AddRow();

            Bind(modelState, valueProvider, modelName, dataSet[0]);
        }

        private void BindCollection(ModelStateDictionary modelState, IValueProvider valueProvider, string modelName, DataSet dataSet)
        {
            var indexNames = GetIndexNames(valueProvider, modelName);

            bool indexNamesIsFinite;
            if (indexNames != null)
                indexNamesIsFinite = true;
            else
            {
                indexNamesIsFinite = false;
                indexNames = Enumerable.Range(0, int.MaxValue).Select(i => i.ToString(CultureInfo.InvariantCulture));
            }

            var currentIndex = 0;
            foreach (var indexName in indexNames)
            {
                var dataRowModelName = ModelNames.CreateIndexModelName(modelName, indexName);
                if (!valueProvider.ContainsPrefix(dataRowModelName) && !indexNamesIsFinite)
                    break;

                if (currentIndex >= dataSet.Count)
                    dataSet.AddRow();
                Bind(modelState, valueProvider, dataRowModelName, dataSet[currentIndex++]);
            }
        }

        private static readonly IValueProvider EmptyValueProvider = new CompositeValueProvider();

        private static IEnumerable<string> GetIndexNames(IValueProvider valueProvider, string modelName)
        {
            var indexPropertyName = ModelNames.CreatePropertyModelName(modelName, "index");

            // Remove any value provider that may not use indexPropertyName as-is. Don't match e.g. Model[index].
            if (valueProvider is IKeyRewriterValueProvider keyRewriterValueProvider)
                valueProvider = keyRewriterValueProvider.Filter() ?? EmptyValueProvider;

            var valueProviderResultIndex = valueProvider.GetValue(indexPropertyName);
            return GetIndexNames(valueProviderResultIndex);
        }

        private static IEnumerable<string> GetIndexNames(ValueProviderResult valueProviderResult)
        {
            if (valueProviderResult != null)
            {
                var indexes = (string[])valueProviderResult;
                if (indexes != null && indexes.Length > 0)
                    return indexes;
            }

            return null;
        }

        private void Bind(ModelStateDictionary modelState, IValueProvider valueProvider, string modelName, DataRow dataRow)
        {
            modelState.SetModelValue(modelName, dataRow, string.Empty);

            dataRow.SuspendValueChangedNotification();

            var model = dataRow.Model;
            var columns = model.GetColumns();
            foreach (var column in columns)
            {
                if (column.IsReadOnly(dataRow))
                    continue;
                Bind(modelState, valueProvider, modelName, column, dataRow);
            }

            IReadOnlyList<DataSet> childDataSets = dataRow.ChildDataSets;
            foreach (var childDataSet in childDataSets)
                Bind(modelState, valueProvider, ModelNames.CreatePropertyModelName(modelName, childDataSet.Model.GetName()), childDataSet, isScalar: false);

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
