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

        protected override Severity ValidationSeverity
        {
            get { return Severity.Error; }
        }

        protected override _Boolean GetValidCondition(Column column)
        {
            return !column.IsNull();
        }

        protected override _String FormatMessage(Column column)
        {
            return Strings.RequiredAttribute_DefaultErrorMessage(column);
        }
    }
}
