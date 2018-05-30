using DevZest.Data.Annotations.Primitives;
using DevZest.Data.Utilities;
using System;

namespace DevZest.Data.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ColumnInitializerAttribute : ColumnWireupAttribute
    {
        public ColumnInitializerAttribute(string columnName)
        {
            columnName.VerifyNotEmpty(nameof(columnName));
            _columnName = columnName;
        }

        private readonly string _columnName;
        public string ColumnName
        {
            get { return _columnName; }
        }
    }
}
