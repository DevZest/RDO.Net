using DevZest.Data.Utilities;
using System;
using System.Globalization;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class GeneralValidationColumnAttribute : ValidationColumnAttribute
    {
        protected sealed override IColumnValidationMessages Validate(Column column, DataRow dataRow)
        {
            return IsValid(column, dataRow) ? ColumnValidationMessages.Empty : new ColumnValidationMessage(Severity, FormatMessage(column.DisplayName), column);
        }

        protected abstract bool IsValid(Column column, DataRow dataRow);

        protected virtual ValidationSeverity Severity
        {
            get { return ValidationSeverity.Error; }
        }

        public string Message { get; set; }

        public Type MessageResourceType { get; set; }

        protected string MessageString
        {
            get
            {
                var messageGetter = MessageGetter;
                if (messageGetter != null)
                    return messageGetter();

                if (Message != null)
                    return Message;

                return DefaultMessageString;
            }
        }

        protected virtual string FormatMessage(string columnDisplayName)
        {
            return String.Format(CultureInfo.CurrentCulture, MessageString, columnDisplayName);
        }

        protected abstract string DefaultMessageString { get; }

        private Func<string> _messageGetter;
        private Func<string> MessageGetter
        {
            get
            {
                if (MessageResourceType == null)
                    return null;

                if (_messageGetter == null)
                    _messageGetter = MessageResourceType.ResolveStringGetter(Message);

                return _messageGetter;
            }
        }
    }
}
