using DevZest.Data.Utilities;
using System;
using System.Diagnostics;
using System.Collections.Generic;

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

            private ValidatorId Id
            {
                get { return _owner.Id; }
            }

            private ValidationSeverity Severity
            {
                get { return _owner.ValidationSeverity; }
            }

            private IColumnSet Columns
            {
                get { return _column; }
            }

            private _Boolean IsValidCondition
            {
                get { return _owner.GetIsValidCondition(_column); }
            }

            private _String Message
            {
                get { return _owner.GetMessage(_column); }
            }

            public ValidationMessage Validate(DataRow dataRow)
            {
                return IsValidCondition[dataRow] == true ? null : new ValidationMessage(Id, Severity, Columns, Message[dataRow]);
            }
        }

        public IValidator GetValidator(Column column)
        {
            return new Validator(this, column);
        }

        public abstract ValidatorId Id { get; }

        protected abstract ValidationSeverity ValidationSeverity { get; }

        protected abstract _Boolean GetIsValidCondition(Column column);

        protected abstract _String FormatMessage(Column column);

        public string Message { get; set; }

        public Type MessageFuncType { get; set; }

        public string MessageFuncName { get; set; }

        private _String GetMessage(Column column)
        {
            var messageFunc = MessageFunc;
            if (messageFunc != null)
                return messageFunc(column);

            if (Message != null)
                return Message;

            return FormatMessage(column);
        }

        private Func<Column, _String> _messageFunc;
        private Func<Column, _String> MessageFunc
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

        private static Func<Column, _String> GetMessageFunc(Type funcType, string funcName)
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
