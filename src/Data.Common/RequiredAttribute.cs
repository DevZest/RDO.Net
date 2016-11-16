using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredAttribute : ColumnValidatorAttribute
    {
        protected internal sealed override void Initialize(Column column)
        {
            column.Nullable(false);
        }

        protected override ValidationSeverity ValidationSeverity
        {
            get { return ValidationSeverity.Error; }
        }

        protected override _Boolean GetIsValidCondition(Column column)
        {
            return !column.IsNull();
        }

        protected override _String FormatMessage(Column column)
        {
            return Strings.RequiredAttribute_DefaultErrorMessage(column);
        }
    }
}
