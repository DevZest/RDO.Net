using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace DevZest.Data.AspNetCore
{
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, string, Type, string, Exception> _attemptingToBindParameterDataSetModel;
        private static readonly Action<ILogger, string, Type, Exception> _doneAttemptingToBindParameterDataSetModel;
        private static readonly Action<ILogger, Type, string, Type, string, Exception> _attemptingToBindPropertyDataSetModel;
        private static readonly Action<ILogger, Type, string, Type, Exception> _doneAttemptingToBindPropertyDataSetModel;
        private static readonly Action<ILogger, Type, string, Exception> _attemptingToBindDataSetModel;
        private static readonly Action<ILogger, Type, string, Exception> _doneAttemptingToBindDataSetModel;

        private static readonly Action<ILogger, string, Type, string, Type, Exception> _foundNoDataSetValueForPropertyInRequest;
        private static readonly Action<ILogger, string, string, Type, Exception> _foundNoDataSetValueForParameterInRequest;
        private static readonly Action<ILogger, string, Type, Exception> _foundNoDataSetValueInRequest;
        private static readonly Action<ILogger, string, string, Exception> _foundNoColumnValueInRequest;

        static LoggerExtensions()
        {
            _attemptingToBindParameterDataSetModel = LoggerMessage.Define<string, Type, string>(
                LogLevel.Debug,
                1000,
                "Attempting to bind parameter '{ParameterName}' of type DataSet<'{ModelType}'> using the name '{ModelName}' in request data ...");

            _doneAttemptingToBindParameterDataSetModel = LoggerMessage.Define<string, Type>(
               LogLevel.Debug,
               1001,
               "Done attempting to bind parameter '{ParameterName}' of type DataSet<'{ModelType}'>.");

            _attemptingToBindPropertyDataSetModel = LoggerMessage.Define<Type, string, Type, string>(
               LogLevel.Debug,
               1002,
               "Attempting to bind property '{PropertyContainerType}.{PropertyName}' of type DataSet<'{ModelType}'> using the name '{ModelName}' in request data ...");

            _doneAttemptingToBindPropertyDataSetModel = LoggerMessage.Define<Type, string, Type>(
               LogLevel.Debug,
               1003,
               "Done attempting to bind property '{PropertyContainerType}.{PropertyName}' of type DataSet<'{ModelType}'>.");

            _attemptingToBindDataSetModel = LoggerMessage.Define<Type, string>(
                LogLevel.Debug,
                1004,
                "Attempting to bind model of type DataSet<'{ModelType}'> using the name '{ModelName}' in request data ...");

            _doneAttemptingToBindDataSetModel = LoggerMessage.Define<Type, string>(
                LogLevel.Debug,
                1005,
                "Done attempting to bind model of type DataSet<'{ModelType}'> using the name '{ModelName}'.");

            _foundNoDataSetValueForParameterInRequest = LoggerMessage.Define<string, string, Type>(
               LogLevel.Debug,
               1006,
               "Could not find a value in the request with name '{ModelName}' for binding parameter '{ModelFieldName}' of type DataSet<'{ModelType}'>.");

            _foundNoDataSetValueForPropertyInRequest = LoggerMessage.Define<string, Type, string, Type>(
               LogLevel.Debug,
               1007,
               "Could not find a value in the request with name '{ModelName}' for binding property '{PropertyContainerType}.{ModelFieldName}' of type DataSet<'{ModelType}'>.");

            _foundNoDataSetValueInRequest = LoggerMessage.Define<string, Type>(
               LogLevel.Debug,
               1008,
               "Could not find a value in the request with name '{ModelName}' of type DataSet<'{ModelType}'>.");

            _foundNoColumnValueInRequest = LoggerMessage.Define<string, string>(
               LogLevel.Debug,
               1008,
               "Could not find a value in the request - DataRow: '{DataRowModelName}'; Column: '{ColumnName}'.");
        }

        public static void AttemptingToBindDataSetModel<T>(this ILogger logger, ModelBindingContext bindingContext)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
                return;

            var modelMetadata = bindingContext.ModelMetadata;
            switch (modelMetadata.MetadataKind)
            {
                case ModelMetadataKind.Parameter:
                    _attemptingToBindParameterDataSetModel(
                        logger,
                        modelMetadata.ParameterName,
                        typeof(T),
                        bindingContext.ModelName,
                        null);
                    break;
                case ModelMetadataKind.Property:
                    _attemptingToBindPropertyDataSetModel(
                        logger,
                        modelMetadata.ContainerType,
                        modelMetadata.PropertyName,
                        typeof(T),
                        bindingContext.ModelName,
                        null);
                    break;
                case ModelMetadataKind.Type:
                    _attemptingToBindDataSetModel(logger, typeof(T), bindingContext.ModelName, null);
                    break;
            }
        }

        public static void DoneAttemptingToBindDataSetModel<T>(this ILogger logger, ModelBindingContext bindingContext)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
                return;

            var modelMetadata = bindingContext.ModelMetadata;
            switch (modelMetadata.MetadataKind)
            {
                case ModelMetadataKind.Parameter:
                    _doneAttemptingToBindParameterDataSetModel(
                        logger,
                        modelMetadata.ParameterName,
                        typeof(T),
                        null);
                    break;
                case ModelMetadataKind.Property:
                    _doneAttemptingToBindPropertyDataSetModel(
                        logger,
                        modelMetadata.ContainerType,
                        modelMetadata.PropertyName,
                        typeof(T),
                        null);
                    break;
                case ModelMetadataKind.Type:
                    _doneAttemptingToBindDataSetModel(logger, typeof(T), bindingContext.ModelName, null);
                    break;
            }
        }

        public static void FoundNoDataSetValueInRequest<T>(this ILogger logger, ModelBindingContext bindingContext)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
                return;

            var modelMetadata = bindingContext.ModelMetadata;
            switch (modelMetadata.MetadataKind)
            {
                case ModelMetadataKind.Parameter:
                    _foundNoDataSetValueForParameterInRequest(
                        logger,
                        bindingContext.ModelName,
                        modelMetadata.ParameterName,
                        typeof(T),
                        null);
                    break;
                case ModelMetadataKind.Property:
                    _foundNoDataSetValueForPropertyInRequest(
                        logger,
                        bindingContext.ModelName,
                        modelMetadata.ContainerType,
                        modelMetadata.PropertyName,
                        typeof(T),
                        null);
                    break;
                case ModelMetadataKind.Type:
                    _foundNoDataSetValueInRequest(
                        logger,
                        bindingContext.ModelName,
                        typeof(T),
                        null);
                    break;
            }
        }

        public static void FoundNoColumnValueInRequest(this ILogger logger, string dataRowModelName, Column column)
        {
            if (!logger.IsEnabled(LogLevel.Debug))
                return;

            _foundNoColumnValueInRequest(
                logger,
                dataRowModelName,
                column.Name,
                null);
        }
    }
}
