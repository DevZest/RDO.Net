using DevZest.Data.Utilities;
using System;
using System.Diagnostics;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class GeneralValidationColumnAttribute : ValidationColumnAttribute
    {
        protected sealed override IColumnValidationMessages Validate(Column column, DataRow dataRow)
        {
            return IsValid(column, dataRow) ? ColumnValidationMessages.Empty : new ColumnValidationMessage(Severity, GetMessage(column, dataRow), column);
        }

        protected abstract bool IsValid(Column column, DataRow dataRow);

        protected virtual ValidationSeverity Severity
        {
            get { return ValidationSeverity.Error; }
        }

        public string Message { get; set; }

        public Type ResourceType { get; set; }

        protected string GetMessage(Column column, DataRow dataRow)
        {
            var messageFunc = MessageGetter;
            if (messageFunc != null)
                return messageFunc(column, dataRow);

            if (Message != null)
                return Message;

            return GetDefaultMessage(column, dataRow);
        }

        protected abstract string GetDefaultMessage(Column column, DataRow dataRow);

        private Func<Column, DataRow, string> _messageGetter;
        private Func<Column, DataRow, string> MessageGetter
        {
            get
            {
                if (ResourceType == null)
                    return null;

                if (_messageGetter == null)
                    _messageGetter = GetMessageGetter(ResourceType, Message);

                return _messageGetter;
            }
        }

        private static Func<Column, DataRow, string> GetMessageGetter(Type funcType, string funcName)
        {
            Debug.Assert(funcType != null);
            if (string.IsNullOrWhiteSpace(funcName))
                throw new InvalidOperationException(Strings.ValidatorColumnAttribute_InvalidMessageFunc(funcType, funcName));

            try
            {
                return funcType.GetMessageFunc(funcName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.ValidatorColumnAttribute_InvalidMessageFunc(funcType, funcName), ex);
            }
        }
    }
}
