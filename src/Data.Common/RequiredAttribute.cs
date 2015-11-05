using DevZest.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevZest.Data
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredAttribute : ColumnValidationAttribute
    {
        public RequiredAttribute()
        {
        }

        protected internal sealed override void Initialize(Column column)
        {
            column.Nullable(false);
        }

        protected override string ValidationName
        {
            get { return "DevZest.Data.Required"; }
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
