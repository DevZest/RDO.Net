using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ColumnValidatorAttribute : ColumnWireupAttribute
    {
        public ColumnValidatorAttribute(string columnName, string message)
        {
            Check.NotEmpty(columnName, nameof(columnName));
            Check.NotEmpty(message, nameof(message));
            _columnName = columnName;
            _message = message;
        }

        public ColumnValidatorAttribute(string columnName, Type resourceType, string message)
            : this(columnName, message)
        {
            Check.NotNull(resourceType, nameof(resourceType));
            _resourceType = resourceType;
        }

        private readonly string _columnName;
        public string ColumnName
        {
            get { return _columnName; }
        }

        public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;

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

        internal string GetMessage(Column column, DataRow dataRow)
        {
            var messageFunc = MessageGetter;
            return messageFunc != null ? messageFunc(column, dataRow) : Message;
        }

        private Func<Column, DataRow, string> _messageGetter;
        private Func<Column, DataRow, string> MessageGetter
        {
            get
            {
                if (ResourceType == null)
                    return null;

                if (_messageGetter == null)
                    _messageGetter = ResourceType.GetMessageGetter(Message);

                return _messageGetter;
            }
        }
    }
}
