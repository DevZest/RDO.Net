﻿using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class GeneralValidationColumnAttribute : ValidationColumnAttribute
    {
        protected sealed override IColumnValidationMessages Validate(Column column, DataRow dataRow)
        {
            return IsValid(column, dataRow) ? ColumnValidationMessages.Empty : new ColumnValidationMessage(Severity, GetMessage(column, dataRow), column);
        }

        protected abstract bool IsValid(Column column, DataRow dataRow);

        protected virtual ValidationSeverity Severity
        {
            get { return ValidationSeverity.Error; }
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
