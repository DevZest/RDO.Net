using System.Diagnostics;

namespace DevZest.Data.Annotations.Primitives
{
    public abstract class ValidationColumnAttribute : ColumnAttribute
    {
        private sealed class Validator : IValidator
        {
            internal Validator(ValidationColumnAttribute owner, Column column)
            {
                Debug.Assert(owner != null);
                Debug.Assert(column != null);

                _owner = owner;
                _column = column;
            }

            private ValidationColumnAttribute _owner;
            private Column _column;

            public IColumnValidationMessages Validate(DataRow dataRow)
            {
                return _owner.Validate(_column, dataRow);
            }
        }

        protected override void Initialize(Column column)
        {
            var model = column.ParentModel;
            model.Validators.Add(new Validator(this, column));
        }

        protected abstract IColumnValidationMessages Validate(Column column, DataRow dataRow);
    }
}
