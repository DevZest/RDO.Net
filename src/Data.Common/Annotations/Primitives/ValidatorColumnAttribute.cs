using DevZest.Data.Utilities;
using System;
using System.Diagnostics;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ValidatorColumnAttribute : ColumnAttribute
    {
        private sealed class Validator : IValidator
        {
            internal Validator(ValidatorColumnAttribute owner, Column column)
            {
                Debug.Assert(owner != null);
                Debug.Assert(column != null);

                _owner = owner;
                _column = column;
            }

            private ValidatorColumnAttribute _owner;
            private Column _column;

            public IColumnValidationMessages Validate(DataRow dataRow)
            {
                return _owner.Validate(_column, dataRow);
            }
        }

        protected override void Initialize(Column column)
        {
            var model = column.ParentModel;
            model.Validators.Add(new Validator(this, column));
        }

        protected abstract IColumnValidationMessages Validate(Column column, DataRow dataRow);

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

        protected string GetMessage(Column column, DataRow dataRow)
        {
            var messageFunc = MessageFunc;
            if (messageFunc != null)
                return messageFunc(column, dataRow);

            if (Message != null)
                return Message;

            return GetDefaultMessage(column, dataRow);
        }

        protected abstract string GetDefaultMessage(Column column, DataRow dataRow);

        private Func<Column, DataRow, string> _messageFunc;
        private Func<Column, DataRow, string> MessageFunc
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

        private static Func<Column, DataRow, string> GetMessageFunc(Type funcType, string funcName)
        {
            if (!(funcType != null && funcName != null))
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
