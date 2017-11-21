using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Primitives
{
    public abstract class ValidatorAttribute : ColumnAttribute
    {
        private string _messageId;
        public string MessageId
        {
            get { return string.IsNullOrEmpty(_messageId) ? DefaultMessageId : _messageId; }
            set { _messageId = value; }
        }

        protected virtual string DefaultMessageId
        {
            get
            {
                var type = GetType();
                var typeName = type.Name;
                if (typeName.EndsWith("Attribute"))
                    typeName = typeName.Substring(0, typeName.Length - "Attribute".Length);
                return type.Namespace + "." + typeName;
            }
        }

        public string Message { get; set; }

        public Type MessageFuncType { get; set; }

        public string MessageFuncName { get; set; }
    }
}
