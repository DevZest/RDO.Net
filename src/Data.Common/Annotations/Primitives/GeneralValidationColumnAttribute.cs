using DevZest.Data.Utilities;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

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

        public Type MessageResourceType { get; set; }

        private string GetMessage(Column column, DataRow dataRow)
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
                if (MessageResourceType == null)
                    return null;

                if (_messageGetter == null)
                    _messageGetter = MessageResourceType.GetMessageGetter(Message);

                return _messageGetter;
            }
        }
    }
}
