using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;

namespace DevZest.Data
{
    public abstract class ColumnValidatorAttribute : ColumnAttribute, IValidator
    {
        private Column _column;
        protected internal sealed override void Initialize(Column column)
        {
            _column = column;
            OnInitialize(column);
        }

        protected virtual void OnInitialize(Column column)
        {
        }

        public abstract ValidatorId Id { get; }

        protected abstract ValidationLevel ValidationLevel { get; }

        protected abstract bool IsValid(Column column, DataRow dataRow);

        protected abstract string FormatMessage(Column column, DataRow dataRow);

        public string Message { get; set; }

        public Type MessageFuncType { get; set; }

        public string MessageFuncName { get; set; }

        private string FormatMessage(DataRow dataRow)
        {
            var messageFunc = MessageFunc;
            if (messageFunc != null)
                return messageFunc(_column, dataRow);

            if (Message != null)
                return Message;

            return FormatMessage(_column, dataRow);
        }

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
                throw new InvalidOperationException(Strings.ColumnValidatorAttribute_InvalidMessageFunc(funcType, funcName));

            try
            {
                return funcType.GetMessageFunc(funcName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.ColumnValidatorAttribute_InvalidMessageFunc(funcType, funcName), ex);
            }
        }

        public IEnumerable<ValidationResult> Validate(DataRow dataRow)
        {
            if (!IsValid(_column, dataRow))
                yield return new ValidationResult(Id, ValidationLevel, FormatMessage(dataRow), _column);
        }
    }
}
