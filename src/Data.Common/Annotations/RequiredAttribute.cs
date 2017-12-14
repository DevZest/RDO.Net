using DevZest.Data.Annotations.Primitives;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredAttribute : ValidationColumnAttribute
    {
        protected override void Initialize(Column column)
        {
            base.Initialize(column);
            column.Nullable(false);
        }

        protected override bool IsValid(Column column, DataRow dataRow)
        {
            return !column.IsNull(dataRow);
        }

        protected override string DefaultMessageString
        {
            get { return Strings.RequiredAttribute_DefaultErrorMessage; }
        }

        protected override bool CoerceDeclaringTypeOnly(bool value)
        {
            return true;
        }
    }
}
