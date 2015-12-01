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
                private static Column[] s_emptyColumns = new Column[0];

                internal ValidatorImpl(ValidatorId id, ValidationLevel level, IReadOnlyList<Column> columns, _Boolean condition, Func<DataRow, string> messageFunc)
                    : base(id)
                {
                    _level = level;
                    _columns = columns ?? s_emptyColumns;
                    _condition = condition;
                    _messageFunc = messageFunc;
                }

                ValidationLevel _level;
                _Boolean _condition;
                Func<DataRow, string> _messageFunc;
                IReadOnlyList<Column> _columns;

                protected override ValidationLevel Level
                {
                    get { return _level; }
                }

                protected override IReadOnlyList<Column> Columns
                {
                    get { return _columns; }
                }

                protected override bool IsValid(DataRow dataRow)
                {
                    return _condition.Eval(dataRow) == true;
                }

                protected override string GetMessage(DataRow dataRow)
                {
                    return _messageFunc(dataRow);
                }
            }

            public static Validator Create(ValidatorId validatorId, ValidationLevel level, _Boolean condition, Func<DataRow, string> errorMessageFunc, Column column)
            {
                Utilities.Check.NotNull(condition, nameof(condition));
                Utilities.Check.NotNull(errorMessageFunc, nameof(errorMessageFunc));
                Utilities.Check.NotNull(column, nameof(column));

                return new ValidatorImpl(validatorId, level, column, condition, errorMessageFunc);
            }

            public static Validator Create(ValidatorId validatorId, ValidationLevel level, _Boolean condition, Func<DataRow, string> errorMessageFunc, params Column[] columns)
            {
                Utilities.Check.NotNull(condition, nameof(condition));
                Utilities.Check.NotNull(errorMessageFunc, nameof(errorMessageFunc));

                return new ValidatorImpl(validatorId, level, columns, condition, errorMessageFunc);
            }

            public static Validator Create(ValidatorId validatorId, ValidationLevel level, _Boolean condition, Func<DataRow, string> errorMessageFunc)
            {
                Utilities.Check.NotNull(condition, nameof(condition));
                Utilities.Check.NotNull(errorMessageFunc, nameof(errorMessageFunc));

                return new ValidatorImpl(validatorId, level, null, condition, errorMessageFunc);
            }


            protected Validator(ValidatorId id)
            {
                Id = id;
            }

            public ValidatorId Id { get; private set; }

            protected abstract ValidationLevel Level { get; }

            protected abstract IReadOnlyList<Column> Columns { get; }

            protected abstract bool IsValid(DataRow dataRow);

            protected abstract string GetMessage(DataRow dataRow);

            public ValidationMessage Validate(DataRow dataRow)
            {
                return IsValid(dataRow) ? null : new ValidationMessage(Id, Level, GetMessage(dataRow), Columns);
            }
        }
    }
}
