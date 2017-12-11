using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ColumnValidatorAttribute : ColumnWiringAttribute
    {
        public ColumnValidatorAttribute(string columnName)
        {
            Check.NotEmpty(columnName, nameof(columnName));
            _columnName = columnName;
        }

        private readonly string _columnName;
        public string ColumnName
        {
            get { return _columnName; }
        }
    }
}
