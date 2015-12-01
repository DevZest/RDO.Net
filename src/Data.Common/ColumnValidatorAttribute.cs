using DevZest.Data.Utilities;
using System;
using System.Diagnostics;

namespace DevZest.Data
{
    public abstract class ColumnValidatorAttribute : ColumnAttribute, IColumnValidator
    {
        private sealed class Validator : IValidator
        {
            internal Validator(ColumnValidatorAttribute owner, Column column)
            {
                Debug.Assert(owner != null);
                Debug.Assert(column != null);

                _owner = owner;
                _column = column;
            }

            private ColumnValidatorAttribute _owner;
            private Column _column;

            public ValidatorId Id
            {
                get { return _owner.Id; }
            }

            private ValidationLevel ValidationLevel
            {
                get { return _owner.ValidationLevel; }
            }

            private string GetMessage(DataRow dataRow)
            {
                return _owner.GetMessage(_column, dataRow);
            }

            public ValidationMessage Validate(DataRow dataRow)
            {
                return _owner.IsValid(_column, dataRow) ? null : new ValidationMessage(Id, ValidationLevel, GetMessage(dataRow), _column);
            }
        }

        public IValidator GetValidator(Column column)
        {
            return new Validator(this, column);
        }

        public abstract ValidatorId Id { get; }

        protected abstract ValidationLevel ValidationLevel { get; }

        protected abstract bool IsValid(Column column, DataRow dataRow);

        protected abstract string FormatMessage(Column column, DataRow dataRow);

        public string Message { get; set; }

        public Type MessageFuncType { get; set; }

        public string MessageFuncName { get; set; }

        private string GetMessage(Column column, DataRow dataRow)
        {
            var messageFunc = MessageFunc;
            if (messageFunc != null)
                return messageFunc(column, dataRow);

            if (Message != null)
                return Message;

            return FormatMessage(column, dataRow);
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
    }
}
