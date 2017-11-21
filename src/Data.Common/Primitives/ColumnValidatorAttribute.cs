using DevZest.Data.Utilities;
using System;
using System.Diagnostics;

namespace DevZest.Data.Primitives
{
    public abstract class ColumnValidatorAttribute : ValidatorAttribute
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

            public IColumnValidationMessages Validate(DataRow dataRow)
            {
                return _owner.Validate(_column, dataRow);
            }
        }

        protected internal override void Initialize(Column column)
        {
            var model = column.ParentModel;
            model.Validators.Add(new Validator(this, column));
        }

        protected abstract IColumnValidationMessages Validate(Column column, DataRow dataRow);

        protected string GetMessage(Column column, DataRow dataRow)
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
