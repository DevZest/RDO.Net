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

            public string MessageId
            {
                get { return _owner.MessageId; }
            }

            public ValidationSeverity Severity
            {
                get { return _owner.ValidationSeverity; }
            }

            public IColumns Columns
            {
                get { return _column; }
            }

            public bool IsValid(DataRow dataRow)
            {
                return _owner.IsValid(_column, dataRow);
            }

            public string GetMessage(DataRow dataRow)
            {
                return _owner.GetMessage(_column, dataRow);
            }
        }

        protected internal override void Initialize(Column column)
        {
            var model = column.ParentModel;
            model.Validators.Add(new Validator(this, column));
        }

        protected abstract bool IsValid(Column column, DataRow dataRow);
    }
}
