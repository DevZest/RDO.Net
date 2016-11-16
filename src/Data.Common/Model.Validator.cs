using DevZest.Data.Primitives;
using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    partial class Model
    {
        protected static class Validator
        {
            private sealed class SimpleValidator : IValidator
            {
                public SimpleValidator(string messageId, ValidationSeverity severity, Column[] columns, _Boolean isValidCondition, _String message)
                {
                    _messageId = messageId;
                    _severity = severity;
                    _columns = ColumnSet.New(columns);
                    _isValidCondition = isValidCondition;
                    _message = message;
                }

                string _messageId;
                ValidationSeverity _severity;
                IColumnSet _columns;
                _Boolean _isValidCondition;
                _String _message;

                public ValidationMessage Validate(DataRow dataRow)
                {
                    return _isValidCondition[dataRow] == true ? null : new ValidationMessage(_messageId, _severity, _columns, _message[dataRow]);
                }
            }

            public static IValidator Create(string messageId, ValidationSeverity severity, _Boolean isValidCondition, _String message, params Column[] columns)
            {
                Utilities.Check.NotNull(isValidCondition, nameof(isValidCondition));
                Utilities.Check.NotNull(message, nameof(message));

                return new SimpleValidator(messageId, severity, columns, isValidCondition, message);
            }

            private sealed class DelegateValidator : IValidator
            {
                public DelegateValidator(Func<DataRow, ValidationMessage> func)
                {
                    Debug.Assert(func != null);
                    _func = func;
                }

                Func<DataRow, ValidationMessage> _func;

                public ValidationMessage Validate(DataRow dataRow)
                {
                    return _func(dataRow);
                }
            }

            public static IValidator Create(Func<DataRow, ValidationMessage> func)
            {
                Utilities.Check.NotNull(func, nameof(func));
                return new DelegateValidator(func);
            }
        }
    }
}
