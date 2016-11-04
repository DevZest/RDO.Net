using DevZest.Data.Utilities;
using System;
using System.Collections.Generic;

namespace DevZest.Data
{
    partial class Model
    {
        protected abstract class Validator : IValidator
        {
            private sealed class ValidatorImpl : Validator
            {
                internal ValidatorImpl(ValidatorId id, ValidationSeverity severity, Column[] columns, _Boolean isValidCondition, _String message)
                    : base(id)
                {
                    _severity = severity;
                    _columns = ColumnSet.Create(columns);
                    _isValidCondition = isValidCondition;
                    _message = message;
                }

                _String _message;

                ValidationSeverity _severity;
                public override ValidationSeverity Severity
                {
                    get { return _severity; }
                }

                IColumnSet _columns;
                public override IColumnSet Columns
                {
                    get { return _columns; }
                }

                _Boolean _isValidCondition;
                public override _Boolean IsValidCondition
                {
                    get { return _isValidCondition; }
                }

                public override _String Message
                {
                    get { return _message; }
                }
            }

            public static Validator Create(ValidatorId validatorId, ValidationSeverity severity, _Boolean isValidCondition, _String message, params Column[] columns)
            {
                Utilities.Check.NotNull(isValidCondition, nameof(isValidCondition));
                Utilities.Check.NotNull(message, nameof(message));

                return new ValidatorImpl(validatorId, severity, columns, isValidCondition, message);
            }

            protected Validator(ValidatorId id)
            {
                Id = id;
            }

            public ValidatorId Id { get; private set; }

            public abstract ValidationSeverity Severity { get; }

            public abstract IColumnSet Columns { get; }

            public abstract _Boolean IsValidCondition { get; }

            public abstract _String Message { get; }
        }
    }
}
