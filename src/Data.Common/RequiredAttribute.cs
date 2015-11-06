using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredAttribute : ColumnValidationAttribute
    {
        public readonly static object ValidationId = new object();

        public RequiredAttribute()
        {
        }

        protected internal sealed override void Initialize(Column column)
        {
            column.Nullable(false);
        }

        protected override object GetValidationId()
        {
            return ValidationId;
        }

        protected override Func<Column, DataRow, bool> IsValidFunc
        {
            get {  return (Column column, DataRow dataRow) => !column.IsNull(dataRow); }
        }

        protected override Func<Column, DataRow, string> DefaultErrorMessageFunc
        {
            get { return (Column column, DataRow dataRow) => Strings.RequiredAttribute_DefaultErrorMessage(column); }
        }
    }
}
