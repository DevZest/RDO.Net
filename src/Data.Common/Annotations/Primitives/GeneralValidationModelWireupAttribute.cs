using DevZest.Data.Utilities;
using System;

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
                    _messageFunc = GetMessageFunc(ResourceType, Message);

                return _messageFunc;
            }
        }

        private static Func<Model, DataRow, string> GetMessageFunc(Type funcType, string funcName)
        {
            throw new NotImplementedException();
            //if (!(funcType != null && funcName != null))
            //    throw new InvalidOperationException(Strings.ValidatorColumnAttribute_InvalidMessageFunc(funcType, funcName));

            //try
            //{
            //    throw new NotImplementedException();
            //}
            //catch (Exception ex)
            //{
            //    throw new InvalidOperationException(Strings.ValidatorColumnAttribute_InvalidMessageFunc(funcType, funcName), ex);
            //}
        }
    }
}
