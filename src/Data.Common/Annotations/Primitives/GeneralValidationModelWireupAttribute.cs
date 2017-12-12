using DevZest.Data.Utilities;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class GeneralValidationModelWireupAttribute : ValidationModelWireupAttribute
    {
        protected GeneralValidationModelWireupAttribute(string message)
        {
            Check.NotEmpty(message, nameof(message));
            _message = message;
        }

        protected GeneralValidationModelWireupAttribute(Type resourceType, string message)
            : this(message)
        {
            Check.NotNull(resourceType, nameof(resourceType));
            _resourceType = resourceType;
        }

        protected sealed override IColumnValidationMessages Validate(Model model, DataRow dataRow)
        {
            return IsValid(model, dataRow) ? ColumnValidationMessages.Empty : new ColumnValidationMessage(Severity, GetMessage(model, dataRow), GetValidationSource(model));
        }

        protected abstract bool IsValid(Model model, DataRow dataRow);

        protected abstract IColumns GetValidationSource(Model model);

        protected virtual ValidationSeverity Severity
        {
            get { return ValidationSeverity.Error; }
        }

        private readonly string _message;
        public string Message
        {
            get { return _message; }
        }

        private readonly Type _resourceType;
        public Type ResourceType
        {
            get { return _resourceType; }
        }

        private string GetMessage(Model model, DataRow dataRow)
        {
            var messageFunc = MessageFunc;
            return messageFunc != null ? messageFunc(model, dataRow) : Message;
        }

        private Func<Model, DataRow, string> _messageFunc;
        private Func<Model, DataRow, string> MessageFunc
        {
            get
            {
                if (ResourceType == null)
                    return null;

                if (_messageFunc == null)
                    _messageFunc = GetMessageGetter(ModelType, ResourceType, Message);

                return _messageFunc;
            }
        }

#if DEBUG
        internal    // For unit test
#else
        private
#endif
        static Func<Model, DataRow, string> GetMessageGetter(Type modelType, Type funcType, string funcName)
        {
            Debug.Assert(funcType != null);
            if (string.IsNullOrWhiteSpace(funcName))
                throw new InvalidOperationException(Strings.GeneralValidationModelWireupAttribute_InvalidMessageFunc(modelType, funcType, funcName));

            try
            {
                return GetMessageFunc(funcType, funcName, modelType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.GeneralValidationModelWireupAttribute_InvalidMessageFunc(modelType, funcType, funcName), ex);
            }
        }

        private static Func<Model, DataRow, string> GetMessageFunc(Type funcType, string funcName, Type modelType)
        {
            Debug.Assert(funcType != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(funcName));

            var methodInfo = funcType.GetStaticMethodInfo(funcName);
            var paramModel = Expression.Parameter(typeof(Model), methodInfo.GetParameters()[0].Name);
            var model = Expression.Convert(paramModel, modelType);
            var paramDataRow = Expression.Parameter(typeof(DataRow), methodInfo.GetParameters()[1].Name);
            var call = Expression.Call(methodInfo, model, paramDataRow);
            return Expression.Lambda<Func<Model, DataRow, string>>(call, paramModel, paramDataRow).Compile();
        }
    }
}
