using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredAttribute : ColumnValidatorAttribute
    {
        public readonly static ValidatorId ValidatorId = new ValidatorId(typeof(RequiredAttribute), nameof(ValidatorId));

        protected sealed override void OnInitialize(Column column)
        {
            column.Nullable(false);
        }

        public override ValidatorId Id
        {
            get { return ValidatorId; }
        }

        protected override ValidationLevel ValidationLevel
        {
            get { return ValidationLevel.Error; }
        }

        protected override bool IsValid(Column column, DataRow dataRow)
        {
            return !column.IsNull(dataRow);
        }

        protected override string FormatMessage(Column column, DataRow dataRow)
        {
            return Strings.RequiredAttribute_DefaultErrorMessage(column);
        }
    }
}
