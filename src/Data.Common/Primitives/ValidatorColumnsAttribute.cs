using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    public abstract class ValidatorColumnsAttribute : ColumnsAttribute
    {
        protected ValidatorColumnsAttribute(string name)
            : base(name)
        {
        }

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

        public string GetMessage(IReadOnlyList<Column> columns, DataRow dataRow)
        {
            var messageFunc = MessageFunc;
            if (messageFunc != null)
                return messageFunc(Name, columns, dataRow);

            if (Message != null)
                return Message;

            return GetDefaultMessage(columns, dataRow);
        }

        private Func<string, IReadOnlyList<Column>, DataRow, string> _messageFunc;
        private Func<string, IReadOnlyList<Column>, DataRow, string> MessageFunc
        {
            get
            {
                if (MessageFuncType == null && MessageFuncName == null)
                    return null;

                if (_messageFunc == null)
                    _messageFunc = GetMessageFunc(MessageFuncType, MessageFuncName);

                return _messageFunc;
            }
        }

        private static Func<string, IReadOnlyList<Column>, DataRow, string> GetMessageFunc(Type funcType, string funcName)
        {
            if (!(funcType != null && funcName != null))
                throw new InvalidOperationException(Strings.ValidatorColumnsAttribute_InvalidMessageFunc(funcType, funcName));

            try
            {
                return funcType.GetColumnsMessageFunc(funcName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.ValidatorColumnsAttribute_InvalidMessageFunc(funcType, funcName), ex);
            }
        }

        protected abstract string GetDefaultMessage(IReadOnlyList<Column> columns, DataRow dataRow);
    }
}
