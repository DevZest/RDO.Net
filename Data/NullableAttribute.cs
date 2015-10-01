using DevZest.Data.Primitives;
using System;

namespace DevZest.Data
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class NullableAttribute : ColumnAttribute
    {
        public NullableAttribute(bool isNullable)
        {
            IsNullable = isNullable;
        }

        public bool IsNullable { get; private set; }

        protected internal sealed override void Initialize(Column column)
        {
            column.Nullable(IsNullable);
        }
    }
}
