using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredAttribute : ColumnAttribute, IColumnValidationFactory
    {
        private sealed class RequiredValidation : DataValidation
        {
            internal RequiredValidation(Column column, string errorMessage)
                : base("DevZest.Data.Required")
            {
                Debug.Assert(column != null);
                Debug.Assert(!string.IsNullOrEmpty(errorMessage));

                _column = column;
                _errorMessage = errorMessage;
            }

            private Column _column;
            private string _errorMessage;


            public override int ColumnCount
            {
                get { return 1; }
            }

            public override Column this[int index]
            {
                get
                {
                    if (index != 0)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return _column;
                }
            }

            public override string Validate(DataRow dataRow)
            {
                return _column.IsNull(dataRow) ? _errorMessage : null;
            }
        }

        public RequiredAttribute()
        {
        }

        public string ErrorMessage { get; private set; }

        protected internal sealed override void Initialize(Column column)
        {
            column.Nullable(false);
        }

        IEnumerable<DataValidation> IColumnValidationFactory.GetValidations(Column column)
        {
            throw new NotImplementedException();
        }
    }
}
