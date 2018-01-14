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

        public ColumnValidatorAttribute(string columnName, Type messageResourceType, string message)
            : this(columnName, message)
        {
            Check.NotNull(messageResourceType, nameof(messageResourceType));
            _messageResourceType = messageResourceType;
            _messageGetter = messageResourceType.ResolveStringGetter(message);
        }

        private readonly string _columnName;
        public string ColumnName
        {
            get { return _columnName; }
        }

        private readonly string _message;
        public string Message
        {
            get { return _message; }
        }

        private readonly Type _messageResourceType;
        public Type MessageResourceType
        {
            get { return _messageResourceType; }
        }

        private readonly Func<string> _messageGetter;
        internal string MessageString
        {
            get { return _messageGetter != null ? _messageGetter() : _message; }
        }

    }
}
