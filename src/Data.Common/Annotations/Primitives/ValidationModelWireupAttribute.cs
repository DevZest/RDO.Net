using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ValidationModelWireupAttribute : ModelWireupAttribute
    {
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
