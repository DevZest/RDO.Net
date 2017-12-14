using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ValidationColumnGroupAttribute : ColumnGroupAttribute
    {
        protected ValidationColumnGroupAttribute(string name, string message)
            : base(name)
        {
            Check.NotEmpty(message, nameof(message));
            _message = message;
        }

        protected ValidationColumnGroupAttribute(string name, Type messageResourceType, string message)
            : this(name, message)
        {
            Check.NotNull(messageResourceType, nameof(messageResourceType));
            _messageResourceType = messageResourceType;
            _messageGetter = messageResourceType.ResolveStringGetter(message);
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

        private readonly Func<String> _messageGetter;
        internal string MessageString
        {
            get { return _messageGetter != null ? _messageGetter() : _message; }
        }
    }
}
