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

        protected internal sealed override void Initialize(Column column)
        {
            column.Nullable(false);
        }

        public override ValidatorId Id
        {
            get { return ValidatorId; }
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
